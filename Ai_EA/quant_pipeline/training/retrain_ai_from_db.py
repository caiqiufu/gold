# -*- coding: utf-8 -*-
"""
retrain_ai_from_db.py
=====================================
在线增量再训练 (fine-tuning)：
1. 从数据库获取1M K线 → 合并成5M K线 → 训练收盘价
2. 获取 AI 推理结果 → 判断预测是否正确 → 再训练模型
3. 自动记录训练日志到 MySQL
"""

import torch
import torch.nn as nn
import torch.optim as optim
import pandas as pd
import numpy as np
from sqlalchemy import text
import os
import sys
import time
from data.db_helper import db_fetch_ai_results,db_log_training_to_db

# ===============================
# 项目路径修正
# ===============================
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), "..")))
from data.data_loader import create_db_engineLocal, load_latest_ohlcv

MODEL_PATH = "models/latest_model.pt"
SEQ_LEN = 50

# ===============================
# 1. 获取 AI 预测结果
# ===============================
def fetch_ai_results(symbol="XAUUSD", minutes=120):
    engine = create_db_engineLocal()
    df = db_fetch_ai_results(engine,symbol,minutes)
    return df

# ===============================
# 2. 合并 1M → 5M K线
# ===============================
def resample_1m_to_5m(df):
    df = df.sort_values('timestamp')
    data = df[['timestamp','O','H','L','C','V']].values
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
        resampled.append([dt,o,h,l,c,v])
    df_5m = pd.DataFrame(resampled, columns=['timestamp','O','H','L','C','V'])
    df_5m['actual_close'] = df_5m['C']
    return df_5m

# ===============================
# 3. 转换序列张量
# ===============================
def create_sequence_tensors(df, feature_cols, target_col='actual_close', seq_len=SEQ_LEN):
    data = df[feature_cols].values
    targets = df[target_col].values
    X_seq, y_seq = [], []
    for i in range(len(data)-seq_len):
        X_seq.append(data[i:i+seq_len])
        y_seq.append(targets[i+seq_len])
    if not X_seq:
        return None, None
    X = torch.tensor(np.array(X_seq), dtype=torch.float32)
    y = torch.tensor(np.array(y_seq), dtype=torch.float32).view(-1,1)
    return X, y

# ===============================
# 4. 训练函数
# ===============================
def train_on_data(model, X, y, epochs=3, lr=1e-5):
    if X is None:
        return None
    optimizer = optim.Adam(model.parameters(), lr=lr)
    criterion = nn.MSELoss()
    model.train()
    for epoch in range(epochs):
        optimizer.zero_grad()
        preds = model(X)
        loss = criterion(preds, y)
        loss.backward()
        optimizer.step()
        print(f"[Epoch {epoch+1}/{epochs}] Loss: {loss.item():.6f}")
    return float(loss.item())

# ===============================
# 5. 写入训练日志
# ===============================
def log_training_to_db(engine, symbol, kline_count=0, ai_count=0, 
                       loss_kline=None, loss_ai=None, duration=0, comment=None):
    """
    将训练日志写入数据库
    """
    model_version = time.strftime("%Y%m%d_%H%M%S")
    db_log_training_to_db(engine,symbol,model_version,kline_count,ai_count,loss_kline,loss_ai,duration,model_version,comment or "Auto retrain");
    print(f"[DB] ✅ 已记录训练日志 (版本: {model_version})")

# ===============================  
# 6. 再训练入口（增强版）  
# ===============================  
def retrain_model(symbol="XAUUSD", minutes=120, return_tensors=False):
    from model import HybridCNNLSTM
    engine = create_db_engineLocal()

    start_time = time.time()

    # Step1: K线数据训练
    df_1m = load_latest_ohlcv(symbol=symbol, n=1000, engine=engine)
    if df_1m.empty:
        print("[⚠] 没有 K线数据，跳过 K线训练")
        X1 = y1 = None
        input_dim1 = 1
    else:
        df_5m = resample_1m_to_5m(df_1m)
        print(f"[INFO] Step1: {len(df_5m)} 条 5M K线用于训练")
        feature_cols1 = ['O','H','L','C','V']
        X1, y1 = create_sequence_tensors(df_5m, feature_cols1, 'actual_close')
        input_dim1 = X1.shape[2] if X1 is not None else 1

    # Step2: AI 数据训练
    df_ai = fetch_ai_results(symbol, minutes)
    if df_ai.empty:
        print("[⚠] 没有 AI 数据，跳过 AI 再训练")
        X2 = y2 = None
        input_dim2 = input_dim1
    elif len(df_ai) < 50:
        print(f"[⚠] AI 数据仅 {len(df_ai)} 条，数量不足（<50），跳过 AI 再训练")
        X2 = y2 = None
        input_dim2 = input_dim1
    else:
        feature_cols2 = ['close','prob_long']
        X2, y2 = create_sequence_tensors(df_ai, feature_cols2, 'label')
        input_dim2 = X2.shape[2] if X2 is not None else input_dim1
        print(f"[INFO] Step2: {len(df_ai)} 条 AI 数据用于训练")

    input_dim = max(input_dim1, input_dim2)
    model = HybridCNNLSTM(input_dim=input_dim)

    if os.path.exists(MODEL_PATH):
        try:
            model.load_state_dict(torch.load(MODEL_PATH, map_location="cpu"))
            print("[INFO] 已加载已有模型权重")
        except RuntimeError:
            print("[⚠] 模型维度不匹配，重新初始化")

    # Pad input 以匹配模型维度
    def pad_input(X, target_dim):
        if X is None:
            return None
        if X.shape[2] < target_dim:
            pad = torch.zeros(X.shape[0], X.shape[1], target_dim - X.shape[2])
            X = torch.cat([X, pad], dim=2)
        return X

    X1 = pad_input(X1, input_dim)
    X2 = pad_input(X2, input_dim)

    trained_kline = False
    trained_ai = False
    loss_kline = None
    loss_ai = None

    if X1 is not None and y1 is not None:
        print("[INFO] 开始使用 K线 数据微调模型 ...")
        loss_kline = train_on_data(model, X1, y1)
        trained_kline = True

    if X2 is not None and y2 is not None:
        print("[INFO] 开始使用 AI 结果 数据微调模型 ...")
        loss_ai = train_on_data(model, X2, y2)
        trained_ai = True

    duration = round(time.time() - start_time, 2)

    if trained_kline or trained_ai:
        torch.save(model.state_dict(), MODEL_PATH)
        print(f"[✅] 模型已微调并保存 | 耗时 {duration:.1f}s")

        # 写入训练日志
        log_training_to_db(
            engine=engine,
            symbol=symbol,
            kline_count=len(df_1m),
            ai_count=len(df_ai),
            loss_kline=loss_kline,
            loss_ai=loss_ai,
            duration=duration,
            comment="K线 + AI 再训练"
        )
    else:
        print("[⚠] 没有足够的数据，未执行模型训练与保存")

    if return_tensors:
        return X2, y2
    return model

# ===============================
# 入口
# ===============================
if __name__ == "__main__":
    retrain_model(symbol="XAUUSD", minutes=120)
