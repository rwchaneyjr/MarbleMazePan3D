using System.Collections.Generic;

namespace DragonBoxAlgebra.Core
{
    public class AlgebraBoard
    {
        public BoardSide Left { get; } = new();
        public BoardSide Right { get; } = new();

        public void Reset(BoardSide left, BoardSide right)
        {
            Left.Cards.Clear();
            Right.Cards.Clear();

            foreach (BoardCard card in left.Cards)
            {
                Left.Cards.Add(card);
            }

            foreach (BoardCard card in right.Cards)
            {
                Right.Cards.Add(card);
            }
        }

        public bool TryCombineOnSide(BoardSide side, int indexA, int indexB)
        {
            if (indexA < 0 || indexB < 0 || indexA >= side.Cards.Count || indexB >= side.Cards.Count)
            {
                return false;
            }

            if (!CombineRules.CanCombine(side.Cards[indexA], side.Cards[indexB]))
            {
                return false;
            }

            CombineRules.RemovePair(side, indexA, indexB);
            return true;
        }

        public bool TryAddBalanced(BoardCard card, bool toLeftFirst = true)
        {
            if (toLeftFirst)
            {
                Left.Cards.Add(card);
                Right.Cards.Add(new BoardCard(card.Kind, card.Value));
            }
            else
            {
                Right.Cards.Add(card);
                Left.Cards.Add(new BoardCard(card.Kind, card.Value));
            }

            return true;
        }

        public void ResolveAllAutoCombines()
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                changed |= ResolveSide(Left);
                changed |= ResolveSide(Right);
            }
        }

        private static bool ResolveSide(BoardSide side)
        {
            if (!CombineRules.TryAutoCombine(side, out List<(int indexA, int indexB)> pairs))
            {
                return false;
            }

            (int indexA, int indexB) = pairs[0];
            CombineRules.RemovePair(side, indexA, indexB);
            return true;
        }
    }
}
