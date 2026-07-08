using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelGenerator
    {
        public const int TotalLevels = 50;
        private const int HandPlayFromIndex = 36;
        private const int MirroredHandFromIndex = 43;

        public static IReadOnlyList<LevelDefinition> GenerateAll(int seed = 20260703)
        {
            var levels = new List<LevelDefinition>(TotalLevels);
            levels.AddRange(GenerateMergeIntroLevels());
            levels.AddRange(GenerateHandPlayLevels(seed));
            return levels;
        }

        public static int HandCountForLevelIndex(int levelIndex)
        {
            if (levelIndex < HandPlayFromIndex)
            {
                return 0;
            }

            int handSection = levelIndex - HandPlayFromIndex;
            if (handSection >= MirroredHandFromIndex - HandPlayFromIndex)
            {
                return 1;
            }

            return handSection switch
            {
                0 or 1 or 2 or 3 => 1,
                _ => 2
            };
        }

        private static IEnumerable<LevelDefinition> GenerateMergeIntroLevels()
        {
            for (int i = 0; i < HandPlayFromIndex; i++)
            {
                int theme = i % 10;
                yield return BuildMergeIntroLevel(i, theme);
            }
        }

        /// <summary>
        /// Levels 1-36: pre-placed light/dark pairs to combine on the board (empty hand).
        /// 1-7 left pair beside box, 8-15 right pair beside box,
        /// 16-25 pair on right (box on left), 26-36 two pairs on left.
        /// </summary>
        private static LevelDefinition BuildMergeIntroLevel(int index, int theme)
        {
            int display = index + 1;
            string title;

            CardKind[] left;
            CardKind[] right;
            int parMoves;

            if (index < 7)
            {
                title = $"Pair on Left {display}";
                left = new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature };
                right = Array.Empty<CardKind>();
                parMoves = 2;
            }
            else if (index < 15)
            {
                title = $"Pair on Right {display}";
                left = Array.Empty<CardKind>();
                right = new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature };
                parMoves = 2;
            }
            else if (index < 25)
            {
                title = $"Match on Right {display}";
                left = new[] { CardKind.Box };
                right = new[] { CardKind.DayCreature, CardKind.NightCreature };
                parMoves = 2;
            }
            else
            {
                title = $"Double Pair on Left {display}";
                left = new[]
                {
                    CardKind.Box,
                    CardKind.DayCreature,
                    CardKind.NightCreature,
                    CardKind.DayCreature,
                    CardKind.NightCreature
                };
                right = Array.Empty<CardKind>();
                parMoves = 3;
            }

            var level = new LevelDefinition
            {
                Title = title,
                CreatureTheme = theme,
                LeftCards = new List<CardKind>(left),
                RightCards = new List<CardKind>(right),
                ParMoves = parMoves,
                ParCards = 0
            };

            level.LeftValues = ValuesForCreatures(left, 1);
            level.RightValues = ValuesForCreatures(right, 1);
            AssignMatchingPairThemes(level);
            return level;
        }

        private static List<LevelDefinition> GenerateHandPlayLevels(int seed)
        {
            var rng = new Random(seed);
            var levels = new List<LevelDefinition>();
            string[] placeNames =
            {
                "Reef", "Coral", "Lagoon", "Tide", "Shoal", "Harbor", "Cove",
                "Forest", "Meadow", "Grove", "Summit", "Valley", "Glade", "Dune"
            };

            for (int i = 0; i < TotalLevels - HandPlayFromIndex; i++)
            {
                int levelIndex = HandPlayFromIndex + i;
                int handCount = HandCountForLevelIndex(levelIndex);
                int theme = (levelIndex + 3) % 10;
                int value = 1 + (i / 5);
                int pattern = i % 3;
                bool mirrorBox = levelIndex >= MirroredHandFromIndex;
                string place = placeNames[i % placeNames.Length];
                string handLabel = handCount == 1 ? string.Empty : " (2 tiles)";
                string title = $"{place} Puzzle {levelIndex + 1}{handLabel}";

                LevelDefinition level = BuildHandPatternLevel(title, theme, value, handCount, pattern, mirrorBox);

                if (!mirrorBox && LevelSolvabilityRules.ShouldConfigureBoxSide(handCount))
                {
                    LevelSolvabilityRules.ConfigureStandardSolvableLevel(level, handCount, diceLevel: false, value);
                    level.ParMoves = handCount + 1;
                    level.ParCards = handCount;
                    HandVisualRules.AssignLevelHandVisualThemes(level);
                }

                levels.Add(level);

                if (rng.NextDouble() < 0.15)
                {
                    level.ParMoves++;
                }
            }

            return levels;
        }

        /// <summary>
        /// Levels 37-50: hand-play puzzles like procedural patterns 41-43 (box-left),
        /// mirrored on the other side for 44-50.
        /// </summary>
        private static LevelDefinition BuildHandPatternLevel(string title, int theme, int value, int handCount,
            int pattern, bool mirrorBox)
        {
            if (mirrorBox)
            {
                return pattern switch
                {
                    0 => CreatureLevel(title, theme, value, handCount,
                        new[] { CardKind.DayCreature },
                        new[] { CardKind.Box, CardKind.DayCreature },
                        CardKind.NightCreature, value, handCount * 2, handCount),
                    1 => CreatureLevel(title, theme, value, handCount,
                        new[] { CardKind.NightCreature },
                        new[] { CardKind.Box, CardKind.NightCreature },
                        CardKind.DayCreature, value, handCount * 2, handCount),
                    _ => CreatureLevel(title, theme, value, handCount,
                        Array.Empty<CardKind>(),
                        new[] { CardKind.Box, CardKind.DayCreature },
                        CardKind.NightCreature, value, handCount * 2, handCount)
                };
            }

            return pattern switch
            {
                0 => CreatureLevel(title, theme, value, handCount,
                    new[] { CardKind.Box, CardKind.DayCreature },
                    new[] { CardKind.DayCreature },
                    CardKind.NightCreature, value, handCount * 2, handCount),
                1 => CreatureLevel(title, theme, value, handCount,
                    new[] { CardKind.Box, CardKind.NightCreature },
                    new[] { CardKind.NightCreature },
                    CardKind.DayCreature, value, handCount * 2, handCount),
                _ => CreatureLevel(title, theme, value, handCount,
                    new[] { CardKind.Box, CardKind.DayCreature },
                    Array.Empty<CardKind>(),
                    CardKind.NightCreature, value, handCount * 2, handCount)
            };
        }

        private static void AssignMatchingPairThemes(LevelDefinition level)
        {
            level.LeftVisualThemes.Clear();
            level.RightVisualThemes.Clear();

            foreach (CardKind kind in level.LeftCards)
            {
                level.LeftVisualThemes.Add(
                    kind is CardKind.DayCreature or CardKind.NightCreature ? level.CreatureTheme : -1);
            }

            foreach (CardKind kind in level.RightCards)
            {
                level.RightVisualThemes.Add(
                    kind is CardKind.DayCreature or CardKind.NightCreature ? level.CreatureTheme : -1);
            }
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

        private static void FillHand(LevelDefinition level, int handCount, CardKind primaryHand, int handValue, int value,
            bool diceLevel)
        {
            level.HandCards.Clear();
            level.HandValues.Clear();
            level.HandVisualThemes.Clear();

            CardKind solver = HandCompositionRules.PrimarySolverCard(level, diceLevel, primaryHand);
            CardKind companion = HandCompositionRules.CompanionCreature(
                solver is CardKind.DayCreature or CardKind.NightCreature
                    ? solver
                    : CardKind.NightCreature);

            if (handCount <= 1)
            {
                level.HandCards.Add(solver);
                level.HandValues.Add(diceLevel && solver is CardKind.PositiveConstant or CardKind.NegativeConstant
                    ? value
                    : handValue);
                HandVisualRules.AssignLevelHandVisualThemes(level);
                return;
            }

            if (handCount == 2)
            {
                level.HandCards.Add(solver);
                level.HandValues.Add(value);
                level.HandCards.Add(companion);
                level.HandValues.Add(value);
                HandVisualRules.AssignLevelHandVisualThemes(level);
                return;
            }

            level.HandCards.Add(solver);
            level.HandValues.Add(value);
            level.HandCards.Add(companion);
            level.HandValues.Add(value);
            level.HandCards.Add(CardKind.NegativeConstant);
            level.HandValues.Add(value);
            HandVisualRules.AssignLevelHandVisualThemes(level);
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
    }
}
