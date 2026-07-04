using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class HandVisualRules
    {
        private const int ThemeCount = 10;

        public static int PickHandTheme(int boardTheme, int slot, ISet<int> used = null)
        {
            int theme = ((boardTheme + slot) % ThemeCount + ThemeCount) % ThemeCount;
            if (used != null && used.Contains(theme))
            {
                for (int candidate = 0; candidate < ThemeCount; candidate++)
                {
                    if (!used.Contains(candidate))
                    {
                        return candidate;
                    }
                }
            }

            return theme;
        }

        public static void AssignLevelHandVisualThemes(LevelDefinition level)
        {
            level.HandVisualThemes.Clear();
            if (level.HandCards.Count == 0)
            {
                return;
            }

            for (int i = 0; i < level.HandCards.Count; i++)
            {
                CardKind kind = level.HandCards[i];
                if (kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    level.HandVisualThemes.Add(level.CreatureTheme);
                    continue;
                }

                level.HandVisualThemes.Add(-1);
            }
        }

        public static void EnsureDistinctHandVisuals(List<BoardCard> hand, int boardTheme)
        {
            for (int i = 0; i < hand.Count; i++)
            {
                BoardCard card = hand[i];
                if (card.Kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    card.VisualTheme = boardTheme;
                    hand[i] = card;
                }
            }
        }
    }
}
