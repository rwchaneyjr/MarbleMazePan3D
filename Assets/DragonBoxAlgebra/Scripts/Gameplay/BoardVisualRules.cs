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
            for (int i = 0; i < cards.Count; i++)
            {
                CardKind kind = cards[i];
                themes.Add(kind is CardKind.DayCreature or CardKind.NightCreature ? boardTheme : -1);
            }
        }
    }
}
