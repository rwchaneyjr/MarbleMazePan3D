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
                Left.Cards.Add(card.Clone());
            }

            foreach (BoardCard card in right.Cards)
            {
                Right.Cards.Add(card.Clone());
            }
        }

        public BoardSide GetSide(string sideName) => sideName == "Left" ? Left : Right;

        public bool TryCombineOnSide(BoardSide side, int indexA, int indexB, out CombineActionType action)
        {
            action = default;
            if (indexA < 0 || indexB < 0 || indexA >= side.Cards.Count || indexB >= side.Cards.Count)
            {
                return false;
            }

            CombineActionType? resolved = CombineRules.GetCombineAction(side.Cards[indexA], side.Cards[indexB]);
            if (resolved == null)
            {
                return false;
            }

            action = resolved.Value;
            CombineRules.ApplyCombine(side, indexA, indexB, action);
            return true;
        }

        public bool TryAddBalanced(BoardCard card)
        {
            Left.Cards.Add(card.Clone());
            Right.Cards.Add(new BoardCard(card.Kind, card.Value));
            return true;
        }

        public void ResolveAllAutoCombines(out List<(string side, int a, int b, CombineActionType action)> resolved)
        {
            resolved = new List<(string, int, int, CombineActionType)>();
        }
    }
}
