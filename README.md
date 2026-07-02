# MarbleMazePan3D (Unity)

A 3D marble maze game for Unity, ported from the original Panda3D prototype. Guide the ball through the maze to the yellow goal cube using **WASD**. Press **R** to restart.

## Requirements

- Unity **2022.3 LTS** or newer (2022.3.50f1 recommended)
- TextMeshPro (imported automatically on first open)

## Quick start

1. Clone this repo and open the project folder in Unity Hub.
2. In the menu bar, choose **Marble Maze → Create Main Scene**.
3. Open `Assets/Scenes/MainScene.unity`.
4. Press **Play**.

## Controls

| Key | Action |
|-----|--------|
| W / A / S / D | Move the marble |
| R | Restart the level |

## How it works

- `MazeBuilder` reads the ASCII layout in `MazeLayout.cs` and spawns floor, walls, and the goal.
- `BallController` moves a kinematic `Rigidbody` with `MovePosition` so Unity's physics colliders handle wall sliding.
- `MarbleMazeGame` tracks the win condition and updates the on-screen status text.

## Project layout

```
Assets/
  Editor/           # One-click scene setup menu item
  Scenes/           # Created by the setup menu
  Scripts/          # Gameplay scripts
marble_maze.py      # Original Panda3D prototype (reference)
```

## Customizing the maze

Edit the `Layout` array in `Assets/Scripts/MazeLayout.cs`:

- `#` = wall
- `S` = start (ball spawn)
- `G` = goal
- ` ` (space) = open path

## Mobile / tilt controls (optional)

The current build uses keyboard input like the Panda3D version. To add accelerometer tilt on mobile, replace the input line in `BallController.cs` with:

```csharp
var input = new Vector3(Input.acceleration.x, 0f, Input.acceleration.y);
```

Then rotate the maze board instead of moving the ball directly for a classic wooden-labyrinth feel.
