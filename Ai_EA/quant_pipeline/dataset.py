# -*- coding: utf-8 -*-
"""
dataset.py
KLineDataset & ReplayBuffer
- KLineDataset: 用于生成训练/推理的序列数据集
- ReplayBuffer: 数据缓冲区，支持增量数据加入和生成数据集
"""

import numpy as np
import torch
from torch.utils.data import Dataset

# ========================
# 1. KLineDataset
# ========================
class KLineDataset(Dataset):
    """
    K线序列数据集，用于训练/推理模型
    输入:
        data: numpy array, shape [N, features], 特征顺序需与 add_technical_indicators 输出一致
        seq_len: int, 每个样本序列长度
        scaler: sklearn.preprocessing 对象, 用于归一化
        threshold: float, 用于生成多空目标
    输出:
        self.X: tensor, shape [样本数, seq_len, features]
        self.y: tensor, shape [样本数, 1]
    """

    def __init__(self, data, seq_len=50, scaler=None, threshold=0.3):
        self.X, self.y = [], []

        # ----------------------
        # 遍历整个数据生成序列
        # ----------------------
        for i in range(len(data) - seq_len - 1):
            seq = data[i:i+seq_len, :].astype(np.float32)  # 当前序列
            future_close = np.float32(data[i+seq_len, 3])  # 下一个时间步收盘价
            last_close   = np.float32(data[i+seq_len-1, 3])  # 当前序列最后收盘价
            price_diff = future_close - last_close

            # ----------------------
            # 生成目标 y
            # ----------------------
            if price_diff > threshold:
                target = 1.0   # 多头
            elif price_diff < -threshold:
                target = 0.0   # 空头
            else:
                continue       # 小幅波动，不考虑

            # ----------------------
            # 序列归一化
            # ----------------------
            if scaler is not None:
                seq = scaler.transform(seq)

            self.X.append(seq)
            self.y.append(target)

        # ----------------------
        # 转换为 torch tensor
        # 如果数据为空，则生成空 tensor 避免报错
        # ----------------------
        if len(self.X) == 0:
            self.X = torch.empty((0, seq_len, data.shape[1]), dtype=torch.float32)
            self.y = torch.empty((0,1), dtype=torch.float32)
        else:
            self.X = torch.tensor(np.array(self.X), dtype=torch.float32)
            self.y = torch.tensor(np.array(self.y), dtype=torch.float32).unsqueeze(1)

    # ----------------------
    # 返回数据集长度
    # ----------------------
    def __len__(self):
        return len(self.X)

    # ----------------------
    # 返回单条样本 (X, y)
    # ----------------------
    def __getitem__(self, idx):
        return self.X[idx], self.y[idx]


# ========================
# 2. ReplayBuffer
# ========================
class ReplayBuffer:
    """
    数据缓冲区
    功能:
    - 支持增量加入新数据
    - 自动保持固定最大长度
    - 生成 KLineDataset
    """

    def __init__(self, max_size=200000):
        self.max_size = max_size
        self.data = []  # 存储所有特征行，每行是 np.array

    # ----------------------
    # 增量加入新数据
    # ----------------------
    def add(self, new_data):
        """
        new_data: iterable, 每个元素为一行特征
        """
        # 转换为 np.float32 并加入缓冲区
        self.data.extend([np.array(row, dtype=np.float32) for row in new_data])

        # 超过最大长度时，保留最新的 max_size 数据
        if len(self.data) > self.max_size:
            self.data = self.data[-self.max_size:]

    # ----------------------
    # 返回 KLineDataset
    # ----------------------
    def get_dataset(self, seq_len=50, scaler=None, threshold=0.3):
        """
        生成 KLineDataset
        参数:
            seq_len: int, 每个样本序列长度
            scaler: sklearn.preprocessing 对象，用于归一化
            threshold: float, 生成多空目标
        """
        return KLineDataset(
            np.array(self.data, dtype=np.float32),
            seq_len=seq_len,
            scaler=scaler,
            threshold=threshold
        )
