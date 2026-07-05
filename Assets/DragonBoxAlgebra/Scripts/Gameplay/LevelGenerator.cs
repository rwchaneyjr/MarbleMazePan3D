using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelGenerator
    {
        private const int GeneratedCount = 24;
        private const int TwoCardFromIndex = 8;
        private const int ThreeCardFromIndex = 12;

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

                if (LevelSolvabilityRules.ShouldConfigureBoxSide(handCount))
                {
                    bool diceLevel = pattern == 4;
                    if (LevelSolvabilityRules.IsExtraPuzzleLevel(levelIndex))
                    {
                        int extraIndex = levelIndex - LevelSolvabilityRules.ExtraPuzzleFromIndex;
                        int otherSideCount = LevelSolvabilityRules.OtherSideCountForExtraLevel(extraIndex);
                        LevelSolvabilityRules.ConfigureExtraPuzzleLevel(level, handCount, diceLevel, value,
                            otherSideCount);
                        level.ParMoves = handCount + otherSideCount + 1;
                        level.ParCards = handCount;
                    }
                    else
                    {
                        LevelSolvabilityRules.ConfigureStandardSolvableLevel(level, handCount, diceLevel, value);
                        level.ParMoves = handCount + 1;
                        level.ParCards = handCount;
                    }
                }

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
            FillHand(level, handCount, primaryHand, handValue, value, diceLevel: false);
            if (handCount >= 2)
            {
                LevelSolvabilityRules.ConfigureStandardSolvableLevel(level, handCount, diceLevel: false, value);
            }
            else
            {
                BoardVisualRules.AssignDistinctSideThemes(level);
            }

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
            FillHand(level, handCount, primaryHand, handValue, value, diceLevel: true);
            if (handCount >= 2)
            {
                LevelSolvabilityRules.ConfigureStandardSolvableLevel(level, handCount, diceLevel: true, value);
            }
            else
            {
                BoardVisualRules.AssignDistinctSideThemes(level);
            }

            return level;
        }

        private static void FillHand(LevelDefinition level, int handCount, CardKind primaryHand, int handValue, int value,
            bool diceLevel)
        {
            level.HandCards.Clear();
            level.HandValues.Clear();
            level.HandVisualThemes.Clear();

            CardKind solver = HandCompositionRules.PrimarySolverCard(level, diceLevel, primaryHand);

            if (handCount <= 1)
            {
                level.HandCards.Add(solver);
                level.HandValues.Add(diceLevel && solver is CardKind.PositiveConstant or CardKind.NegativeConstant
                    ? value
                    : handValue);
                HandVisualRules.AssignLevelHandVisualThemes(level);
                return;
            }

            for (int i = 0; i < handCount; i++)
            {
                level.HandCards.Add(solver);
                level.HandValues.Add(diceLevel && solver is CardKind.PositiveConstant or CardKind.NegativeConstant
                    ? value
                    : value);
            }
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
