# -*- coding: utf-8 -*-
import os
import glob
import re
import numpy as np
import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import Dataset, DataLoader
from datetime import datetime
import struct
import matplotlib.pyplot as plt
from sklearn.preprocessing import MinMaxScaler
import pandas as pd
import json

# ========================
# 1. 模型定义 (双层CNN + LSTM128 + BatchNorm)
# ========================
class HybridCNNLSTM(nn.Module):
    def __init__(self, input_dim=10, cnn_channels=64, lstm_hidden=128, dropout=0.3):
        super().__init__()
        self.cnn = nn.Sequential(
            nn.Conv1d(input_dim, cnn_channels, kernel_size=3, padding=1),
            nn.BatchNorm1d(cnn_channels),
            nn.ReLU(),
            nn.Conv1d(cnn_channels, cnn_channels, kernel_size=3, padding=1),
            nn.ReLU(),
            nn.MaxPool1d(kernel_size=2)
        )
        self.lstm = nn.LSTM(cnn_channels, lstm_hidden, batch_first=True)
        self.dropout = nn.Dropout(dropout)
        self.fc1 = nn.Linear(lstm_hidden, 64)
        self.relu = nn.ReLU()
        self.fc2 = nn.Linear(64, 1)

    def forward(self, x):
        x = x.permute(0, 2, 1)  # [B, features, seq_len]
        x = self.cnn(x)
        x = x.permute(0, 2, 1)  # [B, seq_len/2, channels]
        out, _ = self.lstm(x)
        out = self.dropout(out[:, -1, :])
        out = self.fc1(out)
        out = self.relu(out)
        return self.fc2(out)

# ========================
# 2. 技术指标计算
# ========================
def add_technical_indicators(ohlcv_array):
    df = pd.DataFrame(ohlcv_array, columns=["O","H","L","C","V"])
    df["EMA_14"] = df["C"].ewm(span=14, adjust=False).mean()
    df["H-L"] = df["H"] - df["L"]
    df["H-PC"] = np.abs(df["H"] - df["C"].shift(1))
    df["L-PC"] = np.abs(df["L"] - df["C"].shift(1))
    df["TR"] = df[["H-L","H-PC","L-PC"]].max(axis=1)
    df["ATR_14"] = df["TR"].rolling(14).mean()
    delta = df["C"].diff()
    gain = delta.clip(lower=0)
    loss = -delta.clip(upper=0)
    avg_gain = gain.rolling(14).mean()
    avg_loss = loss.rolling(14).mean()
    rs = avg_gain / (avg_loss + 1e-8)
    df["RSI_14"] = 100 - 100 / (1 + rs)
    ema12 = df["C"].ewm(span=12, adjust=False).mean()
    ema26 = df["C"].ewm(span=26, adjust=False).mean()
    df["MACD"] = ema12 - ema26
    df["MACD_signal"] = df["MACD"].ewm(span=9, adjust=False).mean()
    df.fillna(0, inplace=True)

    features = df[[
        "O","H","L","C","V",
        "EMA_14","ATR_14","RSI_14","MACD","MACD_signal"
    ]].values
    return features.astype(np.float32)

# ========================
# 3. 数据集 默认使用50根K线,推理的时候需要保持同样的数据量
# ========================
class KLineDataset(Dataset):
    def __init__(self, data, seq_len=50, scaler=None, threshold=0.3):
        self.X, self.y = [], []
        for i in range(len(data) - seq_len - 1):
            seq = data[i:i+seq_len, :].astype(np.float32)
            future_close = np.float32(data[i+seq_len, 3])
            last_close   = np.float32(data[i+seq_len-1, 3])
            price_diff = future_close - last_close
            if price_diff > threshold:
                target = 1.0
            elif price_diff < -threshold:
                target = 0.0
            else:
                continue
            if scaler:
                seq = scaler.transform(seq)
            self.X.append(seq)
            self.y.append(target)
        self.X = torch.tensor(np.array(self.X), dtype=torch.float32)
        self.y = torch.tensor(np.array(self.y), dtype=torch.float32).unsqueeze(1)

    def __len__(self):
        return len(self.X)

    def __getitem__(self, idx):
        return self.X[idx], self.y[idx]

