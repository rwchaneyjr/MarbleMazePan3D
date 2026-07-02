using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    public static class SpriteFactory
    {
        private static Sprite _roundedCard;
        private static Sprite _roundedButton;
        private static Texture2D _boardTexture;

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
