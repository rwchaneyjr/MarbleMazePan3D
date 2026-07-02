# MarbleMazePan3D

A 3D Unity recreation of the classic **marble labyrinth pan** toy — tilt the board to roll a marble through walls, avoid holes, and reach the goal.

![Marble maze concept: tilt a wooden board to guide a marble through a maze while avoiding holes](docs/concept.svg)

## Quick start

1. Open this folder in **Unity 6** (2023.3 LTS also works with minor package adjustments).
2. Open `Assets/Scenes/Main.unity`.
3. Press **Play**.

The scene uses `GameBootstrap`, which builds the maze, marble, camera, and lighting at runtime — no manual scene wiring required.

### First-time Unity setup

If Unity prompts you to install the **Input System** package, accept and set **Active Input Handling** to *Both* or *Input System Package* under **Edit → Project Settings → Player**.

Add a **Marble** tag if missing: **Edit → Project Settings → Tags and Layers → Tags → + → `Marble`**.

## Controls

| Input | Action |
|-------|--------|
| **Mouse drag** | Tilt the maze board |
| **WASD / Arrow keys** | Tilt the maze board |
| **Mobile accelerometer** | Tilt using device orientation (Android/iOS builds) |

## How it works

```
┌─────────────────────────────────────┐
│  MazeBoard (MazeTiltController)     │  ← you tilt this
│  ├── Board surface + walls          │
│  ├── Holes (HoleTrigger)            │  ← marble respawns
│  ├── Goal (GoalTrigger)             │  ← win condition
│  └── Marble (Rigidbody + physics)   │
└─────────────────────────────────────┘
         ↓ gravity + tilt
    marble rolls through maze
```

### Scripts

| Script | Purpose |
|--------|---------|
| `MazeTiltController` | Reads input and rotates the board |
| `MazeBoardBuilder` | Procedurally creates walls, holes, and goal |
| `MarbleController` | Physics marble with respawn logic |
| `HoleTrigger` | Sends marble back to spawn |
| `GoalTrigger` | Fires win event |
| `GameManager` | Level timer, reset, and win state |
| `MazeCamera` | Smooth camera follow |
| `GameBootstrap` | One-click runtime scene assembly |

## Customizing the maze

Edit values on `MazeBoardBuilder` in the Inspector (after running once, or add the component manually):

- **boardSize** — overall play area
- **holePositions** — where traps are placed
- **goalPosition** — finish zone location
- Inner wall layout — edit `BuildWalls()` in `MazeBoardBuilder.cs`

For hand-authored levels, disable `MazeBoardBuilder` and model your maze in Blender or ProBuilder, then parent it under the `MazeBoard` object.

## Building for mobile

1. **File → Build Settings → Android / iOS**
2. Ensure accelerometer input is enabled (default on mobile).
3. `MazeTiltController.useAccelerometerOnMobile` is enabled by default.

## Project structure

```
Assets/
  Scenes/Main.unity       ← open this scene
  Scripts/                ← all gameplay code
Packages/manifest.json    ← Unity package dependencies
ProjectSettings/          ← Unity project config
```

## Next steps

- Add sound effects for rolling, falling, and winning
- Create multiple level scenes with different maze layouts
- Add a UI canvas with timer and reset button (`GameUI.cs` is included as a starting point)
- Import wood/metal materials for a more realistic pan look
- Add a rim/frame mesh around the board like the physical toy

## License

MIT — use freely for learning and game jams.
