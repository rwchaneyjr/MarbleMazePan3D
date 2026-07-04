using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class BoardVisualRules
    {
        public static void AssignDistinctSideThemes(LevelDefinition level)
        {
            level.LeftVisualThemes.Clear();
            level.RightVisualThemes.Clear();

            int boardCreatureCount = CountCreatures(level.LeftCards) + CountCreatures(level.RightCards);
            if (ShouldUseLevelThemeOnly(level, boardCreatureCount))
            {
                AssignLevelThemeOnly(level.LeftCards, level.LeftVisualThemes, level.CreatureTheme);
                AssignLevelThemeOnly(level.RightCards, level.RightVisualThemes, level.CreatureTheme);
                return;
            }

            var used = HandVisualRules.CollectHandCreatureThemes(level);
            AssignSideThemes(level.LeftCards, level.LeftVisualThemes, level.CreatureTheme, used);
            AssignSideThemes(level.RightCards, level.RightVisualThemes, level.CreatureTheme, used);
        }

        private static bool ShouldUseLevelThemeOnly(LevelDefinition level, int boardCreatureCount)
        {
            if (boardCreatureCount <= 1)
            {
                return true;
            }

            return level.HandCards.Count <= 1 && boardCreatureCount <= 2;
        }

        private static int CountCreatures(List<CardKind> cards)
        {
            int count = 0;
            foreach (CardKind kind in cards)
            {
                if (kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AssignLevelThemeOnly(List<CardKind> cards, List<int> themes, int boardTheme)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                CardKind kind = cards[i];
                themes.Add(kind is CardKind.DayCreature or CardKind.NightCreature ? boardTheme : -1);
            }
        }

        private static void AssignSideThemes(List<CardKind> cards, List<int> themes, int boardTheme,
            ISet<int> usedThemes)
        {
            int creatureCount = 0;
            foreach (CardKind kind in cards)
            {
                if (kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    creatureCount++;
                }
            }

            List<int> creatureThemes = ThemeAssignment.DistinctThemesExcluding(
                creatureCount, usedThemes, boardTheme);
            int creatureIndex = 0;

            for (int i = 0; i < cards.Count; i++)
            {
                CardKind kind = cards[i];
                if (kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    themes.Add(creatureThemes[creatureIndex]);
                    usedThemes.Add(creatureThemes[creatureIndex]);
                    creatureIndex++;
                    continue;
                }

                themes.Add(-1);
            }
        }
    }
}
