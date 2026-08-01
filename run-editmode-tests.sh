#!/bin/zsh
set -eu
SCRIPT_DIR="${0:A:h}"
UNITY_PATH="${UNITY_PATH:-/Applications/Unity/Hub/Editor/6000.5.6f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$SCRIPT_DIR/Builds/Logs" "$SCRIPT_DIR/Builds/TestResults"
"$UNITY_PATH" -batchmode -nographics -projectPath "$SCRIPT_DIR" -runTests -testPlatform EditMode -testFilter HeroDefense.Tests -testResults "$SCRIPT_DIR/Builds/TestResults/editmode.xml" -logFile "$SCRIPT_DIR/Builds/Logs/editmode-tests.log"
echo "EditMode results: $SCRIPT_DIR/Builds/TestResults/editmode.xml"
