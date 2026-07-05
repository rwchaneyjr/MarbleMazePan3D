using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// Red-side obstacles and hand solvers share the same distinct theme list:
    /// hand slot i is the light/dark partner for red-side obstacle i.
    /// Other-side extras append more distinct themes and matching hand solvers.
    /// </summary>
    public static class CoordinatedCreatureThemes
    {
        public static List<int> BuildRedSideThemes(int count, int levelTheme) =>
            ThemeAssignment.DistinctThemes(count, levelTheme);

        public static List<int> BuildOtherSideThemes(int count, ISet<int> usedThemes, int levelTheme) =>
            ThemeAssignment.DistinctThemesExcluding(count, usedThemes, levelTheme);

        public static void ApplyRedSideAndHand(LevelDefinition level, IReadOnlyList<int> redThemes)
        {
            ApplyHandThemes(level, redThemes, otherThemes: null);
        }

        public static void ApplyRedSideAndOtherHand(LevelDefinition level, IReadOnlyList<int> redThemes,
            IReadOnlyList<int> otherThemes)
        {
            ApplyHandThemes(level, redThemes, otherThemes);
        }

        private static void ApplyHandThemes(LevelDefinition level, IReadOnlyList<int> redThemes,
            IReadOnlyList<int> otherThemes)
        {
            level.HandVisualThemes.Clear();
            int creatureIndex = 0;

            for (int i = 0; i < level.HandCards.Count; i++)
            {
                CardKind kind = level.HandCards[i];
                if (kind is not (CardKind.DayCreature or CardKind.NightCreature))
                {
                    level.HandVisualThemes.Add(-1);
                    continue;
                }

                int theme;
                if (creatureIndex < redThemes.Count)
                {
                    theme = redThemes[creatureIndex];
                }
                else if (otherThemes != null)
                {
                    int otherIndex = creatureIndex - redThemes.Count;
                    theme = otherIndex < otherThemes.Count ? otherThemes[otherIndex] : level.CreatureTheme;
                }
                else
                {
                    theme = level.CreatureTheme;
                }

                level.HandVisualThemes.Add(theme);
                creatureIndex++;
            }
        }

        public static CardKind OppositeCreature(CardKind kind) =>
            kind == CardKind.NightCreature ? CardKind.DayCreature : CardKind.NightCreature;

        public static List<CardKind> BuildAlternatingCreatureKinds(int count)
        {
            var kinds = new List<CardKind>(count);
            for (int i = 0; i < count; i++)
            {
                kinds.Add(i % 2 == 0 ? CardKind.DayCreature : CardKind.NightCreature);
            }

            return kinds;
        }
    }
}
