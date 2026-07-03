using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    public static class SpriteFactory
    {
        private static Sprite _roundedCard;
        private static Sprite _roundedButton;
        private static Sprite _fishCreature;
        private static Sprite _turtleCreature;
        private static Texture2D _boardTexture;

        public static Sprite FishCreature => _fishCreature ??= CreateFishSprite();
        public static Sprite TurtleCreature => _turtleCreature ??= CreateTurtleSprite();

        public static Sprite RoundedCard => _roundedCard ??= CreateRoundedSprite(128, 128, 18, Color.white);
        public static Sprite RoundedButton => _roundedButton ??= CreateRoundedSprite(128, 64, 14, Color.white);

        public static Texture2D BoardTexture
        {
            get
            {
                if (_boardTexture != null)
                {
                    return _boardTexture;
                }

                _boardTexture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                _boardTexture.filterMode = FilterMode.Bilinear;
                var baseColor = new Color(0.45f, 0.72f, 0.78f);
                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.08f;
                        _boardTexture.SetPixel(x, y, baseColor + new Color(noise, noise, noise));
                    }
                }

                _boardTexture.Apply();
                return _boardTexture;
            }
        }

        public static Sprite CreateRoundedSprite(int width, int height, int radius, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inside = IsInsideRoundedRect(x, y, width, height, radius);
                    texture.SetPixel(x, y, inside ? color : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite CreateFishSprite()
        {
            return CreateCreatureSprite(new Color(0.2f, 0.55f, 0.95f), new Color(0.95f, 0.45f, 0.2f), true);
        }

        public static Sprite CreateTurtleSprite()
        {
            return CreateCreatureSprite(new Color(0.25f, 0.65f, 0.35f), new Color(0.85f, 0.75f, 0.2f), false);
        }

        private static Sprite CreateCreatureSprite(Color bodyColor, Color accentColor, bool isFish)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            if (isFish)
            {
                FillEllipse(texture, 48, 50, 34, 18, bodyColor);
                FillEllipse(texture, 72, 50, 10, 8, accentColor);
                FillEllipse(texture, 28, 50, 8, 8, Color.white);
                texture.SetPixel(26, 52, Color.black);
            }
            else
            {
                FillEllipse(texture, 48, 52, 30, 22, bodyColor);
                FillEllipse(texture, 48, 68, 24, 10, accentColor);
                FillEllipse(texture, 34, 44, 6, 6, Color.white);
                FillEllipse(texture, 62, 44, 6, 6, Color.white);
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static void FillEllipse(Texture2D texture, int cx, int cy, int rx, int ry, Color color)
        {
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
                    {
                        continue;
                    }

                    float dx = (x - cx) / (float)rx;
                    float dy = (y - cy) / (float)ry;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
        {
            if (x >= radius && x < width - radius)
            {
                return true;
            }

            if (y >= radius && y < height - radius)
            {
                return true;
            }

            float cx = x < radius ? radius : width - radius - 1;
            float cy = y < radius ? radius : height - radius - 1;
            float dx = x - cx;
            float dy = y - cy;
            return dx * dx + dy * dy <= radius * radius;
        }
    }
}
