using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class HandRules
    {
        public static void DedupeFlipFamilies(List<BoardCard> hand)
        {
            var seen = new HashSet<int>();
            for (int i = hand.Count - 1; i >= 0; i--)
            {
                if (!seen.Add(FlipFamilyKey(hand[i])))
                {
                    hand.RemoveAt(i);
                }
            }
        }

        private static int FlipFamilyKey(BoardCard card)
        {
            int themePart = card.VisualTheme >= 0 ? card.VisualTheme : 0;
            return card.Kind switch
            {
                CardKind.DayCreature => 100 + card.Value * 20 + themePart,
                CardKind.NightCreature => 200 + card.Value * 20 + themePart,
                CardKind.PositiveConstant or CardKind.NegativeConstant => 300 + card.Value,
                _ => 1000 + (int)card.Kind
            };
        }
    }
}
