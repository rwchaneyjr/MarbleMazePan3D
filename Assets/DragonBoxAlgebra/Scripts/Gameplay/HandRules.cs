using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class HandRules
    {
        public static void AssertAllHandCardsFlippable(IReadOnlyList<LevelDefinition> levels)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                LevelDefinition level = levels[i];
                for (int h = 0; h < level.HandCards.Count; h++)
                {
                    CardKind kind = level.HandCards[h];
                    if (!CardFlipRules.CanFlip(kind))
                    {
                        throw new System.InvalidOperationException(
                            $"Level {i + 1} ({level.Title}) has non-flippable hand card: {kind}.");
                    }
                }
            }
        }

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
            CardKind.DayCreature => 100 + card.Value,
            CardKind.NightCreature => 200 + card.Value,
            CardKind.PositiveConstant or CardKind.NegativeConstant => 300 + card.Value,
            _ => 1000 + (int)card.Kind
        };
    }
}
