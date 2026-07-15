#!/usr/bin/env bash
# Overwrite CardWidget.cs with the bundled clean copy — no git merge needed.
# Fixes Unity CS8300 at line ~789.
#
# Usage:
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/force-fix-cardwidget.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
TARGET="$ROOT/Assets/DragonBoxAlgebra/Scripts/UI/CardWidget.cs"
BUNDLED="$SCRIPT_DIR/CardWidget.clean.cs"
DROPIN="$ROOT/dropins/CardWidget.cs"

pick_source() {
  if [[ -f "$BUNDLED" ]] && ! grep -qE '^(<<<<<<<|=======|>>>>>>>)' "$BUNDLED" 2>/dev/null; then
    echo "$BUNDLED"
    return
  fi
  if [[ -f "$DROPIN" ]] && ! grep -qE '^(<<<<<<<|=======|>>>>>>>)' "$DROPIN" 2>/dev/null; then
    echo "$DROPIN"
    return
  fi
  echo ""
}

echo "==> Force-fix CardWidget.cs (CS8300)"

SRC="$(pick_source)"
if [[ -z "$SRC" ]]; then
  BRANCH="cursor/working-up-to-variable-100-3fe3-7-14-26"
  for remote in origin source; do
    if git -C "$ROOT" fetch "$remote" "$BRANCH" 2>/dev/null \
      && git -C "$ROOT" show "$remote/$BRANCH:scripts/CardWidget.clean.cs" > "$BUNDLED.tmp" 2>/dev/null; then
      mv "$BUNDLED.tmp" "$BUNDLED"
      SRC="$BUNDLED"
      break
    fi
  done
fi

if [[ -z "$SRC" ]]; then
  echo "ERROR: No clean CardWidget.cs found." >&2
  exit 1
fi

mkdir -p "$(dirname "$TARGET")"
cp "$SRC" "$TARGET"
cp "$SRC" "$DROPIN" 2>/dev/null || true

if grep -qE '^(<<<<<<<|=======|>>>>>>>)' "$TARGET" 2>/dev/null; then
  echo "ERROR: CardWidget.cs still has conflict markers." >&2
  exit 1
fi

echo "    Fixed: $TARGET"
echo "    Source: $SRC"
echo ""
echo "OK — open Unity, wait for compile, press Play."
