# SymbolAlgebra — copy these files into your Unity project

## Where to copy (SymbolAlgebra on main branch)

Copy each file from this `dropins/` folder into the matching path under your project:

```
C:\Users\rober\SymbolAlgebra\Assets\DragonBoxAlgebra\Scripts\
```

| Drop-in file | Copy to |
|--------------|---------|
| AlgebraGameController.cs | Gameplay/ |
| BalancePending.cs | Gameplay/ |
| GameSnapshot.cs | Gameplay/ |
| CardFlipRules.cs | Gameplay/ |
| HandRules.cs | Gameplay/ |
| PendingCancelMarker.cs | Gameplay/ |
| LevelLibrary.cs | Gameplay/ |
| CombineRules.cs | Core/ |
| BoardCard.cs | Core/ |
| BoardView.cs | UI/ |
| CardWidget.cs | UI/ |
| HandView.cs | UI/ |
| BoardDropZone.cs | UI/ |
| BalanceHoleWidget.cs | UI/ |
| AsteriskCancelWidget.cs | UI/ |
| AlgebraBootstrap.cs | Scripts/ (parent of Core) |
| AlgebraUI.cs | UI/ |

**New files** (create folder if missing): `BalancePending.cs`, `BalanceHoleWidget.cs`, `AsteriskCancelWidget.cs`, `PendingCancelMarker.cs`, `CardFlipRules.cs`, `HandRules.cs`

## After copying

1. Return to Unity — wait for compile
2. Press **Play**
3. Level 1 test:
   - Drag night to RIGHT → * on right, ? on left
   - Same card stays in hand at bottom
   - Drag it to the ? on LEFT → hand empty, balanced

## Fixes included

- ? hole merges with incoming tile
- Hand card stays visible after first drag
- One * per side max
- Light + dark on same side → spinning *
- Click hand card to flip light/dark before playing
- Dice cancel instantly (level 4), no asterisks
