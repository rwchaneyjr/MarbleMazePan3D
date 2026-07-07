#!/usr/bin/env bash
# Remove duplicate gameplay scripts wrongly copied into UI/ (fixes CS0101 in Unity).
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SYMBOL_ALGEBRA_DIR="${SYMBOL_ALGEBRA_DIR:-/c/Users/rober/SymbolAlgebra}"

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

TARGET="$(normalize_windows_path "$SYMBOL_ALGEBRA_DIR")"
SCRIPTS="$TARGET/Assets/DragonBoxAlgebra/Scripts"

if [[ ! -d "$SCRIPTS" ]]; then
  echo "Unity scripts folder not found: $SCRIPTS" >&2
  echo "Set SYMBOL_ALGEBRA_DIR to your Unity project root." >&2
  exit 1
fi

echo "==> Removing duplicate UI copies from $SCRIPTS"
for stale in HandVisualRules.cs ThemeAssignment.cs ChapterLevelGenerator.cs DragMergeLevelGenerator.cs; do
  rm -fv "$SCRIPTS/UI/$stale" "$SCRIPTS/UI/${stale}.meta" 2>/dev/null || true
done
rm -fv "$SCRIPTS/UI/BoardSideLayout.cs" "$SCRIPTS/UI/BoardSideLayout.cs.meta" 2>/dev/null || true

echo ""
echo "Done. Return to Unity and wait for recompile."
