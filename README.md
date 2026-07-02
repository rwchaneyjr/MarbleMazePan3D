# MarbleMazePan3D / DragonBox Algebra (Unity)

This repo contains Unity prototypes inspired by classic physics puzzles and **DragonBox Algebra**.

## DragonBox Algebra clone (start here)

Recreates the core DragonBox Algebra gameplay from your reference video:

- **Two-sided board** (left/right equation sides)
- **The Box** — isolate it to win
- **Day/Night creatures** — opposite variables that cancel when combined
- **Dice constants** — positive/negative numbers that cancel
- **Balanced hand** — cards from your hand are added to **both** sides
- **Level complete screen** — stars based on moves and cards played
- **4 sample levels** to start

### Quick start

1. Open this folder in **Unity 2022.3+** or **Unity 6**
2. Open `Assets/DragonBoxAlgebra/Scenes/DragonBox.unity`
3. Press **Play**

No manual scene wiring needed — `AlgebraBootstrap` builds the UI at runtime.

### Controls

| Action | How |
|--------|-----|
| Combine cards | **Drag** one card onto another on the **same side** |
| Play from hand | **Drag** a hand card onto either board panel |
| Opposites cancel | Day + Night, +1 + -1 → green vortex animation |
| Merge to One | Drag identical cards together |
| One eliminates | Drag a **One** card onto any other card |
| Divide | Drag **➗** from hand onto the board (merges identical pair) |
| Undo | **↩** button (top-right) |
| Rewind | **⏪** button — back to level start |
| Next level | **Next** after winning |

### Features matching DragonBox

- ✅ Two-sided textured board
- ✅ Drag-and-drop card interaction
- ✅ Opposite cancel with **green vortex** animation
- ✅ Merge identical → **One** (multiply mechanic)
- ✅ **One** eliminates cards (×1 mechanic)
- ✅ **Divide tool** for identical pairs
- ✅ **Undo** and **Rewind**
- ✅ Procedural card sprites and board texture
- ✅ Sound effects (combine, play, undo, win)
- ✅ Creature bounce reactions on cards
- ✅ Win screen with stars and checklist
- ✅ 6 sample levels

### Project layout

```
Assets/DragonBoxAlgebra/
  Scenes/DragonBox.unity     ← open this scene
  Scripts/
    Core/                    ← board, cards, combine rules, win check
    Gameplay/                ← levels, moves, game controller
    UI/                      ← canvas, cards, win screen
    AlgebraBootstrap.cs      ← one-click setup
reference/frames/            ← screenshots extracted from your video
```

### Adding levels

Edit `Assets/DragonBoxAlgebra/Scripts/Gameplay/LevelLibrary.cs`:

```csharp
new LevelDefinition
{
    Title = "My Level",
    LeftCards = { CardKind.Box, CardKind.DayCreature },
    RightCards = { CardKind.DayCreature },
    HandCards = { CardKind.NightCreature },
    ParMoves = 3,
    ParCards = 1
}
```

### Next steps (optional polish)

- Import official-style DragonBox art sprites
- Show equation notation at top (e.g. `2x + 1 = 5`)
- Chapter select map screen
- Animated dragon character on win screen

---

## Marble Maze Pan 3D (bonus prototype)

A 3D marble labyrinth pan toy is also included under `Assets/Scripts/`.

Open `Assets/Scenes/Main.unity` and press Play to tilt a maze board with a physics marble.

---

## Reference video

Your Google Drive video (`algebra.mp4`) was used as reference. Extracted frames are in `reference/frames/`. The full video is excluded from git due to size — keep it in Google Drive or push with Git LFS if needed.

## License

MIT — for learning and personal projects. DragonBox is a trademark of its respective owners; this is an educational recreation.
