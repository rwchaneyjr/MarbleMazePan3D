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

        /// <summary>True when the board has exactly one red box (other tiles may remain).</summary>
        public static bool HasSingleBox(AlgebraBoard board)
        {
            int leftBoxes = board.Left.Cards.Count(c => c.Kind == CardKind.Box);
            int rightBoxes = board.Right.Cards.Count(c => c.Kind == CardKind.Box);
            return leftBoxes + rightBoxes == 1;
        }

        public static bool HasPendingOpposites(AlgebraBoard board)
        {
            return CombineRules.TryAutoCombine(board.Left, out _)
                || CombineRules.TryAutoCombine(board.Right, out _);
        }
    }
}
