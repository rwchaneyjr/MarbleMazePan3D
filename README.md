# AlgebraDragon

A **DragonBox Algebra** recreation in Unity — drag cards on a two-sided board to learn algebra through play.

Built from the reference video shared by [rwchaneyjr](https://github.com/rwchaneyjr).

## Quick start

1. Clone this repo
2. Open the folder in **Unity 2022.3+** or **Unity 6**
3. Open `Assets/DragonBoxAlgebra/Scenes/DragonBox.unity`
4. Press **Play**

No manual scene wiring — `AlgebraBootstrap` builds the UI at runtime.

## Controls

| Action | How |
|--------|-----|
| Combine cards | **Drag** one card onto another on the **same side** |
| Play from hand | **Drag** a hand card onto either board panel |
| Opposites cancel | Day + Night, +1 + -1 → green vortex animation |
| Merge to One | Drag identical cards together |
| One eliminates | Drag a **One** card onto any other card |
| Divide | Drag **➗** from hand onto the board |
| Undo | **↩** button (top-right) |
| Rewind | **⏪** button — back to level start |

## Features

- Two-sided textured board (left/right equation sides)
- Drag-and-drop card interaction
- Opposite cancel with green vortex animation
- Merge identical → **One** (multiply mechanic)
- **One** eliminates cards (×1 mechanic)
- Divide tool for identical pairs
- Undo and Rewind
- Procedural card sprites and board texture
- Sound effects (combine, play, undo, win)
- Creature bounce reactions
- Win screen with stars and checklist
- 6 sample levels

## Project layout

```
Assets/DragonBoxAlgebra/
  Scenes/DragonBox.unity
  Scripts/
    Core/         board, cards, combine rules
    Gameplay/     levels, undo, game controller
    UI/           canvas, drag-drop, animations, audio
reference/frames/ reference screenshots from demo video
```

## Adding levels

Edit `Assets/DragonBoxAlgebra/Scripts/Gameplay/LevelLibrary.cs`.

## License

MIT — for learning and personal projects. DragonBox is a trademark of its respective owners; this is an educational recreation.
