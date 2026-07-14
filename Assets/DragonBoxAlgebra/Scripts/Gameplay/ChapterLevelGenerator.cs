using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// DragonBox-style intro: Ch1 has 12 tutorials; Ch2 has 18; Ch3 has 15; Ch4 has 20 (65 total).
    /// Ch1 asterisk dismiss, Ch2 opposite drag, Ch3 balance, Ch4 multi-card moves.
    /// </summary>
    public static class ChapterLevelGenerator
    {
        public const int LevelsPerChapter = 20;
        public const int Chapter1LevelCount = 12;
        public const int Chapter2LevelCount = 18;
        public const int Chapter3LevelCount = 15;
        public const int Chapter4LevelCount = 20;
        public const int ChapterCount = 4;
        public const int TotalLevels = Chapter1LevelCount + Chapter2LevelCount + Chapter3LevelCount
            + Chapter4LevelCount;

        /// <summary>First global level number (1-based) for Chapter 4 / Move Cards.</summary>
        public const int Chapter4StartLevel = Chapter1LevelCount + Chapter2LevelCount + Chapter3LevelCount + 1;

        private static readonly int[] ChapterLevelCounts =
        {
            Chapter1LevelCount,
            Chapter2LevelCount,
            Chapter3LevelCount,
            Chapter4LevelCount
        };

        private static readonly string[] ChapterNames =
        {
            "Matching Pairs",
            "Opposite Cards",
            "Balance Sides",
            "Move Cards"
        };

        public static IReadOnlyList<LevelDefinition> GenerateAll()
        {
            var levels = new List<LevelDefinition>(TotalLevels);
            levels.AddRange(GenerateChapter(1));
            levels.AddRange(GenerateChapter(2));
            levels.AddRange(GenerateChapter(3));
            levels.AddRange(GenerateChapter(4));
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
            index is 18 or 19;

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

        /// <summary>Ch2: levels 1–16 dual pairs on both sides; later levels use hand opposite.</summary>
        private static LevelDefinition BuildChapter2Level(int index, int theme, int displayNumber)
        {
            string title = $"Ch2 • {ChapterNames[1]} {displayNumber}";

            if (index < 16)
            {
                return MakeDualPairDragLevel(
                    title,
                    chapter: 2,
                    theme,
                    index,
                    differentSideThemes: index >= 9);
            }

            if (index < 18)
            {
                return Make(
                    title,
                    chapter: 2,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature },
                    right: System.Array.Empty<CardKind>(),
                    hand: new[] { CardKind.NightCreature },
                    parMoves: 2,
                    parCards: 1);
            }

            return Make(
                title,
                chapter: 2,
                theme,
                left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.DayCreature },
                right: System.Array.Empty<CardKind>(),
                hand: new[] { CardKind.NightCreature },
                parMoves: 3,
                parCards: 1);
        }

        /// <summary>Ch3: place hand tile → ? on other side → balance.</summary>
        private static LevelDefinition BuildChapter3Level(int index, int theme, int displayNumber)
        {
            string title = $"Ch3 • {ChapterNames[2]} {displayNumber}";

            if (index < 12)
            {
                return Make(
                    title,
                    chapter: 3,
                    theme,
                    left: new[] { CardKind.Box, CardKind.NightCreature },
                    right: System.Array.Empty<CardKind>(),
                    hand: new[] { CardKind.NightCreature },
                    parMoves: 2,
                    parCards: 1);
            }

            if (index < 17)
            {
                return Make(
                    title,
                    chapter: 3,
                    theme,
                    left: System.Array.Empty<CardKind>(),
                    right: new[] { CardKind.Box, CardKind.DayCreature },
                    hand: new[] { CardKind.DayCreature },
                    parMoves: 2,
                    parCards: 1);
            }

            return Make(
                title,
                chapter: 3,
                theme,
                left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                right: System.Array.Empty<CardKind>(),
                hand: new[] { CardKind.DayCreature },
                parMoves: 3,
                parCards: 1);
        }

        /// <summary>Ch4: two or three hand tiles, tiles on both sides.</summary>
        private static LevelDefinition BuildChapter4Level(int index, int theme)
        {
            // Levels 46–49: intro layout (single dark tile on right).
            if (index < 4)
            {
                return Make(
                    $"Ch4 • {ChapterNames[3]} {index + 1}",
                    chapter: 4,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature },
                    right: new[] { CardKind.NightCreature },
                    hand: new[] { CardKind.NightCreature, CardKind.DayCreature },
                    parMoves: 3,
                    parCards: 2);
            }

            // Levels 50+: include light+dark on the right so those tiles are playable (merge/balance).
            if (index < 10)
            {
                return Make(
                    $"Ch4 • {ChapterNames[3]} {index + 1}",
                    chapter: 4,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature },
                    right: new[] { CardKind.DayCreature, CardKind.NightCreature },
                    hand: new[] { CardKind.NightCreature, CardKind.DayCreature },
                    parMoves: 3,
                    parCards: 2);
            }

            if (index < 16)
            {
                return Make(
                    $"Ch4 • {ChapterNames[3]} {index + 1}",
                    chapter: 4,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.DayCreature },
                    right: new[] { CardKind.DayCreature, CardKind.NightCreature },
                    hand: new[] { CardKind.NightCreature, CardKind.NightCreature },
                    parMoves: 4,
                    parCards: 2);
            }

            if (index < 19)
            {
                return Make(
                    $"Ch4 • {ChapterNames[3]} {index + 1}",
                    chapter: 4,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.DayCreature },
                    right: new[] { CardKind.DayCreature, CardKind.NightCreature, CardKind.NightCreature },
                    hand: new[] { CardKind.NightCreature, CardKind.NightCreature, CardKind.DayCreature },
                    parMoves: 5,
                    parCards: 3);
            }

            return Make(
                $"Ch4 • {ChapterNames[3]} {index + 1}",
                chapter: 4,
                theme,
                left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.DayCreature },
                right: new[] { CardKind.DayCreature, CardKind.NightCreature, CardKind.NightCreature },
                hand: new[] { CardKind.NightCreature, CardKind.NightCreature, CardKind.DayCreature },
                parMoves: 5,
                parCards: 3);
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
    }
}
