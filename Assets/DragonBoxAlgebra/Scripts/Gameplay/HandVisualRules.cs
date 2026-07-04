using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class HandVisualRules
    {
        private static readonly int[] ThemeOffsets = { 3, 7, 5, 2, 4, 6, 8, 1, 9 };

        public static void EnsureDistinctHandVisuals(List<BoardCard> hand, int boardTheme)
        {
            if (hand.Count <= 1)
            {
                return;
            }

            var usedThemes = new HashSet<int> { boardTheme };
            int creatureSlot = 0;

            for (int i = 0; i < hand.Count; i++)
            {
                BoardCard card = hand[i];
                if (card.Kind is not (CardKind.DayCreature or CardKind.NightCreature))
                {
                    continue;
                }

                int theme = card.VisualTheme;
                if (theme < 0 || theme == boardTheme || usedThemes.Contains(theme))
                {
                    theme = PickUnusedTheme(usedThemes, boardTheme, creatureSlot);
                }

                usedThemes.Add(theme);
                creatureSlot++;
                card.VisualTheme = theme;
                hand[i] = card;
            }
        }

        private static int PickUnusedTheme(HashSet<int> used, int boardTheme, int slot)
        {
            for (int i = 0; i < ThemeOffsets.Length; i++)
            {
                int candidate = (boardTheme + ThemeOffsets[(slot + i) % ThemeOffsets.Length]) % 10;
                if (candidate != boardTheme && !used.Contains(candidate))
                {
                    return candidate;
                }
            }

            for (int candidate = 0; candidate < 10; candidate++)
            {
                if (candidate != boardTheme && !used.Contains(candidate))
                {
                    return candidate;
                }
            }

            return (boardTheme + 1) % 10;
        }
    }
}
