using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelGenerator
    {
        private const int GeneratedCount = 24;
        private const int TwoCardFromIndex = 12;
        private const int ThreeCardFromIndex = 18;

        public static IReadOnlyList<LevelDefinition> GenerateAll(int seed = 20260703)
        {
            var levels = new List<LevelDefinition>();
            levels.AddRange(TutorialLevels());
            levels.AddRange(GenerateProcedural(seed, levels.Count));
            return levels;
        }

        public static int HandCountForLevelIndex(int levelIndex)
        {
            if (levelIndex < TwoCardFromIndex)
            {
                return 1;
            }

            if (levelIndex < ThreeCardFromIndex)
            {
                return 2;
            }

            return 3;
        }

        private static IEnumerable<LevelDefinition> TutorialLevels()
        {
            yield return CreatureLevel("Isolate the Box", 0, 1, 1,
                new[] { CardKind.Box, CardKind.DayCreature },
                new[] { CardKind.DayCreature },
                CardKind.NightCreature, 1, 2, 1);

            yield return CreatureLevel("Balance Both Sides", 1, 1, 1,
                new[] { CardKind.Box, CardKind.NightCreature },
                new[] { CardKind.PositiveConstant },
                CardKind.DayCreature, 1, 2, 1);

            yield return CreatureLevel("Clear the Creatures", 2, 1, 1,
                new[] { CardKind.Box, CardKind.DayCreature },
                Array.Empty<CardKind>(),
                CardKind.NightCreature, 1, 1, 1);

            yield return DiceLevel("Cancel the Dice", 3, 1, 1,
                new[] { CardKind.Box, CardKind.PositiveConstant },
                new[] { CardKind.PositiveConstant, CardKind.NegativeConstant },
                CardKind.NegativeConstant, 1, 2, 1);

            yield return CreatureLevel("Fish and Turtle", 0, 1, 1,
                new[] { CardKind.Box, CardKind.DayCreature },
                new[] { CardKind.NightCreature },
                CardKind.NightCreature, 1, 2, 1);

            yield return DiceLevel("Final Balance", 4, 1, 1,
                new[] { CardKind.Box, CardKind.NegativeConstant },
                new[] { CardKind.PositiveConstant },
                CardKind.PositiveConstant, 1, 2, 1);
        }

        private static List<LevelDefinition> GenerateProcedural(int seed, int startIndex)
        {
            var rng = new Random(seed);
            var levels = new List<LevelDefinition>();
            string[] placeNames =
            {
                "Reef", "Coral", "Lagoon", "Tide", "Shoal", "Harbor", "Cove", "Current",
                "Forest", "Meadow", "Grove", "Summit", "Valley", "Glade", "Dune", "Canyon",
                "Cloud", "Star", "Moon", "Comet", "Nova", "Orbit", "Aurora", "Eclipse"
            };

            for (int i = 0; i < GeneratedCount; i++)
            {
                int levelIndex = startIndex + i;
                int handCount = HandCountForLevelIndex(levelIndex);
                int theme = i % 10;
                int value = 1 + (i / 10);
                int pattern = i % 5;
                string place = placeNames[i % placeNames.Length];
                string handLabel = handCount == 1 ? string.Empty : handCount == 2 ? " (2 tiles)" : " (3 tiles)";
                string title = $"{place} Puzzle {i + 1}{handLabel}";

                LevelDefinition level = pattern switch
                {
                    0 => CreatureLevel(title, theme, value, handCount,
                        new[] { CardKind.Box, CardKind.DayCreature },
                        new[] { CardKind.DayCreature },
                        CardKind.NightCreature, value, handCount * 2, handCount),
                    1 => CreatureLevel(title, theme, value, handCount,
                        new[] { CardKind.Box, CardKind.NightCreature },
                        new[] { CardKind.NightCreature },
                        CardKind.DayCreature, value, handCount * 2, handCount),
                    2 => CreatureLevel(title, theme, value, handCount,
                        new[] { CardKind.Box, CardKind.DayCreature },
                        Array.Empty<CardKind>(),
                        CardKind.NightCreature, value, handCount * 2, handCount),
                    3 => CreatureLevel(title, theme, value, handCount,
                        new[] { CardKind.Box, CardKind.DayCreature },
                        new[] { CardKind.NightCreature },
                        CardKind.NightCreature, value, handCount * 2, handCount),
                    _ => DiceLevel(title, theme, value, handCount,
                        new[] { CardKind.Box, CardKind.PositiveConstant },
                        new[] { CardKind.PositiveConstant, CardKind.NegativeConstant },
                        CardKind.NegativeConstant, value, handCount * 2, handCount)
                };

                levels.Add(level);

                if (rng.NextDouble() < 0.15)
                {
                    level.ParMoves++;
                }
            }

            return levels;
        }

        private static LevelDefinition CreatureLevel(string title, int theme, int value, int handCount,
            CardKind[] left, CardKind[] right, CardKind primaryHand, int handValue, int parMoves, int parCards)
        {
            var level = new LevelDefinition
            {
                Title = title,
                CreatureTheme = theme,
                LeftCards = new List<CardKind>(left),
                RightCards = new List<CardKind>(right),
                LeftValues = ValuesForCreatures(left, value),
                RightValues = ValuesForCreatures(right, value),
                ParMoves = parMoves,
                ParCards = parCards
            };
            FillHand(level, handCount, primaryHand, handValue, value, theme);
            return level;
        }

        private static LevelDefinition DiceLevel(string title, int theme, int value, int handCount,
            CardKind[] left, CardKind[] right, CardKind primaryHand, int handValue, int parMoves, int parCards)
        {
            var level = new LevelDefinition
            {
                Title = title,
                CreatureTheme = theme,
                LeftCards = new List<CardKind>(left),
                RightCards = new List<CardKind>(right),
                LeftValues = ValuesForDice(left, value),
                RightValues = ValuesForDice(right, value),
                ParMoves = parMoves,
                ParCards = parCards
            };
            FillHand(level, handCount, primaryHand, handValue, value, theme);
            return level;
        }

        private static void FillHand(LevelDefinition level, int handCount, CardKind primaryHand, int handValue, int value,
            int levelTheme)
        {
            level.HandCards.Clear();
            level.HandValues.Clear();
            level.HandVisualThemes.Clear();

            if (handCount <= 1)
            {
                level.HandCards.Add(primaryHand);
                level.HandValues.Add(handValue);
                level.HandVisualThemes.Add(levelTheme);
                return;
            }

            int companionTheme = (levelTheme + 3) % 10;

            if (handCount == 2)
            {
                // e.g. sun (level theme) + turtle (companion theme dark side)
                level.HandCards.Add(CardKind.DayCreature);
                level.HandValues.Add(value);
                level.HandVisualThemes.Add(levelTheme);

                level.HandCards.Add(CardKind.NightCreature);
                level.HandValues.Add(value);
                level.HandVisualThemes.Add(companionTheme);
                return;
            }

            // 3 tiles: e.g. sun + dice + fish — three distinct images
            level.HandCards.Add(CardKind.DayCreature);
            level.HandValues.Add(value);
            level.HandVisualThemes.Add(levelTheme);

            CardKind diceKind = primaryHand is CardKind.PositiveConstant or CardKind.NegativeConstant
                ? primaryHand
                : CardKind.NegativeConstant;
            level.HandCards.Add(diceKind);
            level.HandValues.Add(value);
            level.HandVisualThemes.Add(-1);

            level.HandCards.Add(CardKind.DayCreature);
            level.HandValues.Add(value);
            level.HandVisualThemes.Add(companionTheme);
        }

        private static List<int> ValuesForCreatures(CardKind[] cards, int value)
        {
            var values = new List<int>();
            foreach (CardKind kind in cards)
            {
                values.Add(kind is CardKind.DayCreature or CardKind.NightCreature ? value : 1);
            }

            return values;
        }

        private static List<int> ValuesForDice(CardKind[] cards, int value)
        {
            var values = new List<int>();
            foreach (CardKind kind in cards)
            {
                values.Add(kind is CardKind.PositiveConstant or CardKind.NegativeConstant ? value : 1);
            }

            return values;
        }
    }
}
