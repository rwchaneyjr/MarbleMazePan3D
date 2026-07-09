#!/usr/bin/env bash
# Pull latest game code + sync scripts + copy creature images.
#
# Usage (Git Bash):
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/update.sh
#
# Custom folder:
#   SYMBOL_ALGEBRA_DIR="/c/Users/rober/SymbolAlgebra" bash scripts/update.sh

set -euo pipefail

BRANCH="${UPDATE_BRANCH:-cursor/level-curriculum-50-3fe3}"
REPO="${SYMBOL_ALGEBRA_DIR:-/c/Users/rober/SymbolAlgebra}"
SRC="$REPO/creature-images"
DEST="$REPO/Assets/DragonBoxAlgebra/Resources/CreatureSprites"

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
mkdir -p "$SRC" "$DEST"

echo "==> Updating SymbolAlgebra from origin/$BRANCH"
echo "    Folder: $REPO"
echo ""

git fetch origin "$BRANCH"
git checkout "$BRANCH"
git pull origin "$BRANCH"

echo ""
echo "==> Restoring game scripts, editor tools, and scene from git"
git checkout "origin/$BRANCH" -- Assets/DragonBoxAlgebra/Scripts/
git checkout "origin/$BRANCH" -- Assets/DragonBoxAlgebra/Editor/ 2>/dev/null || true
git checkout "origin/$BRANCH" -- Assets/DragonBoxAlgebra/Scenes/

echo ""
echo "==> Syncing drop-in scripts"
bash scripts/sync-dropins.sh import --here

echo ""
echo "==> Copying creature images (if any in creature-images/)"
shopt -s nullglob
count=0
for f in "$SRC"/*.{png,jpg,jpeg,PNG,JPG,JPEG}; do
  cp -f "$f" "$DEST/"
  echo "  + $(basename "$f")"
  count=$((count + 1))
done

if [[ "$count" -eq 0 ]]; then
  echo "  (none — drop PNGs into creature-images/ and run again)"
else
  echo "  Copied $count image(s) to CreatureSprites."
fi

echo ""
echo "Done. In Unity:"
echo "  1. Menu: DragonBox Algebra → Open Game Scene and Setup"
echo "  2. Menu: DragonBox Algebra → Import Creature Images (Drag and Drop)"
echo "  3. Drag PNGs into the window, then press Play"
echo ""
echo "If Hierarchy is empty: DragonBox Algebra → Setup Scene (Camera + Bootstrap)"
