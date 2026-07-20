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
        /// <summary>Fraction underlines under 5·x and the dice should refresh (drag/flip divisor).</summary>
        public event Action FractionGuideChanged;
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
        public bool HasPendingDivide => _pendingDivide != null;
        public DividePending PendingDivide => _pendingDivide;
        public bool HasActiveMergeAnimations => _activeMergeAnimations > 0;
        public bool IsLevelComplete => _levelComplete;

        private readonly List<BoardCard> _hand = new();
        private readonly List<BoardCard> _handTemplates = new();
        private readonly List<PendingCancelMarker> _pendingCancels = new();
        private readonly HashSet<int> _spentHandIndices = new();
        private readonly Stack<GameSnapshot> _undoStack = new();
        private GameSnapshot _initialSnapshot;
        private BalancePending _pendingBalance;
        private DividePending _pendingDivide;
        private int? _dragFractionDivisor;
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

        /// <summary>
        /// Hand tile may cancel directly onto a board opposite (Ch2 opposite-play, Ch8/Ch9 addend cancel).
        /// Ch3+ balance chapters must NOT do this — night onto day should start ? balance, not merge.
        /// </summary>
        public bool UsesHandOntoOppositeCancel =>
            UsesOppositeHandPlay || UsesMultiplyAdditionLevels;

        /// <summary>Ch1/Ch2 drag-to-merge: same-side light/dark tiles should snap together into *.</summary>
        public bool UsesDragToMergePairs => CurrentLevel.DragToMergePairs;

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
            CurrentLevel.Chapter >= 5;

        public bool ShouldKeepHandCardInPanel(int handIndex) =>
            !_levelComplete
            && handIndex >= 0
            && handIndex < _hand.Count;

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

            // Dual-hand / multi-hand: never lock other cards while one is mid-balance.
            return true;
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
            // Keep every hand slot visible and present until the level ends.
            if (handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            return !_levelComplete;
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

            if (_hand.Count > 0)
            {
                HandVisualRules.ApplyLevelThemeToHand(_hand, level.CreatureTheme);
            }

            // Unique hand images only — never keep a card and its opposite, or the same image twice.
            HandRules.DedupeFlipFamilies(_hand);

            CaptureHandTemplates();
            Moves.Reset();
            _undoStack.Clear();
            _levelComplete = false;
            _pendingBalance = null;
            _pendingDivide = null;
            Board.Left.ClearDenominator();
            Board.Right.ClearDenominator();
            _pendingCancels.Clear();
            _spentHandIndices.Clear();
            _activeHandSlot = -1;
            _activeMergeAnimations = 0;
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _pendingCancels,
                _spentHandIndices, _pendingDivide);

            LevelLoaded?.Invoke(_levelIndex + 1, LevelCount);
            ResolveCombines();
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _pendingCancels,
                _spentHandIndices, _pendingDivide);
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
                string cancelHint = UsesZeroCancelSymbol ? "0" : "*";
                return "Drag a tile onto a side (onto its opposite when present). A ? appears on the other side — " +
                       $"drag the same tile to the ?. Opposites merge on top into {cancelHint}, then {cancelHint} clears.";
            }

            if (count == 2)
            {
                if (level.Chapter >= 8)
                {
                    return "Cancel the addend with its opposite → 0. " +
                           "Flip the coefficient (e.g. -5 → 5), drop it under the line below 5·x and under the dice. " +
                           "5/5 → 1, dice/5 → answer, so x = dice/5.";
                }

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

        /// <summary>Jump to the first level of the next distinct problem type (⏭ button).</summary>
        public void SkipToNextProblemType()
        {
            int nextIndex = ChapterLevelGenerator.GetNextProblemTypeLevelIndex(_levelIndex);
            LoadLevel(nextIndex);
            int global = nextIndex + 1;
            MessageChanged?.Invoke($"Skipped to next problem type — level {global}.");
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

            _initialSnapshot.Apply(Board, _hand, Moves, out _pendingBalance, _pendingCancels, _spentHandIndices,
                out _pendingDivide);
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

            _undoStack.Pop().Apply(Board, _hand, Moves, out _pendingBalance, _pendingCancels, _spentHandIndices,
                out _pendingDivide);
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
            if (UsesMultiplyAdditionLevels)
            {
                FractionGuideChanged?.Invoke();
            }

            bool creatureOnly = _hand[handIndex].VariableLetter == '\0';
            MessageChanged?.Invoke(CardFlipRules.IsLight(_hand[handIndex])
                ? creatureOnly || !UsesVariablePositiveNegative
                    ? $"Flipped to {LightTerm}. Click again for {DarkTerm}."
                    : UsesMultiplyAdditionLevels && BoardHasCoefficient(_hand[handIndex].Value)
                        ? $"Flipped to {_hand[handIndex].Value}. Drop it under the line below {_hand[handIndex].Value}·x and under the dice."
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
                    string cancelSymbol = UsesZeroCancelSymbol ? "0" : "swirl";
                    MessageChanged?.Invoke(_pendingBalance != null
                        ? $"{Capitalize(LightTerm)} met {DarkTerm} — {cancelSymbol} appears. The ? hole stays until you fill it."
                        : $"{Capitalize(LightTerm)} met {DarkTerm} — {cancelSymbol} appears.");
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

            // Free opposite merges already handled above. Other combine types stay available
            // even while a ? hole or swirl exists on the other side.
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

        /// <summary>
        /// DragonBox divide: drop a number under the line on one side, then the same under the other.
        /// Matching a/a becomes 1; other numbers stay as dice/a until resolved.
        /// </summary>
        public bool TryPlaceDenominatorFromHand(int handIndex, string sideName)
        {
            if (!UsesMultiplyAdditionLevels || _levelComplete)
            {
                return false;
            }

            if (handIndex < 0 || handIndex >= _hand.Count || _spentHandIndices.Contains(handIndex))
            {
                return false;
            }

            if (_pendingBalance != null || _pendingCancels.Count > 0 || _activeMergeAnimations > 0)
            {
                MessageChanged?.Invoke("Finish cancels and ? holes before dividing.");
                return false;
            }

            if (sideName != "Left" && sideName != "Right")
            {
                return false;
            }

            BoardCard handCard = _hand[handIndex];
            if (handCard.Kind is not (CardKind.PositiveConstant or CardKind.NegativeConstant))
            {
                MessageChanged?.Invoke("Drop a number under the line.");
                return false;
            }

            // Use positive face for the denominator (flip if needed conceptually).
            BoardCard denomTemplate = handCard.Kind == CardKind.NegativeConstant
                ? new BoardCard(CardKind.PositiveConstant, handCard.Value)
                : handCard.CloneForPlacement();

            if (_pendingDivide != null)
            {
                return TryCompleteDenominator(handIndex, sideName, denomTemplate);
            }

            BoardSide side = Board.GetSide(sideName);
            if (side.HasDenominator)
            {
                MessageChanged?.Invoke("That side already has a number under the line.");
                return false;
            }

            PushUndo();
            side.Denominator = denomTemplate;
            _pendingDivide = new DividePending
            {
                Card = denomTemplate.Clone(),
                PlacedSide = sideName,
                HandIndex = handIndex
            };

            MessageChanged?.Invoke(
                $"Line under {sideName.ToLowerInvariant()} — drop the same {denomTemplate.Value} under the dice / {denomTemplate.Value}·x on the other side.");
            BoardChanged?.Invoke();
            FractionGuideChanged?.Invoke();
            return true;
        }

        private bool TryCompleteDenominator(int handIndex, string sideName, BoardCard denomTemplate)
        {
            if (handIndex != _pendingDivide.HandIndex)
            {
                MessageChanged?.Invoke("Finish the divide — use the same hand card under the other line.");
                return false;
            }

            if (sideName != _pendingDivide.HoleSide)
            {
                MessageChanged?.Invoke("Drop under the line on the other side.");
                return false;
            }

            if (!_pendingDivide.Matches(denomTemplate)
                && !(_pendingDivide.Card.Value == denomTemplate.Value
                     && denomTemplate.Kind is CardKind.PositiveConstant or CardKind.NegativeConstant))
            {
                MessageChanged?.Invoke("The number under the line must match.");
                return false;
            }

            PushUndo();
            BoardSide holeSide = Board.GetSide(sideName);
            holeSide.Denominator = new BoardCard(CardKind.PositiveConstant, denomTemplate.Value);
            int handForSpend = _pendingDivide.HandIndex;
            _pendingDivide = null;

            if (UsesReusableVariableHandCards)
            {
                RefreshHandSpentStateForReusableCards();
            }
            else
            {
                _spentHandIndices.Add(handForSpend);
            }

            Moves.RegisterCombine();
            int d = holeSide.Denominator.Value.Value;
            MessageChanged?.Invoke(
                $"Both sides ÷{d}. Even dice reduce (e.g. 6/{d}→{6 / Math.Max(1, d)}); uneven stays a fraction.");
            HandChanged?.Invoke();
            BoardChanged?.Invoke();
            FractionGuideChanged?.Invoke();
            TryAutoResolveAfterBothDenominators(d);
            return true;
        }

        /// <summary>
        /// 151–165 only after both sides have the divisor under the line:
        /// coeff/coeff → 1; even dice once (8/2 → 4); uneven (7/2) stays a fraction.
        /// </summary>
        private void TryAutoResolveAfterBothDenominators(int divisor)
        {
            if (!UsesMultiplyAdditionLevels || divisor <= 0)
            {
                return;
            }

            TryResolveMatchingCoefficientOnSide("Left", divisor);
            TryResolveMatchingCoefficientOnSide("Right", divisor);

            // One division by the coefficient on each side — 8/2 → 4 (not left as 8/2).
            ReduceEvenDiceOnce("Left", divisor);
            ReduceEvenDiceOnce("Right", divisor);

            TryClearDenominatorsIfResolved();
            HandChanged?.Invoke();
            BoardChanged?.Invoke();
            FractionGuideChanged?.Invoke();
            CheckWin();
        }

        private void TryResolveMatchingCoefficientOnSide(string sideName, int divisor)
        {
            BoardSide side = Board.GetSide(sideName);
            for (int i = 0; i < side.Cards.Count; i++)
            {
                BoardCard target = side.Cards[i];
                if (target.Kind is not (CardKind.PositiveConstant or CardKind.NegativeConstant))
                {
                    continue;
                }

                if (target.Value != divisor)
                {
                    continue;
                }

                bool wasCoefficient = i + 1 < side.Cards.Count
                    && VariableGoalRules.IsVariableXGoal(side.Cards[i + 1]);
                if (!wasCoefficient)
                {
                    continue;
                }

                side.Cards.RemoveAt(i);
                side.Cards.Insert(i, new BoardCard(CardKind.One, 1));
                ResolveOneIdentitiesOnSide(sideName);
                Moves.RegisterCombine();
                MessageChanged?.Invoke($"{divisor}/{divisor} → 1");
                return;
            }
        }

        /// <summary>
        /// 151–165: divide dice by the coefficient once when even — 8/2 → 4, then clear that line.
        /// Uneven (7/2) is left alone as a fraction.
        /// </summary>
        private bool ReduceEvenDiceOnce(string sideName, int divisor)
        {
            if (divisor <= 0)
            {
                return false;
            }

            BoardSide side = Board.GetSide(sideName);
            for (int i = 0; i < side.Cards.Count; i++)
            {
                BoardCard target = side.Cards[i];
                if (target.Kind is not (CardKind.PositiveConstant or CardKind.NegativeConstant))
                {
                    continue;
                }

                bool isCoefficient = i + 1 < side.Cards.Count
                    && VariableGoalRules.IsVariableXGoal(side.Cards[i + 1]);
                if (isCoefficient)
                {
                    continue;
                }

                // Must divide evenly and be a real quotient (8/2 → 4). Skip uneven fractions.
                if (target.Value % divisor != 0)
                {
                    continue;
                }

                int newValue = target.Value / divisor;
                if (newValue <= 0)
                {
                    continue;
                }

                // Same value (2/2 as lone dice) → 1; larger (8/2) → 4.
                side.Cards[i] = new BoardCard(
                    target.Kind == CardKind.NegativeConstant
                        ? CardKind.NegativeConstant
                        : CardKind.PositiveConstant,
                    newValue);
                // Keep the line when a letter remains (Ch9: x = (b−2)/3).
                if (!SideHasPairVariable(side))
                {
                    side.ClearDenominator();
                }

                Moves.RegisterCombine();
                MessageChanged?.Invoke($"{target.Value}/{divisor} → {newValue}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// With denominators on both sides: tap/drop onto a card above the line.
        /// Same value → 1 (3/3). Other divisible number → dice/d.
        /// </summary>
        public bool TryResolveDivisionOnCard(string sideName, int boardIndex)
        {
            if (!UsesMultiplyAdditionLevels || _levelComplete || _pendingDivide != null)
            {
                return false;
            }

            if (!Board.Left.HasDenominator || !Board.Right.HasDenominator)
            {
                return false;
            }

            int divisor = Board.Left.Denominator.Value.Value;
            if (Board.Right.Denominator.Value.Value != divisor)
            {
                return false;
            }

            BoardSide side = Board.GetSide(sideName);
            if (boardIndex < 0 || boardIndex >= side.Cards.Count)
            {
                return false;
            }

            BoardCard target = side.Cards[boardIndex];
            if (VariableGoalRules.IsVariableXGoal(target) || target.Kind == CardKind.One)
            {
                return false;
            }

            if (target.Kind is not (CardKind.PositiveConstant or CardKind.NegativeConstant))
            {
                return false;
            }

            PushUndo();

            // Matching a/a → 1. Coefficient beside x uses the 1 cancel marker; lone dice become 1.
            if (target.Value == divisor)
            {
                bool wasCoefficient = boardIndex + 1 < side.Cards.Count
                    && VariableGoalRules.IsVariableXGoal(side.Cards[boardIndex + 1]);

                if (wasCoefficient)
                {
                    side.Cards.RemoveAt(boardIndex);
                    side.Cards.Insert(boardIndex, new BoardCard(CardKind.One, 1));
                    ResolveOneIdentitiesOnSide(sideName);
                }
                else
                {
                    side.Cards[boardIndex] = new BoardCard(CardKind.PositiveConstant, 1);
                    CreateOneResultMarker(sideName);
                }

                Moves.RegisterCombine();
                MessageChanged?.Invoke($"{divisor}/{divisor} → 1");
                TryClearDenominatorsIfResolved();
                HandChanged?.Invoke();
                BoardChanged?.Invoke();
                CheckWin();
                return true;
            }

            // dice/d → integer once when even (8/2 → 4); otherwise keep fraction (7/2).
            if (target.Value % divisor != 0)
            {
                Moves.RegisterCombine();
                MessageChanged?.Invoke($"{target.Value}/{divisor} stays as the answer.");
                TryClearDenominatorsIfResolved();
                HandChanged?.Invoke();
                BoardChanged?.Invoke();
                FractionGuideChanged?.Invoke();
                CheckWin();
                return true;
            }

            int newValue = target.Value / divisor;
            side.Cards[boardIndex] = new BoardCard(
                target.Kind == CardKind.NegativeConstant ? CardKind.NegativeConstant : CardKind.PositiveConstant,
                newValue);
            side.ClearDenominator();

            Moves.RegisterCombine();
            MessageChanged?.Invoke($"{target.Value}/{divisor} → {newValue}");
            TryClearDenominatorsIfResolved();
            HandChanged?.Invoke();
            BoardChanged?.Invoke();
            FractionGuideChanged?.Invoke();
            CheckWin();
            return true;
        }

        private void TryClearDenominatorsIfResolved()
        {
            if (!UsesMultiplyAdditionLevels)
            {
                if (Board.Left.HasDenominator && Board.Right.HasDenominator)
                {
                    int d = Board.Left.Denominator.Value.Value;
                    if (!SideStillNeedsDenominator(Board.Left, d) && !SideStillNeedsDenominator(Board.Right, d))
                    {
                        Board.Left.ClearDenominator();
                        Board.Right.ClearDenominator();
                    }
                }

                return;
            }

            // 151–165: never leave an even fraction hanging (8/2 must become 4).
            ForceReduceEvenDenominators("Left");
            ForceReduceEvenDenominators("Right");
            MaybeClearSideDenominator("Left");
            MaybeClearSideDenominator("Right");
        }

        private void ForceReduceEvenDenominators(string sideName)
        {
            BoardSide side = Board.GetSide(sideName);
            if (!side.HasDenominator)
            {
                return;
            }

            int divisor = side.Denominator.Value.Value;
            if (divisor <= 0)
            {
                return;
            }

            ReduceEvenDiceOnce(sideName, divisor);
        }

        private void MaybeClearSideDenominator(string sideName)
        {
            BoardSide side = Board.GetSide(sideName);
            if (!side.HasDenominator)
            {
                return;
            }

            int divisor = side.Denominator.Value.Value;

            // Uneven fraction (7/2): keep the line + denominator — that IS the answer.
            if (SideHasUnevenFraction(side, divisor))
            {
                return;
            }

            // Coefficient a·x still needs a/a → 1.
            if (SideStillNeedsCoefficientCancel(side, divisor))
            {
                return;
            }

            // Ch9 letter answer: keep /coeff under the letter expression.
            if (SideHasPairVariable(side))
            {
                return;
            }

            // Even dice already reduced (or nothing left to divide) — drop the line.
            side.ClearDenominator();
        }

        private static bool SideStillNeedsCoefficientCancel(BoardSide side, int divisor)
        {
            for (int i = 0; i < side.Cards.Count; i++)
            {
                BoardCard card = side.Cards[i];
                if (card.Kind is not (CardKind.PositiveConstant or CardKind.NegativeConstant))
                {
                    continue;
                }

                bool isCoefficient = i + 1 < side.Cards.Count
                    && VariableGoalRules.IsVariableXGoal(side.Cards[i + 1]);
                if (isCoefficient && card.Value == divisor)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SideHasUnevenFraction(BoardSide side, int divisor)
        {
            for (int i = 0; i < side.Cards.Count; i++)
            {
                BoardCard card = side.Cards[i];
                if (card.Kind is not (CardKind.PositiveConstant or CardKind.NegativeConstant))
                {
                    continue;
                }

                bool isCoefficient = i + 1 < side.Cards.Count
                    && VariableGoalRules.IsVariableXGoal(side.Cards[i + 1]);
                if (isCoefficient)
                {
                    continue;
                }

                if (card.Value % divisor != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SideStillNeedsDenominator(BoardSide side, int divisor)
        {
            for (int i = 0; i < side.Cards.Count; i++)
            {
                BoardCard card = side.Cards[i];
                if (card.Kind is not (CardKind.PositiveConstant or CardKind.NegativeConstant))
                {
                    continue;
                }

                bool isCoefficient = i + 1 < side.Cards.Count
                    && VariableGoalRules.IsVariableXGoal(side.Cards[i + 1]);

                // Coefficient a beside x still needs a/a → 1.
                if (isCoefficient && card.Value == divisor)
                {
                    return true;
                }

                // Larger multiples still need dice/d → integer.
                if (card.Value > divisor && card.Value % divisor == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Legacy single-bar entry: place under Left first when no denoms yet.</summary>
        public bool TryDivideBothSidesFromHand(int handIndex)
        {
            if (Board.Left.HasDenominator && !Board.Right.HasDenominator)
            {
                return TryPlaceDenominatorFromHand(handIndex, "Right");
            }

            if (Board.Right.HasDenominator && !Board.Left.HasDenominator)
            {
                return TryPlaceDenominatorFromHand(handIndex, "Left");
            }

            if (Board.Left.HasDenominator && Board.Right.HasDenominator)
            {
                MessageChanged?.Invoke("Tap a number above the line (3/3 → 1, or dice/3).");
                return false;
            }

            return TryPlaceDenominatorFromHand(handIndex, "Left");
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

            // Snap targeting allows any opposite so the hand tile can sit on top visually.
            // Cancel-vs-balance is decided by UsesHandOntoOppositeCancel in the drop handlers.
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
            if (!UsesHandOntoOppositeCancel)
            {
                return false;
            }

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

            // 151–165: drop −1 onto +1 → cancel on this side, blank ? on the other side to drag into.
            if (UsesMultiplyAdditionLevels
                && CombineRules.IsDiceOppositePair(handCard, targetCard)
                && CombineRules.UsesAsteriskCancel(handCard, targetCard))
            {
                return TryStartMultiplyCancelWithBalanceHole(handIndex, sideName, targetBoardIndex, handCard,
                    targetCard);
            }

            _pendingBalance = null;

            if (CombineRules.UsesAsteriskCancel(handCard, targetCard))
            {
                // Place the hand clone beside the target so the swirl sits in that slot.
                int insertAt = Math.Clamp(targetBoardIndex + 1, 0, side.Cards.Count);
                side.Cards.Insert(insertAt, handCard.CloneForPlacement());
                BoardCard placed = side.Cards[insertAt];
                if (!TryCreateCancelMarker(sideName, targetCard.Id, placed.Id))
                {
                    // Marker failed — still cancel instantly so the drop never "pops to the side".
                    CombineRules.RemovePairById(side, targetCard.Id, placed.Id);
                    CombineOccurred?.Invoke(new CombineEvent
                    {
                        SideName = sideName,
                        Action = CombineActionType.OppositeCancel,
                        IndexA = targetBoardIndex,
                        IndexB = insertAt
                    });
                }

                _spentHandIndices.Add(handIndex);
                Moves.RegisterCombine();
                string cancelSymbol = UsesZeroCancelSymbol ? "0" : "swirl";
                MessageChanged?.Invoke(
                    $"{Capitalize(LightTerm)} met {DarkTerm} — {cancelSymbol} appears.");
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
                MessageChanged?.Invoke("Numbers canceled.");
            }

            if (UsesReusableVariableHandCards)
            {
                // Unlock sea/variable/number hand tiles while matching board tiles remain.
                RefreshHandSpentStateForReusableCards();
            }

            HandChanged?.Invoke();
            BoardChanged?.Invoke();
            if (UsesMultiplyAdditionLevels)
            {
                FractionGuideChanged?.Invoke();
            }

            CheckWin();
            return true;
        }

        /// <summary>
        /// 151–165 only: cancel addend on this side (0), and open a blank ? on the other side
        /// so the same hand tile is dragged in to balance.
        /// </summary>
        private bool TryStartMultiplyCancelWithBalanceHole(int handIndex, string sideName,
            int targetBoardIndex, BoardCard handCard, BoardCard targetCard)
        {
            BoardSide side = Board.GetSide(sideName);
            int insertAt = Math.Clamp(targetBoardIndex + 1, 0, side.Cards.Count);
            side.Cards.Insert(insertAt, handCard.CloneForPlacement());
            BoardCard placed = side.Cards[insertAt];
            if (!TryCreateCancelMarker(sideName, targetCard.Id, placed.Id))
            {
                CombineRules.RemovePairById(side, targetCard.Id, placed.Id);
                CombineOccurred?.Invoke(new CombineEvent
                {
                    SideName = sideName,
                    Action = CombineActionType.OppositeCancel,
                    IndexA = targetBoardIndex,
                    IndexB = insertAt
                });
            }

            Moves.RegisterCombine();

            string holeSide = sideName == "Left" ? "Right" : "Left";
            BoardCard template = handCard.Clone();
            // Show blank before the dice: ? + 7 (then −3 into ? → 4).
            _pendingBalance = new BalancePending
            {
                Card = template,
                PlacedSide = sideName,
                PlacedIndex = insertAt,
                HandIndex = handIndex,
                HoleInsertIndex = 0
            };
            _activeHandSlot = handIndex;
            _spentHandIndices.Remove(handIndex);

            string cancelSymbol = UsesZeroCancelSymbol ? "0" : "swirl";
            MessageChanged?.Invoke(
                $"{cancelSymbol} on this side — drag the same tile into the blank ? on the other side.");
            HandChanged?.Invoke();
            BoardChanged?.Invoke();
            FractionGuideChanged?.Invoke();
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
            // Sit on / beside the opposite so the merge shows stacked in that slot (not a free end park).
            int insertAt = FindInsertIndexBesideOpposite(placedSide, targetSide, template);
            placedSide.Cards.Insert(insertAt, template.CloneForPlacement());
            int placedIndex = insertAt;
            string holeSide = targetSide == "Left" ? "Right" : "Left";
            BoardSide holeSideBoard = Board.GetSide(holeSide);
            int holeInsert = FindInsertIndexBesideOpposite(holeSideBoard, holeSide, template);

            _pendingBalance = new BalancePending
            {
                Card = template.Clone(),
                PlacedSide = targetSide,
                PlacedIndex = placedIndex,
                HandIndex = handIndex,
                HoleInsertIndex = holeInsert
            };

            if (UsesPlayableHandDisplay)
            {
                _activeHandSlot = handIndex;
            }

            // 29–150: night on day → merge on top → 0; ? stays on the other side until filled.
            if (!UsesMultiplyAdditionLevels)
            {
                ActivateOppositePairOrCancelDice(targetSide, placedIndex);
            }

            string cancelHint = UsesZeroCancelSymbol ? "0" : "*";
            MessageChanged?.Invoke(_pendingCancels.Count > 0
                ? $"? on the other side — drag the same tile to fill it. {Capitalize(LightTerm)} met {DarkTerm}: {cancelHint} appears."
                : "? appeared on the other side — drag the same tile to fill the hole.");
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
                insertIndex = FindInsertIndexBesideOpposite(balancedSide, targetSide, template);
            }

            // 151–180: fold into dice when numeric; beside a letter keep the constant (b + −2).
            if (UsesMultiplyAdditionLevels
                && template.Kind is CardKind.PositiveConstant or CardKind.NegativeConstant)
            {
                if (SideHasPairVariable(balancedSide))
                {
                    balancedSide.Cards.Insert(insertIndex, template.CloneForPlacement());
                }
                else
                {
                    ApplyConstantToSide(balancedSide, template);
                    insertIndex = -1;
                }
            }
            else
            {
                // Sit on the opposite (merge-on-top), not a free park at the end of the side.
                insertIndex = FindInsertIndexBesideOpposite(balancedSide, targetSide, template);
                balancedSide.Cards.Insert(insertIndex, template.CloneForPlacement());
            }

            int holePlacedIndex = insertIndex;
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

            if (!UsesMultiplyAdditionLevels && holePlacedIndex >= 0)
            {
                ActivateOppositePairOrCancelDice(targetSide, holePlacedIndex);
            }

            Moves.RegisterBalancedPlay();
            string cancelHint = UsesZeroCancelSymbol ? "0" : "*";
            MessageChanged?.Invoke(UsesMultiplyAdditionLevels
                ? "Balanced both sides. Now divide with the coefficient."
                : _pendingCancels.Count > 0
                    ? $"Balanced! {Capitalize(LightTerm)} met {DarkTerm} — {cancelHint} appears, then clears."
                    : UsesManualPairMerge
                        ? $"Balanced! Drag {LightTerm} onto {DarkTerm} on the same side to make {cancelHint}."
                        : "Balanced!");
            PruneInvalidCancelMarkers();
            ResolveCombines();
            if (UsesMultiplyAdditionLevels)
            {
                FractionGuideChanged?.Invoke();
            }

            return true;
        }

        /// <summary>
        /// Insert index so a new tile sits immediately after its opposite (merge-on-top slot),
        /// or at the end when no opposite is present on that side.
        /// </summary>
        private int FindInsertIndexBesideOpposite(BoardSide side, string sideName, BoardCard template)
        {
            for (int j = 0; j < side.Cards.Count; j++)
            {
                if (IsCardPendingCancelOnSide(side.Cards[j].Id, sideName))
                {
                    continue;
                }

                if (CombineRules.GetCombineAction(template, side.Cards[j]) == CombineActionType.OppositeCancel)
                {
                    return j + 1;
                }
            }

            return side.Cards.Count;
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
                string clearing = UsesZeroCancelSymbol ? "0" : "Swirl";
                MessageChanged?.Invoke(_pendingCancels.Count > 1
                    ? $"{clearing}s clearing…"
                    : $"{clearing} clearing…");
                return;
            }

            if (!WinChecker.CanWin(Board, Moves.Moves, _pendingBalance != null, _pendingCancels.Count,
                    _activeMergeAnimations, UsesExtraOppositeTileLevel, UsesVariableXGoalWin,
                    UsesMultiplyAdditionLevels, UsesLetterOppositeAnswerWin))
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
            if (UsesMultiplyAdditionLevels
                && (WinChecker.IsVariableXEqualsConstant(Board)
                    || WinChecker.IsVariableXEqualsFraction(Board)
                    || WinChecker.IsVariableXEqualsLetterExpression(Board)))
            {
                if (WinChecker.IsVariableXEqualsLetterExpression(Board))
                {
                    BoardSide letterSide = SideHasPairVariable(Board.Right) ? Board.Right : Board.Left;
                    MessageChanged?.Invoke($"You win! x = {FormatLetterAnswer(letterSide)}.");
                }
                else if (WinChecker.IsVariableXEqualsFraction(Board))
                {
                    BoardSide fractionSide = Board.Right.HasDenominator ? Board.Right : Board.Left;
                    int num = fractionSide.Cards[0].Value;
                    int den = fractionSide.Denominator.Value.Value;
                    MessageChanged?.Invoke($"You win! x = {num}/{den}.");
                }
                else
                {
                    int answer = Board.Left.Cards.Count == 1 && Board.Left.Cards[0].Kind == CardKind.PositiveConstant
                        ? Board.Left.Cards[0].Value
                        : Board.Right.Cards[0].Value;
                    MessageChanged?.Invoke($"You win! x = {answer}.");
                }
            }
            else
            {
                bool equalsZero = UsesPlusBetweenBoardTiles
                    && (WinChecker.IsZeroOnlySide(Board.Left) || WinChecker.IsZeroOnlySide(Board.Right));
                bool equalsLetter = UsesLetterOppositeAnswerWin
                    && (WinChecker.IsLoneNonGoalVariableSide(Board.Left)
                        || WinChecker.IsLoneNonGoalVariableSide(Board.Right));
                char letterAnswer = equalsLetter
                    ? (WinChecker.IsLoneNonGoalVariableSide(Board.Left)
                        ? Board.Left.Cards[0].VariableLetter
                        : Board.Right.Cards[0].VariableLetter)
                    : '\0';
                MessageChanged?.Invoke(equalsZero
                    ? "You win! x = 0."
                    : equalsLetter
                        ? $"You win! x = {letterAnswer}."
                        : UsesVariableXGoalWin
                            ? "You win! x is alone."
                            : "You win! The red box is alone.");
            }

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

        /// <summary>Levels 140–150: opposite answer is a letter (x = a/b/c/r), not scene 0.</summary>
        public bool UsesLetterOppositeAnswerWin =>
            _levelIndex + 1 >= ChapterLevelGenerator.NumberLevelsStartLevel
            && _levelIndex + 1 <= ChapterLevelGenerator.PlusBetweenTilesEndLevel;

        public bool UsesExtraOppositeTileLevel =>
            _levelIndex + 1 >= ChapterLevelGenerator.OppositeExtraTileStartLevel
            && _levelIndex + 1 <= ChapterLevelGenerator.OppositeExtraTileEndLevel;

        public bool UsesPlusBetweenBoardTiles =>
            ChapterLevelGenerator.UsesPlusBetweenBoardTiles(_levelIndex + 1);

        /// <summary>
        /// Levels 29–150: cancel shows 0 (then dismisses). Ch8/Ch9 addition cancels also use 0.
        /// Ch1–Ch2 keep the swirl.
        /// </summary>
        public bool UsesZeroCancelSymbol =>
            (_levelIndex + 1 >= ChapterLevelGenerator.Chapter3BalanceStartLevel
             && _levelIndex + 1 <= ChapterLevelGenerator.PlusBetweenTilesEndLevel)
            || UsesMultiplyAdditionLevels;

        /// <summary>Levels 151–180: a·x + b = c (number) or a·x + b = letter with divide-both-sides.</summary>
        public bool UsesMultiplyAdditionLevels =>
            ChapterLevelGenerator.UsesMultiplyAddition(_levelIndex + 1);

        public bool UsesMultiplyLetterRhsLevels =>
            ChapterLevelGenerator.UsesMultiplyLetterRhs(_levelIndex + 1);

        private CancelResultSymbol AdditionCancelSymbol =>
            UsesZeroCancelSymbol ? CancelResultSymbol.Zero : CancelResultSymbol.Swirl;

        /// <summary>
        /// Active divisor for fraction lines (e.g. 5 when isolating x from 5·x).
        /// Set while dragging, after flip to +coeff, or when denoms are partially placed.
        /// </summary>
        public int? GetActiveFractionDivisor()
        {
            if (!UsesMultiplyAdditionLevels)
            {
                return null;
            }

            if (Board.Left.HasDenominator)
            {
                return Board.Left.Denominator.Value.Value;
            }

            if (Board.Right.HasDenominator)
            {
                return Board.Right.Denominator.Value.Value;
            }

            if (_pendingDivide != null)
            {
                return _pendingDivide.Card.Value;
            }

            if (_dragFractionDivisor.HasValue)
            {
                return _dragFractionDivisor;
            }

            for (int i = 0; i < _hand.Count; i++)
            {
                if (_spentHandIndices.Contains(i))
                {
                    continue;
                }

                BoardCard handCard = _hand[i];
                // Positive face ready to drop under the line (151–165 only).
                if (handCard.Kind == CardKind.PositiveConstant
                    && BoardHasCoefficient(handCard.Value))
                {
                    return handCard.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Show a line under coeff·x and under the dice (151–165 only) so you can
        /// place the coeff under both → (a·x)/a and dice/a → x = dice/a.
        /// </summary>
        public bool ShouldShowFractionLineUnder(string sideName, int boardIndex)
        {
            if (!UsesMultiplyAdditionLevels)
            {
                return false;
            }

            int? divisor = GetActiveFractionDivisor();
            if (divisor == null)
            {
                return false;
            }

            BoardSide side = Board.GetSide(sideName);
            if (boardIndex < 0 || boardIndex >= side.Cards.Count)
            {
                return false;
            }

            BoardCard card = side.Cards[boardIndex];
            bool isNumber = card.Kind is CardKind.PositiveConstant or CardKind.NegativeConstant;
            bool isCoefficient = isNumber
                && boardIndex + 1 < side.Cards.Count
                && VariableGoalRules.IsVariableXGoal(side.Cards[boardIndex + 1]);

            // Line under the a of a·x
            if (isCoefficient && card.Value == divisor.Value)
            {
                return true;
            }

            // Line continues under the x of a·x (same product term).
            if (VariableGoalRules.IsVariableXGoal(card)
                && boardIndex > 0
                && side.Cards[boardIndex - 1].Kind is (CardKind.PositiveConstant or CardKind.NegativeConstant)
                && side.Cards[boardIndex - 1].Value == divisor.Value)
            {
                return true;
            }

            // Line under the dice (side without x) — not under the addend beside x.
            if (isNumber && !isCoefficient && !SideHasVariableX(side))
            {
                return true;
            }

            // Ch9: line under the letter on the side opposite x (e.g. b in 3·x = b).
            if (VariableGoalRules.IsPairVariable(card) && !SideHasVariableX(side))
            {
                return true;
            }

            return false;
        }

        /// <summary>True when this board index is the start of coeff·x for the active divisor (151–165).</summary>
        public bool IsFractionProductAnchor(string sideName, int boardIndex)
        {
            if (!UsesMultiplyAdditionLevels || !ShouldShowFractionLineUnder(sideName, boardIndex))
            {
                return false;
            }

            BoardSide side = Board.GetSide(sideName);
            return boardIndex + 1 < side.Cards.Count
                && side.Cards[boardIndex].Kind is CardKind.PositiveConstant or CardKind.NegativeConstant
                && VariableGoalRules.IsVariableXGoal(side.Cards[boardIndex + 1]);
        }

        private static bool SideHasVariableX(BoardSide side)
        {
            for (int i = 0; i < side.Cards.Count; i++)
            {
                if (VariableGoalRules.IsVariableXGoal(side.Cards[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SideHasPairVariable(BoardSide side)
        {
            for (int i = 0; i < side.Cards.Count; i++)
            {
                if (VariableGoalRules.IsPairVariable(side.Cards[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static string FormatLetterAnswer(BoardSide side)
        {
            var parts = new System.Text.StringBuilder();
            for (int i = 0; i < side.Cards.Count; i++)
            {
                BoardCard card = side.Cards[i];
                if (i > 0 && card.Kind is CardKind.PositiveConstant or CardKind.DayCreature)
                {
                    parts.Append('+');
                }

                if (VariableGoalRules.IsPairVariable(card))
                {
                    parts.Append(card.VariableLetter);
                }
                else if (card.Kind == CardKind.NegativeConstant)
                {
                    parts.Append('-').Append(card.Value);
                }
                else if (card.Kind == CardKind.PositiveConstant)
                {
                    parts.Append(card.Value);
                }
            }

            string body = parts.Length > 0 ? parts.ToString() : "?";
            if (side.HasDenominator)
            {
                int d = side.Denominator.Value.Value;
                if (side.Cards.Count > 1)
                {
                    return $"({body})/{d}";
                }

                return $"{body}/{d}";
            }

            return body;
        }

        public void BeginFractionDrag(int handIndex)
        {
            if (!UsesMultiplyAdditionLevels || handIndex < 0 || handIndex >= _hand.Count)
            {
                return;
            }

            BoardCard handCard = _hand[handIndex];
            if (handCard.Kind is not (CardKind.PositiveConstant or CardKind.NegativeConstant))
            {
                return;
            }

            // Coefficient of a·x: dragging it shows lines under a·x and the dice (151–165 only).
            if (!BoardHasCoefficient(handCard.Value))
            {
                _dragFractionDivisor = null;
                FractionGuideChanged?.Invoke();
                return;
            }

            _dragFractionDivisor = handCard.Value;
            FractionGuideChanged?.Invoke();
            if (handCard.Kind == CardKind.PositiveConstant)
            {
                MessageChanged?.Invoke(
                    $"Drop {handCard.Value} under the line below {handCard.Value}·x and under the dice.");
            }
            else
            {
                MessageChanged?.Invoke(
                    $"Flip to +{handCard.Value}, then drop it under the line below {handCard.Value}·x and under the dice.");
            }
        }

        public void EndFractionDrag()
        {
            if (!_dragFractionDivisor.HasValue)
            {
                return;
            }

            _dragFractionDivisor = null;
            FractionGuideChanged?.Invoke();
        }

        private bool BoardHasCoefficient(int value)
        {
            return SideHasCoefficient(Board.Left, value) || SideHasCoefficient(Board.Right, value);
        }

        private static bool SideHasCoefficient(BoardSide side, int value)
        {
            for (int i = 0; i < side.Cards.Count - 1; i++)
            {
                if (side.Cards[i].Kind == CardKind.PositiveConstant
                    && side.Cards[i].Value == value
                    && VariableGoalRules.IsVariableXGoal(side.Cards[i + 1]))
                {
                    return true;
                }
            }

            return false;
        }

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
                _spentHandIndices, _pendingDivide));
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

                // Addition chapters: show 0 cancel marker instead of silently deleting dice.
                if (UsesZeroCancelSymbol && TryCreateCancelMarker(sideName, placed.Id, side.Cards[j].Id))
                {
                    Moves.RegisterCombine();
                    MessageChanged?.Invoke($"{Capitalize(LightTerm)} met {DarkTerm} — 0 appears.");
                    return true;
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

        private bool TryCreateCancelMarker(string sideName, string cardIdA, string cardIdB)
        {
            if (IsCardPendingCancelOnSide(cardIdA, sideName) || IsCardPendingCancelOnSide(cardIdB, sideName))
            {
                return false;
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
                return false;
            }

            if (!SideContainsBothCards(side, cardIdA, cardIdB))
            {
                return false;
            }

            if (!CardExistsOnlyOnSide(sideName, cardIdA) || !CardExistsOnlyOnSide(sideName, cardIdB))
            {
                return false;
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
                    return false;
                }
            }

            _pendingCancels.Add(new PendingCancelMarker
            {
                SideName = sideName,
                CardIdA = cardIdA,
                CardIdB = cardIdB,
                ResultSymbol = AdditionCancelSymbol
            });
            return true;
        }

        private void CreateOneResultMarker(string sideName)
        {
            _pendingCancels.Add(new PendingCancelMarker
            {
                SideName = sideName,
                CardIdA = string.Empty,
                CardIdB = string.Empty,
                SwirlOnly = true,
                ResultSymbol = CancelResultSymbol.One
            });
        }

        /// <summary>Remove identity 1 next to x after division (1·x → x), with a 1 dismiss marker.</summary>
        private void ResolveOneIdentitiesAfterDivide()
        {
            ResolveOneIdentitiesOnSide("Left");
            ResolveOneIdentitiesOnSide("Right");
        }

        private void ResolveOneIdentitiesOnSide(string sideName)
        {
            BoardSide side = Board.GetSide(sideName);
            for (int i = side.Cards.Count - 1; i >= 0; i--)
            {
                if (side.Cards[i].Kind != CardKind.One)
                {
                    continue;
                }

                bool nextToX = (i + 1 < side.Cards.Count && VariableGoalRules.IsVariableXGoal(side.Cards[i + 1]))
                    || (i > 0 && VariableGoalRules.IsVariableXGoal(side.Cards[i - 1]));
                if (!nextToX && side.Cards.Count > 1)
                {
                    continue;
                }

                side.Cards.RemoveAt(i);
                CreateOneResultMarker(sideName);
            }
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
                // Keep the tile free while a blank ? still needs this same hand card (151–165 balance).
                if (_pendingBalance != null && _pendingBalance.HandIndex == i)
                {
                    _spentHandIndices.Remove(i);
                    continue;
                }

                // Numbers use matching +/- board constants (not light-sea count).
                // Sea / variable tiles use HandTileStillNeededOnBoard as before.
                if (HandTileStillNeededOnBoard(i))
                {
                    _spentHandIndices.Remove(i);
                }
                else
                {
                    _spentHandIndices.Add(i);
                }
            }
        }

        private bool HandTileStillNeededOnBoard(int handIndex)
        {
            if (handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            // 151–165: blank ? still waiting for this tile.
            if (_pendingBalance != null && _pendingBalance.HandIndex == handIndex)
            {
                return true;
            }

            BoardCard handCard = _hand[handIndex];
            if (handCard.Kind is CardKind.PositiveConstant or CardKind.NegativeConstant)
            {
                // Negative hand cancels positive board numbers (and the reverse after flip).
                CardKind needed = handCard.Kind == CardKind.NegativeConstant
                    ? CardKind.PositiveConstant
                    : CardKind.NegativeConstant;
                if (CountConstantsOnBoard(needed, handCard.Value) > 0)
                {
                    return true;
                }

                // Multiply levels: keep the coefficient card available to drop on the divide line.
                return UsesMultiplyAdditionLevels
                    && DivisionRules.CanDivideBothSides(Board, handCard.Value);
            }

            char letter = handCard.VariableLetter;
            if (letter != '\0')
            {
                return CountPositiveVariablesOnBoard(letter) > 0;
            }

            // Sea hand tile stays unlocked while any light sea remains on the board.
            return CountLightSeaCreaturesOnBoard() > 0;
        }

        /// <summary>
        /// Fold a constant into the first number on a side (9 + −3 → 6), or append if none.
        /// </summary>
        private static void ApplyConstantToSide(BoardSide side, BoardCard delta)
        {
            int deltaSigned = delta.SignedValue;
            for (int i = 0; i < side.Cards.Count; i++)
            {
                BoardCard card = side.Cards[i];
                if (card.Kind is not (CardKind.PositiveConstant or CardKind.NegativeConstant))
                {
                    continue;
                }

                int sum = card.SignedValue + deltaSigned;
                if (sum == 0)
                {
                    side.Cards.RemoveAt(i);
                    return;
                }

                side.Cards[i] = new BoardCard(
                    sum > 0 ? CardKind.PositiveConstant : CardKind.NegativeConstant,
                    Math.Abs(sum),
                    card.StackCount,
                    card.VisualTheme,
                    '\0');
                return;
            }

            side.Cards.Add(delta.CloneForPlacement());
        }

        private int CountConstantsOnBoard(CardKind kind, int value)
        {
            int count = 0;
            foreach (BoardCard card in Board.Left.Cards)
            {
                if (card.Kind == kind && card.Value == value)
                {
                    count++;
                }
            }

            foreach (BoardCard card in Board.Right.Cards)
            {
                if (card.Kind == kind && card.Value == value)
                {
                    count++;
                }
            }

            return count;
        }

        private bool HandLetterStillNeededOnBoard(int handIndex) =>
            HandTileStillNeededOnBoard(handIndex);

        private int CountLightSeaCreaturesOnBoard()
        {
            int count = 0;
            foreach (BoardCard card in Board.Left.Cards)
            {
                if (card.Kind == CardKind.DayCreature && card.VariableLetter == '\0')
                {
                    count++;
                }
            }

            foreach (BoardCard card in Board.Right.Cards)
            {
                if (card.Kind == CardKind.DayCreature && card.VariableLetter == '\0')
                {
                    count++;
                }
            }

            return count;
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
