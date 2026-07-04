using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelSolvabilityRules
    {
        public const int ExtraBoxSideFromLevel = 13;
        public const int ExtraBoxSideToLevel = 30;

        public static bool ShouldAddExtraBoxSideTiles(int levelIndex) =>
            levelIndex + 1 >= ExtraBoxSideFromLevel && levelIndex + 1 <= ExtraBoxSideToLevel;

        public static int ExtraTileCountForHand(int handCount) => handCount switch
        {
            >= 3 => 3,
            2 => 2,
            _ => 0
        };

        public static void AddRandomBoxSideTiles(LevelDefinition level, int count, int value, Random rng)
        {
            if (count <= 0)
            {
                return;
            }

            var allowed = CancellableKindsForHand(level);

            for (int i = 0; i < count; i++)
            {
                CardKind kind = allowed[rng.Next(allowed.Count)];
                level.LeftCards.Add(kind);
                level.LeftValues.Add(ValueForKind(kind, value));
            }
        }

        private static List<CardKind> CancellableKindsForHand(LevelDefinition level)
        {
            bool hasNight = false;
            bool hasDay = false;
            bool hasPositive = false;
            bool hasNegative = false;

            foreach (CardKind kind in level.HandCards)
            {
                switch (kind)
                {
                    case CardKind.NightCreature:
                        hasNight = true;
                        break;
                    case CardKind.DayCreature:
                        hasDay = true;
                        break;
                    case CardKind.PositiveConstant:
                        hasPositive = true;
                        break;
                    case CardKind.NegativeConstant:
                        hasNegative = true;
                        break;
                }
            }

            var allowed = new List<CardKind>();
            if (hasNight || hasDay)
            {
                allowed.Add(CardKind.DayCreature);
                allowed.Add(CardKind.NightCreature);
            }

            if (hasNegative)
            {
                allowed.Add(CardKind.PositiveConstant);
            }

            if (hasPositive)
            {
                allowed.Add(CardKind.NegativeConstant);
            }

            if (allowed.Count == 0)
            {
                allowed.Add(CardKind.DayCreature);
                allowed.Add(CardKind.NightCreature);
            }

            return allowed;
        }

        private static int ValueForKind(CardKind kind, int value) => kind switch
        {
            CardKind.DayCreature or CardKind.NightCreature => value,
            CardKind.PositiveConstant or CardKind.NegativeConstant => value,
            _ => 1
        };
    }
}
