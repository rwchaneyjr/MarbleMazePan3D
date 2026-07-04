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
            AssignSideThemes(level.LeftCards, level.LeftVisualThemes, level.CreatureTheme);
            AssignSideThemes(level.RightCards, level.RightVisualThemes, level.CreatureTheme);
        }

        private static void AssignSideThemes(List<CardKind> cards, List<int> themes, int boardTheme)
        {
            var usedThemes = new HashSet<int>();
            int creatureSlot = 0;

            for (int i = 0; i < cards.Count; i++)
            {
                CardKind kind = cards[i];
                if (kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    int theme = HandVisualRules.PickHandTheme(boardTheme, creatureSlot, usedThemes);
                    themes.Add(theme);
                    usedThemes.Add(theme);
                    creatureSlot++;
                    continue;
                }

                themes.Add(-1);
            }
        }
    }
}
