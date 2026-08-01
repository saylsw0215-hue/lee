@echo off
setlocal
rem Set UNITY_EDITOR, or edit the fallback path below for your Unity installation.
if not defined UNITY_EDITOR set "UNITY_EDITOR=C:\Program Files\Unity\Hub\Editor\6000.0.38f1\Editor\Unity.exe"
set "PROJECT_PATH=%~dp0"
set "LOG_PATH=%PROJECT_PATH%Builds\Android\build.log"
if not exist "%PROJECT_PATH%Builds\Android" mkdir "%PROJECT_PATH%Builds\Android"
if not exist "%UNITY_EDITOR%" (
  echo Unity Editor not found: %UNITY_EDITOR%
  exit /b 2
)
"%UNITY_EDITOR%" -batchmode -quit -projectPath "%PROJECT_PATH%" -executeMethod HeroDefense.Editor.BuildAutomation.BuildAndroid -logFile "%LOG_PATH%"
if errorlevel 1 (
  echo Android build failed. Install Android Build Support, SDK, NDK and OpenJDK, then see: %LOG_PATH%
  exit /b 1
)
echo Build succeeded: %PROJECT_PATH%Builds\Android\HeroDefense.apk
exit /b 0
