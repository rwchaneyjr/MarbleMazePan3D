# Put the game on AlgebraDragon

The cloud agent **cannot push directly** to `AlgebraDragon` until Cursor is granted access to that repo.

## Fastest way (about 1 minute) — run on your PC

Open **Git Bash** or **PowerShell** and paste:

```bash
git clone -b cursor/algebra-dragon-setup-1ad2 https://github.com/rwchaneyjr/MarbleMazePan3D.git AlgebraDragon
cd AlgebraDragon
git remote remove origin
git remote add origin https://github.com/rwchaneyjr/AlgebraDragon.git
git push -u origin HEAD:main
```

Then open **https://github.com/rwchaneyjr/AlgebraDragon** — the full Unity project will be there.

---

## Or: grant Cursor access (so the agent can push for you)

1. Open https://github.com/settings/installations
2. Click **Cursor** → **Configure**
3. Under **Repository access**, add **AlgebraDragon**
4. Save, then tell the agent: **"try pushing to AlgebraDragon again"**

---

## Or: GitHub Actions one-click sync

1. Create a Personal Access Token: https://github.com/settings/tokens (scope: **repo**)
2. Go to **MarbleMazePan3D → Settings → Secrets → Actions**
3. New secret: name `ALGEBRA_DRAGON_TOKEN`, paste your token
4. Go to **Actions → Push to AlgebraDragon → Run workflow**

This copies the DragonBox-only branch to **AlgebraDragon/main**.

---

## What's included

- Full DragonBox Algebra Unity game
- Drag-drop, undo, rewind, multiply/divide, sounds, animations
- 6 levels
- No marble maze files

Open in Unity: `Assets/DragonBoxAlgebra/Scenes/DragonBox.unity`
