using System.Linq;

namespace DragonBoxAlgebra.Core
{
    public static class WinChecker
    {
        public static bool IsBoxAlone(AlgebraBoard board)
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

        public static bool HasPendingOpposites(AlgebraBoard board)
        {
            return CombineRules.TryAutoCombine(board.Left, out _)
                || CombineRules.TryAutoCombine(board.Right, out _);
        }
    }
}
