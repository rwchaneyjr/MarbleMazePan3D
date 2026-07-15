#!/usr/bin/env bash
# One-time setup: install hooks + CardWidget merge driver so CS8300 never comes back.
#
# Usage:
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/install-git-hooks.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

if [[ ! -d "$ROOT/.git" ]]; then
  echo "Not a git repo: $ROOT" >&2
  exit 1
fi

chmod +x "$ROOT/scripts/git-merge-dropins-cardwidget.sh"
chmod +x "$ROOT/scripts/verify-no-conflict-markers.sh"
chmod +x "$ROOT/scripts/fix-unity-compile.sh"

mkdir -p "$ROOT/.git/hooks"
for hook in post-merge pre-commit; do
  cp "$ROOT/githooks/$hook" "$ROOT/.git/hooks/$hook"
  chmod +x "$ROOT/.git/hooks/$hook"
  echo "Installed .git/hooks/$hook"
done

git config merge.dropins-cardwidget.name "Always use dropins/CardWidget.cs"
git config merge.dropins-cardwidget.driver \
  "bash \"$ROOT/scripts/git-merge-dropins-cardwidget.sh\" %O %A %B"

echo ""
echo "CardWidget merge driver configured."
echo "After every merge, hooks run: bash scripts/fix-unity-compile.sh"
echo ""
bash "$ROOT/scripts/verify-no-conflict-markers.sh" "$ROOT"
