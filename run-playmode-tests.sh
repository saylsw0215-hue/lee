#!/bin/zsh
set -eu
SCRIPT_DIR="${0:A:h}"
UNITY_PATH="${UNITY_PATH:-/Applications/Unity/Hub/Editor/6000.5.6f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$SCRIPT_DIR/Builds/Logs" "$SCRIPT_DIR/Builds/TestResults"
"$UNITY_PATH" -batchmode -nographics -projectPath "$SCRIPT_DIR" -runTests -testPlatform PlayMode -testFilter HeroDefense.Tests -testResults "$SCRIPT_DIR/Builds/TestResults/playmode.xml" -logFile "$SCRIPT_DIR/Builds/Logs/playmode-tests.log"
echo "PlayMode results: $SCRIPT_DIR/Builds/TestResults/playmode.xml"
