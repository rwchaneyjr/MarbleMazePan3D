#!/usr/bin/env bash
# Run ONCE on your PC so CardWidget never breaks again after merges.
# cd /c/Users/rober/SymbolAlgebra && bash scripts/install-git-hooks.sh

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
chmod +x "$ROOT/scripts/git-merge-dropins-cardwidget.sh"
mkdir -p "$ROOT/.git/hooks"
cp "$ROOT/githooks/post-merge" "$ROOT/.git/hooks/post-merge" 2>/dev/null || cat > "$ROOT/.git/hooks/post-merge" <<'HOOK'
#!/usr/bin/env bash
ROOT="$(git rev-parse --show-toplevel)"
bash "$ROOT/scripts/force-fix-cardwidget.sh" 2>/dev/null || true
HOOK
chmod +x "$ROOT/.git/hooks/post-merge"
git config merge.dropins-cardwidget.name "Use dropins CardWidget"
git config merge.dropins-cardwidget.driver "bash \"$ROOT/scripts/git-merge-dropins-cardwidget.sh\" %O %A %B"
echo "Hooks installed. CardWidget protected."
