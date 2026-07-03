using System.Collections.Generic;

namespace DragonBoxAlgebra.Core
{
    public static class CombineRules
    {
        public static bool CanCombine(BoardCard a, BoardCard b) => GetCombineAction(a, b) != null;

        public static CombineActionType? GetCombineAction(BoardCard a, BoardCard b)
        {
            if (a.Kind == CardKind.Box || b.Kind == CardKind.Box)
            {
                return null;
            }

            if (IsOppositePair(a, b))
            {
                return CombineActionType.OppositeCancel;
            }

            return null;
        }

        public static bool TryAutoCombine(BoardSide side, out List<(int indexA, int indexB, CombineActionType action)> pairs)
        {
            pairs = new List<(int, int, CombineActionType)>();

            for (int i = 0; i < side.Cards.Count; i++)
            {
                for (int j = i + 1; j < side.Cards.Count; j++)
                {
                    CombineActionType? action = GetCombineAction(side.Cards[i], side.Cards[j]);
                    if (action == CombineActionType.OppositeCancel)
                    {
                        pairs.Add((i, j, action.Value));
                    }
                }
            }

            return pairs.Count > 0;
        }

        public static void ApplyCombine(BoardSide side, int indexA, int indexB, CombineActionType action)
        {
            if (action != CombineActionType.OppositeCancel)
            {
                return;
            }

            int first = indexA < indexB ? indexA : indexB;
            int second = indexA < indexB ? indexB : indexA;
            side.Cards.RemoveAt(second);
            side.Cards.RemoveAt(first);
        }

        public static void RemovePair(BoardSide side, int indexA, int indexB)
        {
            int first = indexA < indexB ? indexA : indexB;
            int second = indexA < indexB ? indexB : indexA;
            side.Cards.RemoveAt(second);
            side.Cards.RemoveAt(first);
        }

        private static bool IsOppositePair(BoardCard a, BoardCard b)
        {
            return (a.Kind == CardKind.DayCreature && b.Kind == CardKind.NightCreature && a.Value == b.Value)
                || (a.Kind == CardKind.NightCreature && b.Kind == CardKind.DayCreature && a.Value == b.Value)
                || (a.Kind == CardKind.PositiveConstant && b.Kind == CardKind.NegativeConstant && a.Value == b.Value)
                || (a.Kind == CardKind.NegativeConstant && b.Kind == CardKind.PositiveConstant && a.Value == b.Value);
        }
    }
}
