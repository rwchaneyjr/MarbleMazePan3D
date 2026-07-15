#!/usr/bin/env bash
# ONE command to fix Unity compile errors (CS8300 CardWidget merge junk, duplicates, etc.)
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
  echo "==> Step 3: Pull clean dropins from remote (no merge)"
  for f in dropins/CardWidget.cs dropins/AlgebraUI.cs dropins/AlgebraGameController.cs; do
    if git show "$FETCH_REMOTE/$BRANCH:$f" > "$ROOT/$f.tmp" 2>/dev/null; then
      mv "$ROOT/$f.tmp" "$ROOT/$f"
      echo "    Updated $f"
    else
      rm -f "$ROOT/$f.tmp"
    fi
  done
else
  echo "==> Step 3: Skipped remote pull (offline?)"
fi

echo "==> Step 4: Force-fix CardWidget.cs"
bash "$SCRIPT_DIR/force-fix-cardwidget.sh"

echo "==> Step 5: Full unity compile fix (dropins import + duplicate cleanup)"
bash "$SCRIPT_DIR/fix-unity-compile.sh" "$ROOT"

echo "==> Step 6: Verify"
bash "$SCRIPT_DIR/verify-no-conflict-markers.sh" "$ROOT"

echo ""
echo "============================================"
echo "DONE — Unity errors should be fixed."
echo "1. Go to Unity"
echo "2. Stop Play if running"
echo "3. Wait for compile to finish"
echo "4. Press Play"
echo "============================================"
