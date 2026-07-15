#!/usr/bin/env bash
# Fix compile errors: restore clean CardWidget.cs + re-import dropins.
# Run: cd /c/Users/rober/SymbolAlgebra && bash scripts/fix-unity-compile.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${1:-.}"
if [[ "$ROOT" == "." ]]; then
  ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
fi

SCRIPTS="$ROOT/Assets/DragonBoxAlgebra/Scripts"
DROPIN="$ROOT/dropins/CardWidget.cs"
TARGET="$ROOT/Scripts/UI/CardWidget.cs"
TARGET="$ROOT/Assets/DragonBoxAlgebra/Scripts/UI/CardWidget.cs"

has_markers() {
  [[ -f "$1" ]] && grep -qE '^(<<<<<<<|=======|>>>>>>>)' "$1"
}

echo "==> Restore clean CardWidget.cs (fixes CS8300)..."
if [[ -f "$DROPIN" ]]; then
  mkdir -p "$(dirname "$TARGET")"
  cp "$DROPIN" "$TARGET"
  echo "    Copied dropins/CardWidget.cs"
fi

if has_markers "$TARGET"; then
  echo "ERROR: CardWidget.cs still has merge conflict markers." >&2
  echo "Run: bash scripts/force-fix-cardwidget.sh" >&2
  exit 1
fi

echo "==> Removing stale UI duplicates..."
for f in ChapterLevelGenerator.cs VariableGoalRules.cs WinChecker.cs CombineRules.cs \
  BoardCard.cs BoardSide.cs CardKind.cs AlgebraBoard.cs \
  HandVisualRules.cs ThemeAssignment.cs DragMergeLevelGenerator.cs; do
  rm -f "$SCRIPTS/UI/$f" "$SCRIPTS/UI/${f}.meta"
done

echo "==> Re-import dropins..."
if [[ -f "$ROOT/scripts/sync-dropins.sh" ]]; then
  (cd "$ROOT" && bash scripts/sync-dropins.sh import --here)
fi

echo "==> Final CardWidget restore..."
if [[ -f "$DROPIN" ]]; then
  cp "$DROPIN" "$TARGET"
fi

if has_markers "$TARGET"; then
  echo "ERROR: CardWidget.cs still broken." >&2
  exit 1
fi

echo ""
echo "Done. Unity should compile. Expect 128/128 levels."
