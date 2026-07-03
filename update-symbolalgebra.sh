#!/usr/bin/env bash
# Pull latest algebra game into SymbolAlgebra (Git Bash on Windows)
set -euo pipefail

TARGET="${1:-/c/Users/rober/SymbolAlgebra}"
REPO="https://github.com/rwchaneyjr/MarbleMazePan3D.git"
BRANCH="cursor/clean-repo-1ad2"
CACHE="/c/Users/rober/_MarbleMazePan3D_cache"

echo "=== SymbolAlgebra updater ==="
echo "Target: $TARGET"
echo "Branch: $BRANCH"
echo

if [[ ! -d "$TARGET" ]]; then
  echo "Target folder missing. Cloning fresh..."
  git clone -b "$BRANCH" "$REPO" "$TARGET"
  echo "Done. Open $TARGET in Unity."
  exit 0
fi

echo "Fetching latest into cache..."
rm -rf "$CACHE"
git clone -b "$BRANCH" --depth 1 "$REPO" "$CACHE"

echo "Removing orphaned .meta files (meta without .cs)..."
find "$TARGET/Assets/DragonBoxAlgebra" -name '*.meta' 2>/dev/null | while read -r meta; do
  asset="${meta%.meta}"
  if [[ ! -e "$asset" ]]; then
    echo "  delete orphan: $meta"
    rm -f "$meta"
  fi
done

echo "Copying Assets/DragonBoxAlgebra..."
mkdir -p "$TARGET/Assets"
rm -rf "$TARGET/Assets/DragonBoxAlgebra"
cp -r "$CACHE/Assets/DragonBoxAlgebra" "$TARGET/Assets/"

echo "Copying Packages + ProjectSettings (if missing)..."
for dir in Packages ProjectSettings; do
  if [[ -d "$CACHE/$dir" ]]; then
    mkdir -p "$TARGET/$dir"
    cp -r "$CACHE/$dir/." "$TARGET/$dir/"
  fi
done

if [[ -f "$CACHE/Packages/manifest.json" ]]; then
  cp "$CACHE/Packages/manifest.json" "$TARGET/Packages/manifest.json"
fi

rm -rf "$CACHE"

echo
echo "=== Done ==="
echo "Open Unity project: $TARGET"
echo "Scene: Assets/DragonBoxAlgebra/Scenes/DragonBox.unity"
echo
echo "Level 3 should show:"
echo "  Left: box + light creature"
echo "  Hand: 1 dark creature"
echo "  Right: empty"

