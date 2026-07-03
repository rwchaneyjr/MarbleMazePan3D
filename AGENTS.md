# AGENTS.md

## Cursor Cloud specific instructions

This repo is a single **Unity 6 game** (`MarbleMazePan3D`) — pure C# `MonoBehaviour`
scripts under `Assets/Scripts/`, one scene at `Assets/Scenes/Main.unity`, and the
`GameBootstrap` component that builds the whole playable scene (marble, board,
maze walls, holes, goal, camera, light) at runtime. There is **no backend, server,
database, or web service** and **no npm/pip/etc. package manager** — Unity restores
`Packages/manifest.json` automatically the first time the project is opened.

### Unity Editor (already installed in the VM snapshot)
- Version is pinned by `ProjectSettings/ProjectVersion.txt`: **6000.0.32f1**
  (changeset `b2e806cf271c`).
- Installed at `~/unity/6000.0.32f1/Editor/Unity` (Linux Editor + bundled
  `LinuxStandaloneSupport` playback engine). Runtime `.so` deps (libgtk-3, libnss3,
  libgbm1, libasound2t64, xvfb, etc.) are installed system-wide.
- The Editor is GUI-based; run it headlessly with `-batchmode -nographics` and/or
  under the Xvfb display `DISPLAY=:1` (already running in the VM).

### Licensing (REQUIRED before Unity will do anything — this is the main gotcha)
- Unity refuses to open a project, compile, build, or run **without an activated
  license**, even the free Personal tier. Without one you get:
  `No valid Unity Editor license found. Please activate your license.`
- Activation needs a **Unity account**, supplied via secrets/env vars:
  - `UNITY_EMAIL` + `UNITY_PASSWORD` (+ optional `UNITY_SERIAL`; leave empty for a
    free Personal license), **or**
  - `UNITY_LICENSE` = the full contents of a `.ulf` license file.
- Helper: `~/unity-tools/activate.sh` performs activation from those env vars.
  Run it once per VM before building/running.

### Build / run / test (all require an activated license)
- **Compile check / import:**
  `~/unity/6000.0.32f1/Editor/Unity -batchmode -nographics -quit -projectPath /workspace -logFile /tmp/unity.log`
  (success = no compile errors in the log).
- **Build + run the game headlessly and screenshot it:** `~/unity-tools/build_and_run.sh [screenshot.png]`
  — builds a `StandaloneLinux64` player that includes `Main.unity` (via a temporary
  `Assets/Editor/CloudBuildCLI.cs` that is auto-created then deleted; do not commit
  it), runs it on `DISPLAY=:1`, and captures a screenshot of the running maze.
- **Tests:** the project has **no automated tests** (Unity Test Framework is in the
  manifest but no test assemblies exist). "Testing" = pressing Play in the Editor,
  or the standalone build+run above.
- **Lint:** none configured.

### Notes
- `ProjectSettings/` only commits `ProjectVersion.txt` and `TagManager.asset`; Unity
  regenerates the rest (incl. `EditorBuildSettings.asset`) with defaults on first
  open, so `Main.unity` is **not** in the default build settings — always pass the
  scene explicitly when building (the helper script does this).
- `Assets/Scenes/Main.unity` references `GameBootstrap` by GUID
  `a1b2c3d4e5f6789012345678abcdef01`; keep `GameBootstrap.cs.meta` in sync.
