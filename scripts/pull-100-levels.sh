#!/usr/bin/env bash
# Pull the 100-level curriculum (Ch6 levels 81–100) into your Unity project.
#
# Use in MarbleMazePan3D repo:
#   bash scripts/pull-100-levels.sh
#
# Use in SymbolAlgebra (Desktop) — merges from MarbleMazePan3D:
#   cd /c/Users/rober/Desktop/SymbolAlgebra
#   bash scripts/pull-100-levels.sh symbolalgebra
#
# Then in Unity: wait for compile → Play → top bar should show …/100

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
BRANCH="${LEVELS_BRANCH:-cursor/ch5-gradual-from-save-3fe3}"
SOURCE_REMOTE="${LEVELS_SOURCE_REMOTE:-https://github.com/rwchaneyjr/MarbleMazePan3D.git}"

normalize_windows_path() {
  local p="$1"
  if [[ "$p" =~ ^[A-Za-z]: ]]; then
    local drive="${p:0:1}"
    drive=$(printf '%s' "$drive" | tr '[:upper:]' '[:lower:]')
    p="${p:2}"
    p="${p//\\//}"
    p="/${drive}${p}"
  fi
  printf '%s' "$p"
}

cleanup_stale_generator() {
  local scripts_dir="$1/Assets/DragonBoxAlgebra/Scripts"
  rm -f "$scripts_dir/UI/ChapterLevelGenerator.cs" "$scripts_dir/UI/ChapterLevelGenerator.cs.meta"
  rm -f "$scripts_dir/UI/VariableGoalRules.cs" "$scripts_dir/UI/VariableGoalRules.cs.meta"
  rm -f "$scripts_dir/UI/WinChecker.cs" "$scripts_dir/UI/WinChecker.cs.meta"
}

pull_marblemazepan3d() {
  cd "$REPO_ROOT"
  echo "==> MarbleMazePan3D: fetch $BRANCH"
  git fetch origin "$BRANCH"
  git checkout "$BRANCH"
  git reset --hard "origin/$BRANCH"
  bash scripts/sync-dropins.sh export
  cleanup_stale_generator "$REPO_ROOT"
  echo ""
  echo "Done. Open Unity in this repo and press Play."
  echo "Console should show: 100/100 levels (2026-07-ch6-100-v2)"
}

pull_symbolalgebra() {
  local target
  target="$(normalize_windows_path "${SYMBOL_ALGEBRA_DIR:-/c/Users/rober/Desktop/SymbolAlgebra}")"
  if [[ ! -d "$target/.git" ]]; then
    echo "SymbolAlgebra repo not found: $target" >&2
    exit 1
  fi

  cd "$target"
  echo "==> SymbolAlgebra: merge $BRANCH from MarbleMazePan3D"
  if ! git remote get-url source &>/dev/null; then
    git remote add source "$SOURCE_REMOTE"
  fi
  git fetch source "$BRANCH"
  git merge "source/$BRANCH" -m "Sync 100 levels ($BRANCH)" || {
    echo "Merge conflict — resolve, then run: bash scripts/sync-dropins.sh import --here" >&2
    exit 1
  }

  if [[ -f scripts/sync-dropins.sh ]]; then
    bash scripts/sync-dropins.sh import --here
  fi
  cleanup_stale_generator "$target"

  local count
  count="$(grep -E 'public const int TotalLevels' Assets/DragonBoxAlgebra/Scripts/Gameplay/ChapterLevelGenerator.cs \
    | head -1 || true)"
  echo ""
  echo "Done. $count"
  echo "Open Unity → Play → progress should show …/100 (level 81 = Ch6 Multi Variables)."
}

case "${1:-here}" in
  symbolalgebra|sa)
    pull_symbolalgebra
    ;;
  here|"")
    pull_marblemazepan3d
    ;;
  -h|--help|help)
    echo "Usage: bash scripts/pull-100-levels.sh [here|symbolalgebra]"
    ;;
  *)
    echo "Unknown mode: $1 (use here or symbolalgebra)" >&2
    exit 1
    ;;
esac
