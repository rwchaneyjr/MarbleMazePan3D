using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public class AlgebraGameController
    {
        public event Action BoardChanged;
        public event Action<int, int> LevelCompleted;
        public event Action<int, int> LevelLoaded;
        public event Action<string> MessageChanged;

        public AlgebraBoard Board { get; } = new();
        public MoveTracker Moves { get; } = new();
        public IReadOnlyList<BoardCard> Hand => _hand;

        private readonly List<BoardCard> _hand = new();
        private int _levelIndex;

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

            LevelLoaded?.Invoke(_levelIndex + 1, LevelCount);
            BoardChanged?.Invoke();
            MessageChanged?.Invoke("Combine opposite cards on the same side, or play from your hand to both sides.");
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

        public bool TryCombine(string sideName, int indexA, int indexB)
        {
            BoardSide side = sideName == "Left" ? Board.Left : Board.Right;
            if (!Board.TryCombineOnSide(side, indexA, indexB))
            {
                MessageChanged?.Invoke("Those cards cannot combine.");
                return false;
            }

            Moves.RegisterCombine();
            Board.ResolveAllAutoCombines();
            BoardChanged?.Invoke();
            CheckWin();
            return true;
        }

        public bool TryPlayFromHand(int handIndex, bool addToLeftSide)
        {
            if (handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            BoardCard template = _hand[handIndex];
            Board.TryAddBalanced(template, addToLeftSide);
            _hand.RemoveAt(handIndex);

            Moves.RegisterBalancedPlay();
            Board.ResolveAllAutoCombines();
            BoardChanged?.Invoke();
            MessageChanged?.Invoke("Balanced move! Same card added to both sides.");
            CheckWin();
            return true;
        }

        private void CheckWin()
        {
            if (!WinChecker.IsBoxAlone(Board))
            {
                return;
            }

            if (WinChecker.HasPendingOpposites(Board))
            {
                MessageChanged?.Invoke("The box is almost alone — combine remaining opposites.");
                return;
            }

            int stars = Moves.CalculateStars(CurrentLevel);
            LevelCompleted?.Invoke(stars, Moves.Moves);
        }
    }
}
