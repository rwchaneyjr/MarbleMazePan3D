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

        /// <summary>Returns Left/Right when that side has only the red box; otherwise null.</summary>
        public static string GetSideWithBoxAlone(AlgebraBoard board)
        {
            if (board.Left.Cards.Count == 1 && board.Left.Cards[0].Kind == CardKind.Box)
            {
                return "Left";
            }

            if (board.Right.Cards.Count == 1 && board.Right.Cards[0].Kind == CardKind.Box)
            {
                return "Right";
            }

            return null;
        }

        public static string OppositeSide(string sideName) => sideName == "Left" ? "Right" : "Left";

        public static bool HasPendingOpposites(AlgebraBoard board)
        {
            return CombineRules.TryAutoCombine(board.Left, out _)
                || CombineRules.TryAutoCombine(board.Right, out _);
        }
    }
}
