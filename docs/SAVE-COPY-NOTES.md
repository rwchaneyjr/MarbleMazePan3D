# Save copy — frog art + level fixes

**Saved:** 2026-07-05  
**Branch:** `cursor/snapshot-frog-only-1c5d`  
**Tag:** `save/frog-level-fixes-jul-2026`

## What's in this save

- Theme 6: frog & snake (not bee)
- Theme 1: robin & owl (not bee-like wings)
- Distinct hand tiles and board themes beside the red box
- Dice levels 4 & 6: green +1 / purple −1 dice, clean card layout
- Multi-card dice band (11, 16, 21): **creatures** beside box, matching creatures in hand (no stray dice)
- 2-hand creature levels: two matching solvers with distinct themes (e.g. level 9)

## Known issues

- **No right matches** — the main remaining bug; everything else is in pretty good shape
- Level 26 not specifically tested in this save

## Restore this save

```bash
cd /c/Users/rober/SymbolAlgebra
git merge --abort 2>/dev/null || true
git fetch origin
git checkout save/frog-level-fixes-jul-2026
RESTORE_BRANCH=cursor/snapshot-frog-only-1c5d bash scripts/restore-scripts.sh
```

Or by branch:

```bash
git fetch origin
git checkout cursor/snapshot-frog-only-1c5d
RESTORE_BRANCH=cursor/snapshot-frog-only-1c5d bash scripts/restore-scripts.sh
```

Or:

```bash
bash scripts/restore-snapshot-frog.sh
```

Do **not** run plain `bash scripts/restore-scripts.sh` — it defaults to `main`.

## SymbolAlgebra folder (Windows)

`C:\Users\rober\SymbolAlgebra`

## Older snapshot (before these fixes)

Tag: `save/no-right-matches-problem-11-16-21-26`  
Branch: `cursor/snapshot-known-issues-1c5d`
