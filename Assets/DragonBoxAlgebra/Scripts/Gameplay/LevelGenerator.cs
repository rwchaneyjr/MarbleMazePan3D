using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelGenerator
    {
        private const int GeneratedCount = 24;

        public static IReadOnlyList<LevelDefinition> GenerateAll(int seed = 20260703)
        {
            var levels = new List<LevelDefinition>();
            levels.AddRange(TutorialLevels());
            levels.AddRange(GenerateProcedural(seed));
            return levels;
        }

        private static IEnumerable<LevelDefinition> TutorialLevels()
        {
            yield return CreatureLevel("Isolate the Box", 0, 1,
                new[] { CardKind.Box, CardKind.DayCreature },
                new[] { CardKind.DayCreature },
                CardKind.NightCreature, 1, 2, 1);

            yield return CreatureLevel("Balance Both Sides", 1, 1,
                new[] { CardKind.Box, CardKind.NightCreature },
                new[] { CardKind.PositiveConstant },
                CardKind.DayCreature, 1, 2, 1);

            yield return CreatureLevel("Clear the Creatures", 2, 1,
                new[] { CardKind.Box, CardKind.DayCreature },
                Array.Empty<CardKind>(),
                CardKind.NightCreature, 1, 1, 1);

            yield return DiceLevel("Cancel the Dice", 3, 1,
                new[] { CardKind.Box, CardKind.PositiveConstant },
                new[] { CardKind.PositiveConstant, CardKind.NegativeConstant },
                CardKind.NegativeConstant, 1, 2, 1);

            yield return CreatureLevel("Fish and Turtle", 0, 1,
                new[] { CardKind.Box, CardKind.DayCreature },
                new[] { CardKind.NightCreature },
                CardKind.NightCreature, 1, 2, 1);

            yield return DiceLevel("Final Balance", 4, 1,
                new[] { CardKind.Box, CardKind.NegativeConstant },
                new[] { CardKind.PositiveConstant },
                CardKind.PositiveConstant, 1, 2, 1);
        }

        private static List<LevelDefinition> GenerateProcedural(int seed)
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
                int theme = i % 10;
                int value = 1 + (i / 10);
                int pattern = i % 5;
                string place = placeNames[i % placeNames.Length];
                string title = $"{place} Puzzle {i + 1}";

                levels.Add(pattern switch
                {
                    0 => CreatureLevel(title, theme, value,
                        new[] { CardKind.Box, CardKind.DayCreature },
                        new[] { CardKind.DayCreature },
                        CardKind.NightCreature, value, 2, 1),
                    1 => CreatureLevel(title, theme, value,
                        new[] { CardKind.Box, CardKind.NightCreature },
                        new[] { CardKind.NightCreature },
                        CardKind.DayCreature, value, 2, 1),
                    2 => CreatureLevel(title, theme, value,
                        new[] { CardKind.Box, CardKind.DayCreature },
                        Array.Empty<CardKind>(),
                        CardKind.NightCreature, value, 1, 1),
                    3 => CreatureLevel(title, theme, value,
                        new[] { CardKind.Box, CardKind.DayCreature },
                        new[] { CardKind.NightCreature },
                        CardKind.NightCreature, value, 2, 1),
                    _ => DiceLevel(title, theme, value,
                        new[] { CardKind.Box, CardKind.PositiveConstant },
                        new[] { CardKind.PositiveConstant, CardKind.NegativeConstant },
                        CardKind.NegativeConstant, value, 2, 1)
                });

                if (rng.NextDouble() < 0.15)
                {
                    LevelDefinition last = levels[levels.Count - 1];
                    last.ParMoves++;
                }
            }

            return levels;
        }

        private static LevelDefinition CreatureLevel(string title, int theme, int value,
            CardKind[] left, CardKind[] right, CardKind hand, int handValue, int parMoves, int parCards)
        {
            return new LevelDefinition
            {
                Title = title,
                CreatureTheme = theme,
                LeftCards = new List<CardKind>(left),
                RightCards = new List<CardKind>(right),
                LeftValues = ValuesForCreatures(left, value),
                RightValues = ValuesForCreatures(right, value),
                HandCards = new List<CardKind> { hand },
                HandValues = new List<int> { handValue },
                ParMoves = parMoves,
                ParCards = parCards
            };
        }

        private static LevelDefinition DiceLevel(string title, int theme, int value,
            CardKind[] left, CardKind[] right, CardKind hand, int handValue, int parMoves, int parCards)
        {
            return new LevelDefinition
            {
                Title = title,
                CreatureTheme = theme,
                LeftCards = new List<CardKind>(left),
                RightCards = new List<CardKind>(right),
                LeftValues = ValuesForDice(left, value),
                RightValues = ValuesForDice(right, value),
                HandCards = new List<CardKind> { hand },
                HandValues = new List<int> { handValue },
                ParMoves = parMoves,
                ParCards = parCards
            };
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
