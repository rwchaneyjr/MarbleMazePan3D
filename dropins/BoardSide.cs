using System.Collections.Generic;

namespace DragonBoxAlgebra.Core
{
    public class BoardSide
    {
        public readonly List<BoardCard> Cards = new();

        /// <summary>Card sitting below the division line (DragonBox fraction denominator).</summary>
        public BoardCard? Denominator;

        public bool HasDenominator => Denominator.HasValue;

        public void ClearDenominator() => Denominator = null;

        public BoardSide Clone()
        {
            var copy = new BoardSide();
            foreach (BoardCard card in Cards)
            {
                copy.Cards.Add(card);
            }

            if (Denominator.HasValue)
            {
                copy.Denominator = Denominator.Value.Clone();
            }

            return copy;
        }
    }
}
