using System.Collections.Generic;

namespace DragonBoxAlgebra.Core
{
    public class BoardSide
    {
        public readonly List<BoardCard> Cards = new();

        public BoardSide Clone()
        {
            var copy = new BoardSide();
            foreach (BoardCard card in Cards)
            {
                copy.Cards.Add(card);
            }

            return copy;
        }
    }
}
