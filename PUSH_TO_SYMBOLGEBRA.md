# Push to SymbolAlgebra

GitHub repo: **https://github.com/rwchaneyjr/SymbolAlgebra**  
Local folder: **`C:\Users\rober\SymbolAlgebra`**

The cloud agent **cannot push** (403). Run this on your PC in **Git Bash**:

## Option A — First time (empty SymbolAlgebra repo)

```bash
git clone -b cursor/algebra-dragon-setup-1ad2 https://github.com/rwchaneyjr/MarbleMazePan3D.git SymbolAlgebra-temp
cd SymbolAlgebra-temp
git remote remove origin
git remote add origin https://github.com/rwchaneyjr/SymbolAlgebra.git
git push -u origin HEAD:main
```

Then on your PC:

```bash
cd C:/Users/rober
rmdir /s /q SymbolAlgebra
git clone https://github.com/rwchaneyjr/SymbolAlgebra.git SymbolAlgebra
```

Open **`C:\Users\rober\SymbolAlgebra`** in Unity Hub.

---

## Option B — You already have `C:\Users\rober\SymbolAlgebra`

```bash
cd /c/Users/rober/SymbolAlgebra
git init
git remote add origin https://github.com/rwchaneyjr/SymbolAlgebra.git
git add .
git commit -m "SymbolAlgebra Unity project"
git branch -M main
git push -u origin main
```

---

## Open in Unity

1. Unity Hub → **Open** → `C:\Users\rober\SymbolAlgebra`
2. Scene: `Assets/DragonBoxAlgebra/Scenes/DragonBox.unity`
3. Press **Play**

---

## Grant Cursor push access (optional)

https://github.com/settings/installations → **Cursor** → **Configure** → add **SymbolAlgebra**
