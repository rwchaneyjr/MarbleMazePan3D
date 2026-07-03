# Adding fish / turtle sprites

Unity loads sprites at runtime from a **`Resources`** folder.

## Where to put your images

Move (or copy) your PNG files to:

```
Assets/Resources/Sprites/
```

Example names that work automatically:

| File name contains | Used for |
|--------------------|----------|
| `fish` or `day` | Day creature (+x) |
| `turtle` or `night` | Night creature (-x) |
| `box` or `dragon` | Box card |

## In Unity Inspector

1. Click each PNG in `Assets/Resources/Sprites/`
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
