using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// 20 tutorial levels: drag light onto dark (same side) to form a spinning *.
    /// </summary>
    public static class DragMergeLevelGenerator
    {
        public const int TotalLevels = 20;

        private static readonly CardKind[] Empty = System.Array.Empty<CardKind>();

        public static IReadOnlyList<LevelDefinition> GenerateAll()
        {
            var levels = new List<LevelDefinition>(TotalLevels);

            for (int i = 0; i < 5; i++)
            {
                levels.Add(Make(
                    $"Drag Merge {i + 1} • Left side",
                    theme: i % 10,
                    left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                    right: Empty,
                    parMoves: 2));
            }

            for (int i = 0; i < 5; i++)
            {
                levels.Add(Make(
                    $"Drag Merge {i + 6} • Right side",
                    theme: (5 + i) % 10,
                    left: Empty,
                    right: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                    parMoves: 2));
            }

            for (int i = 0; i < 5; i++)
            {
                bool creaturesOnLeft = i % 2 == 0;
                levels.Add(Make(
                    $"Drag Merge {i + 11} • Two on one side",
                    theme: (10 + i) % 10,
                    left: creaturesOnLeft
                        ? new[] { CardKind.DayCreature, CardKind.NightCreature }
                        : new[] { CardKind.Box },
                    right: creaturesOnLeft
                        ? new[] { CardKind.Box }
                        : new[] { CardKind.DayCreature, CardKind.NightCreature },
                    parMoves: 2));
            }

            for (int i = 0; i < 5; i++)
            {
                levels.Add(Make(
                    $"Drag Merge {i + 16} • Both sides",
                    theme: (15 + i) % 10,
                    left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                    right: new[] { CardKind.DayCreature, CardKind.NightCreature },
                    parMoves: 4));
            }

            return levels;
        }

        private static LevelDefinition Make(string title, int theme, CardKind[] left, CardKind[] right, int parMoves)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 5,
                CreatureTheme = theme,
                DragToMergePairs = true,
                LeftCards = new List<CardKind>(left),
                RightCards = new List<CardKind>(right),
                HandCards = new List<CardKind>(),
                ParMoves = parMoves,
                ParCards = 0
            };

            level.LeftValues = ValuesFor(level.LeftCards);
            level.RightValues = ValuesFor(level.RightCards);
            level.HandValues = ValuesFor(level.HandCards);
            return level;
        }

        private static List<int> ValuesFor(List<CardKind> cards)
        {
            var values = new List<int>();
            for (int i = 0; i < cards.Count; i++)
            {
                values.Add(1);
            }

            return values;
        }
    }
}
