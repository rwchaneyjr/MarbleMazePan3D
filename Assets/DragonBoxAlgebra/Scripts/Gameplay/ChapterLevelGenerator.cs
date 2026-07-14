using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// DragonBox-style intro: Ch1 has 12 tutorials; Ch2 has 16; Ch3 has 15; Ch4 has 20; Ch5 has 17 (80 total).
    /// Ch1 asterisk dismiss, Ch2 opposite drag, Ch3 balance, Ch4 multi-card moves, Ch5 letter variables.
    /// Levels 1–63 are unchanged from the saved working copy; Ch5 adds 64–80 after that.
    /// </summary>
    public static class ChapterLevelGenerator
    {
        public const int LevelsPerChapter = 20;
        public const int Chapter1LevelCount = 12;
        public const int Chapter2LevelCount = 16;
        public const int Chapter3LevelCount = 15;
        public const int Chapter4LevelCount = 20;
        public const int Chapter5LevelCount = 17;
        public const int ChapterCount = 5;
        public const int TotalLevels = Chapter1LevelCount + Chapter2LevelCount + Chapter3LevelCount
            + Chapter4LevelCount + Chapter5LevelCount;

        /// <summary>First global level number (1-based) for Chapter 4 / Move Cards.</summary>
        public const int Chapter4StartLevel = Chapter1LevelCount + Chapter2LevelCount + Chapter3LevelCount + 1;

        /// <summary>First global level number (1-based) for Chapter 5 / Letter Variables.</summary>
        public const int Chapter5StartLevel = Chapter4StartLevel + Chapter4LevelCount;

        /// <summary>Levels 40–63 get one random creature on the side opposite the red box.</summary>
        public const int OppositeExtraTileStartLevel = 40;
        public const int OppositeExtraTileEndLevel = Chapter4StartLevel + Chapter4LevelCount - 1;

        private const int OppositeExtraTileStartIndex = OppositeExtraTileStartLevel - 1;
        private const int OppositeExtraTileEndIndex = OppositeExtraTileEndLevel - 1;

        private static readonly int[] ChapterLevelCounts =
        {
            Chapter1LevelCount,
            Chapter2LevelCount,
            Chapter3LevelCount,
            Chapter4LevelCount,
            Chapter5LevelCount
        };

        private static readonly string[] ChapterNames =
        {
            "Matching Pairs",
            "Opposite Cards",
            "Balance Sides",
            "Move Cards",
            "Letter Variables"
        };

        public static IReadOnlyList<LevelDefinition> GenerateAll()
        {
            var levels = new List<LevelDefinition>(TotalLevels);
            levels.AddRange(GenerateChapter(1));
            levels.AddRange(GenerateChapter(2));
            levels.AddRange(GenerateChapter(3));
            levels.AddRange(GenerateChapter(4));
            levels.AddRange(GenerateChapter5());

            for (int i = OppositeExtraTileStartIndex;
                 i <= OppositeExtraTileEndIndex && i < levels.Count;
                 i++)
            {
                AddRandomExtraTileOppositeBox(levels[i], i);
            }

            HandRules.AssertAllHandCardsFlippable(levels);
            return levels;
        }

        public static int ChapterForLevelIndex(int levelIndex)
        {
            if (levelIndex < 0)
            {
                return 1;
            }

            int cursor = 0;
            for (int chapter = 0; chapter < ChapterCount; chapter++)
            {
                if (levelIndex < cursor + ChapterLevelCounts[chapter])
                {
                    return chapter + 1;
                }

                cursor += ChapterLevelCounts[chapter];
            }

            return ChapterCount;
        }

        public static int IndexWithinChapter(int levelIndex)
        {
            if (levelIndex < 0)
            {
                return 0;
            }

            int cursor = 0;
            for (int chapter = 0; chapter < ChapterCount; chapter++)
            {
                if (levelIndex < cursor + ChapterLevelCounts[chapter])
                {
                    return levelIndex - cursor;
                }

                cursor += ChapterLevelCounts[chapter];
            }

            return 0;
        }

        private static IEnumerable<LevelDefinition> GenerateChapter5()
        {
            for (int i = 0; i < Chapter5LevelCount; i++)
            {
                int displayNumber = i + 1;
                int theme = (5 * 3 + i) % 10;
                yield return BuildChapter5Level(i, theme, displayNumber);
            }
        }

        /// <summary>
        /// Ch5 (levels 64–80): x replaces the red box; a/b/c/r are +/- pairs that merge into swirls.
        /// </summary>
        private static LevelDefinition BuildChapter5Level(int index, int theme, int displayNumber)
        {
            char pairLetter = Chapter5PairLetterForDisplay(displayNumber);
            string title = $"Ch5 • {ChapterNames[4]} {displayNumber} (x + {pairLetter})";

            if (displayNumber is 7 or 8)
            {
                return MakeCh5MergedWinLevel(title, theme, pairLetter, xOnLeft: displayNumber == 7);
            }

            bool xOnLeft = displayNumber <= 6
                ? displayNumber % 2 == 1
                : displayNumber % 2 == 0;
            return MakeCh5BalanceLevel(title, theme, pairLetter, xOnLeft);
        }

        /// <summary>Pair letter introduced gradually; x is always the isolation goal.</summary>
        private static char Chapter5PairLetterForDisplay(int displayNumber) => displayNumber switch
        {
            <= 10 => 'a',
            11 or 12 => 'b',
            13 or 14 => 'c',
            15 or 16 => 'r',
            17 => 'a',
            _ => 'a'
        };

        private static LevelDefinition MakeCh5BalanceLevel(string title, int theme, char pairLetter,
            bool xOnLeft)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 5,
                CreatureTheme = theme,
                ParMoves = 3,
                ParCards = 1
            };

            if (xOnLeft)
            {
                level.LeftCards.AddRange(new[] { CardKind.DayCreature, CardKind.DayCreature });
                level.LeftVariableLetters.AddRange(new[] { VariableGoalRules.GoalLetter, pairLetter });
                level.RightCards.Add(CardKind.DayCreature);
                level.RightVariableLetters.Add(pairLetter);
            }
            else
            {
                level.LeftCards.Add(CardKind.DayCreature);
                level.LeftVariableLetters.Add(pairLetter);
                level.RightCards.AddRange(new[] { CardKind.DayCreature, CardKind.DayCreature });
                level.RightVariableLetters.AddRange(new[] { pairLetter, VariableGoalRules.GoalLetter });
            }

            level.HandCards.Add(CardKind.NightCreature);
            level.HandVariableLetters.Add(pairLetter);
            level.LeftValues = ValuesFor(level.LeftCards);
            level.RightValues = ValuesFor(level.RightCards);
            level.HandValues = ValuesFor(level.HandCards);
            return level;
        }

        private static LevelDefinition MakeCh5MergedWinLevel(string title, int theme, char pairLetter,
            bool xOnLeft)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 5,
                CreatureTheme = theme,
                ParMoves = 1,
                ParCards = 0
            };

            if (xOnLeft)
            {
                level.LeftCards.AddRange(new[]
                {
                    CardKind.DayCreature, CardKind.DayCreature, CardKind.NightCreature
                });
                level.LeftVariableLetters.AddRange(new[]
                {
                    VariableGoalRules.GoalLetter, pairLetter, pairLetter
                });
            }
            else
            {
                level.RightCards.AddRange(new[]
                {
                    CardKind.DayCreature, CardKind.DayCreature, CardKind.NightCreature
                });
                level.RightVariableLetters.AddRange(new[]
                {
                    VariableGoalRules.GoalLetter, pairLetter, pairLetter
                });
            }

            level.LeftValues = ValuesFor(level.LeftCards);
            level.RightValues = ValuesFor(level.RightCards);
            level.HandValues = ValuesFor(level.HandCards);
            return level;
        }

        private static IEnumerable<LevelDefinition> GenerateChapter(int chapter)
        {
            if (chapter == 1)
            {
                foreach (LevelDefinition level in GenerateChapter1())
                {
                    yield return level;
                }

                yield break;
            }

            int displayNumber = 0;
            for (int i = 0; i < LevelsPerChapter; i++)
            {
                if (chapter == 2 && ShouldSkipChapter2Slot(i))
                {
                    continue;
                }

                if (chapter == 3 && ShouldSkipChapter3Slot(i))
                {
                    continue;
                }

                displayNumber++;
                int theme = (chapter * 3 + i) % 10;
                yield return chapter switch
                {
                    2 => BuildChapter2Level(i, theme, displayNumber),
                    3 => BuildChapter3Level(i, theme, displayNumber),
                    _ => BuildChapter4Level(i, theme)
                };
            }
        }

        private static IEnumerable<LevelDefinition> GenerateChapter1()
        {
            int displayNumber = 0;
            for (int i = 0; i < LevelsPerChapter; i++)
            {
                if (ShouldSkipChapter1Slot(i))
                {
                    continue;
                }

                displayNumber++;
                int theme = (3 + i) % 10;
                yield return BuildChapter1Level(i, theme, displayNumber);
            }
        }

        private static bool ShouldSkipChapter1Slot(int index) =>
            index is >= 6 and <= 9 or >= 16 and <= 19;

        private static bool ShouldSkipChapter2Slot(int index) =>
            index is 16 or 17 or 18 or 19;

        private static bool ShouldSkipChapter3Slot(int index) =>
            index is 0 or 8 or 9 or 10 or 11;

        /// <summary>Ch1: drag day/night together on one side → *; box alone on the other.</summary>
        private static LevelDefinition BuildChapter1Level(int index, int theme, int displayNumber)
        {
            string title = $"Ch1 • {ChapterNames[0]} {displayNumber}";

            if (index < 10)
            {
                return Make(
                    title,
                    chapter: 1,
                    theme,
                    left: new[] { CardKind.DayCreature, CardKind.NightCreature },
                    right: new[] { CardKind.Box },
                    hand: System.Array.Empty<CardKind>(),
                    parMoves: 2,
                    parCards: 0,
                    dragToMergePairs: true);
            }

            return Make(
                title,
                chapter: 1,
                theme,
                left: new[] { CardKind.Box },
                right: new[] { CardKind.DayCreature, CardKind.NightCreature },
                hand: System.Array.Empty<CardKind>(),
                parMoves: 2,
                parCards: 0,
                dragToMergePairs: true);
        }

        /// <summary>Ch2: dual-pair drag levels only (opposite-hand intro levels 29–30 removed).</summary>
        private static LevelDefinition BuildChapter2Level(int index, int theme, int displayNumber)
        {
            string title = $"Ch2 • {ChapterNames[1]} {displayNumber}";

            return MakeDualPairDragLevel(
                title,
                chapter: 2,
                theme,
                index,
                differentSideThemes: index >= 9);
        }

        /// <summary>Ch3: balance hand tile → ? → merge * on each side.</summary>
        private static LevelDefinition BuildChapter3Level(int index, int theme, int displayNumber)
        {
            string title = $"Ch3 • {ChapterNames[2]} {displayNumber}";

            // Levels 31–37: intro balance — box + day, day on other side, night in hand.
            if (index is >= 1 and <= 7)
            {
                return MakeBalanceIntroLevel(title, theme, displayNumber, boxOnLeft: index % 2 == 1);
            }

            // Levels 38–42: same pattern, box alternates.
            if (index is >= 12 and <= 16)
            {
                return MakeBalanceIntroLevel(title, theme, displayNumber, boxOnLeft: index % 2 == 0);
            }

            // Levels 43–45: board already has opposite pair — merge to win.
            if (index < 19)
            {
                return index % 2 == 1
                    ? Make(
                        title,
                        chapter: 3,
                        theme,
                        left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                        right: System.Array.Empty<CardKind>(),
                        hand: System.Array.Empty<CardKind>(),
                        parMoves: 1,
                        parCards: 0)
                    : Make(
                        title,
                        chapter: 3,
                        theme,
                        left: System.Array.Empty<CardKind>(),
                        right: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                        hand: System.Array.Empty<CardKind>(),
                        parMoves: 1,
                        parCards: 0);
            }

            return MakeBalanceIntroLevel(title, theme, displayNumber, boxOnLeft: true);
        }

        /// <summary>
        /// Solvable balance puzzle: place night, fill ?, then cancel day+night on each side.
        /// Box on left: drag night to left. Box on right: drag night to left (creates ? on right).
        /// </summary>
        private static LevelDefinition MakeBalanceIntroLevel(string title, int theme, int displayNumber,
            bool boxOnLeft, int parMoves = 3)
        {
            return boxOnLeft
                ? Make(
                    title,
                    chapter: 3,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature },
                    right: new[] { CardKind.DayCreature },
                    hand: new[] { CardKind.NightCreature },
                    parMoves: parMoves,
                    parCards: 1)
                : Make(
                    title,
                    chapter: 3,
                    theme,
                    left: new[] { CardKind.DayCreature },
                    right: new[] { CardKind.Box, CardKind.DayCreature },
                    hand: new[] { CardKind.NightCreature },
                    parMoves: parMoves,
                    parCards: 1);
        }

        /// <summary>Ch4: multi-hand balance — same solvable board as Ch3, extra hand slots for sequencing.</summary>
        private static LevelDefinition BuildChapter4Level(int index, int theme)
        {
            int handCount = index < 10 ? 2 : 3;
            bool boxOnLeft = index % 2 == 0;
            string title = $"Ch4 • {ChapterNames[3]} {index + 1}";
            return MakeCh4BalanceLevel(title, theme, handCount, boxOnLeft);
        }

        /// <summary>
        /// Solvable Ch4 puzzle: balance one hand tile (box+day / day), merge both sides, dismiss swirls.
        /// Extra hand tiles teach sequential play; the first tile completes the board.
        /// </summary>
        private static LevelDefinition MakeCh4BalanceLevel(string title, int theme, int handCount, bool boxOnLeft)
        {
            var hand = new CardKind[handCount];
            for (int i = 0; i < handCount; i++)
            {
                hand[i] = i % 2 == 0 ? CardKind.NightCreature : CardKind.DayCreature;
            }

            int parMoves = 2 + handCount;
            return boxOnLeft
                ? Make(
                    title,
                    chapter: 4,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature },
                    right: new[] { CardKind.DayCreature },
                    hand: hand,
                    parMoves: parMoves,
                    parCards: 1)
                : Make(
                    title,
                    chapter: 4,
                    theme,
                    left: new[] { CardKind.DayCreature },
                    right: new[] { CardKind.Box, CardKind.DayCreature },
                    hand: hand,
                    parMoves: parMoves,
                    parCards: 1);
        }

        private static LevelDefinition MakeDualPairDragLevel(string title, int chapter, int theme, int index,
            bool differentSideThemes = false)
        {
            bool boxOnLeft = index % 2 == 0;
            LevelDefinition level = boxOnLeft
                ? Make(
                    title,
                    chapter,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                    right: new[] { CardKind.DayCreature, CardKind.NightCreature },
                    hand: System.Array.Empty<CardKind>(),
                    parMoves: 4,
                    parCards: 0,
                    dragToMergePairs: true)
                : Make(
                    title,
                    chapter,
                    theme,
                    left: new[] { CardKind.DayCreature, CardKind.NightCreature },
                    right: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                    hand: System.Array.Empty<CardKind>(),
                    parMoves: 4,
                    parCards: 0,
                    dragToMergePairs: true);

            if (differentSideThemes)
            {
                List<int> themes = ThemeAssignment.DistinctThemes(2, theme);
                level.LeftCreatureTheme = themes[0];
                level.RightCreatureTheme = themes[1];
            }

            return level;
        }

        private static LevelDefinition Make(string title, int chapter, int theme,
            CardKind[] left, CardKind[] right, CardKind[] hand, int parMoves, int parCards,
            bool dragToMergePairs = false)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = chapter,
                CreatureTheme = theme,
                DragToMergePairs = dragToMergePairs,
                LeftCards = new List<CardKind>(left),
                RightCards = new List<CardKind>(right),
                HandCards = new List<CardKind>(hand),
                ParMoves = parMoves,
                ParCards = parCards
            };

            level.LeftValues = ValuesFor(level.LeftCards);
            level.RightValues = ValuesFor(level.RightCards);
            level.HandValues = ValuesFor(level.HandCards);
            return level;
        }

        private static List<int> ValuesFor(List<CardKind> cards)
        {
            var values = new List<int>();
            foreach (CardKind kind in cards)
            {
                values.Add(kind is CardKind.DayCreature or CardKind.NightCreature
                    or CardKind.PositiveConstant or CardKind.NegativeConstant
                    ? 1
                    : 1);
            }

            return values;
        }

        /// <summary>One random day or night on the side away from the red box.</summary>
        private static void AddRandomExtraTileOppositeBox(LevelDefinition level, int globalLevelIndex)
        {
            if (!TryGetOppositeSideLists(level, out List<CardKind> oppositeCards, out List<int> oppositeValues))
            {
                return;
            }

            var rng = new System.Random(globalLevelIndex * 7919 + 31);
            CardKind extra = PickExtraCreature(oppositeCards, rng);
            oppositeCards.Add(extra);
            oppositeValues.Add(1);
        }

        private static CardKind PickExtraCreature(List<CardKind> oppositeCards, System.Random rng)
        {
            bool hasDay = false;
            bool hasNight = false;
            foreach (CardKind kind in oppositeCards)
            {
                if (kind == CardKind.DayCreature)
                {
                    hasDay = true;
                }
                else if (kind == CardKind.NightCreature)
                {
                    hasNight = true;
                }
            }

            if (hasDay && !hasNight)
            {
                return CardKind.NightCreature;
            }

            if (hasNight && !hasDay)
            {
                return CardKind.DayCreature;
            }

            return rng.Next(2) == 0 ? CardKind.DayCreature : CardKind.NightCreature;
        }

        private static bool TryGetOppositeSideLists(LevelDefinition level, out List<CardKind> oppositeCards,
            out List<int> oppositeValues)
        {
            oppositeCards = null;
            oppositeValues = null;

            bool boxOnLeft = level.LeftCards.Contains(CardKind.Box);
            bool boxOnRight = level.RightCards.Contains(CardKind.Box);
            if (boxOnLeft == boxOnRight)
            {
                return false;
            }

            if (boxOnLeft)
            {
                oppositeCards = level.RightCards;
                oppositeValues = level.RightValues;
            }
            else
            {
                oppositeCards = level.LeftCards;
                oppositeValues = level.LeftValues;
            }

            return true;
        }
    }
}
