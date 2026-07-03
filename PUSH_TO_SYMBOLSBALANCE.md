# Push to SymbolsBalance

GitHub repo: **https://github.com/rwchaneyjr/SymbolsBalance**  
Local folder: **`C:\Users\rober\Desktop\SymbolAlgebra`** (or your Desktop copy)

---

## Option A — Push from Desktop (easiest)

In **Git Bash**:

```bash
cd /c/Users/rober/Desktop/SymbolAlgebra
git init
git remote add origin https://github.com/rwchaneyjr/SymbolsBalance.git
git add .
git commit -m "SymbolsBalance Unity project"
git branch -M main
git push -u origin main
```

If `origin` already points elsewhere:

```bash
git remote set-url origin https://github.com/rwchaneyjr/SymbolsBalance.git
git push -u origin main
```

---

## Option B — Clone latest from MarbleMazePan3D, then push

```bash
git clone -b cursor/algebra-dragon-setup-1ad2 https://github.com/rwchaneyjr/MarbleMazePan3D.git SymbolsBalance-temp
cd SymbolsBalance-temp
git remote remove origin
git remote add origin https://github.com/rwchaneyjr/SymbolsBalance.git
git push -u origin HEAD:main
```

---

## Option C — GitHub Action (one-click sync)

1. Create a **Personal Access Token** at https://github.com/settings/tokens (check **repo** scope)
2. **MarbleMazePan3D** → Settings → Secrets → Actions → add **`SYMBOLS_BALANCE_TOKEN`**
3. **Actions** → **Push to SymbolsBalance** → **Run workflow** → branch **`cursor/algebra-dragon-setup-1ad2`**

---

## Open in Unity

1. Unity Hub → **Open** → your Desktop folder
2. Scene: `Assets/DragonBoxAlgebra/Scenes/DragonBox.unity`
3. Press **Play**
