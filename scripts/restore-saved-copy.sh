#!/usr/bin/env bash
# Restore Ch1 saved copy WITH CreatureSprites PNG support + debug.
#
# Usage:
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/restore-saved-copy.sh

set -euo pipefail

BRANCH="${RESTORE_BRANCH:-cursor/ch1-saved-sprites-3fe3}"
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

echo "==> Ch1 saved copy + PNG sprites: origin/$BRANCH"
echo ""

rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/ThemeAssignment.cs.meta

git fetch origin "$BRANCH"
git checkout -B "$BRANCH" "origin/$BRANCH"
git reset --hard "origin/$BRANCH"

echo ""
echo "Done. Restart Unity → Play"
echo "  SPRITE DEBUG line under title | Console filter: DragonBox"
echo "  PNGs: Assets/DragonBoxAlgebra/Resources/CreatureSprites/"
