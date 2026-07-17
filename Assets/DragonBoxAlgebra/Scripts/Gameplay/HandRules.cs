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

        /// <summary>
        /// Ch5+ hand cards must be variable creatures with a letter so tap can flip +/-.
        /// </summary>
        public static void AssertVariableHandCardsFlippable(IReadOnlyList<LevelDefinition> levels)
        {
            for (int i = ChapterLevelGenerator.Chapter5StartLevel - 1; i < levels.Count; i++)
            {
                LevelDefinition level = levels[i];
                if (level.Chapter < 5)
                {
                    continue;
                }

                for (int h = 0; h < level.HandCards.Count; h++)
                {
                    CardKind kind = level.HandCards[h];
                    // Number constants (+/- images) are valid flippable hand tiles on Ch7 129–139.
                    if (kind is CardKind.PositiveConstant or CardKind.NegativeConstant)
                    {
                        continue;
                    }

                    if (kind is not (CardKind.DayCreature or CardKind.NightCreature))
                    {
                        throw new System.InvalidOperationException(
                            $"Level {i + 1} ({level.Title}) hand card {h} must be a flippable variable creature, not {kind}.");
                    }

                    char letter = h < level.HandVariableLetters.Count
                        ? level.HandVariableLetters[h]
                        : '\0';
                    if (letter == '\0' && level.Chapter < 7)
                    {
                        throw new System.InvalidOperationException(
                            $"Level {i + 1} ({level.Title}) hand card {h} needs a variable letter for +/- flip.");
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

        private static int FlipFamilyKey(BoardCard card)
        {
            int letterOffset = card.VariableLetter != '\0'
                ? (char.ToLowerInvariant(card.VariableLetter) - 'a') * 10
                : 0;
            return card.Kind switch
            {
                CardKind.DayCreature => 100 + letterOffset + card.Value,
                CardKind.NightCreature => 200 + letterOffset + card.Value,
                CardKind.PositiveConstant or CardKind.NegativeConstant => 300 + card.Value,
                _ => 1000 + (int)card.Kind
            };
        }
    }
}
