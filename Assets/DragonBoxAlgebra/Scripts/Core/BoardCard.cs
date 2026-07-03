using System;

namespace DragonBoxAlgebra.Core
{
    [Serializable]
    public struct BoardCard
    {
        public string Id;
        public CardKind Kind;
        public int Value;
        public int StackCount;

        public BoardCard(CardKind kind, int value = 1, int stackCount = 1)
        {
            Id = Guid.NewGuid().ToString("N");
            Kind = kind;
            Value = value;
            StackCount = stackCount;
        }

        public BoardCard Clone()
        {
            return new BoardCard
            {
                Id = Id,
                Kind = Kind,
                Value = Value,
                StackCount = StackCount
            };
        }

        public BoardCard CloneForPlacement()
        {
            return new BoardCard
            {
                Id = Guid.NewGuid().ToString("N"),
                Kind = Kind,
                Value = Value,
                StackCount = StackCount
            };
        }

        public bool IsVariable =>
            Kind is CardKind.Box or CardKind.DayCreature or CardKind.NightCreature;

        public bool IsConstant =>
            Kind is CardKind.PositiveConstant or CardKind.NegativeConstant or CardKind.One;

        public bool IsDraggableFromBoard => Kind != CardKind.Box;

        public bool IsPlayableFromHand => Kind is CardKind.DayCreature or CardKind.NightCreature
            or CardKind.PositiveConstant or CardKind.NegativeConstant;

        public int SignedValue => Kind switch
        {
            CardKind.PositiveConstant => Value,
            CardKind.NegativeConstant => -Value,
            CardKind.DayCreature => Value,
            CardKind.NightCreature => -Value,
            _ => 0
        };

        public bool MatchesKind(BoardCard other)
        {
            if (Kind != other.Kind)
            {
                return false;
            }

            return Kind switch
            {
                CardKind.Box => true,
                CardKind.One => true,
                CardKind.DivideTool => true,
                _ => Value == other.Value
            };
        }
    }
}
