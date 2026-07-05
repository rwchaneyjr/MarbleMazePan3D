# SymbolAlgebra — copy these files into your Unity project

## Git Bash (fastest)

```bash
cd /c/Users/rober/SymbolAlgebra
bash scripts/sync-dropins.sh import
```

See `scripts/DROPIN-BASH.txt` for more options.

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
| GameSnapshot.cs | Gameplay/ |
| CardFlipRules.cs | Gameplay/ |
| HandRules.cs | Gameplay/ |
| PendingCancelMarker.cs | Gameplay/ |
| LevelLibrary.cs | Gameplay/ |
| LevelGenerator.cs | Gameplay/ |
| LevelDefinition.cs | Gameplay/ |
| BoardVisualRules.cs | Gameplay/ |
| CoordinatedCreatureThemes.cs | Gameplay/ |
| HandCompositionRules.cs | Gameplay/ |
| HandVisualRules.cs | Gameplay/ |
| LevelSolvabilityRules.cs | Gameplay/ |
| ThemeAssignment.cs | Gameplay/ |
| BoardSideLayout.cs | UI/ |
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

## Fixes included

- Right side accommodates up to 6 tiles (auto-scaled layout)
- ? hole merges with incoming tile
- Hand card stays visible after first drag
- One * per side max
- Light + dark on same side → spinning *
- Day/night cancel waits until balance completes
- Click hand card to flip light/dark before playing
- Dice cancel instantly (level 4), no asterisks
