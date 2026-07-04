using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// When hand has 2+ tiles, hand contents must include cards that match creatures beside the box.
    /// </summary>
    public static class HandCompositionRules
    {
        public static IReadOnlyList<CardKind> CreaturesBesideBox(LevelDefinition level)
        {
            var creatures = new List<CardKind>();
            foreach (CardKind kind in level.LeftCards)
            {
                if (kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    creatures.Add(kind);
                }
            }

            return creatures;
        }

        public static CardKind OppositeCreature(CardKind kind) => kind switch
        {
            CardKind.DayCreature => CardKind.NightCreature,
            CardKind.NightCreature => CardKind.DayCreature,
            _ => kind
        };

        public static void EnsureIncludesBoxSideCards(LevelDefinition level, int handCount, bool diceLevel,
            CardKind primaryHand, int value)
        {
            if (handCount < 2)
            {
                return;
            }

            if (diceLevel)
            {
                EnsureDiceHandIncludesBoxSide(level, primaryHand, value);
                return;
            }

            EnsureCreatureHandIncludesBoxSide(level, value);
        }

        private static void EnsureCreatureHandIncludesBoxSide(LevelDefinition level, int value)
        {
            IReadOnlyList<CardKind> besideBox = CreaturesBesideBox(level);
            if (besideBox.Count == 0)
            {
                return;
            }

            var needed = new HashSet<CardKind>();
            foreach (CardKind kind in besideBox)
            {
                needed.Add(kind);
                needed.Add(OppositeCreature(kind));
            }

            foreach (CardKind kind in level.HandCards)
            {
                needed.Remove(kind);
            }

            if (needed.Count == 0)
            {
                return;
            }

            for (int i = 0; i < level.HandCards.Count && needed.Count > 0; i++)
            {
                foreach (CardKind required in new List<CardKind>(needed))
                {
                    if (level.HandCards[i] == required)
                    {
                        continue;
                    }

                    if (level.HandCards[i] is CardKind.DayCreature or CardKind.NightCreature)
                    {
                        level.HandCards[i] = required;
                        if (i < level.HandValues.Count)
                        {
                            level.HandValues[i] = value;
                        }

                        needed.Remove(required);
                        break;
                    }
                }
            }
        }

        private static void EnsureDiceHandIncludesBoxSide(LevelDefinition level, CardKind primaryHand, int value)
        {
            bool boxSideHasDice = false;
            foreach (CardKind kind in level.LeftCards)
            {
                if (kind is CardKind.PositiveConstant or CardKind.NegativeConstant)
                {
                    boxSideHasDice = true;
                    break;
                }
            }

            if (!boxSideHasDice)
            {
                return;
            }

            bool handHasDice = false;
            foreach (CardKind kind in level.HandCards)
            {
                if (kind is CardKind.PositiveConstant or CardKind.NegativeConstant)
                {
                    handHasDice = true;
                    break;
                }
            }

            if (!handHasDice && level.HandCards.Count > 0)
            {
                level.HandCards[0] = primaryHand;
                if (level.HandValues.Count > 0)
                {
                    level.HandValues[0] = value;
                }
            }
        }

        public static string MultiCardHandHint =>
            "With 2+ hand tiles: every picture must be different, and at least one must match a creature beside the red box.";
    }
}
