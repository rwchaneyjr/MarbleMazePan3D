using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelLibrary
    {
        public static IReadOnlyList<LevelDefinition> Levels { get; } = new List<LevelDefinition>
        {
            new()
            {
                Title = "Isolate the Box",
                LeftCards = new List<CardKind> { CardKind.Box, CardKind.DayCreature },
                RightCards = new List<CardKind> { CardKind.DayCreature },
                HandCards = new List<CardKind> { CardKind.NightCreature },
                HandValues = new List<int> { 1 },
                ParMoves = 2,
                ParCards = 1
            },
            new()
            {
                Title = "Balance Both Sides",
                LeftCards = new List<CardKind> { CardKind.Box, CardKind.NightCreature },
                RightCards = new List<CardKind> { CardKind.PositiveConstant },
                RightValues = new List<int> { 1 },
                HandCards = new List<CardKind> { CardKind.DayCreature },
                HandValues = new List<int> { 1 },
                ParMoves = 2,
                ParCards = 1
            },
            new()
            {
                Title = "Clear the Creatures",
                LeftCards = new List<CardKind> { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                RightCards = new List<CardKind> { CardKind.DayCreature, CardKind.NightCreature },
                HandCards = new List<CardKind>(),
                ParMoves = 1,
                ParCards = 0
            },
            new()
            {
                Title = "Cancel the Dice",
                LeftCards = new List<CardKind> { CardKind.Box, CardKind.PositiveConstant },
                LeftValues = new List<int> { 1, 1 },
                RightCards = new List<CardKind> { CardKind.PositiveConstant, CardKind.NegativeConstant },
                RightValues = new List<int> { 1, 1 },
                HandCards = new List<CardKind> { CardKind.NegativeConstant },
                HandValues = new List<int> { 1 },
                ParMoves = 2,
                ParCards = 1
            },
            new()
            {
                Title = "Fish and Turtle",
                LeftCards = new List<CardKind> { CardKind.Box, CardKind.DayCreature },
                RightCards = new List<CardKind> { CardKind.NightCreature },
                HandCards = new List<CardKind> { CardKind.NightCreature },
                HandValues = new List<int> { 1 },
                ParMoves = 2,
                ParCards = 1
            },
            new()
            {
                Title = "Final Balance",
                LeftCards = new List<CardKind> { CardKind.Box, CardKind.NegativeConstant },
                RightCards = new List<CardKind> { CardKind.PositiveConstant },
                RightValues = new List<int> { 1 },
                HandCards = new List<CardKind> { CardKind.PositiveConstant },
                HandValues = new List<int> { 1 },
                ParMoves = 2,
                ParCards = 1
            }
        };
    }
}
