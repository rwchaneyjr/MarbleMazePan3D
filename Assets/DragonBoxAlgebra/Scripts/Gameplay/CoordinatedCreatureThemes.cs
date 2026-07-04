using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// Red-side obstacles and hand solvers share the same distinct theme list:
    /// hand slot i is the light/dark partner for red-side obstacle i.
    /// </summary>
    public static class CoordinatedCreatureThemes
    {
        public static List<int> BuildRedSideThemes(int count, int levelTheme) =>
            ThemeAssignment.DistinctThemes(count, levelTheme);

        public static List<int> BuildOtherSideThemes(int count, ISet<int> usedThemes, int levelTheme) =>
            ThemeAssignment.DistinctThemesExcluding(count, usedThemes, levelTheme);

        public static void ApplyRedSideAndHand(LevelDefinition level, IReadOnlyList<int> redThemes)
        {
            level.HandVisualThemes.Clear();
            int redIndex = 0;

            for (int i = 0; i < level.HandCards.Count; i++)
            {
                CardKind kind = level.HandCards[i];
                if (kind is not (CardKind.DayCreature or CardKind.NightCreature))
                {
                    level.HandVisualThemes.Add(-1);
                    continue;
                }

                int theme = redIndex < redThemes.Count ? redThemes[redIndex] : level.CreatureTheme;
                level.HandVisualThemes.Add(theme);
                redIndex++;
            }
        }
    }
}
