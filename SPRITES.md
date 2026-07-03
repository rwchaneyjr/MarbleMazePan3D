# Adding fish / turtle sprites

Unity loads sprites at runtime from a **`Resources`** folder.

## Where to put your images

Move (or copy) your PNG files to:

```
Assets/DragonBoxAlgebra/Resources/Sprites/
```

Your files (`LightFish`, `DarkTurtle`, etc.) should live there — **not** in `Assets/DragonBoxAlgebra/Sprites/` alone.

Example names that work automatically:

| File name contains | Used for |
|--------------------|----------|
| `LightFish` | Day creature (+x) |
| `DarkTurtle` | Night creature (-x) |
| `DarkFish` / `LightTurtle` | Fallback day/night art |
| `box` or `dragon` | Box card |

## In Unity Inspector

1. Click each PNG in `Assets/DragonBoxAlgebra/Resources/Sprites/`
2. **Texture Type** → **Sprite (2D and UI)**
3. Click **Apply**
4. Press **Play**

If no images are found, the game uses built-in procedural fish/turtle art.

## Push to GitHub

```bash
cd /c/Users/rober/SymbolAlgebra
git pull origin main
git add .
git commit -m "Add sprites and sprite loading"
git push origin main
```
