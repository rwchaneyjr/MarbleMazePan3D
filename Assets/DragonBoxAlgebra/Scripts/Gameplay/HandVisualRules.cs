using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
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
                if (candidate == boardTheme || (used != null && used.Contains(candidate)))
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

            var usedThemes = new HashSet<int>();
            int extraCreatureSlot = 0;

            for (int i = 0; i < level.HandCards.Count; i++)
            {
                CardKind kind = level.HandCards[i];
                if (kind is CardKind.PositiveConstant or CardKind.NegativeConstant)
                {
                    level.HandVisualThemes.Add(-1);
                    continue;
                }

                if (kind is not (CardKind.DayCreature or CardKind.NightCreature))
                {
                    level.HandVisualThemes.Add(-1);
                    continue;
                }

                int theme = extraCreatureSlot == 0
                    ? level.CreatureTheme
                    : PickHandTheme(level.CreatureTheme, extraCreatureSlot, usedThemes);
                level.HandVisualThemes.Add(theme);
                usedThemes.Add(theme);
                extraCreatureSlot++;
            }
        }

        public static void EnsureDistinctHandVisuals(List<BoardCard> hand, int boardTheme)
        {
            if (hand.Count < 2)
            {
                if (hand.Count == 1 && hand[0].Kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    BoardCard card = hand[0];
                    card.VisualTheme = boardTheme;
                    hand[0] = card;
                }

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
            for (int slot = 0; slot < creatureIndices.Count; slot++)
            {
                int handIndex = creatureIndices[slot];
                BoardCard card = hand[handIndex];
                int theme = slot == 0
                    ? boardTheme
                    : PickHandTheme(boardTheme, slot, usedThemes);
                card.VisualTheme = theme;
                hand[handIndex] = card;
                usedThemes.Add(theme);
            }
        }
    }
}
