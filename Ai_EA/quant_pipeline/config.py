# config.py
import torch

# ========================
# 模型与训练配置
# ========================
MODEL_DIR = "C:\\D\\OneDrive\\07-svn\\01-gold\\Ai_EA\\quant_pipeline\\models"
HST_DIR = "C:\\data\\eatest\\hst_data"
SEQ_LEN = 50
BATCH_SIZE = 64
EPOCHS = 100
LR = 1e-3
ONLINE_LR = 1e-4
BUFFER_SIZE = 200000
SAVE_EVERY = 100  # 在线每 N 步保存一次模型
DEVICE = "cuda" if torch.cuda.is_available() else "cpu"

# ========================
# MySQL 配置
# ========================
MYSQL_CONFIG = {
    "host": "8.217.6.50",        # 数据库地址
    "port": 3306,               # 数据库端口
    "user": "unieap",    # 数据库用户名
    "password": "unieap",# 数据库密码
    "database": "slipper-admin-base" # 数据库名称
}
