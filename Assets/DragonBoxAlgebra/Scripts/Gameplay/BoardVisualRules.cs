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
            var used = HandVisualRules.CollectHandCreatureThemes(level);
            AssignSideThemes(level.LeftCards, level.LeftVisualThemes, level.CreatureTheme, used);
            AssignSideThemes(level.RightCards, level.RightVisualThemes, level.CreatureTheme, used);
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
