#!/usr/bin/env bash
# ONE command to fix Unity compile errors (CS8300, CS1061 mismatched scripts, etc.)
#
# Usage:
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/fix-all-errors.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
BRANCH="${BRANCH:-cursor/extreme-hand-flip-3fe3}"

cd "$ROOT"

echo "==> Step 1: Clear stuck git merge (if any)"
git merge --abort 2>/dev/null || true
git reset --merge 2>/dev/null || true

echo "==> Step 2: Fetch latest clean scripts"
fetched=0
for remote in source origin; do
  if git fetch "$remote" "$BRANCH" 2>/dev/null; then
    echo "    Fetched $remote/$BRANCH"
    fetched=1
    FETCH_REMOTE="$remote"
    break
  fi
done

if [[ $fetched -eq 0 ]]; then
  echo "WARNING: Could not fetch $BRANCH — using local dropins/ only" >&2
fi

if [[ $fetched -eq 1 ]]; then
  echo "==> Step 3: Pull ALL dropins from remote (no merge — keeps scripts in sync)"
  mkdir -p "$ROOT/dropins"
  while IFS= read -r -d '' path; do
    base="${path#dropins/}"
    dest="$ROOT/dropins/$base"
    if git show "$FETCH_REMOTE/$BRANCH:$path" > "$dest.tmp" 2>/dev/null; then
      mv "$dest.tmp" "$dest"
      echo "    $base"
    else
      rm -f "$dest.tmp"
    fi
  done < <(git ls-tree -r --name-only -z "$FETCH_REMOTE/$BRANCH" dropins/ 2>/dev/null | grep '\.cs$' || true)
else
  echo "==> Step 3: Skipped remote pull (offline?)"
fi

echo "==> Step 4: Force-fix CardWidget.cs"
bash "$SCRIPT_DIR/force-fix-cardwidget.sh"

echo "==> Step 5: Full unity compile fix (import ALL dropins + duplicate cleanup)"
bash "$SCRIPT_DIR/fix-unity-compile.sh" "$ROOT"

echo "==> Step 6: Verify scripts match"
bash "$SCRIPT_DIR/verify-no-conflict-markers.sh" "$ROOT"

CONTROLLER="$ROOT/Assets/DragonBoxAlgebra/Scripts/Gameplay/AlgebraGameController.cs"
GENERATOR="$ROOT/Assets/DragonBoxAlgebra/Scripts/Gameplay/ChapterLevelGenerator.cs"
missing=0

check_symbol() {
  local file="$1"
  local symbol="$2"
  local label="$3"
  if [[ ! -f "$file" ]] || ! grep -q "$symbol" "$file"; then
    echo "ERROR: $label missing $symbol" >&2
    missing=1
  fi
}

check_symbol "$CONTROLLER" 'CanFlipHandCard' 'AlgebraGameController.cs'
check_symbol "$GENERATOR" 'StartLevelIndexForChapter' 'ChapterLevelGenerator.cs'
check_symbol "$GENERATOR" 'NameForChapter' 'ChapterLevelGenerator.cs'
check_symbol "$GENERATOR" 'UsesPlusBetweenBoardTiles' 'ChapterLevelGenerator.cs'
check_symbol "$GENERATOR" 'CurriculumVersion = "2026-07-ch7-mixed-128"' 'ChapterLevelGenerator.cs'

if [[ $missing -ne 0 ]]; then
  echo "" >&2
  echo "Scripts are mismatched (old + new mixed). Switch to the clean branch:" >&2
  echo "  git checkout -B $BRANCH ${FETCH_REMOTE:-source}/$BRANCH" >&2
  echo "  bash scripts/fix-all-errors.sh" >&2
  exit 1
fi

echo "OK — AlgebraGameController + ChapterLevelGenerator match (128 levels)"

echo ""
echo "============================================"
echo "DONE — Unity errors should be fixed."
echo "1. Go to Unity"
echo "2. Stop Play if running"
echo "3. Wait for compile to finish"
echo "4. Press Play"
echo "============================================"
