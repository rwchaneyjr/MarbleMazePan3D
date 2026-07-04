#!/usr/bin/env bash
# Pull latest SymbolAlgebra into your local Unity folder (Windows Git Bash).
#
# Default folder: C:\Users\rober\SymbolAlgebra
#   Git Bash path: /c/Users/rober/SymbolAlgebra
#
# Usage (Git Bash):
#   bash scripts/update-symbolalgebra.sh
#
# Custom folder:
#   SYMBOL_ALGEBRA_DIR="/d/Games/SymbolAlgebra" bash scripts/update-symbolalgebra.sh

set -euo pipefail

SOURCE_REPO="${SOURCE_REPO:-https://github.com/rwchaneyjr/MarbleMazePan3D.git}"
SOURCE_BRANCH="${SOURCE_BRANCH:-main}"
SYMBOL_ALGEBRA_DIR="${SYMBOL_ALGEBRA_DIR:-/c/Users/rober/SymbolAlgebra}"

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

SYMBOL_ALGEBRA_DIR="$(normalize_windows_path "$SYMBOL_ALGEBRA_DIR")"

echo "==> SymbolAlgebra update"
echo "    Folder: $SYMBOL_ALGEBRA_DIR"
echo "    Source: $SOURCE_REPO ($SOURCE_BRANCH)"
echo ""

if [[ ! -d "$SYMBOL_ALGEBRA_DIR" ]]; then
  echo "==> Folder not found — cloning fresh project"
  mkdir -p "$(dirname "$SYMBOL_ALGEBRA_DIR")"
  git clone --branch "$SOURCE_BRANCH" "$SOURCE_REPO" "$SYMBOL_ALGEBRA_DIR"
  echo ""
  echo "Done. Open in Unity:"
  echo "  $SYMBOL_ALGEBRA_DIR/Assets/DragonBoxAlgebra/Scenes/DragonBox.unity"
  exit 0
fi

cd "$SYMBOL_ALGEBRA_DIR"

if [[ ! -d .git ]]; then
  echo "==> No git repo here — cloning into this folder is unsafe if Unity files exist."
  echo "    Move your project aside, then re-run, OR run from an empty folder."
  echo ""
  echo "    Quick fix — rename current folder, clone fresh:"
  echo "      mv \"$SYMBOL_ALGEBRA_DIR\" \"${SYMBOL_ALGEBRA_DIR}.bak\""
  echo "      git clone --branch $SOURCE_BRANCH $SOURCE_REPO \"$SYMBOL_ALGEBRA_DIR\""
  exit 1
fi

echo "==> Fetching latest"
git fetch origin "$SOURCE_BRANCH"

if git show-ref --verify --quiet "refs/heads/$SOURCE_BRANCH"; then
  git checkout "$SOURCE_BRANCH"
else
  git checkout -b "$SOURCE_BRANCH" "origin/$SOURCE_BRANCH"
fi

if ! git remote get-url origin >/dev/null 2>&1; then
  git remote add origin "$SOURCE_REPO"
elif [[ "$(git remote get-url origin)" != "$SOURCE_REPO" ]]; then
  echo "==> Note: origin is not MarbleMazePan3D — pulling anyway"
  git remote add upstream "$SOURCE_REPO" 2>/dev/null || git remote set-url upstream "$SOURCE_REPO"
  git pull upstream "$SOURCE_BRANCH"
  echo ""
  echo "Done. Open in Unity:"
  echo "  $SYMBOL_ALGEBRA_DIR/Assets/DragonBoxAlgebra/Scenes/DragonBox.unity"
  exit 0
fi

git pull origin "$SOURCE_BRANCH"

echo ""
echo "Done. Open in Unity:"
echo "  $SYMBOL_ALGEBRA_DIR/Assets/DragonBoxAlgebra/Scenes/DragonBox.unity"
echo ""
echo "Includes Sun & Storm tile fix (puzzle ~13) when on latest main."
