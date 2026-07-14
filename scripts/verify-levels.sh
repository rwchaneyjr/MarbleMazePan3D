#!/usr/bin/env bash
# Quick check: does THIS folder have the 100-level ChapterLevelGenerator?
# Run from your Unity project root (SymbolAlgebra or MarbleMazePan3D).

set -euo pipefail

GEN="${1:-Assets/DragonBoxAlgebra/Scripts/Gameplay/ChapterLevelGenerator.cs}"

if [[ ! -f "$GEN" ]]; then
  echo "MISSING: $GEN"
  echo "Wrong folder, or open the Unity project that contains DragonBoxAlgebra."
  exit 1
fi

stale="Assets/DragonBoxAlgebra/Scripts/UI/ChapterLevelGenerator.cs"
if [[ -f "$stale" ]]; then
  echo "STALE FILE (delete this — it blocks updates):"
  echo "  $stale"
  echo ""
fi

ch6="$(grep -c 'Chapter6LevelCount' "$GEN" || true)"
gen6="$(grep -c 'GenerateChapter6' "$GEN" || true)"
ver="$(grep 'CurriculumVersion' "$GEN" | head -1 | sed 's/.*= "//;s/";.*//')"

echo "File: $GEN"
echo "CurriculumVersion: ${ver:-unknown}"
grep -E 'Chapter[1-6]LevelCount|ChapterCount|TotalLevels' "$GEN" | head -8
echo ""

if [[ "$ch6" -ge 1 && "$gen6" -ge 1 ]]; then
  echo "OK — this copy includes Chapter 6 (should be 100 levels in Unity)."
else
  echo "OLD — only 80 levels. This file has no Chapter 6."
  echo ""
  echo "Fix (SymbolAlgebra):"
  echo "  git remote add source https://github.com/rwchaneyjr/MarbleMazePan3D.git"
  echo "  git fetch source cursor/ch5-gradual-from-save-3fe3"
  echo "  git merge source/cursor/ch5-gradual-from-save-3fe3"
  echo "  bash scripts/sync-dropins.sh import --here"
  exit 1
fi