# ========================
# 4. Replay Buffer
# ========================
class ReplayBuffer:
    def __init__(self, max_size=200000):
        self.max_size = max_size
        self.data = []

    def add(self, new_data):
        self.data.extend(new_data)
        if len(self.data) > self.max_size:
            self.data = self.data[-self.max_size:]

    def get_dataset(self, seq_len=50, scaler=None):
        return KLineDataset(np.array(self.data, dtype=object), seq_len, scaler)

# ========================
# 5. HST 文件解析
# ========================
def get_hst_header_v400(filepath):
    with open(filepath, "rb") as f:
        header = f.read(148)
        version = struct.unpack("<i", header[0:4])[0]
        period = struct.unpack("<i", header[64:68])[0]
        return version, "XAUUSD", period

def parse_hst_file(filepath):
    version, symbol, period = get_hst_header_v400(filepath)
    records = []
    with open(filepath, "rb") as f:
        f.seek(148)
        record_size = 44
        while True:
            chunk = f.read(record_size)
            if len(chunk) < record_size:
                break
            time_int, open_, low, high, close, volume = struct.unpack("<iddddd", chunk)
            try:
                dt = datetime.utcfromtimestamp(time_int)
            except:
                continue
            records.append([dt, open_, high, low, close, volume])
    print(f"[INFO] Parsed {len(records)} records from {filepath}")
    return np.array(records, dtype=object)

# ========================
# 6. 1M → 5M
# ========================
def resample_to_5m(data):
    resampled = []
    n = 5
    for i in range(0, len(data), n):
        chunk = data[i:i+n]
        if len(chunk) < n:
            break
        dt = chunk[0][0]
        o = chunk[0][1]
        h = max(chunk[:,2].astype(float))
        l = min(chunk[:,3].astype(float))
        c = chunk[-1][4]
        v = sum(chunk[:,5].astype(float))
        resampled.append([dt, o, h, l, c, v])
    return np.array(resampled, dtype=object)

# ========================
# 7. 绘制训练曲线
# ========================
def plot_incremental_loss(loss_history, save_path="training_loss_curve.png"):
    plt.figure(figsize=(12, 6))
    for key, losses in loss_history.items():
        plt.plot(range(1, len(losses)+1), losses, label=key)
    plt.xlabel("Epoch")
    plt.ylabel("Loss")
    plt.title("Training Loss Curve (Incremental)")
    plt.legend()
    plt.grid(True)
    plt.savefig(save_path)
    print(f"[INFO] Loss curve saved: {save_path}")
    plt.close()

# ========================
# 8. 方向预测准确率
# ========================
def evaluate_direction_accuracy(model, dataset, batch_size=64):
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    model.eval()
    loader = DataLoader(dataset, batch_size=batch_size, shuffle=False)
    correct, total = 0, 0
    with torch.no_grad():
        for X, y in loader:
            X, y = X.to(device), y.to(device)
            preds = model(X)
            pred_direction = (preds > 0).float()
            correct += (pred_direction == y).sum().item()
            total += y.size(0)
    accuracy = correct / total if total > 0 else 0
    print(f"[INFO] Direction Accuracy: {accuracy*100:.2f}%")
    return accuracy

# ========================
# 9. 导出 TorchScript & ONNX
# ========================
def export_model(model, model_dir, seq_len=50, input_dim=10):
    os.makedirs(model_dir, exist_ok=True)
    example_input = torch.randn(1, seq_len, input_dim)

    # TorchScript
    ts_path = os.path.join(model_dir, "cnn_lstm_model.ts")
    traced = torch.jit.trace(model, example_input)
    traced.save(ts_path)
    print(f"[INFO] TorchScript saved: {ts_path}")

    # ONNX
    onnx_path = os.path.join(model_dir, "cnn_lstm_model.onnx")
    torch.onnx.export(
        model, example_input, onnx_path,
        input_names=["input"], output_names=["output"],
        dynamic_axes={"input": {0: "batch"}, "output": {0: "batch"}},
        opset_version=14
    )


    print(f"[INFO] ONNX saved: {onnx_path}")

