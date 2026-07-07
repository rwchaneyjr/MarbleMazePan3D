#!/usr/bin/env bash
# Sync SymbolAlgebra drop-in C# scripts between dropins/ and a Unity project.
#
# Export (repo -> dropins/ flat folder):
#   bash scripts/sync-dropins.sh export
#
# Import (dropins/ -> your Unity project):
#   bash scripts/sync-dropins.sh import
#   SYMBOL_ALGEBRA_DIR="/c/Users/rober/SymbolAlgebra" bash scripts/sync-dropins.sh import
#
# Import from inside this repo (updates Assets/DragonBoxAlgebra/Scripts):
#   bash scripts/sync-dropins.sh import --here

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
DROPINS_DIR="${DROPINS_DIR:-$REPO_ROOT/dropins}"
ASSETS_SCRIPTS="${ASSETS_SCRIPTS:-$REPO_ROOT/Assets/DragonBoxAlgebra/Scripts}"
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

target_subdir_for() {
  case "$1" in
    AlgebraBootstrap.cs) printf '%s' "" ;;
    Audio/*) printf '%s' "Audio" ;;
    Core/*) printf '%s' "Core" ;;
    Gameplay/*) printf '%s' "Gameplay" ;;
    UI/*) printf '%s' "UI" ;;
    *) return 1 ;;
  esac
}

gameplay_dropin() {
  case "$1" in
    AlgebraGameController.cs|BalancePending.cs|CardFlipRules.cs|ChapterLevelGenerator.cs|\
    DragMergeLevelGenerator.cs|GameSnapshot.cs|HandRules.cs|HandVisualRules.cs|\
    LevelDefinition.cs|LevelGenerator.cs|LevelLibrary.cs|MoveTracker.cs|\
    PendingCancelMarker.cs|ThemeAssignment.cs)
      return 0
      ;;
    *)
      return 1
      ;;
  esac
}

core_dropin() {
  case "$1" in
    AlgebraBoard.cs|BoardCard.cs|BoardSide.cs|CardKind.cs|CombineRules.cs|WinChecker.cs)
      return 0
      ;;
    *)
      return 1
      ;;
  esac
}

audio_dropin() {
  [[ "$1" == "AudioManager.cs" ]]
}

cleanup_stale_imports() {
  local scripts_dir="$1"
  local stale
  for stale in HandVisualRules.cs ThemeAssignment.cs ChapterLevelGenerator.cs DragMergeLevelGenerator.cs; do
    rm -f "$scripts_dir/UI/$stale" "$scripts_dir/UI/${stale}.meta"
  done
  rm -f "$scripts_dir/UI/BoardSideLayout.cs" "$scripts_dir/UI/BoardSideLayout.cs.meta"
}

export_dropins() {
  echo "==> Exporting Assets -> dropins/"
  echo "    From: $ASSETS_SCRIPTS"
  echo "    To:   $DROPINS_DIR"
  echo ""

  mkdir -p "$DROPINS_DIR"

  local count=0
  while IFS= read -r -d '' src; do
    local base
    base="$(basename "$src")"
    cp "$src" "$DROPINS_DIR/$base"
    count=$((count + 1))
    echo "    $base"
  done < <(find "$ASSETS_SCRIPTS" -name "*.cs" -type f -print0 | sort -z)

  echo ""
  echo "Exported $count files to dropins/"
}

import_dropins() {
  local target_root="$1"
  local scripts_dir="$target_root/Assets/DragonBoxAlgebra/Scripts"

  if [[ ! -d "$DROPINS_DIR" ]]; then
    echo "dropins/ folder not found: $DROPINS_DIR" >&2
    exit 1
  fi

  echo "==> Importing dropins/ -> Unity project"
  echo "    From: dropins/"
  echo "    To:   $scripts_dir"
  echo ""

  mkdir -p "$scripts_dir/Core" "$scripts_dir/Gameplay" "$scripts_dir/UI" "$scripts_dir/Audio"

  cleanup_stale_imports "$scripts_dir"

  local count=0
  for src in "$DROPINS_DIR"/*.cs; do
    [[ -f "$src" ]] || continue
    local base dest_subdir dest
    base="$(basename "$src")"

    case "$base" in
      AlgebraBootstrap.cs) dest_subdir="" ;;
      *)
        if gameplay_dropin "$base"; then
          dest_subdir="Gameplay"
        elif core_dropin "$base"; then
          dest_subdir="Core"
        elif audio_dropin "$base"; then
          dest_subdir="Audio"
        else
          dest_subdir="UI"
        fi
        ;;
    esac

    if [[ -z "$dest_subdir" ]]; then
      dest="$scripts_dir/$base"
    else
      dest="$scripts_dir/$dest_subdir/$base"
    fi

    cp "$src" "$dest"
    count=$((count + 1))
    if [[ -z "$dest_subdir" ]]; then
      echo "    $base -> Scripts/"
    else
      echo "    $base -> $dest_subdir/"
    fi
  done

  cleanup_stale_imports "$scripts_dir"

  echo ""
  echo "Imported $count files."
  echo "Open Unity, wait for compile, then press Play."
  echo "Scene: Assets/DragonBoxAlgebra/Scenes/DragonBox.unity"
}

usage() {
  cat <<'EOF'
Usage:
  bash scripts/sync-dropins.sh export
  bash scripts/sync-dropins.sh import [--here]
  SYMBOL_ALGEBRA_DIR="/c/Users/rober/SymbolAlgebra" bash scripts/sync-dropins.sh import

export  Copy all C# from Assets/DragonBoxAlgebra/Scripts into dropins/
import  Copy dropins/*.cs into a Unity project's Scripts subfolders
--here  Import into this repo's Assets (default target is SYMBOL_ALGEBRA_DIR)
EOF
}

main() {
  local cmd="${1:-import}"
  shift || true

  case "$cmd" in
    export)
      export_dropins
      ;;
    import)
      local target
      if [[ "${1:-}" == "--here" ]]; then
        target="$REPO_ROOT"
      else
        target="$(normalize_windows_path "$SYMBOL_ALGEBRA_DIR")"
      fi

      if [[ ! -d "$target/Assets/DragonBoxAlgebra" && "$target" != "$REPO_ROOT" ]]; then
        echo "Unity project not found: $target" >&2
        echo "Set SYMBOL_ALGEBRA_DIR or use: bash scripts/sync-dropins.sh import --here" >&2
        exit 1
      fi

      import_dropins "$target"
      ;;
    -h|--help|help)
      usage
      ;;
    *)
      echo "Unknown command: $cmd" >&2
      usage >&2
      exit 1
      ;;
  esac
}

main "$@"
