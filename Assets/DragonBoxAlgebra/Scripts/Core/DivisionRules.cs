namespace DragonBoxAlgebra.Core
{
    /// <summary>
    /// Divide both sides by a positive integer: remove matching coefficient of x (a·x),
    /// and divide every remaining constant evenly.
    /// </summary>
    public static class DivisionRules
    {
        public static bool CanDivideBothSides(AlgebraBoard board, int divisor)
        {
            if (divisor <= 1)
            {
                return false;
            }

            return CanDivideSide(board.Left, divisor) && CanDivideSide(board.Right, divisor);
        }

        public static bool TryDivideBothSides(AlgebraBoard board, int divisor)
        {
            if (!CanDivideBothSides(board, divisor))
            {
                return false;
            }

            ApplyDivideSide(board.Left, divisor);
            ApplyDivideSide(board.Right, divisor);
            return true;
        }

        /// <summary>
        /// Index of PositiveConstant coefficient sitting immediately before goal x, or -1.
        /// </summary>
        public static int FindCoefficientIndex(BoardSide side, int divisor)
        {
            for (int i = 0; i < side.Cards.Count - 1; i++)
            {
                BoardCard coeff = side.Cards[i];
                BoardCard next = side.Cards[i + 1];
                if (coeff.Kind == CardKind.PositiveConstant
                    && coeff.Value == divisor
                    && VariableGoalRules.IsVariableXGoal(next))
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool IsCoefficientTimesXPair(BoardCard left, BoardCard right) =>
            left.Kind == CardKind.PositiveConstant
            && left.Value > 1
            && VariableGoalRules.IsVariableXGoal(right);

        private static bool CanDivideSide(BoardSide side, int divisor)
        {
            if (side.Cards.Count == 0)
            {
                return false;
            }

            int coeffIndex = FindCoefficientIndex(side, divisor);
            bool changes = coeffIndex >= 0;
            bool onlyGoal = true;

            for (int i = 0; i < side.Cards.Count; i++)
            {
                BoardCard card = side.Cards[i];

                if (VariableGoalRules.IsVariableXGoal(card) || VariableGoalRules.IsIsolationGoal(card))
                {
                    // Bare x (coeff 1) is fine; a different coefficient blocks this divisor.
                    if (i > 0
                        && side.Cards[i - 1].Kind == CardKind.PositiveConstant
                        && side.Cards[i - 1].Value != divisor
                        && IsCoefficientTimesXPair(side.Cards[i - 1], card))
                    {
                        return false;
                    }

                    continue;
                }

                onlyGoal = false;

                if (i == coeffIndex)
                {
                    continue;
                }

                if (card.Kind is CardKind.PositiveConstant or CardKind.NegativeConstant)
                {
                    if (card.Value % divisor != 0)
                    {
                        return false;
                    }

                    changes = true;
                    continue;
                }

                // Other creatures / tools block divide.
                return false;
            }

            // Only x (coefficient 1): OK — the other side carries the numeric change.
            return onlyGoal || changes;
        }

        private static void ApplyDivideSide(BoardSide side, int divisor)
        {
            int coeffIndex = FindCoefficientIndex(side, divisor);
            if (coeffIndex >= 0)
            {
                // a÷a → 1 (DragonBox die / identity), kept beside x until merged.
                side.Cards[coeffIndex] = new BoardCard(CardKind.One, 1);
            }

            for (int i = 0; i < side.Cards.Count; i++)
            {
                BoardCard card = side.Cards[i];
                if (card.Kind is not (CardKind.PositiveConstant or CardKind.NegativeConstant))
                {
                    continue;
                }

                int newValue = card.Value / divisor;
                if (newValue <= 0)
                {
                    side.Cards.RemoveAt(i);
                    i--;
                    continue;
                }

                // Exact self-division of a lone constant → 1.
                if (newValue == 1 && card.Value == divisor)
                {
                    side.Cards[i] = new BoardCard(CardKind.One, 1);
                    continue;
                }

                card.Value = newValue;
                side.Cards[i] = card;
            }
        }
    }
}