# ========================
# 10. 主训练函数
# ========================
def main(hst_folder, seq_len=50, batch_size=64, num_epochs=20,
         model_dir="models", combine_all_years=True):

    os.makedirs(model_dir, exist_ok=True)
    model_path = os.path.join(model_dir, "cnn_lstm_model.pth")

    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    model = HybridCNNLSTM(input_dim=10).to(device)

    # 增量训练
    if os.path.exists(model_path):
        model.load_state_dict(torch.load(model_path, map_location=device))
        print(f"[INFO] Loaded existing model from {model_path}")

    criterion = nn.BCEWithLogitsLoss()
    optimizer = optim.Adam(model.parameters(), lr=1e-3)
    scheduler = torch.optim.lr_scheduler.ReduceLROnPlateau(optimizer, mode="min", factor=0.5, patience=5)

    print("[INFO] Start training...")

    hst_files = sorted(glob.glob(os.path.join(hst_folder, "*.hst")))
    buffer = ReplayBuffer(max_size=200000)
    scaler = MinMaxScaler()

    # 合并所有年份数据
    if combine_all_years:
        for hst_file in hst_files:
            data = parse_hst_file(hst_file)
            data = resample_to_5m(data)
            features = add_technical_indicators(np.array(data)[:,1:].astype(np.float32))
            buffer.add(features)

        scaler.fit(np.array(buffer.data))
        dataset = buffer.get_dataset(seq_len=seq_len, scaler=scaler)
        loader = DataLoader(dataset, batch_size=batch_size, shuffle=True)

        for epoch in range(num_epochs):
            model.train()
            epoch_losses = []
            for X, y in loader:
                X, y = X.to(device), y.to(device)
                optimizer.zero_grad()
                preds = model(X)
                loss = criterion(preds, y)
                loss.backward()
                torch.nn.utils.clip_grad_norm_(model.parameters(), 1.0)
                optimizer.step()
                epoch_losses.append(loss.item())
            avg_loss = np.mean(epoch_losses)

            scheduler.step(avg_loss)
            print(f"Epoch {epoch+1}/{num_epochs}, Loss: {avg_loss:.6f}")

        evaluate_direction_accuracy(model, dataset)

    # 保存 PyTorch 模型
    torch.save(model.state_dict(), model_path)
    print(f"[INFO] Model saved: {model_path}")

    # 保存 Scaler
    scaler_path = os.path.join(model_dir, "scaler.json")
    scaler_dict = {
        "min_": scaler.min_.tolist(),
        "scale_": scaler.scale_.tolist(),
        "data_min_": scaler.data_min_.tolist(),
        "data_max_": scaler.data_max_.tolist(),
        "data_range_": scaler.data_range_.tolist(),
        "feature_range": scaler.feature_range
    }
    with open(scaler_path, "w") as f:
        json.dump(scaler_dict, f)
    print(f"[INFO] Scaler saved: {scaler_path}")

    # 导出 TorchScript & ONNX
    export_model(model, model_dir, seq_len=seq_len, input_dim=10)

# ========================
# 11. 入口
# ========================
if __name__ == "__main__":
    hst_folder = "C:\\data\\eatest\\hst_data"
    main(hst_folder, model_dir="C:\\D\\OneDrive\\07-svn\\01-gold\\Ai_EA\\quant_pipeline\\models")


# ========================
# 12. 在线再训练 (根据数据库预测结果)
# ========================
from sqlalchemy import create_engine, text

