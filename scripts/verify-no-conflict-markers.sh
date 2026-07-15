#!/usr/bin/env bash
# Fail if any C# file still contains git merge conflict markers.
set -euo pipefail

ROOT="$(cd "${1:-.}" && pwd)"
SEARCH_DIRS=()

if [[ -d "$ROOT/Assets" ]]; then
  SEARCH_DIRS+=("$ROOT/Assets")
fi

if [[ -d "$ROOT/dropins" ]]; then
  SEARCH_DIRS+=("$ROOT/dropins")
fi

if [[ ${#SEARCH_DIRS[@]} -eq 0 ]]; then
  echo "No Assets/ or dropins/ under $ROOT" >&2
  exit 1
fi

found=0
while IFS= read -r -d '' file; do
  if grep -qE '^(<<<<<<<|=======|>>>>>>>)' "$file"; then
    echo "MERGE CONFLICT MARKERS: $file" >&2
    grep -nE '^(<<<<<<<|=======|>>>>>>>)' "$file" >&2 || true
    found=1
  fi
done < <(find "${SEARCH_DIRS[@]}" -name '*.cs' -type f -print0 2>/dev/null)

if [[ $found -ne 0 ]]; then
  echo "" >&2
  echo "Fix: bash scripts/fix-unity-compile.sh" >&2
  exit 1
fi

echo "OK — no merge conflict markers in C# files."
