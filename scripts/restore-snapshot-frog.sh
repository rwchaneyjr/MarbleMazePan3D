#!/usr/bin/env bash
# Restore the saved snapshot + frog-only swap (no main gameplay changes).
#
# Usage (Git Bash):
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/restore-snapshot-frog.sh

set -euo pipefail

RESTORE_BRANCH="${RESTORE_BRANCH:-cursor/snapshot-frog-only-1c5d}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESTORE_SCRIPT="$SCRIPT_DIR/restore-scripts.sh"

if [[ ! -f "$RESTORE_SCRIPT" ]]; then
  echo "Missing $RESTORE_SCRIPT" >&2
  exit 1
fi

RESTORE_BRANCH="$RESTORE_BRANCH" bash "$RESTORE_SCRIPT"
