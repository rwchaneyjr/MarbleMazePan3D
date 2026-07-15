#!/usr/bin/env bash
# Fix Unity CS8300 merge-conflict errors without wiping your branch.
#
# Usage:
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/fix-merge-conflicts.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec bash "$SCRIPT_DIR/fix-unity-compile.sh" "$@"
