using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class HandVisualRules
    {
        /// <summary>
        /// Hand creatures use the level theme (same animal as the board).
        /// Light/dark is CardKind — not a different creature per slot.
        /// </summary>
        public static void ApplyLevelThemeToHand(List<BoardCard> hand, int levelTheme)
        {
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i].Kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    BoardCard card = hand[i];
                    card.VisualTheme = levelTheme;
                    hand[i] = card;
                }
            }
        }
    }
}
