#!/usr/bin/env bash
# Restore the saved good working copy (panel-slide come-together win animation).
#
# Saved snapshot:
#   Branch: cursor/good-working-copy-3fe3
#   Tag:    good-working-copy-come-together
#   Commit: a1025bc — Restore panel-slide come-together win animation
#
# Includes:
#   - Levels 40–63 extra opposite tile
#   - Panel-slide come-together win (tiles stay on panels, both sides meet at center)
#   - Levels 29–30 removed; Ch4 win fixes
#   - No drag-layer tile reparenting during win
#
# Usage:
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/restore-good-working-copy.sh

set -euo pipefail

BRANCH="${RESTORE_BRANCH:-cursor/good-working-copy-3fe3}"
TAG="${RESTORE_TAG:-good-working-copy-come-together}"
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

echo "==> Good working copy: origin/$BRANCH (tag: $TAG)"
echo ""

rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/ThemeAssignment.cs.meta

git fetch origin "$BRANCH" tag "$TAG" 2>/dev/null || git fetch origin "$BRANCH"
git checkout -B "$BRANCH" "origin/$BRANCH"
git reset --hard "origin/$BRANCH"

echo ""
echo "Restored good working copy at $(git rev-parse --short HEAD)"
echo "  Panel-slide come-together win animation"
echo "  Levels 40–63 opposite tile + box slide together"
echo ""
echo "If Console shows package error on Play:"
echo "  Open Packages/manifest.json"
echo "  Delete line: \"com.unity.multiplayer.center\": \"1.0.0\","
echo "  Save → wait for Unity to reimport → Play again"
