using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// When the right side starts with 2–3 creatures, fold matching day/night pairs
    /// with the left so only 1–2 playable tiles remain on the right.
    /// </summary>
    public static class BoardFoldRules
    {
        private const int MaxRightCreatures = 2;

        public static void FoldMatchingPairsForPlayableRight(AlgebraBoard board)
        {
            while (ShouldKeepFolding(board) && TryFoldOneMatchingPair(board))
            {
            }
        }

        private static bool ShouldKeepFolding(AlgebraBoard board)
        {
            if (WinChecker.IsRedBoxAloneWinState(board))
            {
                return false;
            }

            int rightCreatures = CountCreatures(board.Right);
            return rightCreatures > MaxRightCreatures && HasFoldablePair(board);
        }

        private static bool HasFoldablePair(AlgebraBoard board) =>
            (FindCreatureIndex(board.Left, CardKind.DayCreature) >= 0
             && FindCreatureIndex(board.Right, CardKind.NightCreature) >= 0)
            || (FindCreatureIndex(board.Left, CardKind.NightCreature) >= 0
                && FindCreatureIndex(board.Right, CardKind.DayCreature) >= 0);

        private static bool TryFoldOneMatchingPair(AlgebraBoard board)
        {
            if (TryFoldDayOnLeftNightOnRight(board))
            {
                return true;
            }

            return TryFoldNightOnLeftDayOnRight(board);
        }

        private static bool TryFoldDayOnLeftNightOnRight(AlgebraBoard board)
        {
            int dayIndex = FindCreatureIndex(board.Left, CardKind.DayCreature);
            int nightIndex = FindCreatureIndex(board.Right, CardKind.NightCreature);
            if (dayIndex < 0 || nightIndex < 0)
            {
                return false;
            }

            CombineRules.RemoveCardById(board.Left, board.Left.Cards[dayIndex].Id);
            CombineRules.RemoveCardById(board.Right, board.Right.Cards[nightIndex].Id);
            return true;
        }

        private static bool TryFoldNightOnLeftDayOnRight(AlgebraBoard board)
        {
            int nightIndex = FindCreatureIndex(board.Left, CardKind.NightCreature);
            int dayIndex = FindCreatureIndex(board.Right, CardKind.DayCreature);
            if (nightIndex < 0 || dayIndex < 0)
            {
                return false;
            }

            CombineRules.RemoveCardById(board.Left, board.Left.Cards[nightIndex].Id);
            CombineRules.RemoveCardById(board.Right, board.Right.Cards[dayIndex].Id);
            return true;
        }

        private static int FindCreatureIndex(BoardSide side, CardKind kind)
        {
            for (int i = 0; i < side.Cards.Count; i++)
            {
                if (side.Cards[i].Kind == kind)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int CountCreatures(BoardSide side)
        {
            int count = 0;
            foreach (BoardCard card in side.Cards)
            {
                if (card.Kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
