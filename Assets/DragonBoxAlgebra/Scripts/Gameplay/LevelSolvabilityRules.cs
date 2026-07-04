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

            CardKind? obstacleKind = PrimaryBoxSideObstacleKind(level);
            if (obstacleKind == null)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                level.LeftCards.Add(obstacleKind.Value);
                level.LeftValues.Add(ValueForKind(obstacleKind.Value, value));
            }
        }

        private static CardKind? PrimaryBoxSideObstacleKind(LevelDefinition level)
        {
            foreach (CardKind kind in level.LeftCards)
            {
                if (kind is CardKind.DayCreature or CardKind.NightCreature
                    or CardKind.PositiveConstant or CardKind.NegativeConstant)
                {
                    return kind;
                }
            }

            return null;
        }

        private static int ValueForKind(CardKind kind, int value) => kind switch
        {
            CardKind.DayCreature or CardKind.NightCreature => value,
            CardKind.PositiveConstant or CardKind.NegativeConstant => value,
            _ => 1
        };
    }
}
