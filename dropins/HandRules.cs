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

        private static int FlipFamilyKey(BoardCard card) => card.Kind switch
        {
            CardKind.DayCreature or CardKind.NightCreature => 1,
            CardKind.PositiveConstant or CardKind.NegativeConstant => 10 + card.Value,
            _ => 100 + (int)card.Kind
        };
    }
}
