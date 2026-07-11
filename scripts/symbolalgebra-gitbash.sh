#!/usr/bin/env bash
# One entry point for Git Bash on your PC.
# Default Unity project: C:\Users\rober\SymbolAlgebra
#
# Usage:
#   bash scripts/symbolalgebra-gitbash.sh pull      # get latest from GitHub
#   bash scripts/symbolalgebra-gitbash.sh push      # push this repo to AlgebraDragon
#   bash scripts/symbolalgebra-gitbash.sh status    # show folder + git status

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SYMBOL_ALGEBRA_DIR="${SYMBOL_ALGEBRA_DIR:-/c/Users/rober/SymbolAlgebra}"
CMD="${1:-pull}"

case "$CMD" in
  pull|update)
    exec bash "$ROOT/scripts/update-symbolalgebra.sh"
    ;;
  push|sync)
    cd "$ROOT"
    exec bash "$ROOT/scripts/sync-algebra-dragon.sh" main
    ;;
  status)
    echo "SymbolAlgebra folder: $SYMBOL_ALGEBRA_DIR"
    if [[ -d "$SYMBOL_ALGEBRA_DIR/.git" ]]; then
      cd "$SYMBOL_ALGEBRA_DIR"
      git status -sb
      echo ""
      git log --oneline -3
    else
      echo "(not a git repo yet — run: bash scripts/symbolalgebra-gitbash.sh pull)"
    fi
    ;;
  -h|--help|help)
    sed -n '2,9p' "$0"
    echo "  pull     Download latest main into C:\\Users\\rober\\SymbolAlgebra"
    echo "  push     Push to AlgebraDragon (needs your GitHub access)"
    echo "  status   Show git status for your SymbolAlgebra folder"
    ;;
  *)
    echo "Unknown command: $CMD"
    echo "Try: bash scripts/symbolalgebra-gitbash.sh pull"
    exit 1
    ;;
esac
