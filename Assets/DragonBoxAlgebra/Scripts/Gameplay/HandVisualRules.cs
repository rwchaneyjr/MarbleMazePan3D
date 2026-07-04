using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class HandVisualRules
    {
        private static readonly int[] ThemeOffsets = { 3, 7, 5, 2, 4, 6, 8, 1, 9 };

        public static void AssignThemesForHand(LevelDefinition level)
        {
            level.HandVisualThemes.Clear();
            var usedThemes = new HashSet<int> { level.CreatureTheme };
            int creatureSlot = 0;

            foreach (CardKind kind in level.HandCards)
            {
                if (kind is CardKind.PositiveConstant or CardKind.NegativeConstant)
                {
                    level.HandVisualThemes.Add(-1);
                    continue;
                }

                if (kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    int theme = PickUnusedTheme(usedThemes, level.CreatureTheme, creatureSlot);
                    usedThemes.Add(theme);
                    creatureSlot++;
                    level.HandVisualThemes.Add(theme);
                    continue;
                }

                level.HandVisualThemes.Add(-1);
            }
        }

        public static void AssignDistinctVisuals(List<BoardCard> hand, int boardTheme)
        {
            if (hand.Count <= 1)
            {
                if (hand.Count == 1 && hand[0].Kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    BoardCard card = hand[0];
                    if (card.VisualTheme < 0)
                    {
                        card.VisualTheme = boardTheme;
                        hand[0] = card;
                    }
                }

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

                int theme = PickUnusedTheme(usedThemes, boardTheme, creatureSlot);
                usedThemes.Add(theme);
                creatureSlot++;
                card.VisualTheme = theme;
                hand[i] = card;
            }
        }

        public static bool WouldDuplicateVisual(IReadOnlyList<BoardCard> hand, int handIndex, BoardCard flipped)
        {
            if (flipped.Kind is not (CardKind.DayCreature or CardKind.NightCreature))
            {
                return false;
            }

            for (int i = 0; i < hand.Count; i++)
            {
                if (i == handIndex)
                {
                    continue;
                }

                BoardCard other = hand[i];
                if (other.Kind is not (CardKind.DayCreature or CardKind.NightCreature))
                {
                    continue;
                }

                if (other.VisualTheme == flipped.VisualTheme)
                {
                    return true;
                }
            }

            return false;
        }

        private static int PickUnusedTheme(HashSet<int> used, int boardTheme, int slot)
        {
            for (int i = 0; i < ThemeOffsets.Length; i++)
            {
                int candidate = (boardTheme + ThemeOffsets[(slot + i) % ThemeOffsets.Length]) % 10;
                if (!used.Contains(candidate))
                {
                    return candidate;
                }
            }

            for (int candidate = 0; candidate < 10; candidate++)
            {
                if (!used.Contains(candidate))
                {
                    return candidate;
                }
            }

            return (boardTheme + 1) % 10;
        }
    }
}
