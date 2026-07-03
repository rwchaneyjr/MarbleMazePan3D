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
        public event Action<int, int> LevelCompleted;
        public event Action<int, int> LevelLoaded;
        public event Action<string> MessageChanged;
        public event Action<CombineEvent> CombineOccurred;

        public AlgebraBoard Board { get; } = new();
        public MoveTracker Moves { get; } = new();
        public IReadOnlyList<BoardCard> Hand => _hand;
        public bool CanUndo => _undoStack.Count > 0;

        private readonly List<BoardCard> _hand = new();
        private readonly Stack<GameSnapshot> _undoStack = new();
        private GameSnapshot _initialSnapshot;
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
            Moves.Reset();
            _undoStack.Clear();
            _levelComplete = false;
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves);

            LevelLoaded?.Invoke(_levelIndex + 1, LevelCount);
            BoardChanged?.Invoke();
            MessageChanged?.Invoke(
                "Click a hand card to flip light/dark. Drag one card at a time to either side. " +
                "Cancel opposites. Keep the box alone on one side.");
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

            _initialSnapshot.Apply(Board, _hand, Moves);
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

            _undoStack.Pop().Apply(Board, _hand, Moves);
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

            BoardCard card = _hand[handIndex];
            if (!CardFlipRules.CanFlip(card.Kind))
            {
                MessageChanged?.Invoke("That card cannot flip.");
                return false;
            }

            PushUndo();
            _hand[handIndex] = CardFlipRules.Flip(card);
            BoardChanged?.Invoke();
            MessageChanged?.Invoke(CardFlipRules.IsLight(_hand[handIndex])
                ? "Flipped to light."
                : "Flipped to dark.");
            return true;
        }

        public bool TryCombine(string sideName, int indexA, int indexB)
        {
            if (_levelComplete)
            {
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
            if (template.Kind == CardKind.DivideTool)
            {
                return TryUseDivideTool(handIndex);
            }

            PushUndo();
            Board.GetSide(targetSide).Cards.Add(template.Clone());
            _hand.RemoveAt(handIndex);

            Moves.RegisterBalancedPlay();
            MessageChanged?.Invoke("Played one card. Light and dark opposites vanish when they meet.");
            ResolveCombines();
            return true;
        }

        public bool TryPlayFromHand(int handIndex)
        {
            return TryPlayFromHand(handIndex, "Left");
        }

        public bool TryUseDivideTool(int handIndex)
        {
            PushUndo();
            bool left = Board.TryApplyDivide(Board.Left);
            bool right = !left && Board.TryApplyDivide(Board.Right);

            if (!left && !right)
            {
                PopUndoWithoutApply();
                MessageChanged?.Invoke("No identical pair to divide on either side.");
                return false;
            }

            _hand.RemoveAt(handIndex);
            Moves.RegisterBalancedPlay();
            CombineOccurred?.Invoke(new CombineEvent
            {
                SideName = left ? "Left" : "Right",
                Action = CombineActionType.DividePair,
                IndexA = 0,
                IndexB = 1
            });
            MessageChanged?.Invoke("Divided identical pair into One!");
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
        }

        private void PushUndo()
        {
            _undoStack.Push(GameSnapshot.Capture(Board, _hand, Moves));
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
