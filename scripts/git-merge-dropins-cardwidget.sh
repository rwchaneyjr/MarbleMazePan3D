#!/usr/bin/env bash
set -euo pipefail
ROOT="$(git rev-parse --show-toplevel)"
cp "$ROOT/dropins/CardWidget.cs" "$2"
exit 0
