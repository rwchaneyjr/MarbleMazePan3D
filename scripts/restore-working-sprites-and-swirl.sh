#!/usr/bin/env bash
# Restore working branch with level 29-65 win fix (commit 0cfae36):
#   Solvable Ch2/Ch3/Ch4 layouts, hand drag fixes, sides-together win animation.
#   Also: PNG creature sprites, SwirlingLight cancel marker, Ch1 x12.
#
# Usage:
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/restore-working-sprites-and-swirl.sh

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

echo "==> Working sprites + swirl snapshot: origin/$BRANCH"
echo ""

rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/ThemeAssignment.cs.meta

git fetch origin "$BRANCH"
git checkout -B "$BRANCH" "origin/$BRANCH"
git reset --hard "origin/$BRANCH"

echo ""
echo "Includes: level 29-65 win fix (0cfae36), sprites, SwirlingLight, Ch1 x12"
echo ""
echo "If Console shows package error on Play:"
echo "  Open Packages/manifest.json"
echo "  Delete line: \"com.unity.multiplayer.center\": \"1.0.0\","
echo "  Save → wait for Unity to reimport → Play again"
echo ""
echo "Debug: DragonBox Algebra → Print Sprite Debug"
