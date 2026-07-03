using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public class GameSnapshot
    {
        public List<BoardCard> Left = new();
        public List<BoardCard> Hand = new();
        public List<BoardCard> Right = new();
        public int Moves;
        public int CardsPlayed;
        public bool HasPendingBalance;
        public string PendingPlacedSide;
        public BoardCard PendingCard;

        public static GameSnapshot Capture(AlgebraBoard board, List<BoardCard> hand, MoveTracker moves,
            BalancePending pendingBalance)
        {
            var snapshot = new GameSnapshot
            {
                Moves = moves.Moves,
                CardsPlayed = moves.CardsPlayed
            };

            foreach (BoardCard card in board.Left.Cards)
            {
                snapshot.Left.Add(card.Clone());
            }

            foreach (BoardCard card in board.Right.Cards)
            {
                snapshot.Right.Add(card.Clone());
            }

            foreach (BoardCard card in hand)
            {
                snapshot.Hand.Add(card.Clone());
            }

            if (pendingBalance != null)
            {
                snapshot.HasPendingBalance = true;
                snapshot.PendingPlacedSide = pendingBalance.PlacedSide;
                snapshot.PendingCard = pendingBalance.Card.Clone();
            }

            return snapshot;
        }

        public void Apply(AlgebraBoard board, List<BoardCard> hand, MoveTracker moves, out BalancePending pendingBalance)
        {
            board.Left.Cards.Clear();
            board.Right.Cards.Clear();
            hand.Clear();
            pendingBalance = null;

            foreach (BoardCard card in Left)
            {
                board.Left.Cards.Add(card.Clone());
            }

            foreach (BoardCard card in Right)
            {
                board.Right.Cards.Add(card.Clone());
            }

            foreach (BoardCard card in Hand)
            {
                hand.Add(card.Clone());
            }

            moves.Moves = Moves;
            moves.CardsPlayed = CardsPlayed;

            if (HasPendingBalance)
            {
                pendingBalance = new BalancePending
                {
                    PlacedSide = PendingPlacedSide,
                    Card = PendingCard.Clone()
                };
            }
        }
    }
}
