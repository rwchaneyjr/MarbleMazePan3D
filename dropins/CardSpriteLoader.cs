using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    /// <summary>
    /// Loads custom PNG/JPG creature art from Unity Resources folders.
    /// Drop images into Assets/DragonBoxAlgebra/Resources/CreatureSprites/
    /// </summary>
    public static class CardSpriteLoader
    {
        private static readonly Sprite[,] ThemedSprites = new Sprite[CreatureArt.ThemeCount, 2];
        private static Sprite _box;
        private static bool _initialized;

        private static readonly string[][] ThemeSlugs =
        {
            new[] { "fish", "turtle" },
            new[] { "bird", "owl" },
            new[] { "crab", "jelly", "jellyfish" },
            new[] { "butterfly", "bat", "wing" },
            new[] { "star", "moon" },
            new[] { "rabbit", "fox", "hopper" },
            new[] { "frog", "snake" },
            new[] { "sun", "storm", "weather" },
            new[] { "dragon", "flame" },
            new[] { "cat", "dog", "pet" }
        };

        public static void EnsureLoaded()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            LoadFromResources("CreatureSprites");
            LoadFromResources("Sprites");
            LoadFromResources("CardSprites");
            LoadFromResources("Cards");
        }

        public static Sprite ForCard(BoardCard card)
        {
            EnsureLoaded();
            int theme = ResolveTheme(card);

            return card.Kind switch
            {
                CardKind.DayCreature => GetThemed(theme, light: true),
                CardKind.NightCreature => GetThemed(theme, light: false),
                CardKind.Box => _box,
                _ => null
            };
        }

        public static bool HasCustomArt(int theme, bool light)
        {
            EnsureLoaded();
            return GetThemed(theme, light) != null;
        }

        private static int ResolveTheme(BoardCard card) =>
            card.VisualTheme >= 0 ? card.VisualTheme : CreatureArt.ThemeIndex;

        private static Sprite GetThemed(int theme, bool light)
        {
            int row = NormalizeTheme(theme);
            int col = light ? 0 : 1;
            return ThemedSprites[row, col];
        }

        private static void LoadFromResources(string folder)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(folder);
            Array.Sort(sprites, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            foreach (Sprite sprite in sprites)
            {
                RegisterByName(sprite);
            }
        }

        private static void RegisterByName(Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            string name = sprite.name.ToLowerInvariant();

            if (IsBox(name))
            {
                _box ??= sprite;
                return;
            }

            if (TryParseThemePairName(name, out int theme, out bool light))
            {
                RegisterThemed(theme, light, sprite, ScoreThemePairName(name));
                return;
            }

            int lightScore = ScoreLightSlug(name, out int lightTheme);
            if (lightScore > 0)
            {
                RegisterThemed(lightTheme, light: true, sprite, lightScore);
            }

            int darkScore = ScoreDarkSlug(name, out int darkTheme);
            if (darkScore > 0)
            {
                RegisterThemed(darkTheme, light: false, sprite, darkScore);
            }
        }

        private static bool TryParseThemePairName(string name, out int theme, out bool light)
        {
            theme = -1;
            light = false;

            if (!name.StartsWith("theme", StringComparison.Ordinal))
            {
                return false;
            }

            int underscore = name.IndexOf('_');
            if (underscore < 6)
            {
                return false;
            }

            string themeDigits = name.Substring(5, underscore - 5);
            if (!int.TryParse(themeDigits, out int parsedTheme))
            {
                return false;
            }

            string side = name.Substring(underscore + 1);
            if (side is "light" or "day")
            {
                theme = parsedTheme;
                light = true;
                return true;
            }

            if (side is "dark" or "night")
            {
                theme = parsedTheme;
                light = false;
                return true;
            }

            return false;
        }

        private static int ScoreThemePairName(string name)
        {
            if (name.Contains("theme"))
            {
                return 100;
            }

            return 0;
        }

        private static int ScoreLightSlug(string name, out int theme)
        {
            theme = -1;
            int bestScore = 0;

            for (int i = 0; i < ThemeSlugs.Length; i++)
            {
                int score = ScoreSlugMatch(name, ThemeSlugs[i][0], light: true);
                if (score > bestScore)
                {
                    bestScore = score;
                    theme = i;
                }
            }

            return bestScore;
        }

        private static int ScoreDarkSlug(string name, out int theme)
        {
            theme = -1;
            int bestScore = 0;

            for (int i = 0; i < ThemeSlugs.Length; i++)
            {
                for (int slugIndex = 1; slugIndex < ThemeSlugs[i].Length; slugIndex++)
                {
                    int score = ScoreSlugMatch(name, ThemeSlugs[i][slugIndex], light: false);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        theme = i;
                    }
                }
            }

            return bestScore;
        }

        private static int ScoreSlugMatch(string name, string slug, bool light)
        {
            string prefix = light ? "light" : "dark";
            string compact = prefix + slug;
            string underscored = prefix + "_" + slug;

            if (name == compact || name == underscored)
            {
                return 100;
            }

            if (name.Contains(underscored))
            {
                return 90;
            }

            if (name.Contains(compact))
            {
                return 80;
            }

            if (light && name.Contains("day") && name.Contains(slug))
            {
                return 70;
            }

            if (!light && name.Contains("night") && name.Contains(slug))
            {
                return 70;
            }

            if (light && name.Contains(slug) && !name.Contains("dark") && !name.Contains("night"))
            {
                return 50;
            }

            if (!light && name.Contains(slug) && !name.Contains("light") && !name.Contains("day"))
            {
                return 50;
            }

            return 0;
        }

        private static void RegisterThemed(int theme, bool light, Sprite sprite, int score)
        {
            int row = NormalizeTheme(theme);
            int col = light ? 0 : 1;
            Sprite current = ThemedSprites[row, col];

            if (current == null || score > ScoreExisting(current.name.ToLowerInvariant(), row, col))
            {
                ThemedSprites[row, col] = sprite;
            }
        }

        private static int ScoreExisting(string name, int theme, int col)
        {
            if (TryParseThemePairName(name, out _, out _))
            {
                return ScoreThemePairName(name);
            }

            return col == 0 ? ScoreLightSlug(name, out _) : ScoreDarkSlug(name, out _);
        }

        private static int NormalizeTheme(int theme) =>
            ((theme % CreatureArt.ThemeCount) + CreatureArt.ThemeCount) % CreatureArt.ThemeCount;

        private static bool IsBox(string name) =>
            name.Contains("box") || name.Contains("dragonbox");
    }
}
