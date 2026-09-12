# -*- coding: utf-8 -*-
"""
realtime_inference.py  — 带更强鲁棒性与调试输出的版本
"""
import os

# ---------------------------
# 在导入 numpy / torch / mkl 等前设置线程限制与环境变量
# 这可以减少 Windows 上因多线程冲突导致的 native crash
# ---------------------------
os.environ.setdefault("OMP_NUM_THREADS", "1")
os.environ.setdefault("OPENBLAS_NUM_THREADS", "1")
os.environ.setdefault("MKL_NUM_THREADS", "1")
os.environ.setdefault("NUMEXPR_NUM_THREADS", "1")
os.environ.setdefault("MKL_SERVICE_FORCE_INTEL", "1")
# 强制 Python stdout 使用 utf-8（若通过 bat 启动也应设置 PYTHONIOENCODING=utf-8）
# 如果你在 bat 已设置 PYTHONIOENCODING 就不用重复设置这里

# 启用 faulthandler（用于捕获 native crash/backtrace）
import faulthandler
faulthandler.enable()

import sys
import time
import json
import traceback
import warnings
from datetime import datetime, timedelta
import numpy as np

# 限制 torch 的线程（减少与 BLAS 的冲突）
try:
    import torch
    torch.set_num_threads(1)
    torch.set_num_interop_threads(1)
except Exception:
    # 如果 torch 导入失败，记录并继续（后续会报错）
    print("[WARN] torch import failed at top-level", file=sys.stderr)
    traceback.print_exc()

# 其他库
import pandas as pd
import zmq
from colorama import Fore, Style, init
from sklearn.preprocessing import MinMaxScaler

# 你的工程模块
from config import SEQ_LEN, MODEL_DIR, DEVICE
from model import HybridCNNLSTM
from common import add_technical_indicators
from data.data_loader import load_latest_ohlcv, resample_to_5m
from data.db_helper import DBHelper
from training.retrain_ai_from_db import retrain_model

# 忽略 FutureWarning（pandas concat 警告会被忽略）
warnings.filterwarnings("ignore", category=FutureWarning)

# colorama
init(autoreset=True)

# --------------- 辅助函数 ---------------
def safe_print(*args, **kwargs):
    """立即 flush 的打印，避免缓冲导致无输出"""
    print(*args, **kwargs)
    try:
        sys.stdout.flush()
    except Exception:
        pass

def get_log_file():
    log_dir = os.path.join(MODEL_DIR, "logs")
    os.makedirs(log_dir, exist_ok=True)
    date_str = datetime.now().strftime("%Y-%m-%d")
    return os.path.join(log_dir, f"realtime_signals_{date_str}.csv")

# --------------- ZeroMQ 初始化（更安全） ---------------
def init_zmq(bind_addr="tcp://*:5751"):
    ctx = zmq.Context()
    sock = ctx.socket(zmq.PUB)
    # 不阻塞关闭，避免 hang
    try:
        sock.setsockopt(zmq.LINGER, 0)
    except Exception:
        pass
    try:
        sock.bind(bind_addr)
        safe_print(f"[INFO] ZeroMQ publisher bound to {bind_addr}")
        return ctx, sock
    except Exception as e:
        safe_print(f"[ERROR] ZeroMQ bind({bind_addr}) failed: {e}")
        # 尝试回退到 localhost 指定地址
        try:
            bind2 = bind_addr.replace("*", "127.0.0.1")
            sock.bind(bind2)
            safe_print(f"[WARN] Bound to {bind2} instead.")
            return ctx, sock
        except Exception as e2:
            safe_print(f"[FATAL] ZeroMQ final bind failed: {e2}", file=sys.stderr)
            # 仍返回 ctx,sock 但上层应检测异常
            return ctx, sock

