using System.Linq;

namespace DragonBoxAlgebra.Core
{
    public static class WinChecker
    {
        public static bool IsBoxAlone(AlgebraBoard board)
        {
            int totalCards = board.Left.Cards.Count + board.Right.Cards.Count;
            if (totalCards != 1)
            {
                return false;
            }

            if (board.Left.Cards.Count == 1)
            {
                return board.Left.Cards[0].Kind == CardKind.Box;
            }

            return board.Right.Cards.Count == 1 && board.Right.Cards[0].Kind == CardKind.Box;
        }

        /// <summary>True when the red box is the only tile on its side (the other side may have cards).</summary>
        public static bool IsBoxAloneOnItsSide(AlgebraBoard board)
        {
            int leftGoals = board.Left.Cards.Count(c => VariableGoalRules.IsIsolationGoal(c));
            int rightGoals = board.Right.Cards.Count(c => VariableGoalRules.IsIsolationGoal(c));
            if (leftGoals + rightGoals != 1)
            {
                return false;
            }

            if (leftGoals == 1)
            {
                return board.Left.Cards.Count == 1;
            }

            return board.Right.Cards.Count == 1;
        }

        /// <summary>Positive x alone on its side (Ch5 goal tile).</summary>
        public static bool IsVariableXAloneOnItsSide(AlgebraBoard board)
        {
            int leftX = board.Left.Cards.Count(c => VariableGoalRules.IsVariableXGoal(c));
            int rightX = board.Right.Cards.Count(c => VariableGoalRules.IsVariableXGoal(c));
            if (leftX + rightX != 1)
            {
                return false;
            }

            if (leftX == 1)
            {
                return board.Left.Cards.Count == 1;
            }

            return board.Right.Cards.Count == 1;
        }

        /// <summary>Opposite side is empty, or only a lone 0 (addition: x = 0).</summary>
        public static bool IsEmptyOrZeroOnlySide(BoardSide side) =>
            side.Cards.Count == 0 || IsZeroOnlySide(side);

        public static bool IsZeroOnlySide(BoardSide side) =>
            side.Cards.Count == 1
            && side.Cards[0].Kind is CardKind.PositiveConstant or CardKind.NegativeConstant
            && side.Cards[0].Value == 0;

        /// <summary>Lone non-x variable letter (140–150 answer: x = a/b/c/r).</summary>
        public static bool IsLoneNonGoalVariableSide(BoardSide side) =>
            side.Cards.Count == 1
            && side.Cards[0].Kind is CardKind.DayCreature or CardKind.NightCreature
            && side.Cards[0].VariableLetter != '\0'
            && !VariableGoalRules.IsVariableXGoal(side.Cards[0]);

        public static bool IsEmptyZeroOrLetterAnswerSide(BoardSide side) =>
            IsEmptyOrZeroOnlySide(side) || IsLoneNonGoalVariableSide(side);

        /// <summary>Box on one side only; the opposite side is empty or only 0.</summary>
        public static bool IsReadyForSidesTogether(AlgebraBoard board)
        {
            if (IsEmptyOrZeroOnlySide(board.Right))
            {
                return board.Left.Cards.Count == 1 && VariableGoalRules.IsIsolationGoal(board.Left.Cards[0]);
            }

            if (IsEmptyOrZeroOnlySide(board.Left))
            {
                return board.Right.Cards.Count == 1 && VariableGoalRules.IsIsolationGoal(board.Right.Cards[0]);
            }

            return false;
        }

        /// <summary>Positive x alone on one side; opposite side empty, only 0, or (optional) a lone letter.</summary>
        public static bool IsVariableXReadyForSidesTogether(AlgebraBoard board, bool allowLoneVariableAnswer = false)
        {
            bool OppositeOk(BoardSide side) =>
                allowLoneVariableAnswer ? IsEmptyZeroOrLetterAnswerSide(side) : IsEmptyOrZeroOnlySide(side);

            if (OppositeOk(board.Right))
            {
                return board.Left.Cards.Count == 1 && VariableGoalRules.IsVariableXGoal(board.Left.Cards[0]);
            }

            if (OppositeOk(board.Left))
            {
                return board.Right.Cards.Count == 1 && VariableGoalRules.IsVariableXGoal(board.Right.Cards[0]);
            }

            return false;
        }

        /// <summary>Ch8: x alone on one side equals a single constant on the other (x = k).</summary>
        public static bool IsVariableXEqualsConstant(AlgebraBoard board)
        {
            if (board.Left.Cards.Count == 1
                && VariableGoalRules.IsVariableXGoal(board.Left.Cards[0])
                && board.Right.Cards.Count == 1
                && board.Right.Cards[0].Kind == CardKind.PositiveConstant
                && board.Right.Cards[0].Value > 0)
            {
                return true;
            }

            if (board.Right.Cards.Count == 1
                && VariableGoalRules.IsVariableXGoal(board.Right.Cards[0])
                && board.Left.Cards.Count == 1
                && board.Left.Cards[0].Kind == CardKind.PositiveConstant
                && board.Left.Cards[0].Value > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Ch8 win: x alone = fraction n/d (dice above the line, divisor below), e.g. x = 9/2.
        /// </summary>
        public static bool IsVariableXEqualsFraction(AlgebraBoard board)
        {
            return IsXEqualsFractionOnSides(board.Left, board.Right)
                || IsXEqualsFractionOnSides(board.Right, board.Left);
        }

        private static bool IsXEqualsFractionOnSides(BoardSide xSide, BoardSide fractionSide)
        {
            if (xSide.Cards.Count != 1 || !VariableGoalRules.IsVariableXGoal(xSide.Cards[0]))
            {
                return false;
            }

            if (xSide.HasDenominator)
            {
                return false;
            }

            if (fractionSide.Cards.Count != 1
                || fractionSide.Cards[0].Kind != CardKind.PositiveConstant
                || fractionSide.Cards[0].Value <= 0
                || !fractionSide.HasDenominator)
            {
                return false;
            }

            int denom = fractionSide.Denominator.Value.Value;
            return denom > 1 && fractionSide.Cards[0].Value % denom != 0;
        }

        /// <summary>Red box alone on one side with the other side empty.</summary>
        public static bool IsRedBoxAloneWinState(AlgebraBoard board) => IsReadyForSidesTogether(board);

        /// <summary>
        /// True when the puzzle may end: box alone, other side empty, no swirls or ? hole,
        /// and the player has made at least one move (no win on load).
        /// </summary>
        public static bool CanWin(AlgebraBoard board, int moves, bool hasPendingBalance, int pendingCancelCount,
            int activeMergeAnimations, bool allowOppositeCreatures = false, bool useVariableXGoal = false,
            bool useMultiplyAdditionWin = false, bool allowLoneVariableAnswer = false)
        {
            if (hasPendingBalance || pendingCancelCount > 0 || activeMergeAnimations > 0)
            {
                return false;
            }

            if (moves <= 0)
            {
                return false;
            }

            if (useMultiplyAdditionWin)
            {
                return IsVariableXEqualsConstant(board) || IsVariableXEqualsFraction(board);
            }

            if (useVariableXGoal)
            {
                return IsVariableXReadyForSidesTogether(board, allowLoneVariableAnswer);
            }

            return allowOppositeCreatures
                ? IsBoxAloneOnItsSide(board)
                : IsRedBoxAloneWinState(board);
        }

        public static bool HasPendingOpposites(AlgebraBoard board)
        {
            return CombineRules.TryAutoCombine(board.Left, out _)
                || CombineRules.TryAutoCombine(board.Right, out _);
        }
    }
}
