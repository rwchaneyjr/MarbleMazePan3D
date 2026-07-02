# Push to your `algebra` repo

The cloud agent **cannot see** your private `algebra` repo yet (404 / no access).

## Run this on your PC (Git Bash) — about 1 minute

```bash
git clone -b cursor/algebra-dragon-setup-1ad2 https://github.com/rwchaneyjr/MarbleMazePan3D.git algebra-game
cd algebra-game
git remote remove origin
git remote add origin https://github.com/rwchaneyjr/algebra.git
git push -u origin HEAD:main
```

Then refresh **https://github.com/rwchaneyjr/algebra** — the full Unity DragonBox game will be there.

## Open in Unity

1. Clone `https://github.com/rwchaneyjr/algebra.git`
2. Open the folder in Unity 2022.3+ or Unity 6
3. Open `Assets/DragonBoxAlgebra/Scenes/DragonBox.unity`
4. Press **Play**

## Let Cursor push automatically next time

1. https://github.com/settings/installations
2. **Cursor** → **Configure**
3. Under **Repository access**, add **`algebra`**
4. Save → tell the agent **"try again"**

## What's included

- Full DragonBox Algebra Unity game
- Drag-drop, undo, rewind, multiply/divide
- Sound effects and green vortex animations
- 6 sample levels
- No marble maze files
