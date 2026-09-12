# db_helper.py
# -*- coding: utf-8 -*-
import pandas as pd
from sqlalchemy import create_engine, text
from datetime import datetime
from config import MYSQL_CONFIG
import time


def create_db_engine():
    """
    创建 SQLAlchemy MySQL 引擎（使用 PyMySQL 驱动，稳定无崩溃）
    """
    user = MYSQL_CONFIG['user']
    password = MYSQL_CONFIG['password']
    host = MYSQL_CONFIG['host']
    port = MYSQL_CONFIG.get('port', 3306)
    database = MYSQL_CONFIG['database']

    # ✅ 使用 PyMySQL 替代 mysqlconnector
    engine = create_engine(
        f"mysql+pymysql://{user}:{password}@{host}:{port}/{database}",
        pool_pre_ping=True
    )
    return engine


class DBHelper:
    """
    数据库助手类 — 保存 AI 推理结果与实际行情数据
    """

    def __init__(self, engine=None):
        self.engine = engine or create_db_engine()
        #self.create_table()

    def create_table(self):
        """
        若不存在则创建预测结果表
        """
        create_sql = """
        CREATE TABLE IF NOT EXISTS trade_ai_result (
            id BIGINT AUTO_INCREMENT PRIMARY KEY,
            timestamp DATETIME NOT NULL,
            symbol VARCHAR(20) NOT NULL,
            close DOUBLE,
            prob_long DOUBLE,
            direction VARCHAR(10),
            actual_close DOUBLE DEFAULT NULL,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        """
        with self.engine.begin() as conn:
            conn.execute(text(create_sql))

    def insert_prediction(self, ts, symbol, close, prob_long, direction):
        """
        插入一条预测记录
        """
        insert_sql = text("""
            INSERT INTO trade_ai_result (timestamp, symbol, close, prob_long, direction)
            VALUES (:timestamp, :symbol, :close, :prob_long, :direction)
        """)
        with self.engine.begin() as conn:
            conn.execute(insert_sql, {
                "timestamp": ts,
                "symbol": symbol,
                "close": close,
                "prob_long": prob_long,
                "direction": direction
            })

    def update_actual_close(self, ts, symbol, actual_close):
        """
        根据时间戳和 symbol 更新实际价格
        """
        update_sql = text("""
            UPDATE trade_ai_result
            SET actual_close = :actual_close
            WHERE timestamp = :timestamp AND symbol = :symbol
        """)
        with self.engine.begin() as conn:
            conn.execute(update_sql, {
                "timestamp": ts,
                "symbol": symbol,
                "actual_close": actual_close
            })

    def fetch_recent_predictions(self, symbol="XAUUSD", n=10):
        """
        查询最近 n 条预测记录
        """
        query = text("""
            SELECT * FROM trade_ai_result
            WHERE symbol = :symbol
            ORDER BY timestamp DESC
            LIMIT :n
        """)
        with self.engine.connect() as conn:
            df = pd.read_sql(query, conn, params={"symbol": symbol, "n": n})
        return df

    def fetch_latest_before(self, symbol, target_time):
        """
        获取指定时间前最近的一条预测记录
        """
        query = text("""
            SELECT * FROM trade_ai_result
            WHERE symbol = :symbol
              AND timestamp <= :target_time
            ORDER BY timestamp DESC
            LIMIT 1
        """)
        with self.engine.connect() as conn:
            df = pd.read_sql(query, conn, params={"symbol": symbol, "target_time": target_time})
        return df.iloc[0] if not df.empty else None


# ===============================
# ✅ 模块级函数（可被 import 调用）
# ===============================
def db_fetch_ai_results(engine, symbol="XAUUSD", minutes=120):
    """
    从数据库获取已带有实际价格的 AI 预测结果，用于模型再训练。
    """
    query = text("""
        SELECT timestamp, symbol, close, prob_long, direction, actual_close
        FROM trade_ai_result
        WHERE actual_close IS NOT NULL
          AND symbol = :symbol
          AND timestamp >= NOW() - INTERVAL :mins MINUTE
        ORDER BY timestamp ASC
    """)

    df = pd.read_sql(query, engine, params={"symbol": symbol, "mins": minutes})
    if not df.empty:
        df["timestamp"] = pd.to_datetime(df["timestamp"])
        # 生成 label：预测方向是否正确
        df['pred_correct'] = ((df['direction']==1) & (df['actual_close']>df['close'])) | \
                             ((df['direction']==0) & (df['actual_close']<df['close']))
        df['label'] = df['pred_correct'].astype(float)
    return df


def db_log_training_to_db(engine, symbol, model_version, kline_count, ai_count,
                          loss_kline, loss_ai, duration, version, comment):
    with engine.begin() as conn:
        sql = text("""
            INSERT INTO trade_model_training_log
            (symbol, model_version, kline_count, ai_count, loss_kline, loss_ai, duration, version, comment, created_at)
            VALUES (:symbol, :model_version, :kline_count, :ai_count, :loss_kline, :loss_ai, :duration, :version, :comment, NOW())
        """)
        conn.execute(sql, {
            "symbol": symbol,
            "model_version": model_version,
            "kline_count": kline_count,
            "ai_count": ai_count,
            "loss_kline": loss_kline,
            "loss_ai": loss_ai,
            "duration": duration,
            "version": version,
            "comment": comment
        })



# ------------------------------
# 使用示例
# ------------------------------
if __name__ == "__main__":
    db = DBHelper()
    now = datetime.now()

    # 插入示例预测
    db.insert_prediction(
        ts=now,
        symbol="XAUUSD",
        close=2475.32,
        prob_long=0.7312,
        direction="LONG"
    )

    # 查询最近几条
    df = db.fetch_recent_predictions("XAUUSD", 5)
    print(df)
