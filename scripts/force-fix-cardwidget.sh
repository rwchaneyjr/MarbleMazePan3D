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
REMOTE_BRANCH="${REMOTE_BRANCH:-cursor/working-up-to-variable-100-3fe3-7-14-26}"
REMOTE="${REMOTE:-source}"

has_markers() {
  [[ -f "$1" ]] && grep -qE '^(<<<<<<<|=======|>>>>>>>)' "$1"
}

echo "==> Force-fix CardWidget.cs (no merge needed)"

if has_markers "$DROPIN"; then
  echo "    dropins/CardWidget.cs has conflict markers — fetching clean copy..."
  fetched=0
  for remote in "$REMOTE" origin source; do
    for branch in "$REMOTE_BRANCH" cursor/extreme-hand-flip-3fe3 cursor/ch7-sea-variable-120-3fe3; do
      if git -C "$ROOT" fetch "$remote" "$branch" 2>/dev/null \
        && git -C "$ROOT" show "$remote/$branch:dropins/CardWidget.cs" > "$DROPIN" 2>/dev/null \
        && ! has_markers "$DROPIN"; then
        echo "    Got clean copy from $remote/$branch"
        fetched=1
        break 2
      fi
    done
  done
  if [[ $fetched -eq 0 ]]; then
    echo "    Stripping conflict marker lines as last resort..."
    sed -i '/^<<<<<<< /d; /^=======$/d; /^>>>>>>> /d' "$DROPIN" 2>/dev/null \
      || sed -i '' '/^<<<<<<< /d; /^=======$/d; /^>>>>>>> /d' "$DROPIN" 2>/dev/null \
      || true
  fi
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

# Emergency: strip any leftover marker lines in the Unity copy
if has_markers "$TARGET"; then
  sed -i '/^<<<<<<< /d; /^=======$/d; /^>>>>>>> /d' "$TARGET" 2>/dev/null \
    || sed -i '' '/^<<<<<<< /d; /^=======$/d; /^>>>>>>> /d' "$TARGET" 2>/dev/null \
    || true
fi

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
