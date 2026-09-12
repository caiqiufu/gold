# model.py
import torch.nn as nn
import torch

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
        # x: [B, seq_len, features]
        x = x.permute(0, 2, 1)  # [B, features, seq_len]
        x = self.cnn(x)        # [B, channels, seq_len/2]
        x = x.permute(0, 2, 1) # [B, seq_len/2, channels]
        out, _ = self.lstm(x)  # [B, seq, hidden]
        out = self.dropout(out[:, -1, :])
        out = self.fc1(out)
        out = self.relu(out)
        return self.fc2(out)   # logits