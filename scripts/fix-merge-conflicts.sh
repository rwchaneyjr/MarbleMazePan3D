#!/usr/bin/env bash
# Fix Unity CS8300 merge-conflict errors by resetting to the clean remote branch.
#
# Usage:
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/fix-merge-conflicts.sh

set -euo pipefail

BRANCH="${RESTORE_BRANCH:-cursor/working-sprites-and-swirl-3fe3}"
REPO="${SYMBOL_ALGEBRA_DIR:-/c/Users/rober/SymbolAlgebra}"

normalize_windows_path() {
  local p="$1"
  if [[ "$p" =~ ^[A-Za-z]: ]]; then
    local drive="${p:0:1}"
    drive=$(printf '%s' "$drive" | tr '[:upper:]' '[:lower:]')
    p="${p:2}"
    p="${p//\\//}"
    p="/${drive}${p}"
  fi
  printf '%s' "$p"
}

REPO="$(normalize_windows_path "$REPO")"

if [[ ! -d "$REPO/.git" ]]; then
  echo "Not a git repo: $REPO" >&2
  exit 1
fi

cd "$REPO"

echo "==> Removing merge conflict markers: reset to origin/$BRANCH"
echo ""

git fetch origin "$BRANCH"
git checkout -B "$BRANCH" "origin/$BRANCH"
git reset --hard "origin/$BRANCH"

echo ""
echo "Done. Clean commit: $(git rev-parse --short HEAD)"
echo "Reopen Unity — Console errors should be gone."
echo ""
echo "Includes: level 29-65 win fix + hand drag to right panel fix."
