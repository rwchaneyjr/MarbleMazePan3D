#!/usr/bin/env bash
# Git merge driver: always write dropins/CardWidget.cs into the conflicted path.
set -euo pipefail

ROOT="$(git rev-parse --show-toplevel)"
DROPIN="$ROOT/dropins/CardWidget.cs"
CURRENT="$2"

if [[ ! -f "$DROPIN" ]]; then
  echo "git-merge-dropins-cardwidget: missing $DROPIN" >&2
  exit 1
fi

cp "$DROPIN" "$CURRENT"
exit 0
