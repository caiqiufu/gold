@echo off
title Python Environment Auto-Fix PLUS
color 0A
setlocal enabledelayedexpansion

set LOGFILE=setup_debug_log.txt
echo ===================================================== > "%LOGFILE%"
echo [START] %date% %time% >> "%LOGFILE%"
echo ===================================================== >> "%LOGFILE%"

echo =====================================================
echo 🌍 PYTHON ENVIRONMENT SETUP (AUTO-FIX PLUS)
echo =====================================================

REM === Step 1: Locate Python ===
for %%X in (python.exe) do set PYTHON_EXE=%%~$PATH:X
if not exist "%PYTHON_EXE%" (
    echo ❌ Python not found in PATH.
    echo Please install Python 3.10+ from https://www.python.org/downloads/
    echo [ERROR] Python not found >> "%LOGFILE%"
    pause
    exit /b
)

for /f "tokens=2 delims= " %%v in ('"%PYTHON_EXE%" -V 2^>^&1') do set PYTHON_VERSION=%%v
echo ✅ Detected Python %PYTHON_VERSION% at: %PYTHON_EXE%
echo [INFO] Found Python: %PYTHON_EXE% >> "%LOGFILE%"
echo [INFO] Python Version: %PYTHON_VERSION% >> "%LOGFILE%"

REM === Step 2: Check/Create Virtual Environment ===
set "VENV_DIR=venv"
if not exist "%VENV_DIR%" (
    echo 🔧 Creating new virtual environment...
    "%PYTHON_EXE%" -m venv "%VENV_DIR%"
) else (
    echo 🔍 Virtual environment exists, verifying...
)

REM === Step 3: Validate venv Python ===
if not exist "%VENV_DIR%\Scripts\python.exe" (
    echo ⚙️ venv Python missing, rebuilding...
    rmdir /s /q "%VENV_DIR%"
    "%PYTHON_EXE%" -m venv "%VENV_DIR%"
)

REM === Step 4: Fix pyvenv.cfg path if needed ===
set "VENV_CFG=%VENV_DIR%\pyvenv.cfg"
if exist "%VENV_CFG%" (
    for /f "tokens=1,* delims== " %%a in ('findstr "home" "%VENV_CFG%"') do (
        set "PY_HOME=%%b"
    )
    set "PY_HOME=!PY_HOME: =!"
    if not exist "!PY_HOME!\python.exe" (
        echo ⚙️ Fixing broken home path in pyvenv.cfg ...
        powershell -Command "(Get-Content '%VENV_CFG%') -replace 'home\s*=\s*.*', 'home = C:\\Program Files\\Python313' | Set-Content '%VENV_CFG%'"
    )
)

REM === Step 5: Activate Virtual Environment ===
call "%VENV_DIR%\Scripts\activate.bat"
if %errorlevel% neq 0 (
    echo ❌ Failed to activate venv!
    echo [ERROR] Activation failed >> "%LOGFILE%"
    pause
    exit /b
)
echo ✅ venv activated successfully.

REM === Step 6: Check venv Python ===
"%VENV_DIR%\Scripts\python.exe" -V >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Virtual environment Python not working.
    echo [ERROR] venv Python broken >> "%LOGFILE%"
    pause
    exit /b
)
for /f "tokens=2 delims= " %%v in ('"%VENV_DIR%\Scripts\python.exe" -V 2^>^&1') do set VENV_PY_VER=%%v
echo ✅ venv Python Version: %VENV_PY_VER%

REM === Step 7: Upgrade pip ===
echo 🔄 Upgrading pip, setuptools, wheel...
"%VENV_DIR%\Scripts\python.exe" -m pip install --upgrade pip setuptools wheel
if %errorlevel% neq 0 (
    echo ⚠️ pip upgrade failed.
    echo [WARN] pip upgrade failed >> "%LOGFILE%"
) else (
    echo ✅ pip upgrade complete.
    echo [INFO] pip upgrade success >> "%LOGFILE%"
)

echo =====================================================
echo ✅ All steps completed successfully!
echo Logs saved to "%CD%\%LOGFILE%"
echo =====================================================
pause
exit /b
