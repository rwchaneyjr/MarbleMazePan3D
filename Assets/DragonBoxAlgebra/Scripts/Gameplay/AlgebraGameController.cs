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
        public bool CanUndo => _undoStack.Count > 0;
        public bool HasPendingBalance => _pendingBalance != null;
        public BalancePending PendingBalance => _pendingBalance;

        private readonly List<BoardCard> _hand = new();
        private readonly Stack<GameSnapshot> _undoStack = new();
        private GameSnapshot _initialSnapshot;
        private BalancePending _pendingBalance;
        private readonly HashSet<string> _spinningCardIds = new();
        private int _levelIndex;
        private bool _levelComplete;

        public int LevelIndex => _levelIndex;
        public int LevelCount => LevelLibrary.Levels.Count;
        public LevelDefinition CurrentLevel => LevelLibrary.Levels[_levelIndex];

        public bool IsSpinningCard(string cardId) => _spinningCardIds.Contains(cardId);

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
            _spinningCardIds.Clear();
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _spinningCardIds);

            LevelLoaded?.Invoke(_levelIndex + 1, LevelCount);
            ResolveCombines();
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _spinningCardIds);
            BoardChanged?.Invoke();
            HandChanged?.Invoke();
            MessageChanged?.Invoke(
                "Drag a tile to one side. A ? appears on the other side. Drag the same tile to the ? to balance. " +
                "When light meets dark, they spin — click either to dismiss.");
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

            _initialSnapshot.Apply(Board, _hand, Moves, out _pendingBalance, _spinningCardIds);
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

            _undoStack.Pop().Apply(Board, _hand, Moves, out _pendingBalance, _spinningCardIds);
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
                ActivateSpinPair(side, indexA, indexB);
                MessageChanged?.Invoke("Light and dark are spinning — click either one to dismiss.");
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

        public bool TryDismissOppositePair(string sideName, int index)
        {
            if (_levelComplete || _pendingBalance != null)
            {
                return false;
            }

            BoardSide side = Board.GetSide(sideName);
            if (index < 0 || index >= side.Cards.Count)
            {
                return false;
            }

            int partner = CombineRules.FindOppositePartnerIndex(side, index);
            if (partner < 0)
            {
                return false;
            }

            BoardCard cardA = side.Cards[index];
            BoardCard cardB = side.Cards[partner];
            if (!_spinningCardIds.Contains(cardA.Id) || !_spinningCardIds.Contains(cardB.Id))
            {
                MessageChanged?.Invoke("Drag light onto dark first to make them spin.");
                return false;
            }

            PushUndo();
            CombineRules.RemovePair(side, index, partner);
            _spinningCardIds.Remove(cardA.Id);
            _spinningCardIds.Remove(cardB.Id);
            Moves.RegisterCombine();
            CombineOccurred?.Invoke(new CombineEvent
            {
                SideName = sideName,
                Action = CombineActionType.OppositeCancel,
                IndexA = index,
                IndexB = partner
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

            ActivateOppositePairsOnSide(targetSide);
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
            string placedSide = _pendingBalance.PlacedSide;
            BoardSide balancedSide = Board.GetSide(targetSide);
            balancedSide.Cards.Add(template.Clone());
            _hand.RemoveAt(handIndex);
            _pendingBalance = null;
            HandChanged?.Invoke();

            ActivateOppositePairsOnSide(placedSide);
            ActivateOppositePairsOnSide(targetSide);

            Moves.RegisterBalancedPlay();
            MessageChanged?.Invoke("Balanced! Click spinning opposites to dismiss them.");
            ResolveCombines();
            return true;
        }

        private void ResolveCombines()
        {
            Board.ResolveAllAutoCombines(out List<(string side, int a, int b, CombineActionType action)> autoResolved);
            foreach ((string side, int a, int b, CombineActionType action) entry in autoResolved)
            {
                CombineOccurred?.Invoke(new CombineEvent
                {
                    SideName = entry.side,
                    Action = entry.action,
                    IndexA = entry.a,
                    IndexB = entry.b
                });
            }

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

            if (HasPendingSpinsOnBoxSide())
            {
                MessageChanged?.Invoke("The box is almost alone — click the spinning pairs on its side to dismiss them.");
                return;
            }

            _levelComplete = true;
            int stars = Moves.CalculateStars(CurrentLevel);
            LevelCompleted?.Invoke(stars, Moves.Moves);
            MessageChanged?.Invoke("You win! The red box is alone.");
        }

        private void PushUndo()
        {
            _undoStack.Push(GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _spinningCardIds));
        }

        private void ActivateSpinPair(BoardSide side, int indexA, int indexB)
        {
            _spinningCardIds.Add(side.Cards[indexA].Id);
            _spinningCardIds.Add(side.Cards[indexB].Id);
        }

        private void ActivateOppositePairsOnSide(string sideName)
        {
            BoardSide side = Board.GetSide(sideName);
            for (int i = 0; i < side.Cards.Count; i++)
            {
                int partner = CombineRules.FindOppositePartnerIndex(side, i);
                if (partner >= 0)
                {
                    ActivateSpinPair(side, i, partner);
                }
            }
        }

        private bool HasPendingSpinsOnBoxSide()
        {
            PruneStaleSpinIds();
            if (_spinningCardIds.Count == 0)
            {
                return false;
            }

            BoardSide boxSide = GetBoxSide();
            if (boxSide == null)
            {
                return false;
            }

            foreach (BoardCard card in boxSide.Cards)
            {
                if (_spinningCardIds.Contains(card.Id))
                {
                    return true;
                }
            }

            return false;
        }

        private BoardSide GetBoxSide()
        {
            foreach (BoardCard card in Board.Left.Cards)
            {
                if (card.Kind == CardKind.Box)
                {
                    return Board.Left;
                }
            }

            foreach (BoardCard card in Board.Right.Cards)
            {
                if (card.Kind == CardKind.Box)
                {
                    return Board.Right;
                }
            }

            return null;
        }

        private bool HasPendingSpins()
        {
            PruneStaleSpinIds();
            return _spinningCardIds.Count > 0;
        }

        private void PruneStaleSpinIds()
        {
            var onBoard = new HashSet<string>();
            foreach (BoardCard card in Board.Left.Cards)
            {
                onBoard.Add(card.Id);
            }

            foreach (BoardCard card in Board.Right.Cards)
            {
                onBoard.Add(card.Id);
            }

            _spinningCardIds.RemoveWhere(id => !onBoard.Contains(id));
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
