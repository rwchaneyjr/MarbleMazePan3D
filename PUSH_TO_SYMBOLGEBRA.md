# Push / Update SymbolAlgebra

GitHub repo: **https://github.com/rwchaneyjr/SymbolAlgebra**  
Local folder (Desktop): **`C:\Users\rober\Desktop\SymbolAlgebra`**

The cloud agent **cannot push** to SymbolAlgebra (403). Run these on your PC in **Git Bash**.

---

## First time — push game to empty GitHub repo

```bash
git clone -b cursor/algebra-dragon-setup-1ad2 https://github.com/rwchaneyjr/MarbleMazePan3D.git SymbolAlgebra-temp
cd SymbolAlgebra-temp
git remote remove origin
git remote add origin https://github.com/rwchaneyjr/SymbolAlgebra.git
git push -u origin HEAD:main
```

Then clone to Desktop:

```bash
cd /c/Users/rober/Desktop
git clone https://github.com/rwchaneyjr/SymbolAlgebra.git
```

---

## Update repo from Desktop (you already have the folder)

**Push your local changes to GitHub:**

```bash
cd /c/Users/rober/Desktop/SymbolAlgebra
git add .
git commit -m "Update SymbolAlgebra"
git push origin main
```

**Pull latest game code (100 levels, Ch6) from MarbleMazePan3D:**

```bash
cd /c/Users/rober/Desktop/SymbolAlgebra
bash scripts/pull-100-levels.sh symbolalgebra
```

Or manually:

```bash
cd /c/Users/rober/Desktop/SymbolAlgebra
git remote add source https://github.com/rwchaneyjr/MarbleMazePan3D.git
git fetch source cursor/ch5-gradual-from-save-3fe3
git merge source/cursor/ch5-gradual-from-save-3fe3 -m "Sync 100 levels from MarbleMazePan3D"
bash scripts/sync-dropins.sh import --here
```

(`git remote add source ...` only needed once.)

**Verify in Unity Console on Play:** `100/100 levels (2026-07-ch6-100)` — if you see `80/80`, the old `ChapterLevelGenerator.cs` is still in use. Delete `Assets/DragonBoxAlgebra/Scripts/UI/ChapterLevelGenerator.cs` if it exists (stale copy).

---

## Open in Unity

1. Unity Hub → **Open** → `C:\Users\rober\Desktop\SymbolAlgebra`
2. Scene: `Assets/DragonBoxAlgebra/Scenes/DragonBox.unity`
3. Press **Play**

---

## Grant Cursor push access (optional)

https://github.com/settings/installations → **Cursor** → **Configure** → add **SymbolAlgebra**
