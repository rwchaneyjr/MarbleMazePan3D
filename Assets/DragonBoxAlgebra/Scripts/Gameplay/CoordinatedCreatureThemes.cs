using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// Pairs distinct visual themes between hand solvers and board obstacles so every
    /// creature on the red side has its matching solution tile in hand.
    /// </summary>
    public static class CoordinatedCreatureThemes
    {
        public static void ApplyHandThemes(LevelDefinition level, IReadOnlyList<int> leftThemes,
            IReadOnlyList<int> rightThemes)
        {
            level.HandVisualThemes.Clear();
            int leftIndex = 0;
            int rightIndex = 0;

            for (int i = 0; i < level.HandCards.Count; i++)
            {
                CardKind kind = level.HandCards[i];
                if (kind is not (CardKind.DayCreature or CardKind.NightCreature))
                {
                    level.HandVisualThemes.Add(-1);
                    continue;
                }

                if (leftIndex < leftThemes.Count)
                {
                    level.HandVisualThemes.Add(leftThemes[leftIndex]);
                    leftIndex++;
                    continue;
                }

                if (rightIndex < rightThemes.Count)
                {
                    level.HandVisualThemes.Add(rightThemes[rightIndex]);
                    rightIndex++;
                    continue;
                }

                level.HandVisualThemes.Add(level.CreatureTheme);
            }
        }

        public static List<int> DistinctThemeSet(int count, int levelTheme) =>
            ThemeAssignment.DistinctThemes(count, levelTheme);
    }
}
