using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public class GameSnapshot
    {
        public List<BoardCard> Left = new();
        public List<BoardCard> Hand = new();
        public List<BoardCard> Right = new();
        public List<PendingCancelMarker> PendingCancels = new();
        public List<BalancePending> PendingBalances = new();
        public int Moves;
        public int CardsPlayed;

        public List<int> SpentHandIndices = new();

        public static GameSnapshot Capture(AlgebraBoard board, List<BoardCard> hand, MoveTracker moves,
            IReadOnlyList<BalancePending> pendingBalances, IReadOnlyList<PendingCancelMarker> pendingCancels,
            IReadOnlyCollection<int> spentHandIndices)
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

            foreach (PendingCancelMarker marker in pendingCancels)
            {
                snapshot.PendingCancels.Add(new PendingCancelMarker
                {
                    SideName = marker.SideName,
                    CardIdA = marker.CardIdA,
                    CardIdB = marker.CardIdB,
                    SwirlOnly = marker.SwirlOnly
                });
            }

            if (pendingBalances != null)
            {
                foreach (BalancePending pending in pendingBalances)
                {
                    if (pending == null)
                    {
                        continue;
                    }

                    snapshot.PendingBalances.Add(new BalancePending
                    {
                        PlacedSide = pending.PlacedSide,
                        PlacedIndex = pending.PlacedIndex,
                        HandIndex = pending.HandIndex,
                        HoleInsertIndex = pending.HoleInsertIndex,
                        Card = pending.Card.Clone()
                    });
                }
            }

            foreach (int spentIndex in spentHandIndices)
            {
                snapshot.SpentHandIndices.Add(spentIndex);
            }

            return snapshot;
        }

        public void Apply(AlgebraBoard board, List<BoardCard> hand, MoveTracker moves,
            List<BalancePending> pendingBalances, List<PendingCancelMarker> pendingCancels,
            HashSet<int> spentHandIndices)
        {
            board.Left.Cards.Clear();
            board.Right.Cards.Clear();
            hand.Clear();
            pendingBalances.Clear();
            pendingCancels.Clear();

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

            foreach (PendingCancelMarker marker in PendingCancels)
            {
                pendingCancels.Add(new PendingCancelMarker
                {
                    SideName = marker.SideName,
                    CardIdA = marker.CardIdA,
                    CardIdB = marker.CardIdB,
                    SwirlOnly = marker.SwirlOnly
                });
            }

            moves.Moves = Moves;
            moves.CardsPlayed = CardsPlayed;

            foreach (BalancePending pending in PendingBalances)
            {
                pendingBalances.Add(new BalancePending
                {
                    PlacedSide = pending.PlacedSide,
                    PlacedIndex = pending.PlacedIndex,
                    HandIndex = pending.HandIndex,
                    HoleInsertIndex = pending.HoleInsertIndex,
                    Card = pending.Card.Clone()
                });
            }

            spentHandIndices.Clear();
            spentHandIndices.UnionWith(SpentHandIndices);
        }
    }
}