# --------------- 模型/Scaler 加载（带保护） ---------------
def load_model_safe(model_path=None):
    if model_path is None:
        model_path = os.path.join(MODEL_DIR, "cnn_lstm_model.pth")
    try:
        model = HybridCNNLSTM(input_dim=10).to(DEVICE)
        # 强制 map 到 DEVICE（一般建议使用 cpu 做调试）
        state = torch.load(model_path, map_location=DEVICE)
        model.load_state_dict(state)
        model.eval()
        safe_print("[INFO] model loaded from", model_path)
        return model
    except FileNotFoundError:
        safe_print(f"[ERROR] model file not found: {model_path}", file=sys.stderr)
        raise
    except Exception as e:
        safe_print(f"[ERROR] load_model_safe failed: {e}", file=sys.stderr)
        traceback.print_exc()
        raise

def load_scaler_safe(scaler_path=None):
    if scaler_path is None:
        scaler_path = os.path.join(MODEL_DIR, "scaler.json")
    try:
        with open(scaler_path, "r", encoding="utf-8") as f:
            scaler_dict = json.load(f)
        scaler = MinMaxScaler()
        scaler.min_ = np.array(scaler_dict["min_"])
        scaler.scale_ = np.array(scaler_dict["scale_"])
        scaler.data_min_ = np.array(scaler_dict["data_min_"])
        scaler.data_max_ = np.array(scaler_dict["data_max_"])
        scaler.data_range_ = np.array(scaler_dict["data_range_"])
        scaler.feature_range = tuple(scaler_dict["feature_range"])
        safe_print("[INFO] scaler loaded from", scaler_path)
        return scaler
    except Exception as e:
        safe_print(f"[ERROR] load_scaler_safe failed: {e}", file=sys.stderr)
        traceback.print_exc()
        raise

# --------------- 数据准备与推理 ---------------
def prepare_sequence(df_latest, scaler, seq_len=SEQ_LEN):
    ohlcv = df_latest[['O', 'H', 'L', 'C', 'V']].values.astype(np.float32)
    features = add_technical_indicators(ohlcv)
    seq = features[-seq_len:]
    seq_scaled = scaler.transform(seq)
    seq_tensor = torch.tensor(seq_scaled, dtype=torch.float32).unsqueeze(0).to(DEVICE)
    return seq_tensor

def predict(model, seq_tensor):
    with torch.no_grad():
        logits = model(seq_tensor)
        prob = torch.sigmoid(logits).item()
        direction = "LONG" if prob > 0.5 else "SHORT"
    return prob, direction

