# train.py
import os
import glob
import numpy as np
import torch
import torch.nn as nn
import torch.optim as optim
from sklearn.preprocessing import MinMaxScaler
from torch.utils.data import DataLoader

from config import MODEL_DIR, HST_DIR, SEQ_LEN, BATCH_SIZE, EPOCHS, LR, ONLINE_LR, BUFFER_SIZE, SAVE_EVERY, DEVICE
from model import HybridCNNLSTM
from dataset import ReplayBuffer
from common import parse_hst_file, resample_to_5m, add_technical_indicators, extract_info, plot_incremental_loss, evaluate_direction_accuracy
import json



os.makedirs(MODEL_DIR, exist_ok=True)
MODEL_PATH = os.path.join(MODEL_DIR, "cnn_lstm_model.pth")
LOSS_FILE = os.path.join(MODEL_DIR, "training_loss_history.npy")

# ---------------- Offline batch training ----------------
def offline_train(hst_folder=HST_DIR, seq_len=SEQ_LEN, batch_size=BATCH_SIZE, num_epochs=EPOCHS, lr=LR):
    device = torch.device(DEVICE)
    model = HybridCNNLSTM(input_dim=10).to(device)
    if os.path.exists(MODEL_PATH):
        model.load_state_dict(torch.load(MODEL_PATH, map_location=device))
        print(f"[INFO] Loaded existing model from {MODEL_PATH}")

    criterion = nn.BCEWithLogitsLoss()
    optimizer = optim.Adam(model.parameters(), lr=lr)
    scheduler = torch.optim.lr_scheduler.ReduceLROnPlateau(optimizer, mode="min", factor=0.5, patience=5)

    loss_history = np.load(LOSS_FILE, allow_pickle=True).item() if os.path.exists(LOSS_FILE) else {}

    hst_files = sorted(glob.glob(os.path.join(hst_folder, "*.hst")), key=lambda f: extract_info(f)[2])
    buffer = ReplayBuffer(max_size=BUFFER_SIZE)
    scaler = MinMaxScaler()

    # 合并所有年份数据到 buffer
    for hst_file in hst_files:
        data = parse_hst_file(hst_file)
        data = resample_to_5m(data)
        features = add_technical_indicators(np.array(data)[:,1:].astype(np.float32))
        buffer.add(features)

    if len(buffer.data) == 0:
        print("[WARN] No data found in buffer. Abort.")
        return

    scaler.fit(np.array(buffer.data))
    dataset = buffer.get_dataset(seq_len=seq_len, scaler=scaler)
    if len(dataset) == 0:
        print("[WARN] Dataset is empty after thresholding. Abort.")
        return
    loader = DataLoader(dataset, batch_size=batch_size, shuffle=True)

    file_losses = []
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
        print(f"Epoch {epoch+1}/{num_epochs}, Loss: {avg_loss:.6f}, LR: {optimizer.param_groups[0]['lr']:.6e}")
        file_losses.append(avg_loss)
        if (epoch+1) % 10 == 0:
            evaluate_direction_accuracy(model, dataset, batch_size=batch_size, device=device)

    # 保存
    torch.save(model.state_dict(), MODEL_PATH)
    save_metadata(MODEL_DIR, scaler=scaler, step=num_epochs,
                  extra_info={"mode": "offline", "epochs": num_epochs, "lr": lr})
    prev_losses = loss_history.get("all_years", [])
    loss_history["all_years"] = prev_losses + file_losses
    np.save(LOSS_FILE, loss_history)
    plot_incremental_loss(loss_history, save_path=os.path.join(MODEL_DIR, "training_loss_curve.png"))
    print("[INFO] Offline training finished and model saved.")

