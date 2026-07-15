#!/usr/bin/env bash
# One command: pull latest branch, remove duplicate UI scripts, import drop-ins.
#
# Usage (repo separate from Unity):
#   SYMBOL_ALGEBRA_DIR="/c/Users/rober/SymbolAlgebra" bash scripts/update-unity.sh
#
# Usage (repo is your Unity project):
#   bash scripts/update-unity.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
BRANCH="${BRANCH:-cursor/ch1-merge-intro-1c5d}"
SYMBOL_ALGEBRA_DIR="${SYMBOL_ALGEBRA_DIR:-/c/Users/rober/SymbolAlgebra}"

cd "$REPO_ROOT"

echo "==> Fetching and checking out $BRANCH"
git fetch origin
git checkout "$BRANCH"
git pull origin "$BRANCH"

echo ""
echo "==> Removing duplicate UI scripts (if any)"
bash "$SCRIPT_DIR/fix-duplicate-scripts.sh"

echo ""
echo "==> Importing drop-ins into Unity project"
SYMBOL_ALGEBRA_DIR="$SYMBOL_ALGEBRA_DIR" bash "$SCRIPT_DIR/sync-dropins.sh" import

echo ""
echo "==> Restore clean CardWidget + verify no conflict markers"
bash "$SCRIPT_DIR/fix-unity-compile.sh" "$SYMBOL_ALGEBRA_DIR"

echo ""
echo "Done. Open Unity, wait for compile, press Play."
echo "One-time: bash scripts/install-git-hooks.sh  (prevents CardWidget merge conflicts)"
echo "Do NOT commit random .meta files from GitHub Desktop unless you know why."
