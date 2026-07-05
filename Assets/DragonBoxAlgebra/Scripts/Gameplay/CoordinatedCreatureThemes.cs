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

        /// <summary>
        /// Right-side starters (1–2 tiles) share themes with hand solvers so day/night pairs can cancel.
        /// </summary>
        public static List<int> BuildRightSideThemesMatchingHand(int rightCount, IReadOnlyList<int> handThemes,
            int levelTheme, int handThemeStartIndex = 0)
        {
            var themes = new List<int>();
            for (int i = 0; i < rightCount; i++)
            {
                int handIndex = handThemeStartIndex + i;
                int theme = handIndex < handThemes.Count
                    ? handThemes[handIndex]
                    : handThemes.Count > 0
                        ? handThemes[i % handThemes.Count]
                        : levelTheme;
                themes.Add(theme);
            }

            return themes;
        }

        public static void PairRightSideWithHand(LevelDefinition level)
        {
            var handThemes = new List<int>();
            var handKinds = new List<CardKind>();
            for (int i = 0; i < level.HandCards.Count; i++)
            {
                CardKind kind = level.HandCards[i];
                if (kind is not (CardKind.DayCreature or CardKind.NightCreature))
                {
                    continue;
                }

                int theme = level.HandVisualThemes != null
                    && i < level.HandVisualThemes.Count
                    && level.HandVisualThemes[i] >= 0
                    ? level.HandVisualThemes[i]
                    : level.CreatureTheme;
                handThemes.Add(theme);
                handKinds.Add(kind);
            }

            if (handThemes.Count == 0)
            {
                return;
            }

            int rightCreatureIndex = 0;
            for (int i = 0; i < level.RightCards.Count; i++)
            {
                CardKind rightKind = level.RightCards[i];
                if (rightKind is not (CardKind.DayCreature or CardKind.NightCreature))
                {
                    continue;
                }

                int pairIndex = rightCreatureIndex % handThemes.Count;
                CardKind handKind = handKinds[pairIndex];
                if (rightKind == handKind)
                {
                    rightKind = HandCompositionRules.CompanionCreature(handKind);
                    level.RightCards[i] = rightKind;
                }

                EnsureThemeList(level.RightVisualThemes, level.RightCards.Count);
                level.RightVisualThemes[i] = handThemes[pairIndex];
                rightCreatureIndex++;
            }
        }

        private static void EnsureThemeList(List<int> themes, int count)
        {
            while (themes.Count < count)
            {
                themes.Add(-1);
            }
        }

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
