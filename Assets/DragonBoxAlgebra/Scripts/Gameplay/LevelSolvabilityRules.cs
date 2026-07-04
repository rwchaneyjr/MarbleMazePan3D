using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelSolvabilityRules
    {
        public const int ExtraTilesForTwoCardHand = 5;
        public const int ExtraTilesForThreeCardHand = 5;

        public static bool ShouldAddExtraBoxSideTiles(int handCount) => handCount >= 2;

        public static int ExtraTileCountForHand(int handCount) => handCount switch
        {
            >= 3 => ExtraTilesForThreeCardHand,
            2 => ExtraTilesForTwoCardHand,
            _ => 0
        };

        public static void AddRandomBoxSideTiles(LevelDefinition level, int count, int value, Random rng)
        {
            if (count <= 0)
            {
                return;
            }

            if (UsesCreatureObstacles(level))
            {
                AddBalancedCreatureExtras(level, count, value);
                return;
            }

            AddBalancedDiceExtras(level, count, value);
        }

        private static bool UsesCreatureObstacles(LevelDefinition level)
        {
            foreach (CardKind kind in level.LeftCards)
            {
                if (kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddBalancedCreatureExtras(LevelDefinition level, int count, int value)
        {
            int dayCount = 0;
            int nightCount = 0;
            foreach (CardKind kind in level.LeftCards)
            {
                if (kind == CardKind.DayCreature)
                {
                    dayCount++;
                }
                else if (kind == CardKind.NightCreature)
                {
                    nightCount++;
                }
            }

            for (int i = 0; i < count; i++)
            {
                bool addDay = dayCount <= nightCount;
                if (addDay)
                {
                    level.LeftCards.Add(CardKind.DayCreature);
                    level.LeftValues.Add(value);
                    dayCount++;
                }
                else
                {
                    level.LeftCards.Add(CardKind.NightCreature);
                    level.LeftValues.Add(value);
                    nightCount++;
                }
            }
        }

        private static void AddBalancedDiceExtras(LevelDefinition level, int count, int value)
        {
            int positiveCount = 0;
            int negativeCount = 0;
            foreach (CardKind kind in level.LeftCards)
            {
                if (kind == CardKind.PositiveConstant)
                {
                    positiveCount++;
                }
                else if (kind == CardKind.NegativeConstant)
                {
                    negativeCount++;
                }
            }

            for (int i = 0; i < count; i++)
            {
                bool addPositive = positiveCount <= negativeCount;
                if (addPositive)
                {
                    level.LeftCards.Add(CardKind.PositiveConstant);
                    level.LeftValues.Add(value);
                    positiveCount++;
                }
                else
                {
                    level.LeftCards.Add(CardKind.NegativeConstant);
                    level.LeftValues.Add(value);
                    negativeCount++;
                }
            }
        }
    }
}
