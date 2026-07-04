using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class HandVisualRules
    {
        // Offsets chosen so hand creatures look clearly different (winged vs weather vs fish, etc.).
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
                    theme = PickHandTheme(boardTheme, creatureSlot, usedThemes);
                }

                usedThemes.Add(theme);
                creatureSlot++;
                card.VisualTheme = theme;
                hand[i] = card;
            }
        }
    }
}
