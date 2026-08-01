@echo off
setlocal
rem Set UNITY_EDITOR, or edit the fallback path below for your Unity installation.
if not defined UNITY_EDITOR set "UNITY_EDITOR=C:\Program Files\Unity\Hub\Editor\6000.0.38f1\Editor\Unity.exe"
set "PROJECT_PATH=%~dp0"
set "LOG_PATH=%PROJECT_PATH%Builds\Windows\build.log"
if not exist "%PROJECT_PATH%Builds\Windows" mkdir "%PROJECT_PATH%Builds\Windows"
if not exist "%UNITY_EDITOR%" (
  echo Unity Editor not found: %UNITY_EDITOR%
  exit /b 2
)
"%UNITY_EDITOR%" -batchmode -quit -projectPath "%PROJECT_PATH%" -executeMethod HeroDefense.Editor.BuildAutomation.BuildWindows -logFile "%LOG_PATH%"
if errorlevel 1 (
  echo Build failed. See: %LOG_PATH%
  exit /b 1
)
echo Build succeeded: %PROJECT_PATH%Builds\Windows\HeroDefense.exe
exit /b 0
