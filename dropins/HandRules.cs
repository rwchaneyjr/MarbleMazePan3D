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
                    // Ch7 140–150 may use number PNGs (+/- constants) in the hand.
                    if (kind is CardKind.PositiveConstant or CardKind.NegativeConstant)
                    {
                        if (level.Chapter < 7)
                        {
                            throw new System.InvalidOperationException(
                                $"Level {i + 1} ({level.Title}) hand card {h} cannot be a number before Ch7.");
                        }

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

        /// <summary>
        /// Keep one hand slot per unique image. Drops duplicates and opposites
        /// (light/dark or +/- of the same creature, letter, theme, or number value).
        /// </summary>
        public static void DedupeFlipFamilies(List<BoardCard> hand)
        {
            var seen = new HashSet<int>();
            for (int i = hand.Count - 1; i >= 0; i--)
            {
                // Same image family: light/dark (or +/-) of one creature/letter/theme = one hand slot.
                if (!seen.Add(FlipFamilyKey(hand[i])))
                {
                    hand.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Same uniqueness rule for level definitions (HandCards / values / letters stay in sync).
        /// </summary>
        public static void DedupeLevelHandDefinitions(IReadOnlyList<LevelDefinition> levels)
        {
            foreach (LevelDefinition level in levels)
            {
                if (level.HandCards == null || level.HandCards.Count == 0)
                {
                    continue;
                }

                var seen = new HashSet<int>();
                for (int i = level.HandCards.Count - 1; i >= 0; i--)
                {
                    CardKind kind = level.HandCards[i];
                    int value = level.HandValues != null && i < level.HandValues.Count
                        ? level.HandValues[i]
                        : 1;
                    char letter = level.HandVariableLetters != null && i < level.HandVariableLetters.Count
                        ? level.HandVariableLetters[i]
                        : '\0';
                    int theme = kind is CardKind.DayCreature or CardKind.NightCreature
                        ? level.CreatureTheme
                        : -1;
                    int key = FlipFamilyKey(kind, value, letter, theme);
                    if (!seen.Add(key))
                    {
                        level.HandCards.RemoveAt(i);
                        if (level.HandValues != null && i < level.HandValues.Count)
                        {
                            level.HandValues.RemoveAt(i);
                        }

                        if (level.HandVariableLetters != null && i < level.HandVariableLetters.Count)
                        {
                            level.HandVariableLetters.RemoveAt(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// One key per unique hand image identity. Light and dark (or +/-) share a key.
        /// </summary>
        private static int FlipFamilyKey(BoardCard card) =>
            FlipFamilyKey(card.Kind, card.Value, card.VariableLetter, card.VisualTheme);

        private static int FlipFamilyKey(CardKind kind, int value, char variableLetter, int visualTheme)
        {
            int letterOffset = variableLetter != '\0'
                ? (char.ToLowerInvariant(variableLetter) - 'a' + 1) * 100
                : 0;
            int themeOffset = visualTheme >= 0 ? (visualTheme + 1) * 10_000 : 0;

            return kind switch
            {
                CardKind.DayCreature or CardKind.NightCreature =>
                    100 + letterOffset + themeOffset + value,
                CardKind.PositiveConstant or CardKind.NegativeConstant =>
                    300 + value,
                _ => 1000 + (int)kind
            };
        }
    }
}
