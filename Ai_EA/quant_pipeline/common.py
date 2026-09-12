# common.py
import struct
import numpy as np
import pandas as pd
from datetime import datetime
import matplotlib.pyplot as plt
from torch.utils.data import DataLoader

def add_technical_indicators(ohlcv_array):
    """
    ohlcv_array: numpy array shape [N,5] columns O,H,L,C,V
    returns features numpy [N,10] floats
    """
    df = pd.DataFrame(ohlcv_array, columns=["O","H","L","C","V"])
    df["EMA_14"] = df["C"].ewm(span=14, adjust=False).mean()
    df["H-L"] = df["H"] - df["L"]
    df["H-PC"] = (df["H"] - df["C"].shift(1)).abs()
    df["L-PC"] = (df["L"] - df["C"].shift(1)).abs()
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

# ---------- HST 解析 ----------
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
            # note: 保持与你原来代码一致的字段顺序（dt, open, high, low, close, volume）
            records.append([dt, open_, high, low, close, volume])
    print(f"[INFO] Parsed {len(records)} records from {filepath}")
    return np.array(records, dtype=object)

def resample_to_5m(data):
    """
    data: numpy array of rows [dt, o, h, l, c, v]
    returns same structure
    """
    resampled = []
    n = 5
    for i in range(0, len(data), n):
        chunk = data[i:i+n]
        if len(chunk) < n:
            break
        dt = chunk[0][0]
        o = chunk[0][1]
        # chunk is object dtype; convert columns
        highs = np.array([row[2] for row in chunk], dtype=float)
        lows = np.array([row[3] for row in chunk], dtype=float)
        cs = np.array([row[4] for row in chunk], dtype=float)
        vs = np.array([row[5] for row in chunk], dtype=float)
        h = highs.max()
        l = lows.min()
        c = cs[-1]
        v = vs.sum()
        resampled.append([dt, o, h, l, c, v])
    return np.array(resampled, dtype=object)

import re, os
def extract_info(filename):
    base = os.path.basename(filename)
    parts = base.replace(".hst", "").split("_")
    if len(parts) >= 3:
        symbol = parts[0]
        timeframe = parts[1] + " " + parts[2]
        year_match = re.search(r"(\d{4})", base)
        year = int(year_match.group(1)) if year_match else 0
        return symbol, timeframe, year
    return "UNKNOWN", "UNKNOWN", 0

# ---------- 绘图与评估 ----------
def plot_incremental_loss(loss_history, save_path="training_loss_curve.png"):
    plt.figure(figsize=(12, 6))
    for key, losses in loss_history.items():
        plt.plot(range(1, len(losses)+1), losses, label=str(key))
    plt.xlabel("Epoch")
    plt.ylabel("Loss")
    plt.title("Training Loss Curve (Incremental)")
    plt.legend()
    plt.grid(True)
    plt.savefig(save_path)
    print(f"[INFO] Incremental training loss curve saved to {save_path}")
    plt.close()

def evaluate_direction_accuracy(model, dataset, batch_size=64, device="cpu"):
    import torch
    loader = DataLoader(dataset, batch_size=batch_size, shuffle=False)
    model.eval()
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