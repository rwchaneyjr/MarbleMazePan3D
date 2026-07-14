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
            int leftBoxes = board.Left.Cards.Count(c => c.Kind == CardKind.Box);
            int rightBoxes = board.Right.Cards.Count(c => c.Kind == CardKind.Box);
            if (leftBoxes + rightBoxes != 1)
            {
                return false;
            }

            if (leftBoxes == 1)
            {
                return board.Left.Cards.Count == 1;
            }

            return board.Right.Cards.Count == 1;
        }

        /// <summary>Box on one side only; the opposite side is completely empty.</summary>
        public static bool IsReadyForSidesTogether(AlgebraBoard board)
        {
            if (board.Right.Cards.Count == 0)
            {
                return board.Left.Cards.Count == 1 && board.Left.Cards[0].Kind == CardKind.Box;
            }

            if (board.Left.Cards.Count == 0)
            {
                return board.Right.Cards.Count == 1 && board.Right.Cards[0].Kind == CardKind.Box;
            }

            return false;
        }

        /// <summary>Red box alone on one side with the other side empty.</summary>
        public static bool IsRedBoxAloneWinState(AlgebraBoard board) => IsReadyForSidesTogether(board);

        /// <summary>
        /// True when the puzzle may end: box alone, other side empty, no swirls or ? hole,
        /// and the player has made at least one move (no win on load).
        /// </summary>
        public static bool CanWin(AlgebraBoard board, int moves, bool hasPendingBalance, int pendingCancelCount,
            int activeMergeAnimations)
        {
            if (hasPendingBalance || pendingCancelCount > 0 || activeMergeAnimations > 0)
            {
                return false;
            }

            if (moves <= 0)
            {
                return false;
            }

            return IsRedBoxAloneWinState(board);
        }

        public static bool HasPendingOpposites(AlgebraBoard board)
        {
            return CombineRules.TryAutoCombine(board.Left, out _)
                || CombineRules.TryAutoCombine(board.Right, out _);
        }
    }
}
