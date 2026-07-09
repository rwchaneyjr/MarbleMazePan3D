Custom creature images for SymbolAlgebra / DragonBoxAlgebra
============================================================

Put your PNG or JPG files here, then in Unity set Texture Type to "Sprite (2D and UI)".

Your sea-turtle style art works great. Example for theme 0 (Fish & Turtle):

  dark_turtle.png     -> night / dark card (the turtle image)
  light_fish.png      -> day / light card (optional fish partner)

Naming options (use any one style):

  1) Theme index (best for all 10 creature pairs)
     theme00_light.png   theme00_dark.png   (Fish & Turtle)
     theme01_light.png   theme01_dark.png   (Bird & Owl)
     theme02_light.png   theme02_dark.png   (Crab & Jelly)
     ... through theme09_

  2) Creature name
     light_fish.png      dark_turtle.png
     light_bird.png      dark_owl.png
     light_crab.png      dark_jelly.png

  3) Legacy names still work
     lightfish.png       darkturtle.png

Box / dragon box (optional):
  box.png

Tips:
- Square images (~512x512 or 1024x1024) look best on cards.
- Use preserveAspect — tall art is auto-fitted inside the card.
- If no image is found, the game falls back to built-in procedural art.
- After adding files: return to Unity, wait for import, press Play.

This folder path in your project:
  Assets/DragonBoxAlgebra/Resources/CreatureSprites/