# ---------------- OnlineTrainer (逐根 / 小批量在线学习) ----------------
class OnlineTrainer:
    def __init__(self, model=None, model_dir=MODEL_DIR, buffer_size=BUFFER_SIZE, seq_len=SEQ_LEN, lr=ONLINE_LR, device=DEVICE):
        import os
        self.device = torch.device(device)
        if model is None:
            self.model = HybridCNNLSTM(input_dim=10).to(self.device)
        else:
            self.model = model.to(self.device)
        self.buffer = ReplayBuffer(max_size=buffer_size)
        self.scaler = MinMaxScaler()
        self.seq_len = seq_len
        self.criterion = nn.BCEWithLogitsLoss()
        self.optimizer = optim.Adam(self.model.parameters(), lr=lr)
        self.model_dir = model_dir
        os.makedirs(model_dir, exist_ok=True)
        self.model_path = os.path.join(model_dir, "cnn_lstm_model.pth")
        if os.path.exists(self.model_path):
            self.model.load_state_dict(torch.load(self.model_path, map_location=self.device))
            print(f"[INFO] Loaded existing online model from {self.model_path}")
        self.step = 0

    def predict(self, seq):
        # seq: np.array shape [seq_len, feat], must be scaled already
        import torch
        self.model.eval()
        with torch.no_grad():
            X = torch.tensor(seq, dtype=torch.float32).unsqueeze(0).to(self.device)
            logits = self.model(X)
            prob = torch.sigmoid(logits).item()
            return 1 if prob > 0.5 else 0, prob

    def update(self, new_feature_row):
        """
        new_feature_row: 1D numpy array length = features (10)
        """
        self.buffer.add([new_feature_row])
        self.step += 1
        if len(self.buffer.data) < self.seq_len + 2:
            return  # 数据不足

        # fit scaler on buffer (you may optimize to incremental scaler)
        self.scaler.fit(np.array(self.buffer.data))
        dataset = self.buffer.get_dataset(seq_len=self.seq_len, scaler=self.scaler)
        if len(dataset) == 0:
            return

        loader = DataLoader(dataset, batch_size=32, shuffle=True)
        self.model.train()
        losses = []
        for X, y in loader:
            X, y = X.to(self.device), y.to(self.device)
            self.optimizer.zero_grad()
            preds = self.model(X)
            loss = self.criterion(preds, y)
            loss.backward()
            torch.nn.utils.clip_grad_norm_(self.model.parameters(), 1.0)
            self.optimizer.step()
            losses.append(loss.item())
        if losses:
            print(f"[OnlineTrain] step={self.step}, mean_loss={np.mean(losses):.6f}, buffer={len(self.buffer.data)}")

        # 定期保存
        if self.step % SAVE_EVERY == 0:
            self.save()

    def save(self):
        torch.save(self.model.state_dict(), self.model_path)
        save_metadata(self.model_dir, scaler=self.scaler, step=self.step,
                      extra_info={"mode": "online", "buffer_size": len(self.buffer.data)})
        print(f"[INFO] Online model + metadata saved to {self.model_path}")


# ================== 保存 JSON 元数据 ==================
def save_metadata(model_dir, scaler=None, step=0, extra_info=None):
    meta_path = os.path.join(model_dir, "cnn_lstm_model_meta.json")
    metadata = {
        "step": step,
        "scaler_min": scaler.data_min_.tolist() if scaler else None,
        "scaler_max": scaler.data_max_.tolist() if scaler else None,
        "scaler_scale": scaler.scale_.tolist() if scaler else None,
        "scaler_minmax": scaler.min_.tolist() if hasattr(scaler, "min_") else None,
        "extra": extra_info or {}
    }
    with open(meta_path, "w") as f:
        json.dump(metadata, f, indent=4)
    print(f"[INFO] Metadata saved to {meta_path}")



# ---------------- convenience CLI ----------------
if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument("--mode", choices=["offline", "online"], default="offline")
    parser.add_argument("--hst_folder", default=HST_DIR)
    parser.add_argument("--epochs", type=int, default=EPOCHS)
    args = parser.parse_args()

    if args.mode == "offline":
        offline_train(hst_folder=args.hst_folder, num_epochs=args.epochs)
    else:
        # online: simulate streaming by iterating over files and rows
        trainer = OnlineTrainer()
        # stream all rows from HST files in time order
        from common import parse_hst_file, resample_to_5m, add_technical_indicators
        files = sorted(glob.glob(os.path.join(HST_DIR, "*.hst")), key=lambda f: extract_info(f)[2])
        for fpath in files:
            raw = parse_hst_file(fpath)
            data5 = resample_to_5m(raw)
            feats = add_technical_indicators(np.array(data5)[:,1:].astype(np.float32))
            for row in feats:
                # 1) predict using last seq if available
                if len(trainer.buffer.data) >= trainer.seq_len:
                    seq = np.array(trainer.buffer.data[-trainer.seq_len:])
                    seq_scaled = trainer.scaler.transform(seq) if len(trainer.buffer.data) > 0 else seq
                    dirc, prob = trainer.predict(seq_scaled)
                    # 你可以在此把预测发送到策略或日志
                # 2) update online trainer with current row
                trainer.update(row)
        trainer.save()