# Drop-in scripts for Unity `algebra` project

Copy **all `.cs` files** from this folder into your Unity project:

```
C:\Users\rober\algebra\Assets\
```

(Replace existing files when Windows asks.)

## After copying

1. Return to Unity — wait for compile to finish
2. Click **Exit Safe Mode** if shown
3. Create an empty scene (or use any scene)
4. **GameObject → Create Empty** → name it `Bootstrap`
5. Add component: **Algebra Bootstrap**
6. Press **Play**

Or open `Assets/DragonBoxAlgebra/Scenes/DragonBox.unity` if you copied that scene too.

## Files included (23 scripts)

| File | Purpose |
|------|---------|
| `AlgebraBootstrap.cs` | Starts the game |
| `AlgebraUI.cs` | Builds UI (fixed) |
| `CardWidget.cs` | Drag-drop cards (fixed) |
| `BoardView.cs` | Left/right board |
| `HandView.cs` | Hand bar |
| `AlgebraGameController.cs` | Game logic |
| `LevelLibrary.cs` | 6 levels |
| ... | See folder for full list |

## Fixes in this version

- Unity 2022: `eventData.hovered` uses `GameObject` not `RaycastResult`
- Unity 2022: `completePanel.gameObject` passed to LevelCompleteView

## Folder structure (optional)

You can put files flat in `Assets/` (like you have now) OR organize as:

```
Assets/DragonBoxAlgebra/Scripts/
  Core/
  Gameplay/
  UI/
  Audio/
  AlgebraBootstrap.cs
```

Both work as long as all files are in the project.
