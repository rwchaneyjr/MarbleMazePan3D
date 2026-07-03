# Publish to SymbolsBalance (one-time)

Run these in **Git Bash** to replace the old cluttered repo with this clean version.

## Option A — Fresh local project (easiest)

```bash
cd /c/Users/rober
rm -rf SymbolAlgebra
git clone -b cursor/symbolsbalance-standalone-1ad2 --depth 1 \
  https://github.com/rwchaneyjr/MarbleMazePan3D.git SymbolAlgebra
```

Open `C:\Users\rober\SymbolAlgebra` in Unity Hub.

## Option B — Push clean copy to SymbolsBalance on GitHub

```bash
cd /c/Users/rober
rm -rf _symbolsbalance_publish
git clone -b cursor/symbolsbalance-standalone-1ad2 --depth 1 \
  https://github.com/rwchaneyjr/MarbleMazePan3D.git _symbolsbalance_publish
cd _symbolsbalance_publish
git remote remove origin
git remote add origin https://github.com/rwchaneyjr/SymbolsBalance.git
git push -u origin HEAD:main --force
cd ..
rm -rf _symbolsbalance_publish
```

After that, clone from your own repo:

```bash
git clone https://github.com/rwchaneyjr/SymbolsBalance.git SymbolAlgebra
```

## What's in the clean repo

- `Assets/DragonBoxAlgebra/` — game only
- `Packages/` + `ProjectSettings/` — Unity project
- `README.md` — setup instructions
- No `dropins/`, no reference videos, no sync scripts
