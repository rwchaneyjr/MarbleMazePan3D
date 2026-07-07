using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// DragonBox-style intro: 4 chapters × 20 puzzles (80 total).
    /// Ch1 asterisk dismiss, Ch2 opposite drag, Ch3 balance, Ch4 multi-card moves.
    /// </summary>
    public static class ChapterLevelGenerator
    {
        public const int LevelsPerChapter = 20;
        public const int ChapterCount = 4;
        public const int TotalLevels = LevelsPerChapter * ChapterCount;

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
            return levels;
        }

        public static int ChapterForLevelIndex(int levelIndex) =>
            levelIndex < 0 ? 1 : levelIndex / LevelsPerChapter + 1;

        public static int IndexWithinChapter(int levelIndex) =>
            levelIndex < 0 ? 0 : levelIndex % LevelsPerChapter;

        private static IEnumerable<LevelDefinition> GenerateChapter(int chapter)
        {
            for (int i = 0; i < LevelsPerChapter; i++)
            {
                int theme = (chapter * 3 + i) % 10;
                yield return chapter switch
                {
                    1 => BuildChapter1Level(i, theme),
                    2 => BuildChapter2Level(i, theme),
                    3 => BuildChapter3Level(i, theme),
                    _ => BuildChapter4Level(i, theme)
                };
            }
        }

        /// <summary>Ch1: pre-placed opposite pairs on the board → tap * (empty hand).</summary>
        private static LevelDefinition BuildChapter1Level(int index, int theme)
        {
            if (index < 8)
            {
                return Make(
                    $"Ch1 • {ChapterNames[0]} {index + 1}",
                    chapter: 1,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                    right: System.Array.Empty<CardKind>(),
                    hand: System.Array.Empty<CardKind>(),
                    parMoves: 2,
                    parCards: 0);
            }

            if (index < 14)
            {
                return Make(
                    $"Ch1 • {ChapterNames[0]} {index + 1}",
                    chapter: 1,
                    theme,
                    left: System.Array.Empty<CardKind>(),
                    right: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                    hand: System.Array.Empty<CardKind>(),
                    parMoves: 2,
                    parCards: 0);
            }

            return Make(
                $"Ch1 • {ChapterNames[0]} {index + 1}",
                chapter: 1,
                theme,
                left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                right: new[] { CardKind.DayCreature, CardKind.NightCreature },
                hand: System.Array.Empty<CardKind>(),
                parMoves: 3,
                parCards: 0);
        }

        /// <summary>Ch2: drag hand opposite onto a board creature → *.</summary>
        private static LevelDefinition BuildChapter2Level(int index, int theme)
        {
            if (index < 10)
            {
                return Make(
                    $"Ch2 • {ChapterNames[1]} {index + 1}",
                    chapter: 2,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature },
                    right: System.Array.Empty<CardKind>(),
                    hand: new[] { CardKind.NightCreature },
                    parMoves: 2,
                    parCards: 1);
            }

            if (index < 16)
            {
                return Make(
                    $"Ch2 • {ChapterNames[1]} {index + 1}",
                    chapter: 2,
                    theme,
                    left: System.Array.Empty<CardKind>(),
                    right: new[] { CardKind.Box, CardKind.NightCreature },
                    hand: new[] { CardKind.DayCreature },
                    parMoves: 2,
                    parCards: 1);
            }

            return Make(
                $"Ch2 • {ChapterNames[1]} {index + 1}",
                chapter: 2,
                theme,
                left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.DayCreature },
                right: System.Array.Empty<CardKind>(),
                hand: new[] { CardKind.NightCreature },
                parMoves: 3,
                parCards: 1);
        }

        /// <summary>Ch3: place hand tile → ? on other side → balance.</summary>
        private static LevelDefinition BuildChapter3Level(int index, int theme)
        {
            if (index < 12)
            {
                return Make(
                    $"Ch3 • {ChapterNames[2]} {index + 1}",
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
                    $"Ch3 • {ChapterNames[2]} {index + 1}",
                    chapter: 3,
                    theme,
                    left: System.Array.Empty<CardKind>(),
                    right: new[] { CardKind.Box, CardKind.DayCreature },
                    hand: new[] { CardKind.DayCreature },
                    parMoves: 2,
                    parCards: 1);
            }

            return Make(
                $"Ch3 • {ChapterNames[2]} {index + 1}",
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
            if (index < 10)
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

            if (index < 16)
            {
                return Make(
                    $"Ch4 • {ChapterNames[3]} {index + 1}",
                    chapter: 4,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.DayCreature },
                    right: new[] { CardKind.NightCreature },
                    hand: new[] { CardKind.NightCreature, CardKind.NightCreature },
                    parMoves: 4,
                    parCards: 2);
            }

            return Make(
                $"Ch4 • {ChapterNames[3]} {index + 1}",
                chapter: 4,
                theme,
                left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.DayCreature },
                right: new[] { CardKind.NightCreature, CardKind.NightCreature },
                hand: new[] { CardKind.NightCreature, CardKind.NightCreature, CardKind.DayCreature },
                parMoves: 5,
                parCards: 3);
        }

        private static LevelDefinition Make(string title, int chapter, int theme,
            CardKind[] left, CardKind[] right, CardKind[] hand, int parMoves, int parCards)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = chapter,
                CreatureTheme = theme,
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
