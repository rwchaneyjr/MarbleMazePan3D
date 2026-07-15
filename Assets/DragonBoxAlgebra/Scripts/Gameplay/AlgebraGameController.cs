using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.UI;

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
        public event Action<int, int> WinSequenceStarted;
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
        public bool HasActiveMergeAnimations => _activeMergeAnimations > 0;
        public bool IsLevelComplete => _levelComplete;

        private readonly List<BoardCard> _hand = new();
        private readonly List<BoardCard> _handTemplates = new();
        private readonly List<PendingCancelMarker> _pendingCancels = new();
        private readonly HashSet<int> _spentHandIndices = new();
        private readonly Stack<GameSnapshot> _undoStack = new();
        private GameSnapshot _initialSnapshot;
        private BalancePending _pendingBalance;
        private int _levelIndex;
        private int _activeHandSlot = -1;
        private bool _levelComplete;
        private int _activeMergeAnimations;
        private bool UsesManualPairMerge =>
            CurrentLevel.Chapter >= 3
            || (CurrentLevel.Chapter == 2 && ChapterLevelGenerator.IndexWithinChapter(_levelIndex) >= 16);

        /// <summary>Ch2 levels 17+: drag hand tile onto opposite creature (not balance).</summary>
        public bool UsesOppositeHandPlay =>
            CurrentLevel.Chapter == 2 && ChapterLevelGenerator.IndexWithinChapter(_levelIndex) >= 16;

        private bool UsesVariablePositiveNegative => CurrentLevel.Chapter >= 5;

        private string LightTerm => UsesVariablePositiveNegative ? "positive" : "light";
        private string DarkTerm => UsesVariablePositiveNegative ? "negative" : "dark";

        private static string Capitalize(string value) =>
            string.IsNullOrEmpty(value) ? value : char.ToUpper(value[0]) + value.Substring(1);

        private string GoalAlonePhrase => UsesVariableXGoalWin ? "Leave x alone!" : "Leave the red box alone!";

        private static readonly System.Random Rng = new();

        public int LevelIndex => _levelIndex;
        public int LevelCount => LevelLibrary.Levels.Count;
        public LevelDefinition CurrentLevel => LevelLibrary.Levels[_levelIndex];

        public bool UsesPlayableHandDisplay =>
            LevelIndex + 1 >= ChapterLevelGenerator.Chapter4StartLevel;

        public bool UsesDualHandPanelDisplay =>
            _hand.Count >= 2 && CurrentLevel.Chapter >= 5;

        private bool UsesReusableVariableHandCards =>
            UsesDualHandPanelDisplay && CurrentLevel.Chapter >= 5;

        public bool ShouldKeepHandCardInPanel(int handIndex) =>
            UsesPlayableHandDisplay
            && handIndex >= 0
            && handIndex < _hand.Count
            && (!UsesDualHandPanelDisplay || !_spentHandIndices.Contains(handIndex));

        public bool IsHandBalanceComplete(int handIndex) =>
            handIndex >= 0 && _spentHandIndices.Contains(handIndex);

        public bool KeepHandSlotVisibleDuringDrag() =>
            UsesPlayableHandDisplay && HasPendingBalance;

        public bool IsHandSlotPlayable(int handIndex)
        {
            if (!UsesPlayableHandDisplay)
            {
                return handIndex >= 0
                    && handIndex < _hand.Count
                    && _hand[handIndex].IsPlayableFromHand;
            }

            if (_levelComplete)
            {
                return false;
            }

            if (handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            if (!_hand[handIndex].IsPlayableFromHand)
            {
                return false;
            }

            if (_spentHandIndices.Contains(handIndex))
            {
                return false;
            }

            if (UsesDualHandPanelDisplay)
            {
                return _pendingBalance == null || handIndex == _pendingBalance.HandIndex;
            }

            return handIndex == CurrentPlayableHandSlotIndex();
        }

        public BoardCard GetHandDisplayCard(int handIndex)
        {
            if (handIndex < 0 || handIndex >= _hand.Count)
            {
                return default;
            }

            BoardCard current = _hand[handIndex];
            if (!UsesPlayableHandDisplay || handIndex >= _handTemplates.Count)
            {
                return current;
            }

            BoardCard display = current.Clone();
            display.VisualTheme = _handTemplates[handIndex].VisualTheme;
            return display;
        }

        public bool ShouldDisplayHandCard(int handIndex)
        {
            if (handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            if (_levelComplete)
            {
                return false;
            }

            if (UsesDualHandPanelDisplay)
            {
                return true;
            }

            if (_spentHandIndices.Contains(handIndex))
            {
                return false;
            }

            if (!UsesPlayableHandDisplay)
            {
                return true;
            }

            return IsHandSlotPlayable(handIndex);
        }

        private int CurrentPlayableHandSlotIndex()
        {
            if (_pendingBalance != null)
            {
                return _pendingBalance.HandIndex;
            }

            if (_activeHandSlot >= 0)
            {
                return _activeHandSlot;
            }

            return NextUnspentHandIndex();
        }

        private void RestoreHandSlotFromSnapshot()
        {
            _activeHandSlot = _pendingBalance?.HandIndex ?? -1;
        }

        private int NextUnspentHandIndex()
        {
            for (int i = 0; i < _hand.Count; i++)
            {
                if (_spentHandIndices.Contains(i))
                {
                    continue;
                }

                if (_hand[i].IsPlayableFromHand)
                {
                    return i;
                }
            }

            return -1;
        }

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

        public bool IsCardPendingCancelOnSide(string cardId, string sideName)
        {
            foreach (PendingCancelMarker marker in _pendingCancels)
            {
                if (marker.SideName != sideName)
                {
                    continue;
                }

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
            CreatureArt.SetTheme(level.CreatureTheme);

            Board.Reset(level.BuildLeftSide(), level.BuildRightSide());

            if (UsesManualPairMerge)
            {
                BoardFoldRules.FoldMatchingPairsForPlayableRight(Board);
            }

            _hand.Clear();
            _hand.AddRange(level.BuildHand());
            if (level.Chapter < 4)
            {
                HandRules.DedupeFlipFamilies(_hand);
            }

            if (_hand.Count > 0)
            {
                HandVisualRules.ApplyLevelThemeToHand(_hand, level.CreatureTheme);
            }

            CaptureHandTemplates();
            Moves.Reset();
            _undoStack.Clear();
            _levelComplete = false;
            _pendingBalance = null;
            _pendingCancels.Clear();
            _spentHandIndices.Clear();
            _activeHandSlot = -1;
            _activeMergeAnimations = 0;
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _pendingCancels,
                _spentHandIndices);

            LevelLoaded?.Invoke(_levelIndex + 1, LevelCount);
            ResolveCombines();
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _pendingCancels,
                _spentHandIndices);
            BoardChanged?.Invoke();
            HandChanged?.Invoke();
            MessageChanged?.Invoke(_pendingCancels.Count > 0 && _hand.Count == 0
                ? level.Chapter == 1
                    ? $"Watch {LightTerm} and {DarkTerm} merge into *. Tap the spinning * to dismiss. Leave the red box alone!"
                    : "Click the spinning * to dismiss the creatures. Leave the red box alone!"
                : UsesOppositeHandPlay
                    ? "Drag the hand tile onto the opposite creature on the board. Tap * to dismiss. Leave the red box alone!"
                    : level.DragToMergePairs
                        ? level.LeftCards.Count >= 2 && level.RightCards.Count >= 2
                            ? $"Drag {LightTerm} onto {DarkTerm} on each side to make *. Tap every * before the puzzle finishes!"
                            : $"Drag {LightTerm} onto {DarkTerm} on the same side. They snap together into *. Tap * to dismiss. Leave the red box alone!"
                        : HandMessage(level));
            CreatureSpriteDebug.LogLevel(Board, _hand, level);
        }

        private string HandMessage(LevelDefinition level)
        {
            int count = level.HandCards.Count;
            string pairPhrase = level.Chapter >= 5 ? "Positive + negative" : "Light + dark";
            if (count == 0)
            {
                return $"Drag {LightTerm} onto {DarkTerm} on the same side. Tap * to dismiss. Leave the red box alone!";
            }

            if (count <= 1)
            {
                return "Drag a tile to one side. A ? appears on the other side. Drag the same tile to the ? to balance. " +
                       $"{pairPhrase} on the same side become one *. Pairs never cross the middle.";
            }

            if (count == 2)
            {
                if (level.Chapter >= 7)
                {
                    return "Sea creatures and x — tap to flip light/dark. Play each hand tile: drag to a side, " +
                           "fill the ?, merge to *, tap to dismiss. Clear every * until x stands alone.";
                }

                if (level.Chapter >= 6)
                {
                    return "Variables in hand — tap to flip positive/negative. Play each one: drag to a side, " +
                           "fill the ? with the same variable, then positive + negative cancel into *. " +
                           "Clear every * until x stands alone.";
                }

                if (level.Chapter >= 5)
                {
                    return "Variable images in hand — tap a card to flip positive/negative. " +
                           "Play each one: drag to a side, fill the ?, merge to *, tap to dismiss. " +
                           "Clear every * until the red box stands alone.";
                }

                return "Two tiles in hand — play each one: drag to a side, then drag the same tile to the ?. " +
                       "Finish one tile before starting the next.";
            }

            return "Three tiles in hand — play each one: drag to a side, then drag the same tile to the ?. " +
                   $"{pairPhrase} on the same side become one *.";
        }

        public void LoadNextLevel()
        {
            if (_levelIndex + 1 < LevelLibrary.Levels.Count)
            {
                LoadLevel(_levelIndex + 1);
            }
            else
            {
                LoadRandomLevel();
            }
        }

        public void LoadRandomLevel()
        {
            if (LevelLibrary.Levels.Count <= 6)
            {
                LoadLevel(0);
                return;
            }

            int index = 6 + Rng.Next(LevelLibrary.Levels.Count - 6);
            LoadLevel(index);
            MessageChanged?.Invoke($"Random puzzle — {CreatureArt.ThemeName}!");
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

            _initialSnapshot.Apply(Board, _hand, Moves, out _pendingBalance, _pendingCancels, _spentHandIndices);
            RestoreHandSlotFromSnapshot();
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

            _undoStack.Pop().Apply(Board, _hand, Moves, out _pendingBalance, _pendingCancels, _spentHandIndices);
            RestoreHandSlotFromSnapshot();
            _levelComplete = false;
            BoardChanged?.Invoke();
            HandChanged?.Invoke();
            MessageChanged?.Invoke("Undid the last move.");
        }

        public void RefreshHandPresentation()
        {
            HandChanged?.Invoke();
        }

        public bool CanFlipHandCard(int handIndex)
        {
            if (_levelComplete || handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            return CardFlipRules.CanFlip(_hand[handIndex]);
        }

        public bool TryFlipHandCard(int handIndex)
        {
            if (!CanFlipHandCard(handIndex))
            {
                MessageChanged?.Invoke("That card cannot flip.");
                return false;
            }

            BoardCard card = _hand[handIndex];

            PushUndo();
            _hand[handIndex] = CardFlipRules.Flip(card);
            SyncHandTemplateForCard(_hand[handIndex]);

            HandChanged?.Invoke();
            bool creatureOnly = _hand[handIndex].VariableLetter == '\0';
            MessageChanged?.Invoke(CardFlipRules.IsLight(_hand[handIndex])
                ? creatureOnly || !UsesVariablePositiveNegative
                    ? $"Flipped to {LightTerm}. Click again for {DarkTerm}."
                    : "Flipped to positive. Click again for negative."
                : creatureOnly || !UsesVariablePositiveNegative
                    ? $"Flipped to {DarkTerm}. Click again for {LightTerm}."
                    : "Flipped to negative. Click again for positive.");
            return true;
        }

        public bool TryCombine(string sideName, int indexA, int indexB)
        {
            if (_levelComplete)
            {
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
                BoardCard cardA = side.Cards[indexA];
                BoardCard cardB = side.Cards[indexB];
                if (CombineRules.UsesAsteriskCancel(cardA, cardB))
                {
                    TryCreateCancelMarker(sideName, cardA.Id, cardB.Id);
                    MessageChanged?.Invoke(_pendingBalance != null
                        ? $"{Capitalize(LightTerm)} met {DarkTerm} — swirl appears. The ? hole stays until you fill it."
                        : $"{Capitalize(LightTerm)} met {DarkTerm} — swirl appears.");
                }
                else
                {
                    CombineRules.RemovePair(side, indexA, indexB);
                    Moves.RegisterCombine();
                    CombineOccurred?.Invoke(new CombineEvent
                    {
                        SideName = sideName,
                        Action = CombineActionType.OppositeCancel,
                        IndexA = indexA,
                        IndexB = indexB
                    });
                    MessageChanged?.Invoke("Dice canceled.");
                }

                BoardChanged?.Invoke();
                CheckWin();
                return true;
            }

            if (_pendingBalance != null)
            {
                MessageChanged?.Invoke("Fill the balance hole first.");
                return false;
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
            if (_levelComplete)
            {
                return false;
            }

            if (markerIndex < 0 || markerIndex >= _pendingCancels.Count)
            {
                return false;
            }

            PendingCancelMarker marker = _pendingCancels[markerIndex];
            if (marker.SwirlOnly)
            {
                PushUndo();
                _pendingCancels.RemoveAt(markerIndex);
                Moves.RegisterCombine();
                MessageChanged?.Invoke("Pair dismissed.");
                BoardChanged?.Invoke();
                if (UsesReusableVariableHandCards)
                {
                    RefreshHandSpentStateForReusableCards();
                    HandChanged?.Invoke();
                }

                CheckWin();
                return true;
            }

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
            if (UsesReusableVariableHandCards)
            {
                RefreshHandSpentStateForReusableCards();
                HandChanged?.Invoke();
            }

            CheckWin();
            return true;
        }

        public bool TryPlayFromHand(int handIndex, string targetSide)
        {
            if (_levelComplete || handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            if (_spentHandIndices.Contains(handIndex))
            {
                return false;
            }

            if (UsesDualHandPanelDisplay)
            {
                if (_pendingBalance != null && handIndex != _pendingBalance.HandIndex)
                {
                    MessageChanged?.Invoke("Fill the ? hole first — finish the tile you started.");
                    return false;
                }
            }
            else if (UsesPlayableHandDisplay && handIndex != CurrentPlayableHandSlotIndex())
            {
                MessageChanged?.Invoke("Play the highlighted hand card first.");
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

            if (UsesOppositeHandPlay)
            {
                MessageChanged?.Invoke("Drag onto the opposite light or dark creature — not an empty side.");
                return false;
            }

            return TryStartBalance(handIndex, targetSide, template);
        }

        public bool TryPlayFromHand(int handIndex)
        {
            return TryPlayFromHand(handIndex, "Left");
        }

        public bool CanPlayHandOntoBoardCard(int handIndex, string sideName, int boardIndex)
        {
            if (_levelComplete || handIndex < 0 || handIndex >= _hand.Count
                || _spentHandIndices.Contains(handIndex))
            {
                return false;
            }

            BoardSide side = Board.GetSide(sideName);
            if (boardIndex < 0 || boardIndex >= side.Cards.Count)
            {
                return false;
            }

            BoardCard targetCard = side.Cards[boardIndex];
            if (IsCardPendingCancelOnSide(targetCard.Id, sideName))
            {
                return false;
            }

            // Snap / drop-on-top only targets true opposites (any chapter).
            if (CombineRules.GetCombineAction(_hand[handIndex], targetCard)
                == CombineActionType.OppositeCancel)
            {
                return true;
            }

            // Older opposite-only chapters reject non-opposites; balance chapters still allow side drops elsewhere.
            return !UsesOppositeHandPlay;
        }

        public bool TryPlayHandOntoOppositeOnSide(int handIndex, string sideName)
        {
            if (!UsesOppositeHandPlay)
            {
                return false;
            }

            BoardSide side = Board.GetSide(sideName);
            for (int i = 0; i < side.Cards.Count; i++)
            {
                if (CanPlayHandOntoBoardCard(handIndex, sideName, i)
                    && TryPlayHandOntoOpposite(handIndex, sideName, i))
                {
                    return true;
                }
            }

            MessageChanged?.Invoke(side.Cards.Count == 0
                ? "That side is empty — drag onto the side with the opposite creature."
                : "Drag onto the opposite light or dark creature.");
            return false;
        }

        public bool TryPlayHandOntoOpposite(int handIndex, string sideName, int targetBoardIndex)
        {
            if (_levelComplete || handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            if (_spentHandIndices.Contains(handIndex))
            {
                return false;
            }

            BoardSide side = Board.GetSide(sideName);
            if (targetBoardIndex < 0 || targetBoardIndex >= side.Cards.Count)
            {
                return false;
            }

            BoardCard handCard = _hand[handIndex];
            BoardCard targetCard = side.Cards[targetBoardIndex];

            if (IsCardPendingCancelOnSide(targetCard.Id, sideName))
            {
                return false;
            }

            if (CombineRules.GetCombineAction(handCard, targetCard) != CombineActionType.OppositeCancel)
            {
                MessageChanged?.Invoke(targetCard.Kind == CardKind.Box
                    ? "Drag onto the creature, not the red box."
                    : "Drag onto the opposite light or dark creature.");
                return false;
            }

            PushUndo();
            _pendingBalance = null;
            if (CombineRules.UsesAsteriskCancel(handCard, targetCard))
            {
                // Place the hand clone beside the target so the swirl sits in that slot.
                int insertAt = Math.Clamp(targetBoardIndex + 1, 0, side.Cards.Count);
                side.Cards.Insert(insertAt, handCard.CloneForPlacement());
                BoardCard placed = side.Cards[insertAt];
                TryCreateCancelMarker(sideName, targetCard.Id, placed.Id);
                _spentHandIndices.Add(handIndex);
                Moves.RegisterCombine();
                MessageChanged?.Invoke($"{Capitalize(LightTerm)} met {DarkTerm} — swirl appears.");
            }
            else
            {
                CombineRules.RemoveCardById(side, targetCard.Id);
                _spentHandIndices.Add(handIndex);
                Moves.RegisterCombine();
                CombineOccurred?.Invoke(new CombineEvent
                {
                    SideName = sideName,
                    Action = CombineActionType.OppositeCancel,
                    IndexA = targetBoardIndex,
                    IndexB = -1
                });
                MessageChanged?.Invoke("Dice canceled.");
            }

            HandChanged?.Invoke();
            BoardChanged?.Invoke();
            CheckWin();
            return true;
        }

        private void CaptureHandTemplates()
        {
            _handTemplates.Clear();
            foreach (BoardCard card in _hand)
            {
                _handTemplates.Add(card.Clone());
            }
        }

        private void SyncHandFromTemplates()
        {
            _hand.Clear();
            foreach (BoardCard template in _handTemplates)
            {
                _hand.Add(template.Clone());
            }
        }

        private void SyncHandTemplateForCard(BoardCard card)
        {
            for (int i = 0; i < _handTemplates.Count; i++)
            {
                if (_handTemplates[i].Id == card.Id)
                {
                    _handTemplates[i] = card.Clone();
                    return;
                }
            }

            for (int i = 0; i < _hand.Count && i < _handTemplates.Count; i++)
            {
                if (_hand[i].Id == card.Id)
                {
                    _handTemplates[i] = card.Clone();
                    return;
                }
            }
        }

        private bool TryStartBalance(int handIndex, string targetSide, BoardCard template)
        {
            PushUndo();
            BoardSide placedSide = Board.GetSide(targetSide);
            placedSide.Cards.Add(template.CloneForPlacement());
            int placedIndex = placedSide.Cards.Count - 1;
            string holeSide = targetSide == "Left" ? "Right" : "Left";

            _pendingBalance = new BalancePending
            {
                Card = template.Clone(),
                PlacedSide = targetSide,
                PlacedIndex = placedIndex,
                HandIndex = handIndex,
                HoleInsertIndex = Board.GetSide(holeSide).Cards.Count
            };

            if (UsesPlayableHandDisplay)
            {
                _activeHandSlot = handIndex;
            }

            MessageChanged?.Invoke("? appeared on the other side — drag the same tile to fill the hole.");
            BoardChanged?.Invoke();
            if (UsesPlayableHandDisplay && !UsesDualHandPanelDisplay)
            {
                HandChanged?.Invoke();
            }

            return true;
        }

        private bool TryCompleteBalance(int handIndex, string targetSide, BoardCard template)
        {
            if (handIndex != _pendingBalance.HandIndex)
            {
                MessageChanged?.Invoke("Fill the ? hole first — finish the tile you started.");
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
            int insertIndex = _pendingBalance.HoleInsertIndex;
            if (insertIndex < 0 || insertIndex > balancedSide.Cards.Count)
            {
                insertIndex = balancedSide.Cards.Count;
            }

            balancedSide.Cards.Insert(insertIndex, template.CloneForPlacement());
            int holePlacedIndex = insertIndex;
            string placedSide = _pendingBalance.PlacedSide;
            int placedBoardIndex = _pendingBalance.PlacedIndex;
            _pendingBalance = null;
            if (UsesPlayableHandDisplay)
            {
                if (UsesReusableVariableHandCards)
                {
                    RefreshHandSpentStateForReusableCards();
                }
                else
                {
                    _spentHandIndices.Add(handIndex);
                }

                _activeHandSlot = -1;
                SyncHandTemplateForCard(_hand[handIndex]);
            }
            else
            {
                SyncHandFromTemplates();
            }

            HandChanged?.Invoke();

            Moves.RegisterBalancedPlay();
            MessageChanged?.Invoke(UsesManualPairMerge
                ? $"Balanced! Drag {LightTerm} onto {DarkTerm} on the same side to make *."
                : "Balanced!");
            PruneInvalidCancelMarkers();
            ResolveCombines();
            return true;
        }

        private void ResolveCombines()
        {
            PruneInvalidCancelMarkers();
            BoardChanged?.Invoke();
            CheckWin();
        }

        public void NotifyMergeAnimationStarted()
        {
            _activeMergeAnimations++;
        }

        public void NotifyMergeAnimationCompleted()
        {
            _activeMergeAnimations = Math.Max(0, _activeMergeAnimations - 1);
            if (_pendingCancels.Count == 0)
            {
                CheckWin();
            }
        }

        public bool CanPresentWin()
        {
            return _pendingBalance == null
                && _pendingCancels.Count == 0
                && _activeMergeAnimations == 0;
        }

        public bool HasRemainingSwirls => _pendingCancels.Count > 0;

        private void CheckWin()
        {
            if (_pendingCancels.Count > 0)
            {
                MessageChanged?.Invoke(_pendingCancels.Count > 1
                    ? "Swirls clearing…"
                    : "Swirl clearing…");
                return;
            }

            if (!WinChecker.CanWin(Board, Moves.Moves, _pendingBalance != null, _pendingCancels.Count,
                    _activeMergeAnimations, UsesExtraOppositeTileLevel, UsesVariableXGoalWin))
            {
                return;
            }

            if (_levelComplete)
            {
                return;
            }

            _levelComplete = true;
            int stars = Moves.CalculateStars(CurrentLevel);
            int moves = Moves.Moves;
            MessageChanged?.Invoke(UsesVariableXGoalWin
                ? "You win! x is alone."
                : "You win! The red box is alone.");
            WinSequenceStarted?.Invoke(stars, moves);
            HandChanged?.Invoke();
        }

        public void ClearOppositeSideAfterSidesTogether()
        {
            if (!UsesExtraOppositeTileLevel)
            {
                return;
            }

            if (!TryGetOppositeSideOfBox(out BoardSide opposite))
            {
                return;
            }

            opposite.Cards.Clear();
        }

        public bool UsesVariableXGoalWin => CurrentLevel.Chapter >= 6;

        public bool UsesExtraOppositeTileLevel =>
            _levelIndex + 1 >= ChapterLevelGenerator.OppositeExtraTileStartLevel
            && _levelIndex + 1 <= ChapterLevelGenerator.OppositeExtraTileEndLevel;

        public bool UsesPlusBetweenBoardTiles =>
            ChapterLevelGenerator.UsesPlusBetweenBoardTiles(_levelIndex + 1);

        public bool TryGetBoxSideNames(out string boxSide, out string oppositeSide)
        {
            boxSide = null;
            oppositeSide = null;

            if (Board.Left.Cards.Count == 1 && VariableGoalRules.IsIsolationGoal(Board.Left.Cards[0]))
            {
                boxSide = "Left";
                oppositeSide = "Right";
                return true;
            }

            if (Board.Right.Cards.Count == 1 && VariableGoalRules.IsIsolationGoal(Board.Right.Cards[0]))
            {
                boxSide = "Right";
                oppositeSide = "Left";
                return true;
            }

            return false;
        }

        private bool TryGetOppositeSideOfBox(out BoardSide opposite)
        {
            opposite = null;
            if (Board.Left.Cards.Count == 1 && VariableGoalRules.IsIsolationGoal(Board.Left.Cards[0]))
            {
                opposite = Board.Right;
                return true;
            }

            if (Board.Right.Cards.Count == 1 && VariableGoalRules.IsIsolationGoal(Board.Right.Cards[0]))
            {
                opposite = Board.Left;
                return true;
            }

            return false;
        }

        public void CompleteWinPresentation(int stars, int moves)
        {
            LevelCompleted?.Invoke(stars, moves);
        }

        private void PushUndo()
        {
            _undoStack.Push(GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _pendingCancels,
                _spentHandIndices));
        }

        private void ActivateOppositePairOrCancelDice(string sideName, int cardIndex)
        {
            if (TryInstantDiceCancelForCard(sideName, cardIndex))
            {
                return;
            }

            ActivateOppositePairForCard(sideName, cardIndex);
        }

        private bool TryInstantDiceCancelForCard(string sideName, int cardIndex)
        {
            BoardSide side = Board.GetSide(sideName);
            if (cardIndex < 0 || cardIndex >= side.Cards.Count)
            {
                return false;
            }

            BoardCard placed = side.Cards[cardIndex];
            for (int j = 0; j < side.Cards.Count; j++)
            {
                if (j == cardIndex)
                {
                    continue;
                }

                if (!CombineRules.IsDiceOppositePair(placed, side.Cards[j]))
                {
                    continue;
                }

                string partnerId = side.Cards[j].Id;
                string placedId = placed.Id;
                CombineRules.RemovePairById(side, placedId, partnerId);
                Moves.RegisterCombine();
                CombineOccurred?.Invoke(new CombineEvent
                {
                    SideName = sideName,
                    Action = CombineActionType.OppositeCancel,
                    IndexA = cardIndex,
                    IndexB = j
                });
                MessageChanged?.Invoke("Dice canceled.");
                return true;
            }

            return false;
        }

        private void ActivateOppositePairForCard(string sideName, int cardIndex)
        {
            BoardSide side = Board.GetSide(sideName);
            if (cardIndex < 0 || cardIndex >= side.Cards.Count)
            {
                return;
            }

            int partner = FindAvailableOppositePartnerIndex(side, sideName, cardIndex);
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
                if (IsCardPendingCancelOnSide(side.Cards[i].Id, sideName))
                {
                    continue;
                }

                int partner = FindAvailableOppositePartnerIndex(side, sideName, i);
                if (partner > i)
                {
                    TryCreateCancelMarker(sideName, side.Cards[i].Id, side.Cards[partner].Id);
                }
            }
        }

        private int FindAvailableOppositePartnerIndex(BoardSide side, string sideName, int index)
        {
            if (index < 0 || index >= side.Cards.Count
                || IsCardPendingCancelOnSide(side.Cards[index].Id, sideName))
            {
                return -1;
            }

            BoardCard card = side.Cards[index];
            for (int j = 0; j < side.Cards.Count; j++)
            {
                if (j == index || IsCardPendingCancelOnSide(side.Cards[j].Id, sideName))
                {
                    continue;
                }

                if (CombineRules.IsCreatureOppositePair(card, side.Cards[j]))
                {
                    return j;
                }
            }

            return -1;
        }

        private void PruneInvalidCancelMarkers()
        {
            for (int i = _pendingCancels.Count - 1; i >= 0; i--)
            {
                PendingCancelMarker marker = _pendingCancels[i];
                if (marker.SwirlOnly)
                {
                    continue;
                }

                BoardSide side = Board.GetSide(marker.SideName);
                if (!SideContainsBothCards(side, marker.CardIdA, marker.CardIdB)
                    || !CardExistsOnlyOnSide(Board, marker.SideName, marker.CardIdA)
                    || !CardExistsOnlyOnSide(Board, marker.SideName, marker.CardIdB))
                {
                    _pendingCancels.RemoveAt(i);
                }
            }
        }

        private void TryCreateCancelMarker(string sideName, string cardIdA, string cardIdB)
        {
            if (IsCardPendingCancelOnSide(cardIdA, sideName) || IsCardPendingCancelOnSide(cardIdB, sideName))
            {
                return;
            }

            BoardSide side = Board.GetSide(sideName);
            BoardCard? cardA = null;
            BoardCard? cardB = null;
            foreach (BoardCard card in side.Cards)
            {
                if (card.Id == cardIdA)
                {
                    cardA = card;
                }

                if (card.Id == cardIdB)
                {
                    cardB = card;
                }
            }

            if (cardA == null || cardB == null || !CombineRules.UsesAsteriskCancel(cardA.Value, cardB.Value))
            {
                return;
            }

            if (!SideContainsBothCards(side, cardIdA, cardIdB))
            {
                return;
            }

            if (!CardExistsOnlyOnSide(sideName, cardIdA) || !CardExistsOnlyOnSide(sideName, cardIdB))
            {
                return;
            }

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

        private static bool CardExistsOnlyOnSide(AlgebraBoard board, string sideName, string cardId)
        {
            BoardSide side = board.GetSide(sideName);
            string otherSide = sideName == "Left" ? "Right" : "Left";
            BoardSide other = board.GetSide(otherSide);

            bool onSide = false;
            foreach (BoardCard card in side.Cards)
            {
                if (card.Id == cardId)
                {
                    onSide = true;
                    break;
                }
            }

            if (!onSide)
            {
                return false;
            }

            foreach (BoardCard card in other.Cards)
            {
                if (card.Id == cardId)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CardExistsOnlyOnSide(string sideName, string cardId) =>
            CardExistsOnlyOnSide(Board, sideName, cardId);

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

        private void RefreshHandSpentStateForReusableCards()
        {
            for (int i = 0; i < _hand.Count; i++)
            {
                if (_hand[i].VariableLetter == '\0')
                {
                    continue;
                }

                if (HandLetterStillNeededOnBoard(i))
                {
                    _spentHandIndices.Remove(i);
                }
                else
                {
                    _spentHandIndices.Add(i);
                }
            }
        }

        private bool HandLetterStillNeededOnBoard(int handIndex)
        {
            if (handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            char letter = _hand[handIndex].VariableLetter;
            if (letter == '\0')
            {
                return false;
            }

            return CountPositiveVariablesOnBoard(letter) > 0;
        }

        private int CountPositiveVariablesOnBoard(char letter)
        {
            int count = 0;
            foreach (BoardCard card in Board.Left.Cards)
            {
                if (card.Kind == CardKind.DayCreature && card.VariableLetter == letter)
                {
                    count++;
                }
            }

            foreach (BoardCard card in Board.Right.Cards)
            {
                if (card.Kind == CardKind.DayCreature && card.VariableLetter == letter)
                {
                    count++;
                }
            }

            return count;
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
