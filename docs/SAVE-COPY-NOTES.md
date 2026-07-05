# Save copy — known issues

**Saved:** 2026-07-05

## Known issues

- **No right matches** — opposite creatures on the right side do not pair / cancel correctly (or right-side tiles do not match when they should).
- **Problem levels:** 11, 16, 21, 26 (partial fixes applied for 4, 6, 16, 21 in `cursor/snapshot-frog-only-1c5d`)

## Level reference (1-based, as shown in game UI)

| Level | Index | Notes |
|------:|------:|-------|
| 11 | 10 | Known problem |
| 16 | 15 | Known problem |
| 21 | 20 | Known problem |
| 26 | 25 | Known problem |

Levels 11, 16, 21, 26 are every 5th puzzle after level 10 (dice / extra-puzzle band in the generator).

## Restore this snapshot

```bash
git fetch origin
git checkout cursor/snapshot-known-issues-1c5d
```

Or by tag:

```bash
git fetch origin
git checkout save/no-right-matches-problem-11-16-21-26
```

## Saved snapshot + frog (bee swapped, nothing else)

```bash
cd /c/Users/rober/SymbolAlgebra
git merge --abort 2>/dev/null || true
git fetch origin
git checkout cursor/snapshot-frog-only-1c5d
RESTORE_BRANCH=cursor/snapshot-frog-only-1c5d bash scripts/restore-scripts.sh
```

Or use `bash scripts/restore-snapshot-frog.sh` after pulling latest.

Do **not** use `restore-scripts.sh` alone — it defaults to `main`, which changes hand tiles and levels.

## SymbolAlgebra folder (Windows)

`C:\Users\rober\SymbolAlgebra`
