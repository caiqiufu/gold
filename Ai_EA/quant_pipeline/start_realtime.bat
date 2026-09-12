@echo off
setlocal enabledelayedexpansion
title 🚀 Realtime Inference Runner

REM ================================
REM 🧠 设置 UTF-8，防止中文乱码或符号报错
REM ================================
chcp 65001 >nul
set PYTHONIOENCODING=utf-8

echo =====================================================
echo 🚀 Realtime Inference Environment Setup
echo =====================================================

set BASE_DIR=%~dp0
cd /d "%BASE_DIR%"

REM ================================
REM 🐍 检查 Python 是否存在
REM ================================
where python >nul 2>nul
if %errorlevel% neq 0 (
    echo ❌ Python 未安装，请先安装 Python。
    pause
    exit /b
)
for /f "delims=" %%i in ('python -c "import sys;print(f'{sys.version_info.major}.{sys.version_info.minor}')"' ) do set PY_VER=%%i
echo [INFO] 检测到 Python %PY_VER%

REM ================================
REM 📦 检查或创建虚拟环境
REM ================================
if not exist "venv\" (
    echo [INFO] 未检测到虚拟环境，正在创建...
    python -m venv venv
    if %errorlevel% neq 0 (
        echo ❌ 虚拟环境创建失败，请检查 Python 安装。
        pause
        exit /b
    )
)

REM ================================
REM ⚙️ 激活虚拟环境
REM ================================
call venv\Scripts\activate
if errorlevel 1 (
    echo ❌ 无法激活虚拟环境，请检查 venv 是否损坏。
    pause
    exit /b
)
echo [OK] 虚拟环境已激活。
python --version

REM ================================
REM 🪄 升级 pip（静默模式）
REM ================================
echo -----------------------------------------------------
echo [INFO] 正在升级 pip...
python -m pip install --upgrade pip setuptools wheel >nul
echo [OK] pip 升级完成。

REM ================================
REM 📋 自动安装依赖
REM ================================
if exist requirements.txt (
    echo -----------------------------------------------------
    echo [INFO] 检测到 requirements.txt，正在安装依赖...
    python -m pip install -r requirements.txt
    echo [OK] 依赖安装完成。
) else (
    echo [WARN] 未找到 requirements.txt，跳过依赖安装。
)

REM ================================
REM 🧾 生成日志文件名
REM ================================
if not exist logs mkdir logs
for /f "tokens=1-3 delims=/ " %%a in ('date /t') do set D=%%a-%%b-%%c
for /f "tokens=1-2 delims=: " %%a in ('time /t') do set T=%%a-%%b
set LOGFILE=logs\inference_!D!_!T!.log
set LOGFILE=!LOGFILE::=-!

echo -----------------------------------------------------
echo [INFO] 日志文件: !LOGFILE!
echo [INFO] 正在启动 realtime_inference.py ...
echo -----------------------------------------------------

REM =====================================================
REM 🚀 运行实时推理脚本 (实时输出 + 写入日志)
REM =====================================================
powershell -NoLogo -ExecutionPolicy Bypass ^
    -Command "python -u 'realtime_inference.py' 2>&1 | Tee-Object -FilePath '!LOGFILE!' -Append"

REM ================================
REM ✅ 结束提示
REM ================================
echo =====================================================
echo [INFO] 程序已结束，日志已保存：
echo !LOGFILE!
echo =====================================================
pause
endlocal
