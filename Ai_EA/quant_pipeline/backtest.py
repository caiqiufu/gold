# backtest.py
import os
import glob
import numpy as np
import torch
import pandas as pd
from sklearn.preprocessing import MinMaxScaler

import config  # <-- 读取配置信息
from model import HybridCNNLSTM
from common import parse_hst_file, resample_to_5m, add_technical_indicators, extract_info

# ========================
# 使用 config 配置
# ========================
MODEL_PATH = os.path.join(config.MODEL_DIR, "cnn_lstm_model.pth")
SCALER_PATH = os.path.join(config.MODEL_DIR, "scaler.json")
SEQ_LEN = config.SEQ_LEN
THRESHOLD = 0.3
DEVICE = config.DEVICE
HST_FOLDER = config.HST_DIR

# 加载 Scaler
import json
def load_scaler(scaler_path):
    with open(scaler_path, "r") as f:
        scaler_dict = json.load(f)
    from sklearn.preprocessing import MinMaxScaler
    scaler = MinMaxScaler()
    scaler.min_ = np.array(scaler_dict["min_"])
    scaler.scale_ = np.array(scaler_dict["scale_"])
    scaler.data_min_ = np.array(scaler_dict["data_min_"])
    scaler.data_max_ = np.array(scaler_dict["data_max_"])
    scaler.data_range_ = np.array(scaler_dict["data_range_"])
    scaler.feature_range = tuple(scaler_dict["feature_range"])
    return scaler

# 回测函数
def backtest(model_path=MODEL_PATH, scaler_path=SCALER_PATH, hst_folder=HST_FOLDER, seq_len=SEQ_LEN, threshold=THRESHOLD):
    device = torch.device(DEVICE)

    # 1. 加载模型
    model = HybridCNNLSTM(input_dim=10).to(device)
    model.load_state_dict(torch.load(model_path, map_location=device))
    model.eval()

    # 2. 加载 Scaler
    scaler = load_scaler(scaler_path)

    # 3. 读取 HST 文件
    hst_files = sorted(glob.glob(os.path.join(hst_folder, "*.hst")), key=lambda f: extract_info(f)[2])
    buffer = []
    for f in hst_files:
        raw = parse_hst_file(f)
        data5 = resample_to_5m(raw)
        feats = add_technical_indicators(np.array(data5)[:,1:].astype(np.float32))
        buffer.extend(feats)

    if len(buffer) < seq_len + 1:
        print("[WARN] 数据不足，无法回测")
        return

    buffer_arr = np.array(buffer, dtype=np.float32)

    balance = 0.0
    pnl_list = []

    with torch.no_grad():
        for i in range(len(buffer_arr) - seq_len):
            seq = buffer_arr[i:i+seq_len]
            seq_scaled = scaler.transform(seq)
            X = torch.tensor(seq_scaled, dtype=torch.float32).unsqueeze(0).to(device)
            logits = model(X)
            prob = torch.sigmoid(logits).item()
            pred = 1 if prob > threshold else 0

            entry_close = float(seq[-1, 3])
            exit_close = float(buffer_arr[i+seq_len, 3])
            ret = exit_close - entry_close
            pnl = ret if pred == 1 else -ret
            pnl_list.append(pnl)
            balance += pnl

    # 输出统计
    pnl_arr = np.array(pnl_list)
    print(f"[BACKTEST] Steps: {len(pnl_list)}, Total PnL: {balance:.6f}, Mean: {pnl_arr.mean():.6f}, Std: {pnl_arr.std():.6f}")

    # 保存 CSV
    out_csv = os.path.join(config.MODEL_DIR, "backtest_pnl.csv")
    df = pd.DataFrame({"pnl": pnl_arr})
    df.to_csv(out_csv, index=False)
    print(f"[INFO] Backtest results saved to {out_csv}")

if __name__ == "__main__":
    backtest()