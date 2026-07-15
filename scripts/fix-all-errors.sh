#!/usr/bin/env bash
# ONE command: fix CS8300 CardWidget + sync all scripts.
# Run: cd /c/Users/rober/SymbolAlgebra && bash scripts/fix-all-errors.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
BRANCH="${BRANCH:-cursor/working-up-to-variable-100-3fe3-7-14-26}"

cd "$ROOT"

echo "==> Clear stuck merge"
git merge --abort 2>/dev/null || true

echo "==> Fetch latest"
for remote in source origin; do
  if git fetch "$remote" "$BRANCH" 2>/dev/null; then
    FETCH_REMOTE="$remote"
    break
  fi
done

if [[ -n "${FETCH_REMOTE:-}" ]]; then
  echo "==> Pull all dropins from $FETCH_REMOTE/$BRANCH"
  mkdir -p dropins
  while IFS= read -r -d '' path; do
    base="${path#dropins/}"
    git show "$FETCH_REMOTE/$BRANCH:$path" > "dropins/$base" 2>/dev/null || true
  done < <(git ls-tree -r --name-only -z "$FETCH_REMOTE/$BRANCH" dropins/ 2>/dev/null | grep '\.cs$' || true)
fi

bash "$SCRIPT_DIR/fix-unity-compile.sh" "$ROOT"

echo ""
echo "DONE — open Unity, wait for compile, press Play."
