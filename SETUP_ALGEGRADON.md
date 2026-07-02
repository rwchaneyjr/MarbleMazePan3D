# Push to AlgebraDragon

The DragonBox Unity project is ready. The cloud agent cannot push directly to
`AlgebraDragon` (permission denied). Run these commands **on your computer**:

## Option A — Push this branch to AlgebraDragon (recommended)

```bash
git clone https://github.com/rwchaneyjr/MarbleMazePan3D.git
cd MarbleMazePan3D
git checkout cursor/algebra-dragon-setup-1ad2

git remote add algebra-dragon https://github.com/rwchaneyjr/AlgebraDragon.git
git push -u algebra-dragon cursor/algebra-dragon-setup-1ad2:main
```

## Option B — Copy from existing DragonBox branch

```bash
git clone https://github.com/rwchaneyjr/MarbleMazePan3D.git temp-copy
cd temp-copy
git checkout cursor/dragonbox-algebra-unity-1ad2

# Remove marble maze files manually, then:
git remote add algebra-dragon https://github.com/rwchaneyjr/AlgebraDragon.git
git push -u algebra-dragon HEAD:main
```

## Option C — GitHub import

1. Open https://github.com/rwchaneyjr/AlgebraDragon
2. Use **Import repository** or push from local as above

## After pushing

1. Open the `AlgebraDragon` folder in Unity
2. Open `Assets/DragonBoxAlgebra/Scenes/DragonBox.unity`
3. Press Play

## Grant cloud agent access (optional)

To let Cursor push to `AlgebraDragon` automatically in the future:

1. Go to https://github.com/rwchaneyjr/AlgebraDragon/settings/installations
2. Ensure the **Cursor** GitHub App has access to this repository
