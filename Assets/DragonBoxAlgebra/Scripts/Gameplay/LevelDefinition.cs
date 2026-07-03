using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    [Serializable]
    public class LevelDefinition
    {
        public string Title;
        public List<CardKind> LeftCards = new();
        public List<CardKind> RightCards = new();
        public List<int> LeftValues = new();
        public List<int> RightValues = new();
        public List<CardKind> HandCards = new();
        public List<int> HandValues = new();
        public int ParMoves = 6;
        public int ParCards = 2;
        public bool SpinPreplacedOppositesAtStart;

        public BoardSide BuildSide(List<CardKind> kinds, List<int> values)
        {
            var side = new BoardSide();
            for (int i = 0; i < kinds.Count; i++)
            {
                int value = values != null && i < values.Count ? values[i] : 1;
                side.Cards.Add(new BoardCard(kinds[i], value));
            }

            return side;
        }

        public List<BoardCard> BuildHand()
        {
            var hand = new List<BoardCard>();
            for (int i = 0; i < HandCards.Count; i++)
            {
                int value = HandValues != null && i < HandValues.Count ? HandValues[i] : 1;
                hand.Add(new BoardCard(HandCards[i], value));
            }

            return hand;
        }
    }
}
