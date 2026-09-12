# -*- coding: utf-8 -*-
import os
import torch
import numpy as np
import pandas as pd
from config import MODEL_DIR, SEQ_LEN, DEVICE
from model import HybridCNNLSTM
from common import add_technical_indicators
from data.data_loader import load_latest_ohlcv  # 之前写的最新K线加载函数

# -------------------------------
# 1. 加载模型
# -------------------------------
def load_model(model_path=None):
    if model_path is None:
        model_path = os.path.join(MODEL_DIR, "cnn_lstm_model.pth")
    model = HybridCNNLSTM(input_dim=10).to(DEVICE)
    model.load_state_dict(torch.load(model_path, map_location=DEVICE))
    model.eval()
    return model

# -------------------------------
# 2. 数据准备
# -------------------------------
def prepare_sequence(df_latest, seq_len=SEQ_LEN):
    """
    df_latest: pd.DataFrame, 最近 N 根 OHLCV K线
    返回: numpy array [seq_len, feature_dim]，带技术指标
    """
    ohlcv = df_latest[['O','H','L','C','V']].values.astype(np.float32)
    features = add_technical_indicators(ohlcv)
    
    if len(features) < seq_len:
        raise ValueError(f"最新K线不足 {seq_len} 根，无法推理")
    
    seq = features[-seq_len:]  # 取最后 seq_len 根
    seq = torch.tensor(seq, dtype=torch.float32).unsqueeze(0).to(DEVICE)  # [1, seq_len, feature_dim]
    return seq

# -------------------------------
# 3. 推理
# -------------------------------
def predict(model, seq):
    """
    返回预测概率和方向
    """
    with torch.no_grad():
        logits = model(seq)
        prob = torch.sigmoid(logits).item()
        direction = "LONG" if prob > 0.5 else "SHORT"
    return prob, direction

# -------------------------------
# 4. 主函数
# -------------------------------
if __name__ == "__main__":
    symbol = "XAUUSD"
    df_latest = load_latest_ohlcv(symbol=symbol, n=SEQ_LEN)
    
    model = load_model()
    seq = prepare_sequence(df_latest, seq_len=SEQ_LEN)
    prob, direction = predict(model, seq)
    
    print(f"[INFO] Symbol: {symbol}")
    print(f"[INFO] Predicted probability (LONG): {prob:.4f}")
    print(f"[INFO] Predicted direction: {direction}")
