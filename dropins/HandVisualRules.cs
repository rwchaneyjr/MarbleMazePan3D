using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class HandVisualRules
    {
        public static void EnsureDistinctHandVisuals(List<BoardCard> hand, int boardTheme)
        {
            if (hand.Count <= 1)
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

            List<int> themes = ThemeAssignment.DistinctThemes(creatureIndices.Count, boardTheme);
            for (int i = 0; i < creatureIndices.Count; i++)
            {
                int index = creatureIndices[i];
                BoardCard card = hand[index];
                card.VisualTheme = themes[i];
                hand[index] = card;
            }
        }
    }
}
