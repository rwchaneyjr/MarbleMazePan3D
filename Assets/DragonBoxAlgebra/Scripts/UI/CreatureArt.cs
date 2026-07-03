using DragonBoxAlgebra.Core;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    public static class CreatureArt
    {
        public const int ThemeCount = 10;

        private static int _themeIndex;

        public static int ThemeIndex => _themeIndex;

        public static void SetTheme(int themeIndex)
        {
            _themeIndex = ((themeIndex % ThemeCount) + ThemeCount) % ThemeCount;
        }

        public static Sprite LightSprite(BoardCard card) =>
            SpriteFactory.LightCreature(_themeIndex, card.Value);

        public static Sprite DarkSprite(BoardCard card) =>
            SpriteFactory.DarkCreature(_themeIndex, card.Value);

        public static string LightEmoji => _themeIndex switch
        {
            0 => "🐠",
            1 => "🐦",
            2 => "🦀",
            3 => "🦋",
            4 => "⭐",
            5 => "🐰",
            6 => "🐝",
            7 => "☀️",
            8 => "🐉",
            _ => "🐱"
        };

        public static string DarkEmoji => _themeIndex switch
        {
            0 => "🐢",
            1 => "🦉",
            2 => "🪼",
            3 => "🦇",
            4 => "🌙",
            5 => "🦊",
            6 => "🐍",
            7 => "🌧️",
            8 => "🔥",
            _ => "🐶"
        };

        public static string ThemeName => _themeIndex switch
        {
            0 => "Fish & Turtle",
            1 => "Bird & Owl",
            2 => "Crab & Jelly",
            3 => "Butterfly & Bat",
            4 => "Star & Moon",
            5 => "Rabbit & Fox",
            6 => "Bee & Snake",
            7 => "Sun & Storm",
            8 => "Dragon & Flame",
            _ => "Cat & Dog"
        };
    }
}
