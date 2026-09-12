# ======================================================
# data_loader.py  — 修复 SQLAlchemy 2.0 执行语法 & 支持 PyMySQL
# ======================================================

import pandas as pd
import time
from sqlalchemy import create_engine, text
from sqlalchemy.exc import OperationalError
from config import MYSQL_CONFIG


# -------------------------------
# 数据库引擎创建（支持自动重试）
# -------------------------------
def create_db_engine(max_retries=3, retry_interval=5):
    """
    创建 SQLAlchemy 数据库引擎，自动使用 PyMySQL 驱动。
    若连接失败，会自动重试多次。
    """
    user = MYSQL_CONFIG['user']
    password = MYSQL_CONFIG['password']
    host = MYSQL_CONFIG['host']
    port = MYSQL_CONFIG.get('port', 3306)
    database = MYSQL_CONFIG['database']

    db_url = f"mysql+pymysql://{user}:{password}@{host}:{port}/{database}"

    for attempt in range(1, max_retries + 1):
        try:
            engine = create_engine(db_url, pool_pre_ping=True)

            # ✅ SQLAlchemy 2.0 需用 text("SELECT 1")
            with engine.connect() as conn:
                conn.execute(text("SELECT 1"))

            print(f"[INFO] ✅ 数据库连接成功")
            return engine

        except OperationalError as e:
            print(f"[WARN] 第 {attempt}/{max_retries} 次连接数据库失败: {e}")
            if attempt < max_retries:
                print(f"[INFO] {retry_interval} 秒后重试连接...")
                time.sleep(retry_interval)
            else:
                print("[ERROR] ❌ 数据库连接多次失败，请检查配置或网络。")
                raise e


# -------------------------------
# 本地数据库引擎创建（备用）
# -------------------------------
def create_db_engineLocal(max_retries=3, retry_interval=5):
    """
    创建 SQLAlchemy 本地数据库引擎，自动重试。
    """
    user = MYSQL_CONFIG['user']
    password = MYSQL_CONFIG['password']
    host = "localhost"
    port = MYSQL_CONFIG.get('port', 3306)
    database = MYSQL_CONFIG['database']

    db_url = f"mysql+pymysql://{user}:{password}@{host}:{port}/{database}"

    for attempt in range(1, max_retries + 1):
        try:
            engine = create_engine(db_url, pool_pre_ping=True)
            with engine.connect() as conn:
                conn.execute(text("SELECT 1"))  # ✅ 修正此处
            print(f"[INFO] ✅ 本地数据库连接成功")
            return engine

        except OperationalError as e:
            print(f"[WARN] 第 {attempt}/{max_retries} 次连接本地数据库失败: {e}")
            if attempt < max_retries:
                print(f"[INFO] {retry_interval} 秒后重试连接...")
                time.sleep(retry_interval)
            else:
                print("[ERROR] ❌ 本地数据库连接多次失败，请检查配置。")
                raise e


# -------------------------------
# 从数据库加载数据
# -------------------------------
def load_latest_ohlcv(symbol="XAUUSD", n=50, engine=None):
    """
    从 MySQL 数据库读取最新 n 根 K 线，按时间升序返回。
    """
    if engine is None:
        engine = create_db_engine()

    query = f"""
        SELECT open_time AS timestamp,
               open_price  AS O,
               high_price  AS H,
               low_price   AS L,
               close_price AS C,
               volume AS V
        FROM trade_kline_data
        WHERE symbol='{symbol}'
        ORDER BY open_time DESC
        LIMIT {n}
    """

    try:
        df = pd.read_sql(query, engine)
        df = df.sort_values("timestamp").reset_index(drop=True)
        print(f"[INFO] 已加载 {symbol} 最新 {len(df)} 条K线数据。")
        return df
    except Exception as e:
        print(f"[ERROR] ❌ 加载数据库数据失败: {e}")
        return pd.DataFrame()


# -------------------------------
# 1M -> 5M 数据聚合
# -------------------------------
def resample_to_5m(df_1m):
    """
    将 1 分钟 K 线聚合为 5 分钟 K 线。
    """
    if df_1m.empty:
        return df_1m

    df = df_1m.copy()
    df["timestamp"] = pd.to_datetime(df["timestamp"])
    df = df.set_index("timestamp")

    df_5m = df.resample("5min").agg({
        "O": "first",
        "H": "max",
        "L": "min",
        "C": "last",
        "V": "sum"
    }).dropna().reset_index()

    return df_5m


# -------------------------------
# 调试主程序入口
# -------------------------------
if __name__ == "__main__":
    engine = create_db_engine()
    df_latest = load_latest_ohlcv(symbol="XAUUSD", n=50, engine=engine)
    print(df_latest.tail())
