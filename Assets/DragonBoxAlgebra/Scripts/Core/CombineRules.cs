using System.Collections.Generic;

namespace DragonBoxAlgebra.Core
{
    public static class CombineRules
    {
        public static bool CanCombine(BoardCard a, BoardCard b)
        {
            if (a.Kind == CardKind.Box || b.Kind == CardKind.Box)
            {
                return false;
            }

            if (IsOppositeVariablePair(a, b))
            {
                return true;
            }

            if (IsOppositeConstantPair(a, b))
            {
                return true;
            }

            return false;
        }

        public static bool TryAutoCombine(BoardSide side, out List<(int indexA, int indexB)> pairs)
        {
            pairs = new List<(int, int)>();

            for (int i = 0; i < side.Cards.Count; i++)
            {
                for (int j = i + 1; j < side.Cards.Count; j++)
                {
                    if (CanCombine(side.Cards[i], side.Cards[j]))
                    {
                        pairs.Add((i, j));
                    }
                }
            }

            return pairs.Count > 0;
        }

        public static void RemovePair(BoardSide side, int indexA, int indexB)
        {
            int first = indexA < indexB ? indexA : indexB;
            int second = indexA < indexB ? indexB : indexA;
            side.Cards.RemoveAt(second);
            side.Cards.RemoveAt(first);
        }

        private static bool IsOppositeVariablePair(BoardCard a, BoardCard b)
        {
            return (a.Kind == CardKind.DayCreature && b.Kind == CardKind.NightCreature && a.Value == b.Value)
                || (a.Kind == CardKind.NightCreature && b.Kind == CardKind.DayCreature && a.Value == b.Value);
        }

        private static bool IsOppositeConstantPair(BoardCard a, BoardCard b)
        {
            return (a.Kind == CardKind.PositiveConstant && b.Kind == CardKind.NegativeConstant && a.Value == b.Value)
                || (a.Kind == CardKind.NegativeConstant && b.Kind == CardKind.PositiveConstant && a.Value == b.Value);
        }
    }
}
