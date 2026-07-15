#!/usr/bin/env bash
# Overwrite CardWidget.cs with the clean dropins copy — NO git merge required.
# Fixes Unity CS8300 "Merge conflict marker encountered".
#
# Usage:
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/force-fix-cardwidget.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
DROPIN="$ROOT/dropins/CardWidget.cs"
TARGET="$ROOT/Assets/DragonBoxAlgebra/Scripts/UI/CardWidget.cs"
REMOTE_BRANCH="${REMOTE_BRANCH:-cursor/extreme-hand-flip-3fe3}"
REMOTE="${REMOTE:-source}"

has_markers() {
  [[ -f "$1" ]] && grep -qE '^(<<<<<<<|=======|>>>>>>>)' "$1"
}

echo "==> Force-fix CardWidget.cs (no merge needed)"

if has_markers "$DROPIN"; then
  echo "    dropins/CardWidget.cs has conflict markers — fetching clean copy from $REMOTE..."
  git -C "$ROOT" fetch "$REMOTE" "$REMOTE_BRANCH" 2>/dev/null || git -C "$ROOT" fetch origin "$REMOTE_BRANCH"
  git -C "$ROOT" show "$REMOTE/$REMOTE_BRANCH:dropins/CardWidget.cs" > "$DROPIN"
fi

if [[ ! -f "$DROPIN" ]]; then
  echo "ERROR: No clean dropins/CardWidget.cs and could not fetch one." >&2
  exit 1
fi

if has_markers "$DROPIN"; then
  echo "ERROR: dropins/CardWidget.cs still has conflict markers after fetch." >&2
  exit 1
fi

mkdir -p "$(dirname "$TARGET")"
cp "$DROPIN" "$TARGET"
echo "    Wrote: $TARGET"

if has_markers "$TARGET"; then
  echo "ERROR: CardWidget.cs still broken after copy." >&2
  exit 1
fi

echo ""
echo "OK — CardWidget.cs is clean. Open Unity and wait for recompile."
echo "Lines 792-793 should be:"
echo "    private bool CanFlipHand() =>"
echo "        _controller != null && SideName == \"Hand\" && _controller.CanFlipHandCard(Index);"