def incremental_retrain_from_db(model_dir="models", seq_len=50, batch_size=64, num_epochs=5):
    """
    从数据库读取 trade_ai_result 的预测与实际收盘价，执行增量再训练
    """
    print("[INFO] 🔁 开始从数据库进行增量再训练...")

    # 连接数据库 (MySQL / SQLite 均可)
    # ⚠️ 若你用 MySQL，请换成对应的连接字符串
    engine = create_engine("sqlite:///C:/D/OneDrive/07-svn/01-gold/Ai_EA/quant_pipeline/trade_ai.db")

    query = text("""
        SELECT timestamp, close, prob_long, direction, actual_close
        FROM trade_ai_result
        WHERE actual_close IS NOT NULL
        ORDER BY timestamp DESC
        LIMIT 5000
    """)
    df = pd.read_sql(query, engine)

    if df.empty:
        print("[WARN] 没有可用数据进行再训练。")
        return

    # 准备特征与标签
    df["label"] = (df["actual_close"] > df["close"]).astype(float)
    X = df[["close", "prob_long"]].values.astype(np.float32)
    y = df["label"].values.astype(np.float32).reshape(-1, 1)

    # 加载模型与scaler
    model_path = os.path.join(model_dir, "cnn_lstm_model.pth")
    scaler_path = os.path.join(model_dir, "scaler.json")

    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    model = HybridCNNLSTM(input_dim=10).to(device)
    if os.path.exists(model_path):
        model.load_state_dict(torch.load(model_path, map_location=device))
        print(f"[INFO] 已加载现有模型用于微调: {model_path}")
    else:
        print("[WARN] 未找到旧模型，将初始化新模型。")

    # 加载scaler
    with open(scaler_path, "r") as f:
        scaler_dict = json.load(f)
    scaler = MinMaxScaler()
    scaler.min_ = np.array(scaler_dict["min_"])
    scaler.scale_ = np.array(scaler_dict["scale_"])
    scaler.data_min_ = np.array(scaler_dict["data_min_"])
    scaler.data_max_ = np.array(scaler_dict["data_max_"])
    scaler.data_range_ = np.array(scaler_dict["data_range_"])
    scaler.feature_range = tuple(scaler_dict["feature_range"])

    # 将close, prob_long变换成匹配输入
    X_scaled = scaler.fit_transform(np.column_stack([
        X[:, 0], X[:, 0], X[:, 0], X[:, 0], np.zeros_like(X[:, 0]),
        X[:, 0], X[:, 0], np.zeros_like(X[:, 0]), X[:, 1], X[:, 1]
    ]))  # 扩展为10维

    X_seq = []
    y_seq = []
    for i in range(len(X_scaled) - seq_len):
        X_seq.append(X_scaled[i:i+seq_len])
        y_seq.append(y[i+seq_len-1])

    X_seq = torch.tensor(X_seq, dtype=torch.float32)
    y_seq = torch.tensor(y_seq, dtype=torch.float32).unsqueeze(1)

    dataset = torch.utils.data.TensorDataset(X_seq, y_seq)
    loader = DataLoader(dataset, batch_size=batch_size, shuffle=True)

    # 训练设置
    criterion = nn.BCEWithLogitsLoss()
    optimizer = optim.Adam(model.parameters(), lr=5e-4)
    model.train()

    print(f"[INFO] 使用 {len(dataset)} 条样本进行微调...")

    for epoch in range(num_epochs):
        epoch_losses = []
        for Xb, yb in loader:
            Xb, yb = Xb.to(device), yb.to(device)
            optimizer.zero_grad()
            preds = model(Xb)
            loss = criterion(preds, yb)
            loss.backward()
            optimizer.step()
            epoch_losses.append(loss.item())
        print(f"[FineTune] Epoch {epoch+1}/{num_epochs} | Loss: {np.mean(epoch_losses):.6f}")

    # 保存更新后的模型
    torch.save(model.state_dict(), model_path)
    print(f"[INFO] ✅ 模型微调完成并保存: {model_path}")
