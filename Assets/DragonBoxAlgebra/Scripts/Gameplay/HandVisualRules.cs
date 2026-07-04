using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// When hand has 2+ tiles: hand creature pictures must all differ, and at least one
    /// must use the board theme (same image family as creatures beside the red box).
    /// </summary>
    public static class HandVisualRules
    {
        private static readonly int[] ThemeOffsets = { 1, 7, 4, 6, 8, 3, 0, 9, 2, 5 };

        public static int PickHandTheme(int boardTheme, int slot, ISet<int> used = null)
        {
            for (int i = 0; i < ThemeOffsets.Length; i++)
            {
                int candidate = (boardTheme + ThemeOffsets[(slot + i) % ThemeOffsets.Length]) % 10;
                if (candidate == boardTheme)
                {
                    continue;
                }

                if (used != null && used.Contains(candidate))
                {
                    continue;
                }

                return candidate;
            }

            for (int candidate = 0; candidate < 10; candidate++)
            {
                if (candidate == boardTheme)
                {
                    continue;
                }

                if (used != null && used.Contains(candidate))
                {
                    continue;
                }

                return candidate;
            }

            return (boardTheme + 1) % 10;
        }

        public static void AssignLevelHandVisualThemes(LevelDefinition level)
        {
            level.HandVisualThemes.Clear();
            if (level.HandCards.Count == 0)
            {
                return;
            }

            if (level.HandCards.Count < 2)
            {
                foreach (CardKind kind in level.HandCards)
                {
                    level.HandVisualThemes.Add(
                        kind is CardKind.DayCreature or CardKind.NightCreature ? level.CreatureTheme : -1);
                }

                return;
            }

            var creatureSlots = new List<int>();
            for (int i = 0; i < level.HandCards.Count; i++)
            {
                CardKind kind = level.HandCards[i];
                if (kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    creatureSlots.Add(i);
                }
            }

            var themes = new int[level.HandCards.Count];
            for (int i = 0; i < themes.Length; i++)
            {
                themes[i] = -1;
            }

            if (creatureSlots.Count == 0)
            {
                level.HandVisualThemes.AddRange(themes);
                return;
            }

            var usedThemes = new HashSet<int>();
            for (int slot = 0; slot < creatureSlots.Count; slot++)
            {
                int handIndex = creatureSlots[slot];
                int theme = slot == 0
                    ? level.CreatureTheme
                    : PickHandTheme(level.CreatureTheme, slot, usedThemes);
                themes[handIndex] = theme;
                usedThemes.Add(theme);
            }

            foreach (int theme in themes)
            {
                level.HandVisualThemes.Add(theme);
            }
        }

        public static void EnsureDistinctHandVisuals(List<BoardCard> hand, int boardTheme)
        {
            if (hand.Count < 2)
            {
                return;
            }

            var creatureIndices = new List<int>();
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i].Kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    creatureIndices.Add(i);
                }
            }

            if (creatureIndices.Count == 0)
            {
                return;
            }

            var usedThemes = new HashSet<int>();

            if (creatureIndices.Count == 1)
            {
                BoardCard card = hand[creatureIndices[0]];
                card.VisualTheme = boardTheme;
                hand[creatureIndices[0]] = card;
                return;
            }

            for (int slot = 0; slot < creatureIndices.Count; slot++)
            {
                int handIndex = creatureIndices[slot];
                BoardCard card = hand[handIndex];
                int theme = slot == 0
                    ? boardTheme
                    : PickHandTheme(boardTheme, slot, usedThemes);

                if (usedThemes.Contains(theme) && slot > 0)
                {
                    theme = PickHandTheme(boardTheme, slot + 1, usedThemes);
                }

                card.VisualTheme = theme;
                hand[handIndex] = card;
                usedThemes.Add(theme);
            }
        }
    }
}
