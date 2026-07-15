#!/usr/bin/env bash
# Restore CardWidget.cs — use fix-unity-compile.sh instead (does the same + more).
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec bash "$SCRIPT_DIR/fix-unity-compile.sh" "$@"