# --------------- 主过程 ---------------
def main():
    # ZMQ init
    ctx, sock = init_zmq("tcp://*:5751")

    # Load model/scaler with protection
    try:
        model = load_model_safe()
        scaler = load_scaler_safe()
    except Exception:
        safe_print("[FATAL] 无法加载模型或 scaler，程序退出。", file=sys.stderr)
        return

    # DB
    try:
        db = DBHelper()
    except Exception:
        safe_print("[ERROR] DBHelper 初始化失败，继续运行但 DB 操作会失败", file=sys.stderr)
        traceback.print_exc()
        db = None

    safe_print("[INFO] Model and Scaler loaded successfully.")
    safe_print("[INFO] Start realtime inference loop for XAUUSD... (Ctrl+C to stop)\n")

    current_log = get_log_file()
    df_log = pd.DataFrame(columns=["timestamp", "close", "prob_long", "direction"])

    retry_count = 0
    MAX_RETRY = 5
    last_retrain_time = datetime.min

    try:
        while True:
            try:
                df_latest = load_latest_ohlcv(symbol="XAUUSD", n=SEQ_LEN * 10)
                df_latest = resample_to_5m(df_latest)
                if df_latest is None or len(df_latest) < SEQ_LEN:
                    safe_print(Fore.YELLOW + "[WARN] 最新 K 线不足 SEQ_LEN，等待下一周期..." + Style.RESET_ALL)
                    time.sleep(60)
                    continue

                close_price = df_latest['C'].iloc[-1]
                ts = pd.Timestamp.now()

                seq_tensor = prepare_sequence(df_latest, scaler, seq_len=SEQ_LEN)
                prob, direction = predict(model, seq_tensor)

                color = Fore.GREEN if direction == "LONG" else Fore.RED
                safe_print(f"{color}[{ts:%Y-%m-%d %H:%M:%S}] Close: {close_price:.2f}, Prob (LONG): {prob:.4f}, → Pred: {direction}{Style.RESET_ALL}")

                # ZMQ send guarded
                message = {
                    "timestamp": str(ts),
                    "symbol": "XAUUSD",
                    "close": float(close_price),
                    "prob_long": round(float(prob), 4),
                    "direction": direction
                }
                try:
                    sock.send_json(message)
                except Exception as e:
                    safe_print(f"[ERROR] ZMQ send failed: {e}", file=sys.stderr)
                    traceback.print_exc()

                # DB insert guarded
                if db is not None:
                    try:
                        db.insert_prediction(ts, "XAUUSD", close_price, prob, direction)
                    except Exception as e:
                        safe_print(f"[WARN] DB insert failed: {e}", file=sys.stderr)
                        traceback.print_exc()

                # 回填与写日志（guarded）
                try:
                    ts_now = pd.Timestamp.now()
                    target_time = ts_now - timedelta(minutes=5, seconds=15)
                    if db is not None:
                        row = db.fetch_latest_before("XAUUSD", target_time)
                        if row is not None and pd.isna(row.get("actual_close")):
                            db.update_actual_close(row["timestamp"], "XAUUSD", float(close_price))
                            safe_print(f"[回填] 已更新 XAUUSD 于 {row['timestamp']} 的实际收盘价为 {close_price:.2f}")
                except Exception as e:
                    safe_print(f"[WARN] 回填失败: {e}", file=sys.stderr)
                    traceback.print_exc()

                new_row = pd.DataFrame({
                    "timestamp": [ts],
                    "close": [close_price],
                    "prob_long": [round(prob, 4)],
                    "direction": [direction]
                })
                try:
                    if not os.path.exists(current_log):
                        new_row.to_csv(current_log, index=False)
                        df_log = new_row
                    else:
                        if new_row is not None and not new_row.empty:
                            df_log = pd.concat([df_log, new_row], ignore_index=True)
                            df_log.to_csv(current_log, index=False)
                        else:
                            safe_print("[WARN] new_row 为空，跳过日志写入。")
                except Exception as e:
                    safe_print(f"[WARN] 写日志失败: {e}", file=sys.stderr)
                    traceback.print_exc()

                # 再训练触发
                now = datetime.now()
                if now - last_retrain_time >= timedelta(hours=1):
                    safe_print(f"[INFO] {now:%H:%M} trigger retrain ...")
                    try:
                        X2, y2 = retrain_model(symbol="XAUUSD", minutes=260, return_tensors=True)
                        if X2 is None or y2 is None:
                            safe_print(Fore.YELLOW + "[⚠] 没有足够 AI 数据进行在线再训练，跳过本次训练" + Style.RESET_ALL)
                        else:
                            # 重新加载模型
                            model = load_model_safe()
                            model.eval()
                            last_retrain_time = now
                            safe_print(Fore.GREEN + "[✅] 模型已在线再训练并重新加载完成。" + Style.RESET_ALL)
                    except Exception as e:
                        safe_print(f"[ERROR] retrain failed: {e}", file=sys.stderr)
                        traceback.print_exc()

                retry_count = 0
                # 主循环睡眠：防止 busy loop，若你要精确到每分钟触发，可设置 60
                time.sleep(60)

            except Exception as e:
                retry_count += 1
                safe_print(Fore.RED + f"[ERROR] 推理循环异常: {e}" + Style.RESET_ALL, file=sys.stderr)
                traceback.print_exc()
                if retry_count > MAX_RETRY:
                    safe_print("[FATAL] 重试次数过多，程序退出。", file=sys.stderr)
                    break
                else:
                    wait_time = min(60 * retry_count, 300)
                    safe_print(Fore.YELLOW + f"[INFO] {wait_time} 秒后自动重试..." + Style.RESET_ALL)
                    time.sleep(wait_time)

    except KeyboardInterrupt:
        safe_print("\n[INFO] Realtime inference stopped by user.")
    finally:
        try:
            sock.close()
            ctx.term()
        except Exception:
            pass

if __name__ == "__main__":
    main()