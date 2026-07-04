using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelSolvabilityRules
    {
        public static bool ShouldConfigureBoxSide(int handCount) => handCount >= 2;

        /// <summary>
        /// Multi-card levels: one creature beside the box (level theme only), empty right side.
        /// Hand solver is the opposite kind of that creature — always solvable via balance.
        /// </summary>
        public static void ConfigureSolvableLevel(LevelDefinition level, int handCount, bool diceLevel, int value)
        {
            if (handCount < 2)
            {
                return;
            }

            level.RightCards.Clear();
            level.RightValues.Clear();
            level.RightVisualThemes.Clear();

            if (diceLevel)
            {
                ConfigureDiceBoxSide(level, value);
                return;
            }

            ConfigureCreatureBoxSide(level, value);
        }

        private static void ConfigureCreatureBoxSide(LevelDefinition level, int value)
        {
            CardKind solverKind = level.HandCards[0];
            CardKind boxSideKind = solverKind == CardKind.NightCreature
                ? CardKind.DayCreature
                : CardKind.NightCreature;

            level.LeftCards = new List<CardKind> { CardKind.Box, boxSideKind };
            level.LeftValues = new List<int> { 1, value };
            level.LeftVisualThemes = new List<int> { -1, level.CreatureTheme };
        }

        private static void ConfigureDiceBoxSide(LevelDefinition level, int value)
        {
            CardKind solverKind = level.HandCards[0];
            CardKind boxSideKind = solverKind == CardKind.NegativeConstant
                ? CardKind.PositiveConstant
                : CardKind.NegativeConstant;

            level.LeftCards = new List<CardKind> { CardKind.Box, boxSideKind };
            level.LeftValues = new List<int> { 1, value };
            level.LeftVisualThemes = new List<int> { -1, -1 };
        }
    }
}
