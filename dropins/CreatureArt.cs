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

        public static int ResolveTheme(BoardCard card) =>
            card.VisualTheme >= 0 ? card.VisualTheme : _themeIndex;

        public static Sprite LightSprite(BoardCard card) =>
            SpriteFactory.LightCreature(ResolveTheme(card), card.Value);

        public static Sprite DarkSprite(BoardCard card) =>
            SpriteFactory.DarkCreature(ResolveTheme(card), card.Value);

        public static string LightEmojiFor(BoardCard card) => LightEmojiForTheme(ResolveTheme(card));

        public static string DarkEmojiFor(BoardCard card) => DarkEmojiForTheme(ResolveTheme(card));

        public static string LightEmoji => LightEmojiForTheme(_themeIndex);

        public static string DarkEmoji => DarkEmojiForTheme(_themeIndex);

        private static int NormalizeTheme(int theme) =>
            ((theme % ThemeCount) + ThemeCount) % ThemeCount;

        public static string LightEmojiForTheme(int theme) => NormalizeTheme(theme) switch
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

        public static string DarkEmojiForTheme(int theme) => NormalizeTheme(theme) switch
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

        public static string PngCreatureNameFor(int theme)
        {
            int row = ((theme % PngCreatureNames.Length) + PngCreatureNames.Length) % PngCreatureNames.Length;
            return PngCreatureNames[row];
        }

        public static string ExpectedLightPng(int theme) =>
            "light" + PngCreatureNameFor(theme).Replace(" ", string.Empty);

        public static string ExpectedDarkPng(int theme) =>
            "dark" + PngCreatureNameFor(theme).Replace(" ", string.Empty);

        private static readonly string[] PngCreatureNames =
        {
            "Fish", "Turtle", "Clam", "Dolphin", "Eel", "Lobster", "Sea Horse", "Starfish"
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
