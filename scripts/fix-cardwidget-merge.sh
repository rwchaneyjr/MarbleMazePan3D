#!/usr/bin/env bash
# Restore CardWidget.cs after a bad git merge left <<<<<<< markers.
# Run from SymbolAlgebra root: bash scripts/fix-cardwidget-merge.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
TARGET="$ROOT/Assets/DragonBoxAlgebra/Scripts/UI/CardWidget.cs"
DROPIN="$ROOT/dropins/CardWidget.cs"
BRANCH="${1:-cursor/ch7-sea-variable-120-3fe3}"
REMOTE="${2:-source}"

cd "$ROOT"

if [[ -f "$TARGET" ]] && grep -q '^<<<<<<< ' "$TARGET" 2>/dev/null; then
  echo "==> Found merge conflict markers in CardWidget.cs"
else
  echo "==> No conflict markers in CardWidget.cs (will still refresh from clean copy)"
fi

if git remote get-url "$REMOTE" &>/dev/null; then
  echo "==> Fetching $REMOTE/$BRANCH ..."
  git fetch "$REMOTE" "$BRANCH" 2>/dev/null || git fetch origin "$BRANCH"
  REF="$REMOTE/$BRANCH"
  if git rev-parse "$REF" &>/dev/null; then
    git checkout "$REF" -- Assets/DragonBoxAlgebra/Scripts/UI/CardWidget.cs
    echo "==> Restored CardWidget.cs from $REF"
  fi
fi

if grep -q '^<<<<<<< ' "$TARGET" 2>/dev/null; then
  if [[ -f "$DROPIN" ]]; then
    cp "$DROPIN" "$TARGET"
    echo "==> Copied clean dropins/CardWidget.cs"
  fi
fi

if grep -q '^<<<<<<< ' "$TARGET" 2>/dev/null; then
  echo "ERROR: conflict markers still present. Open the file and delete lines with <<<<<<<, =======, >>>>>>>" >&2
  exit 1
fi

echo "==> OK — return to Unity and wait for compile."
grep -n "CanFlipHand" -A 20 "$TARGET" | tail -5
