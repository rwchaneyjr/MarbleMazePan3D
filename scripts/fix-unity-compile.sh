#!/usr/bin/env bash
# Fix duplicate-class compile errors and restore clean CardWidget.cs.
# Run from SymbolAlgebra root: cd /c/Users/rober/SymbolAlgebra && bash scripts/fix-unity-compile.sh

set -euo pipefail

ROOT="${1:-.}"
SCRIPTS="$ROOT/Assets/DragonBoxAlgebra/Scripts"
DROPINS="$ROOT/dropins"
CARD_WIDGET="$SCRIPTS/UI/CardWidget.cs"
CARD_DROPIN="$DROPINS/CardWidget.cs"

if [[ ! -d "$SCRIPTS" ]]; then
  echo "Not found: $SCRIPTS" >&2
  exit 1
fi

has_conflict_markers() {
  [[ -f "$1" ]] && grep -qE '^(<<<<<<<|=======|>>>>>>>)' "$1"
}

echo "==> Restoring clean CardWidget.cs (fixes merge conflict markers)..."
if [[ -f "$CARD_DROPIN" ]]; then
  mkdir -p "$SCRIPTS/UI"
  cp "$CARD_DROPIN" "$CARD_WIDGET"
  echo "    Copied dropins/CardWidget.cs -> UI/CardWidget.cs"
else
  echo "    WARNING: $CARD_DROPIN not found — skipping CardWidget restore"
fi

if has_conflict_markers "$CARD_WIDGET"; then
  echo "ERROR: CardWidget.cs still has merge conflict markers after restore." >&2
  exit 1
fi

echo "==> Removing stale UI duplicates..."
for f in ChapterLevelGenerator.cs VariableGoalRules.cs WinChecker.cs CombineRules.cs \
  BoardCard.cs BoardSide.cs CardKind.cs AlgebraBoard.cs \
  HandVisualRules.cs ThemeAssignment.cs DragMergeLevelGenerator.cs CardWidget.cs; do
  if [[ "$f" == "CardWidget.cs" ]]; then
    continue
  fi
  rm -f "$SCRIPTS/UI/$f" "$SCRIPTS/UI/${f}.meta"
done

echo "==> Re-import dropins to correct folders..."
if [[ -f "$ROOT/scripts/sync-dropins.sh" ]]; then
  (cd "$ROOT" && bash scripts/sync-dropins.sh import --here)
else
  echo "sync-dropins.sh not found — only restored CardWidget."
fi

if has_conflict_markers "$CARD_WIDGET"; then
  echo "ERROR: CardWidget.cs has merge conflict markers. Run: bash scripts/fix-unity-compile.sh" >&2
  exit 1
fi

echo ""
echo "Done. Return to Unity — errors should clear. Then Play (expect 120/120)."
