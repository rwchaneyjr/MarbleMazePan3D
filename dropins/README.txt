# SymbolAlgebra — copy these files into your Unity project

## Git Bash (fastest)

```bash
cd /c/path/to/MarbleMazePan3D
git pull origin cursor/drag-merge-levels-1c5d
bash scripts/sync-dropins.sh import
```

See `scripts/DROPIN-BASH.txt` for more options.

**Branches:**
- `cursor/drag-merge-levels-1c5d` — 20 drag-merge tutorial levels
- `cursor/single-hand-levels-1c5d` — main 80-chapter game

---

## Manual copy

Copy each file from this `dropins/` folder into the matching path under your project:

```
C:\Users\rober\SymbolAlgebra\Assets\DragonBoxAlgebra\Scripts\
```

| Drop-in file | Copy to |
|--------------|---------|
| AlgebraBootstrap.cs | Scripts/ (parent of Core) |
| AlgebraGameController.cs | Gameplay/ |
| BalancePending.cs | Gameplay/ |
| ChapterLevelGenerator.cs | Gameplay/ |
| DragMergeLevelGenerator.cs | Gameplay/ |
| GameSnapshot.cs | Gameplay/ |
| CardFlipRules.cs | Gameplay/ |
| HandRules.cs | Gameplay/ |
| PendingCancelMarker.cs | Gameplay/ |
| LevelLibrary.cs | Gameplay/ |
| LevelGenerator.cs | Gameplay/ |
| LevelDefinition.cs | Gameplay/ |
| MoveTracker.cs | Gameplay/ |
| CombineRules.cs | Core/ |
| BoardCard.cs | Core/ |
| AlgebraBoard.cs | Core/ |
| BoardSide.cs | Core/ |
| CardKind.cs | Core/ |
| WinChecker.cs | Core/ |
| BoardView.cs | UI/ |
| CardWidget.cs | UI/ |
| HandView.cs | UI/ |
| BoardDropZone.cs | UI/ |
| BalanceHoleWidget.cs | UI/ |
| AsteriskCancelWidget.cs | UI/ |
| AlgebraUI.cs | UI/ |
| AudioManager.cs | Audio/ |

## After copying

1. Return to Unity — wait for compile
2. Press **Play**
3. Open `Assets/DragonBoxAlgebra/Scenes/DragonBox.unity`

## Fixes included (drag-merge branch)

- Drag light onto dark on same side → spinning *
- 20 tutorial levels (5 left, 5 right, 5 two-vs-one, 5 three-vs-two)
- Merge intro animation (light/dark slide together)
- Tap * to dismiss; win with red box alone
- Right side fits up to 6 tiles (scaled layout)
- Balance ? hole and deferred day/night cancel
