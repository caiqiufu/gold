import logging

# 配置日志系统，包括级别和输出格式
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)

logging.info("realtime_pipeline start...")