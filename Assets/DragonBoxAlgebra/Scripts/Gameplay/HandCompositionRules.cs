using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class HandCompositionRules
    {
        public static CardKind PrimarySolverCard(LevelDefinition level, bool diceLevel, CardKind primaryHand)
        {
            if (diceLevel)
            {
                foreach (CardKind kind in level.LeftCards)
                {
                    if (kind == CardKind.PositiveConstant)
                    {
                        return CardKind.NegativeConstant;
                    }

                    if (kind == CardKind.NegativeConstant)
                    {
                        return CardKind.PositiveConstant;
                    }
                }

                return primaryHand;
            }

            foreach (CardKind kind in level.LeftCards)
            {
                if (kind == CardKind.DayCreature)
                {
                    return CardKind.NightCreature;
                }

                if (kind == CardKind.NightCreature)
                {
                    return CardKind.DayCreature;
                }
            }

            return primaryHand;
        }

        public static CardKind CompanionCreature(CardKind solver) =>
            solver == CardKind.NightCreature ? CardKind.DayCreature : CardKind.NightCreature;

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

        public static string MultiCardHandHint =>
            "Hand tiles stay in your hand. One tile matches creatures beside the red box; the others look different. " +
            "Drag to one side — a ? appears on the other. Drag the same tile to the ? to balance.";
    }
}
