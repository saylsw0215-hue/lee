#!/bin/bash

set -u

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_PATH="$SCRIPT_DIR"
LOG_DIR="$PROJECT_PATH/Builds/Logs"
LOG_PATH="$LOG_DIR/ios-build.log"
XCODE_PATH="$PROJECT_PATH/Builds/iOS/HeroDefenseXcode"

# Override with: UNITY_EDITOR=/absolute/path/to/Unity ./build-ios.sh
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.5.6f1/Unity.app/Contents/MacOS/Unity}"

mkdir -p "$LOG_DIR"

if [ ! -x "$UNITY_EDITOR" ]; then
  echo "Unity Editor executable not found: $UNITY_EDITOR"
  echo "Install Unity 6 LTS and iOS Build Support in Unity Hub, or set UNITY_EDITOR."
  exit 2
fi

"$UNITY_EDITOR" -batchmode -quit -projectPath "$PROJECT_PATH" \
  -executeMethod HeroDefense.Editor.BuildAutomation.BuildIOS -logFile "$LOG_PATH"
status=$?

if [ "$status" -ne 0 ]; then
  echo "iOS Xcode project generation failed with exit code $status."
  echo "Confirm that Unity iOS Build Support is installed. Log: $LOG_PATH"
  exit "$status"
fi

if [ ! -d "$XCODE_PATH/Unity-iPhone.xcodeproj" ] && [ ! -d "$XCODE_PATH/Unity-iPhone.xcworkspace" ]; then
  echo "Unity exited successfully, but no Xcode project/workspace was found in: $XCODE_PATH"
  echo "Log: $LOG_PATH"
  exit 3
fi

echo "iOS Xcode project created: $XCODE_PATH"
echo "Code signing and installation on an iPhone must be completed in Xcode."
exit 0
