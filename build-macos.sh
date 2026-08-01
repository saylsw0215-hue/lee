#!/bin/bash

set -u

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_PATH="$SCRIPT_DIR"
LOG_DIR="$PROJECT_PATH/Builds/Logs"
LOG_PATH="$LOG_DIR/macos-build.log"
APP_PATH="$PROJECT_PATH/Builds/macOS/HeroDefense.app"

# Override with: UNITY_EDITOR=/absolute/path/to/Unity ./build-macos.sh
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.5.6f1/Unity.app/Contents/MacOS/Unity}"

mkdir -p "$LOG_DIR"

if [ ! -x "$UNITY_EDITOR" ]; then
  echo "Unity Editor executable not found: $UNITY_EDITOR"
  echo "Install Unity 6 LTS with macOS Build Support in Unity Hub, or set UNITY_EDITOR."
  exit 2
fi

"$UNITY_EDITOR" -batchmode -quit -projectPath "$PROJECT_PATH" \
  -executeMethod HeroDefense.Editor.BuildAutomation.BuildMacOS -logFile "$LOG_PATH"
status=$?

if [ "$status" -ne 0 ]; then
  echo "macOS build failed with exit code $status. Log: $LOG_PATH"
  exit "$status"
fi

if [ ! -d "$APP_PATH" ]; then
  echo "Unity exited successfully, but the app was not found: $APP_PATH"
  echo "Log: $LOG_PATH"
  exit 3
fi

echo "macOS app created: $APP_PATH"
exit 0
