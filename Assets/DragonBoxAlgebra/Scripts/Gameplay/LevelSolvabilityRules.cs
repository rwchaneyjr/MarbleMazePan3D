using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelSolvabilityRules
    {
        public static bool ShouldConfigureBoxSide(int handCount) => handCount >= 2;

        public static void ConfigureSolvableLevel(LevelDefinition level, int handCount, bool diceLevel, int value)
        {
            if (handCount < 2)
            {
                return;
            }

            if (diceLevel)
            {
                ConfigureDiceBoxSide(level, handCount, value);
                return;
            }

            ConfigureCreatureBoxSide(level, handCount, value);
        }

        private static void ConfigureCreatureBoxSide(LevelDefinition level, int handCount, int value)
        {
            level.RightCards.Clear();
            level.RightValues.Clear();
            level.RightVisualThemes.Clear();

            CardKind solverKind = level.HandCards[0];
            CardKind boxSideKind = solverKind == CardKind.NightCreature
                ? CardKind.DayCreature
                : CardKind.NightCreature;

            var leftCards = new List<CardKind> { CardKind.Box };
            var leftValues = new List<int> { 1 };
            var leftThemes = new List<int> { -1 };

            for (int i = 0; i < handCount; i++)
            {
                leftCards.Add(boxSideKind);
                leftValues.Add(value);
                int theme = i < level.HandVisualThemes.Count ? level.HandVisualThemes[i] : -1;
                leftThemes.Add(theme);
            }

            level.LeftCards = leftCards;
            level.LeftValues = leftValues;
            level.LeftVisualThemes = leftThemes;
        }

        private static void ConfigureDiceBoxSide(LevelDefinition level, int handCount, int value)
        {
            level.RightCards.Clear();
            level.RightValues.Clear();
            level.RightVisualThemes.Clear();

            CardKind solverKind = level.HandCards[0];
            CardKind boxSideKind = solverKind == CardKind.NegativeConstant
                ? CardKind.PositiveConstant
                : CardKind.NegativeConstant;

            var leftCards = new List<CardKind> { CardKind.Box };
            var leftValues = new List<int> { 1 };
            var leftThemes = new List<int> { -1 };

            for (int i = 0; i < handCount; i++)
            {
                leftCards.Add(boxSideKind);
                leftValues.Add(value);
                leftThemes.Add(-1);
            }

            level.LeftCards = leftCards;
            level.LeftValues = leftValues;
            level.LeftVisualThemes = leftThemes;
        }
    }
}
