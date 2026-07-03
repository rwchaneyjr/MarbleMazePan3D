# SymbolAlgebra

DragonBox-style algebra puzzle game for Unity. Drag tiles on a two-sided board, balance equations, and cancel opposite pairs.

## Quick start

1. Clone this repo
2. Open the folder in **Unity 2022.3+** or **Unity 6**
3. Open `Assets/DragonBoxAlgebra/Scenes/DragonBox.unity`
4. Press **Play**

```bash
git clone https://github.com/rwchaneyjr/SymbolsBalance.git SymbolAlgebra
```

Then in Unity Hub: **Open** → select the `SymbolAlgebra` folder.

No manual scene setup needed — the scene already contains `AlgebraBootstrap`, which builds the UI at runtime.

## Controls

- **Drag a hand tile** onto one side → a `?` appears on the other side
- **Drag the same tile to the `?`** → balance complete
- **Light + dark on the same side** → spinning `*` (click to dismiss)
- **Click a hand tile** → flip light/dark
- **Undo / Rewind** → top-right buttons

## Update to latest

If you already have the project cloned:

```bash
cd /c/Users/rober/SymbolAlgebra
git pull origin main
```

Then reopen Unity (or let it recompile).

## Optional custom sprites

Put PNGs in:

```
Assets/DragonBoxAlgebra/Resources/Sprites/
```

Set **Texture Type → Sprite (2D and UI)** in the Inspector. If no sprites are found, the game uses built-in procedural art.

## Project layout

```
Assets/DragonBoxAlgebra/   Game scripts, scene, and assets
Packages/                  Unity package manifest
ProjectSettings/           Unity project settings
```

## License

MIT — educational recreation. DragonBox is a trademark of its respective owners.
