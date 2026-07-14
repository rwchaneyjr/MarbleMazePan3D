#!/usr/bin/env bash
# Fix duplicate-class compile errors after dropins import (wrong copies in UI/).
# Run from SymbolAlgebra root: cd /c/Users/rober/SymbolAlgebra

set -euo pipefail

ROOT="${1:-.}"
SCRIPTS="$ROOT/Assets/DragonBoxAlgebra/Scripts"

if [[ ! -d "$SCRIPTS" ]]; then
  echo "Not found: $SCRIPTS" >&2
  exit 1
fi

echo "==> Removing stale UI duplicates..."
for f in ChapterLevelGenerator.cs VariableGoalRules.cs WinChecker.cs CombineRules.cs \
  BoardCard.cs BoardSide.cs CardKind.cs AlgebraBoard.cs \
  HandVisualRules.cs ThemeAssignment.cs DragMergeLevelGenerator.cs; do
  rm -f "$SCRIPTS/UI/$f" "$SCRIPTS/UI/${f}.meta"
done

echo "==> Re-import dropins to correct folders..."
if [[ -f "$ROOT/scripts/sync-dropins.sh" ]]; then
  (cd "$ROOT" && bash scripts/sync-dropins.sh import --here)
else
  echo "sync-dropins.sh not found — only removed UI duplicates."
fi

echo ""
echo "Done. Return to Unity — errors should clear. Then Play (expect 1/100)."
