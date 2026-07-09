using System;
using DragonBoxAlgebra.Core;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    /// <summary>
    /// Loads custom creature art from Resources/CreatureSprites/.
    /// Each theme is one animal: lightFish + darkFish, lightTurtle + darkTurtle, etc.
    /// </summary>
    public static class CardSpriteLoader
    {
        private static readonly Sprite[,] ThemedSprites = new Sprite[CreatureArt.ThemeCount, 2];
        private static Sprite _box;
        private static bool _initialized;

        /// <summary>One slug per theme — light and dark use the same animal.</summary>
        private static readonly string[] CreatureSlugs =
        {
            "fish", "turtle", "clam", "dolphin", "eel", "lobster", "seahorse", "starfish"
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
                RegisterThemed(theme, light, sprite, 100);
                return;
            }

            if (TryParseCreatureSide(name, out string creature, out bool isLight))
            {
                int creatureTheme = ThemeForCreature(creature);
                if (creatureTheme >= 0)
                {
                    RegisterThemed(creatureTheme, isLight, sprite, 90);
                }
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

        private static bool TryParseCreatureSide(string name, out string creature, out bool light)
        {
            creature = string.Empty;
            light = false;

            if (TryStripPrefix(name, "light_", out creature) || TryStripPrefix(name, "light", out creature))
            {
                light = true;
                creature = NormalizeCreature(creature);
                return creature.Length > 0;
            }

            if (TryStripPrefix(name, "dark_", out creature) || TryStripPrefix(name, "dark", out creature))
            {
                light = false;
                creature = NormalizeCreature(creature);
                return creature.Length > 0;
            }

            if (TryStripPrefix(name, "day_", out creature) || TryStripPrefix(name, "day", out creature))
            {
                light = true;
                creature = NormalizeCreature(creature);
                return creature.Length > 0;
            }

            if (TryStripPrefix(name, "night_", out creature) || TryStripPrefix(name, "night", out creature))
            {
                light = false;
                creature = NormalizeCreature(creature);
                return creature.Length > 0;
            }

            return false;
        }

        private static bool TryStripPrefix(string name, string prefix, out string remainder)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                remainder = name.Substring(prefix.Length);
                return true;
            }

            remainder = string.Empty;
            return false;
        }

        private static string NormalizeCreature(string creature)
        {
            if (string.IsNullOrEmpty(creature))
            {
                return creature;
            }

            if (creature.EndsWith("png", StringComparison.Ordinal))
            {
                creature = creature.Substring(0, creature.Length - 3);
            }

            if (creature == "eeel")
            {
                return "eel";
            }

            if (creature == "seahors")
            {
                return "seahorse";
            }

            return creature;
        }

        private static int ThemeForCreature(string creature)
        {
            if (string.IsNullOrEmpty(creature))
            {
                return -1;
            }

            for (int i = 0; i < CreatureSlugs.Length; i++)
            {
                if (creature == CreatureSlugs[i])
                {
                    return i;
                }
            }

            for (int i = 0; i < CreatureSlugs.Length; i++)
            {
                if (creature.Contains(CreatureSlugs[i]) || CreatureSlugs[i].Contains(creature))
                {
                    return i;
                }
            }

            return -1;
        }

        private static void RegisterThemed(int theme, bool light, Sprite sprite, int score)
        {
            int row = NormalizeTheme(theme);
            int col = light ? 0 : 1;
            Sprite current = ThemedSprites[row, col];

            if (current == null || score >= 90)
            {
                ThemedSprites[row, col] = sprite;
            }
        }

        private static int NormalizeTheme(int theme) =>
            ((theme % CreatureArt.ThemeCount) + CreatureArt.ThemeCount) % CreatureArt.ThemeCount;

        private static bool IsBox(string name) =>
            name.Contains("box") || name.Contains("dragonbox");
    }
}
