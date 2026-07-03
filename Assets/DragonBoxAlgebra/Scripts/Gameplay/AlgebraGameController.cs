using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public struct CombineEvent
    {
        public string SideName;
        public CombineActionType Action;
        public int IndexA;
        public int IndexB;
    }

    public class AlgebraGameController
    {
        public event Action BoardChanged;
        public event Action HandChanged;
        public event Action<int, int> LevelCompleted;
        public event Action<int, int> LevelLoaded;
        public event Action<string> MessageChanged;
        public event Action<CombineEvent> CombineOccurred;

        public AlgebraBoard Board { get; } = new();
        public MoveTracker Moves { get; } = new();
        public IReadOnlyList<BoardCard> Hand => _hand;
        public IReadOnlyList<PendingCancelMarker> PendingCancels => _pendingCancels;
        public bool CanUndo => _undoStack.Count > 0;
        public bool HasPendingBalance => _pendingBalance != null;
        public BalancePending PendingBalance => _pendingBalance;

        private readonly List<BoardCard> _hand = new();
        private readonly List<PendingCancelMarker> _pendingCancels = new();
        private readonly Stack<GameSnapshot> _undoStack = new();
        private GameSnapshot _initialSnapshot;
        private BalancePending _pendingBalance;
        private int _levelIndex;
        private bool _levelComplete;

        public int LevelIndex => _levelIndex;
        public int LevelCount => LevelLibrary.Levels.Count;
        public LevelDefinition CurrentLevel => LevelLibrary.Levels[_levelIndex];

        public bool IsCardPendingCancel(string cardId)
        {
            foreach (PendingCancelMarker marker in _pendingCancels)
            {
                if (marker.CardIdA == cardId || marker.CardIdB == cardId)
                {
                    return true;
                }
            }

            return false;
        }

        public void LoadLevel(int index)
        {
            _levelIndex = Math.Clamp(index, 0, LevelLibrary.Levels.Count - 1);
            LevelDefinition level = CurrentLevel;

            Board.Reset(level.BuildSide(level.LeftCards, level.LeftValues),
                level.BuildSide(level.RightCards, level.RightValues));

            _hand.Clear();
            _hand.AddRange(level.BuildHand());
            HandRules.DedupeFlipFamilies(_hand);
            Moves.Reset();
            _undoStack.Clear();
            _levelComplete = false;
            _pendingBalance = null;
            _pendingCancels.Clear();
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _pendingCancels);

            LevelLoaded?.Invoke(_levelIndex + 1, LevelCount);
            ActivatePreplacedOppositePairs();
            ResolveCombines();
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _pendingCancels);
            BoardChanged?.Invoke();
            HandChanged?.Invoke();
            MessageChanged?.Invoke(_pendingCancels.Count > 0 && _hand.Count == 0
                ? "Click the spinning * to dismiss the creatures. Leave the red box alone!"
                : "Drag a tile to one side. A ? appears on the other side. Drag the same tile to the ? to balance. " +
                  "When light meets dark, a spinning * appears — click it to dismiss.");
        }

        public void LoadNextLevel()
        {
            if (_levelIndex + 1 < LevelLibrary.Levels.Count)
            {
                LoadLevel(_levelIndex + 1);
            }
        }

        public void RestartLevel()
        {
            LoadLevel(_levelIndex);
        }

        public void RewindLevel()
        {
            if (_initialSnapshot == null)
            {
                return;
            }

            _initialSnapshot.Apply(Board, _hand, Moves, out _pendingBalance, _pendingCancels);
            _undoStack.Clear();
            _levelComplete = false;
            BoardChanged?.Invoke();
            HandChanged?.Invoke();
            MessageChanged?.Invoke("Rewound to the start of the level.");
        }

        public void Undo()
        {
            if (_undoStack.Count == 0 || _levelComplete)
            {
                return;
            }

            _undoStack.Pop().Apply(Board, _hand, Moves, out _pendingBalance, _pendingCancels);
            _levelComplete = false;
            BoardChanged?.Invoke();
            HandChanged?.Invoke();
            MessageChanged?.Invoke("Undid the last move.");
        }

        public bool TryFlipHandCard(int handIndex)
        {
            if (_levelComplete || handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            if (_pendingBalance != null && handIndex != _pendingBalance.HandIndex)
            {
                MessageChanged?.Invoke("Fill the balance hole first.");
                return false;
            }

            BoardCard card = _hand[handIndex];
            if (!CardFlipRules.CanFlip(card.Kind))
            {
                MessageChanged?.Invoke("That card cannot flip.");
                return false;
            }

            PushUndo();
            _hand[handIndex] = CardFlipRules.Flip(card);
            HandChanged?.Invoke();
            MessageChanged?.Invoke(CardFlipRules.IsLight(_hand[handIndex])
                ? "Flipped to yellow (light). Click again for dark."
                : "Flipped to dark. Click again for yellow (light).");
            return true;
        }

        public bool TryCombine(string sideName, int indexA, int indexB)
        {
            if (_levelComplete)
            {
                return false;
            }

            if (_pendingBalance != null)
            {
                MessageChanged?.Invoke("Fill the balance hole first.");
                return false;
            }

            BoardSide side = Board.GetSide(sideName);
            if (indexA < 0 || indexB < 0 || indexA >= side.Cards.Count || indexB >= side.Cards.Count)
            {
                return false;
            }

            CombineActionType? action = CombineRules.GetCombineAction(side.Cards[indexA], side.Cards[indexB]);
            if (action == null)
            {
                MessageChanged?.Invoke("Those cards cannot combine. Drag one card onto another on the same side.");
                return false;
            }

            if (action == CombineActionType.OppositeCancel)
            {
                PushUndo();
                TryCreateCancelMarker(sideName, side.Cards[indexA].Id, side.Cards[indexB].Id);
                MessageChanged?.Invoke("A spinning * appeared — click it to dismiss the pair.");
                BoardChanged?.Invoke();
                CheckWin();
                return true;
            }

            PushUndo();
            if (!Board.TryCombineOnSide(side, indexA, indexB, out CombineActionType resolved))
            {
                PopUndoWithoutApply();
                MessageChanged?.Invoke("Those cards cannot combine. Drag one card onto another on the same side.");
                return false;
            }

            Moves.RegisterCombine();
            CombineOccurred?.Invoke(new CombineEvent
            {
                SideName = sideName,
                Action = resolved,
                IndexA = indexA,
                IndexB = indexB
            });

            ResolveCombines();
            return true;
        }

        public bool TryDismissCancelMarker(int markerIndex)
        {
            if (_levelComplete || _pendingBalance != null)
            {
                return false;
            }

            if (markerIndex < 0 || markerIndex >= _pendingCancels.Count)
            {
                return false;
            }

            PendingCancelMarker marker = _pendingCancels[markerIndex];
            BoardSide side = Board.GetSide(marker.SideName);
            if (!SideContainsBothCards(side, marker.CardIdA, marker.CardIdB))
            {
                _pendingCancels.RemoveAt(markerIndex);
                BoardChanged?.Invoke();
                return false;
            }

            PushUndo();
            CombineRules.RemovePairById(side, marker.CardIdA, marker.CardIdB);
            _pendingCancels.RemoveAt(markerIndex);
            Moves.RegisterCombine();
            CombineOccurred?.Invoke(new CombineEvent
            {
                SideName = marker.SideName,
                Action = CombineActionType.OppositeCancel,
                IndexA = -1,
                IndexB = -1
            });

            MessageChanged?.Invoke("Pair dismissed.");
            BoardChanged?.Invoke();
            CheckWin();
            return true;
        }

        public bool TryPlayFromHand(int handIndex, string targetSide)
        {
            if (_levelComplete || handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            BoardCard template = _hand[handIndex];
            if (template.Kind == CardKind.DivideTool || template.Kind == CardKind.One)
            {
                MessageChanged?.Invoke("Only light/dark cards and dice can be played.");
                return false;
            }

            if (_pendingBalance != null)
            {
                return TryCompleteBalance(handIndex, targetSide, template);
            }

            return TryStartBalance(handIndex, targetSide, template);
        }

        public bool TryPlayFromHand(int handIndex)
        {
            return TryPlayFromHand(handIndex, "Left");
        }

        private bool TryStartBalance(int handIndex, string targetSide, BoardCard template)
        {
            PushUndo();
            BoardSide placedSide = Board.GetSide(targetSide);
            placedSide.Cards.Add(template.Clone());
            _pendingBalance = new BalancePending
            {
                Card = template.Clone(),
                PlacedSide = targetSide,
                HandIndex = handIndex
            };

            ActivateOppositePairForCard(targetSide, placedSide.Cards.Count - 1);
            MessageChanged?.Invoke("? appeared on the other side — drag the same tile there.");
            BoardChanged?.Invoke();
            ResolveCombines();
            return true;
        }

        private bool TryCompleteBalance(int handIndex, string targetSide, BoardCard template)
        {
            if (handIndex != _pendingBalance.HandIndex)
            {
                MessageChanged?.Invoke("Use the same hand card to fill the hole.");
                return false;
            }

            if (targetSide != _pendingBalance.HoleSide)
            {
                MessageChanged?.Invoke("Drag the same card to the hole on the other side.");
                return false;
            }

            if (!_pendingBalance.Matches(template))
            {
                MessageChanged?.Invoke("The card must match the hole. Click to flip light/dark if needed.");
                return false;
            }

            PushUndo();
            BoardSide balancedSide = Board.GetSide(targetSide);
            balancedSide.Cards.Add(template.Clone());
            _hand.RemoveAt(handIndex);
            _pendingBalance = null;
            HandChanged?.Invoke();

            ActivateOppositePairForCard(targetSide, balancedSide.Cards.Count - 1);

            Moves.RegisterBalancedPlay();
            MessageChanged?.Invoke("Balanced! Click the spinning * to dismiss opposites.");
            ResolveCombines();
            return true;
        }

        private void ResolveCombines()
        {
            BoardChanged?.Invoke();
            CheckWin();
        }

        private void CheckWin()
        {
            if (_pendingBalance != null)
            {
                return;
            }

            if (!WinChecker.IsBoxAlone(Board))
            {
                return;
            }

            if (HasPendingCancelsOnBoxSide())
            {
                MessageChanged?.Invoke("The box is almost alone — click the spinning * on its side.");
                return;
            }

            _levelComplete = true;
            int stars = Moves.CalculateStars(CurrentLevel);
            LevelCompleted?.Invoke(stars, Moves.Moves);
            MessageChanged?.Invoke("You win! The red box is alone.");
        }

        private void PushUndo()
        {
            _undoStack.Push(GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _pendingCancels));
        }

        private void ActivateOppositePairForCard(string sideName, int cardIndex)
        {
            BoardSide side = Board.GetSide(sideName);
            if (cardIndex < 0 || cardIndex >= side.Cards.Count)
            {
                return;
            }

            int partner = CombineRules.FindOppositePartnerIndex(side, cardIndex);
            if (partner >= 0)
            {
                TryCreateCancelMarker(sideName, side.Cards[cardIndex].Id, side.Cards[partner].Id);
            }
        }

        private void ActivatePreplacedOppositePairs()
        {
            ActivateAllOppositePairsOnSide("Left");
            ActivateAllOppositePairsOnSide("Right");
        }

        private void ActivateAllOppositePairsOnSide(string sideName)
        {
            BoardSide side = Board.GetSide(sideName);
            for (int i = 0; i < side.Cards.Count; i++)
            {
                int partner = CombineRules.FindOppositePartnerIndex(side, i);
                if (partner > i)
                {
                    TryCreateCancelMarker(sideName, side.Cards[i].Id, side.Cards[partner].Id);
                }
            }
        }

        private void TryCreateCancelMarker(string sideName, string cardIdA, string cardIdB)
        {
            foreach (PendingCancelMarker marker in _pendingCancels)
            {
                if (marker.SideName != sideName)
                {
                    continue;
                }

                if ((marker.CardIdA == cardIdA && marker.CardIdB == cardIdB)
                    || (marker.CardIdA == cardIdB && marker.CardIdB == cardIdA))
                {
                    return;
                }
            }

            _pendingCancels.Add(new PendingCancelMarker
            {
                SideName = sideName,
                CardIdA = cardIdA,
                CardIdB = cardIdB
            });
        }

        private bool HasPendingCancelsOnBoxSide()
        {
            string boxSide = GetBoxSideName();
            if (boxSide == null)
            {
                return false;
            }

            foreach (PendingCancelMarker marker in _pendingCancels)
            {
                if (marker.SideName == boxSide)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetBoxSideName()
        {
            foreach (BoardCard card in Board.Left.Cards)
            {
                if (card.Kind == CardKind.Box)
                {
                    return "Left";
                }
            }

            foreach (BoardCard card in Board.Right.Cards)
            {
                if (card.Kind == CardKind.Box)
                {
                    return "Right";
                }
            }

            return null;
        }

        private static bool SideContainsBothCards(BoardSide side, string cardIdA, string cardIdB)
        {
            bool hasA = false;
            bool hasB = false;
            foreach (BoardCard card in side.Cards)
            {
                if (card.Id == cardIdA)
                {
                    hasA = true;
                }

                if (card.Id == cardIdB)
                {
                    hasB = true;
                }
            }

            return hasA && hasB;
        }

        private void PopUndoWithoutApply()
        {
            if (_undoStack.Count > 0)
            {
                _undoStack.Pop();
            }
        }
    }
}
