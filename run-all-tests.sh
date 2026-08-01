#!/bin/zsh
set -eu
SCRIPT_DIR="${0:A:h}"
"$SCRIPT_DIR/run-editmode-tests.sh"
"$SCRIPT_DIR/run-playmode-tests.sh"
echo "All Hero Defense test commands completed successfully."
