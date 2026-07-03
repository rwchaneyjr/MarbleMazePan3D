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
        private int _levelIndex;
        private bool _levelComplete;

        public int LevelIndex => _levelIndex;
        public int LevelCount => LevelLibrary.Levels.Count;
        public LevelDefinition CurrentLevel => LevelLibrary.Levels[_levelIndex];

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
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance);

            LevelLoaded?.Invoke(_levelIndex + 1, LevelCount);
            ResolveCombines();
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance);
            BoardChanged?.Invoke();
            MessageChanged?.Invoke(
                "Goal: red box ALONE. Play a card, then TAP the yellow BALANCE hole. One card per kind in hand — flip before playing. " +
                "Light + dark CANCEL (vanish).");
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

            _initialSnapshot.Apply(Board, _hand, Moves, out _pendingBalance);
            _undoStack.Clear();
            _levelComplete = false;
            BoardChanged?.Invoke();
            MessageChanged?.Invoke("Rewound to the start of the level.");
        }

        public void Undo()
        {
            if (_undoStack.Count == 0 || _levelComplete)
            {
                return;
            }

            _undoStack.Pop().Apply(Board, _hand, Moves, out _pendingBalance);
            _levelComplete = false;
            BoardChanged?.Invoke();
            MessageChanged?.Invoke("Undid the last move.");
        }

        public bool TryFlipHandCard(int handIndex)
        {
            if (_levelComplete || handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            if (_pendingBalance != null)
            {
                MessageChanged?.Invoke("Click the BALANCE hole on the other side first.");
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

            PushUndo();
            BoardSide side = Board.GetSide(sideName);
            if (!Board.TryCombineOnSide(side, indexA, indexB, out CombineActionType action))
            {
                PopUndoWithoutApply();
                MessageChanged?.Invoke("Those cards cannot combine. Drag one card onto another on the same side.");
                return false;
            }

            Moves.RegisterCombine();
            CombineOccurred?.Invoke(new CombineEvent
            {
                SideName = sideName,
                Action = action,
                IndexA = indexA,
                IndexB = indexB
            });

            ResolveCombines();
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
                MessageChanged?.Invoke("Click the yellow BALANCE hole on the other side.");
                return false;
            }

            return TryStartBalance(handIndex, targetSide, template);
        }

        public bool TryCompleteBalanceHole(string side)
        {
            if (_levelComplete || _pendingBalance == null)
            {
                return false;
            }

            if (side != _pendingBalance.HoleSide)
            {
                MessageChanged?.Invoke("Click the BALANCE hole on the other side.");
                return false;
            }

            PushUndo();
            Board.GetSide(side).Cards.Add(_pendingBalance.Card.Clone());
            _pendingBalance = null;

            Moves.RegisterBalancedPlay();
            MessageChanged?.Invoke("Balanced! Light and dark opposites vanish when they meet.");
            ResolveCombines();
            return true;
        }

        public bool TryPlayFromHand(int handIndex)
        {
            return TryPlayFromHand(handIndex, "Left");
        }

        private bool TryStartBalance(int handIndex, string targetSide, BoardCard template)
        {
            PushUndo();
            Board.GetSide(targetSide).Cards.Add(template.Clone());
            _hand.RemoveAt(handIndex);
            HandChanged?.Invoke();

            _pendingBalance = new BalancePending
            {
                Card = template.Clone(),
                PlacedSide = targetSide
            };

            string otherSide = targetSide == "Left" ? "RIGHT" : "LEFT";
            MessageChanged?.Invoke(
                $"BALANCE! Click the yellow hole on the {otherSide} side. Flip before playing if you need light/dark.");
            BoardChanged?.Invoke();
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

            if (WinChecker.HasPendingOpposites(Board))
            {
                MessageChanged?.Invoke("The box is almost alone — cancel remaining light/dark pairs.");
                return;
            }

            _levelComplete = true;
            int stars = Moves.CalculateStars(CurrentLevel);
            LevelCompleted?.Invoke(stars, Moves.Moves);
            MessageChanged?.Invoke("You win! The red box is alone.");
        }

        private void PushUndo()
        {
            _undoStack.Push(GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance));
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
