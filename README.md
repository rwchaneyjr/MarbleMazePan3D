# SymbolAlgebra

DragonBox-style algebra puzzle game for Unity. Drag tiles on a two-sided board, balance equations, and cancel opposite pairs.

## Quick start

1. Open this folder in **Unity 2022.3+** or **Unity 6**
2. Open `Assets/DragonBoxAlgebra/Scenes/DragonBox.unity`
3. Press **Play**

No manual scene setup needed — the scene already contains `AlgebraBootstrap`, which builds the UI at runtime.

## Controls

- **Drag a hand tile** onto one side → a `?` appears on the other side
- **Drag the same tile to the `?`** → balance complete
- **Light + dark on the same side** → spinning `*` (click to dismiss)
- **Click a hand tile** → flip light/dark
- **Undo / Rewind** → top-right buttons

## Sync into your local Unity project

From Git Bash on Windows:

```bash
bash update-symbolalgebra.sh /c/Users/rober/SymbolAlgebra
```

Or manually:

```bash
cd /c/Users/rober/SymbolAlgebra
git clone -b cursor/clean-repo-1ad2 --depth 1 \
  https://github.com/rwchaneyjr/MarbleMazePan3D.git /c/Users/rober/_algebra_tmp
rm -rf Assets/DragonBoxAlgebra
cp -r /c/Users/rober/_algebra_tmp/Assets/DragonBoxAlgebra Assets/
rm -rf /c/Users/rober/_algebra_tmp
```

## Optional custom sprites

Put PNGs in:

```
Assets/DragonBoxAlgebra/Resources/Sprites/
```

Set **Texture Type → Sprite (2D and UI)** in the Inspector. If no sprites are found, the game uses built-in procedural art.

## Repo layout

```
Assets/DragonBoxAlgebra/   Game scripts, scene, and assets
Packages/                  Unity package manifest
ProjectSettings/           Unity project settings
update-symbolalgebra.sh    Sync helper for local projects
```

## License

MIT — educational recreation. DragonBox is a trademark of its respective owners.
