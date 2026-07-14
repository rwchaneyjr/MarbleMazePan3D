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
        public int VisualTheme;
        /// <summary>Algebra variable letter for creature tiles (a, b, c, r, x). '\0' = x.</summary>
        public char VariableLetter;

        public BoardCard(CardKind kind, int value = 1, int stackCount = 1, int visualTheme = -1,
            char variableLetter = '\0')
        {
            Id = Guid.NewGuid().ToString("N");
            Kind = kind;
            Value = value;
            StackCount = stackCount;
            VisualTheme = visualTheme;
            VariableLetter = variableLetter;
        }

        public char ResolvedVariableLetter => VariableLetter != '\0' ? VariableLetter : 'x';

        public BoardCard Clone()
        {
            return new BoardCard
            {
                Id = Id,
                Kind = Kind,
                Value = Value,
                StackCount = StackCount,
                VisualTheme = VisualTheme,
                VariableLetter = VariableLetter
            };
        }

        public BoardCard CloneForPlacement()
        {
            return new BoardCard
            {
                Id = Guid.NewGuid().ToString("N"),
                Kind = Kind,
                Value = Value,
                StackCount = StackCount,
                VisualTheme = VisualTheme,
                VariableLetter = VariableLetter
            };
        }

        public bool IsVariable =>
            Kind is CardKind.Box or CardKind.DayCreature or CardKind.NightCreature;

        public bool IsConstant =>
            Kind is CardKind.PositiveConstant or CardKind.NegativeConstant or CardKind.One;

        public bool IsVariableXGoal => VariableGoalRules.IsVariableXGoal(this);

        public bool IsIsolationGoal => VariableGoalRules.IsIsolationGoal(this);

        public bool IsDraggableFromBoard => !IsIsolationGoal;

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
