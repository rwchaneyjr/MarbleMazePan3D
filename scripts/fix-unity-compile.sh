#!/usr/bin/env bash
# Fix duplicate-class compile errors and restore clean CardWidget.cs.
# Run from SymbolAlgebra root: cd /c/Users/rober/SymbolAlgebra && bash scripts/fix-unity-compile.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${1:-.}"
if [[ "$ROOT" == "." ]]; then
  ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
fi

SCRIPTS="$ROOT/Assets/DragonBoxAlgebra/Scripts"
DROPINS="$ROOT/dropins"
CARD_WIDGET="$SCRIPTS/UI/CardWidget.cs"
CARD_DROPIN="$DROPINS/CardWidget.cs"
VERIFY="$SCRIPT_DIR/verify-no-conflict-markers.sh"

if [[ ! -d "$SCRIPTS" ]]; then
  echo "Not found: $SCRIPTS" >&2
  exit 1
fi

has_conflict_markers() {
  [[ -f "$1" ]] && grep -qE '^(<<<<<<<|=======|>>>>>>>)' "$1"
}

restore_cardwidget() {
  if [[ ! -f "$CARD_DROPIN" ]]; then
    echo "WARNING: $CARD_DROPIN not found — cannot restore CardWidget.cs" >&2
    return 1
  fi

  mkdir -p "$SCRIPTS/UI"
  cp "$CARD_DROPIN" "$CARD_WIDGET"
  echo "    Copied dropins/CardWidget.cs -> UI/CardWidget.cs"
}

echo "==> Restoring clean CardWidget.cs (fixes CS8300 merge conflict markers)..."
if has_conflict_markers "$CARD_WIDGET"; then
  echo "    CardWidget.cs has conflict markers — replacing from dropins/"
fi
if [[ -f "$SCRIPT_DIR/force-fix-cardwidget.sh" ]]; then
  bash "$SCRIPT_DIR/force-fix-cardwidget.sh"
else
  restore_cardwidget
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

echo "==> Final CardWidget restore (ensures dropins wins over any bad merge)..."
restore_cardwidget

if has_conflict_markers "$CARD_WIDGET"; then
  echo "ERROR: CardWidget.cs has merge conflict markers. Run: bash scripts/fix-unity-compile.sh" >&2
  exit 1
fi

if [[ -f "$VERIFY" ]]; then
  bash "$VERIFY" "$ROOT"
fi

echo ""
echo "Done. Return to Unity — CS8300 errors should be gone. Then Play (expect 128/128)."
echo "Tip: run once: bash scripts/install-git-hooks.sh  (auto-fix after future merges)"
