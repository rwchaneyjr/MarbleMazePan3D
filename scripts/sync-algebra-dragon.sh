#!/usr/bin/env bash
# Sync SymbolAlgebra (DragonBox) from MarbleMazePan3D to AlgebraDragon.
# Usage:
#   ./scripts/sync-algebra-dragon.sh              # push current branch
#   ./scripts/sync-algebra-dragon.sh main         # push main
#   ./scripts/sync-algebra-dragon.sh --clone      # fresh clone + push (for a new machine)

set -euo pipefail

SOURCE_REPO="${SOURCE_REPO:-https://github.com/rwchaneyjr/MarbleMazePan3D.git}"
TARGET_REPO="${TARGET_REPO:-https://github.com/rwchaneyjr/AlgebraDragon.git}"
BRANCH="${1:-main}"

if [[ "${1:-}" == "--clone" ]]; then
  DEST="${2:-AlgebraDragon}"
  echo "==> Cloning $SOURCE_REPO into $DEST"
  rm -rf "$DEST"
  git clone --branch main "$SOURCE_REPO" "$DEST"
  cd "$DEST"
  git remote remove origin
  git remote add origin "$TARGET_REPO"
  echo "==> Pushing to $TARGET_REPO (main)"
  git push -u origin HEAD:main
  echo "Done. Open $TARGET_REPO"
  exit 0
fi

if [[ "${1:-}" == "-h" || "${1:-}" == "--help" ]]; then
  sed -n '2,8p' "$0"
  exit 0
fi

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

echo "==> Fetching latest from origin"
git fetch origin "$BRANCH"
git checkout "$BRANCH"
git pull origin "$BRANCH"

if git remote get-url algebra-dragon >/dev/null 2>&1; then
  git remote set-url algebra-dragon "$TARGET_REPO"
else
  git remote add algebra-dragon "$TARGET_REPO"
fi

echo "==> Pushing $BRANCH -> algebra-dragon/main"
git push algebra-dragon "HEAD:main"

echo "Done. Open $TARGET_REPO"
