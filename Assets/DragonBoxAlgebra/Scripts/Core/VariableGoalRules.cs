namespace DragonBoxAlgebra.Core
{
    /// <summary>Ch5+: positive x replaces the red box as the isolation goal.</summary>
    public static class VariableGoalRules
    {
        public const char GoalLetter = 'x';

        public static bool IsVariableXGoal(BoardCard card) =>
            card.Kind == CardKind.DayCreature && card.VariableLetter == GoalLetter;

        public static bool IsIsolationGoal(BoardCard card) =>
            card.Kind == CardKind.Box || IsVariableXGoal(card);

        public static bool IsPairVariable(BoardCard card) =>
            card.VariableLetter != '\0'
            && card.VariableLetter != GoalLetter
            && card.Kind is CardKind.DayCreature or CardKind.NightCreature;
    }
}
