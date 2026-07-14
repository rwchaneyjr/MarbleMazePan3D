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

        private readonly List<BoardCard> _hand = new();
        private readonly List<BoardCard> _handTemplates = new();
        private readonly List<PendingCancelMarker> _pendingCancels = new();
        private readonly HashSet<int> _spentHandIndices = new();
        private readonly Stack<GameSnapshot> _undoStack = new();
        private GameSnapshot _initialSnapshot;
        private BalancePending _pendingBalance;
        private int _levelIndex;
        private bool _levelComplete;
        private int _activeMergeAnimations;
        private bool UsesManualPairMerge =>
            CurrentLevel.Chapter >= 3
            || (CurrentLevel.Chapter == 2 && ChapterLevelGenerator.IndexWithinChapter(_levelIndex) >= 16);
        private static readonly Random Rng = new();

        public int LevelIndex => _levelIndex;
        public int LevelCount => LevelLibrary.Levels.Count;
        public LevelDefinition CurrentLevel => LevelLibrary.Levels[_levelIndex];

        public bool UsesPlayableHandDisplay => CurrentLevel.Chapter >= 4;

        public bool ShouldDisplayHandCard(int handIndex)
        {
            if (!UsesPlayableHandDisplay)
            {
                return true;
            }

            if (_spentHandIndices.Contains(handIndex))
            {
                return false;
            }

            if (_pendingBalance != null)
            {
                return handIndex == _pendingBalance.HandIndex;
            }

            return true;
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

            Board.Reset(level.BuildSide(level.LeftCards, level.LeftValues, level.LeftCreatureTheme),
                level.BuildSide(level.RightCards, level.RightValues, level.RightCreatureTheme));

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

            if (_hand.Count > 1)
            {
                HandVisualRules.EnsureDistinctHandVisuals(_hand, level.CreatureTheme);
            }

            CaptureHandTemplates();
            Moves.Reset();
            _undoStack.Clear();
            _levelComplete = false;
            _pendingBalance = null;
            _pendingCancels.Clear();
            _spentHandIndices.Clear();
            _activeMergeAnimations = 0;
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _pendingCancels,
                _spentHandIndices);

            LevelLoaded?.Invoke(_levelIndex + 1, LevelCount);
            if (_hand.Count == 0 && !level.DragToMergePairs)
            {
                ActivatePreplacedOppositePairs();
            }
            ResolveCombines();
            _initialSnapshot = GameSnapshot.Capture(Board, _hand, Moves, _pendingBalance, _pendingCancels,
                _spentHandIndices);
            BoardChanged?.Invoke();
            HandChanged?.Invoke();
            MessageChanged?.Invoke(_pendingCancels.Count > 0 && _hand.Count == 0
                ? level.Chapter == 1
                    ? "Watch light and dark merge into *. Tap the spinning * to dismiss. Leave the red box alone!"
                    : "Click the spinning * to dismiss the creatures. Leave the red box alone!"
                : level.DragToMergePairs
                    ? level.LeftCards.Count >= 2 && level.RightCards.Count >= 2
                        ? "Drag light onto dark on each side to make *. Tap every * before the puzzle finishes!"
                        : "Drag light onto dark on the same side. They snap together into *. Tap * to dismiss. Leave the red box alone!"
                    : HandMessage(level));
            CreatureSpriteDebug.LogLevel(Board, _hand, level);
        }

        private static string HandMessage(LevelDefinition level)
        {
            int count = level.HandCards.Count;
            if (count <= 1)
            {
                return "Drag a tile to one side. A ? appears on the other side. Drag the same tile to the ? to balance. " +
                       "Light + dark on the same side become one *. Pairs never cross the middle.";
            }

            if (count == 2)
            {
                return "Two tiles in hand — play each one: drag to a side, then drag the same tile to the ?. " +
                       "Finish one tile before starting the next.";
            }

            return "Three tiles in hand — play each one: drag to a side, then drag the same tile to the ?. " +
                   "Light + dark on the same side become one *.";
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
            _levelComplete = false;
            BoardChanged?.Invoke();
            HandChanged?.Invoke();
            MessageChanged?.Invoke("Undid the last move.");
        }

        public void RefreshHandPresentation()
        {
            HandChanged?.Invoke();
        }

        public bool TryFlipHandCard(int handIndex)
        {
            if (_levelComplete || handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            BoardCard card = _hand[handIndex];
            if (!CardFlipRules.CanFlip(card))
            {
                MessageChanged?.Invoke("That card cannot flip.");
                return false;
            }

            PushUndo();
            _hand[handIndex] = CardFlipRules.Flip(card);
            SyncHandTemplateForCard(_hand[handIndex]);

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
                        ? "Light met dark — click *. The ? hole stays until you fill it."
                        : "Light met dark — click the spinning * to dismiss.");
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

        public bool TryPlayHandOntoOpposite(int handIndex, string sideName, int targetBoardIndex)
        {
            if (_levelComplete || handIndex < 0 || handIndex >= _hand.Count)
            {
                return false;
            }

            if (CurrentLevel.Chapter >= 3)
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
                return false;
            }

            PushUndo();
            _pendingBalance = null;
            if (CombineRules.UsesAsteriskCancel(handCard, targetCard))
            {
                side.Cards.Add(handCard.CloneForPlacement());
                BoardCard placed = side.Cards[side.Cards.Count - 1];
                TryCreateCancelMarker(sideName, targetCard.Id, placed.Id);
                SyncHandFromTemplates();
                Moves.RegisterBalancedPlay();
                MessageChanged?.Invoke("Light met dark — click the spinning * to dismiss.");
            }
            else
            {
                CombineRules.RemoveCardById(side, targetCard.Id);
                SyncHandFromTemplates();
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

            MessageChanged?.Invoke("? appeared on the other side — drag the same tile to fill the hole.");
            BoardChanged?.Invoke();
            if (UsesPlayableHandDisplay)
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
            SyncHandFromTemplates();
            if (UsesPlayableHandDisplay)
            {
                _spentHandIndices.Add(handIndex);
            }

            HandChanged?.Invoke();

            if (!UsesManualPairMerge)
            {
                ActivateOppositePairOrCancelDice(placedSide, placedBoardIndex);
                ActivateOppositePairOrCancelDice(targetSide, holePlacedIndex);
            }

            Moves.RegisterBalancedPlay();
            MessageChanged?.Invoke(UsesManualPairMerge
                ? _pendingCancels.Count > 0
                    ? "Balanced! Tap every *. Drag light onto dark on the same side to make more *."
                    : "Balanced! Drag light onto dark on the same side to make *."
                : _pendingCancels.Count > 0
                    ? "Balanced! Click the spinning * to dismiss creatures."
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
            if (_pendingBalance != null)
            {
                return;
            }

            if (_pendingCancels.Count > 0)
            {
                MessageChanged?.Invoke(_pendingCancels.Count > 1
                    ? "Tap each swirl to dismiss — clear them in any order."
                    : "Tap the swirl to dismiss it.");
                return;
            }

            if (_activeMergeAnimations > 0)
            {
                return;
            }

            bool puzzleComplete = WinChecker.IsReadyForSidesTogether(Board)
                || (UsesManualPairMerge
                    ? WinChecker.IsBoxAloneOnItsSide(Board)
                    : WinChecker.IsBoxAlone(Board));
            if (!puzzleComplete)
            {
                return;
            }

            _levelComplete = true;
            int stars = Moves.CalculateStars(CurrentLevel);
            int moves = Moves.Moves;
            MessageChanged?.Invoke(WinChecker.IsReadyForSidesTogether(Board)
                ? "You win! The sides come together."
                : UsesManualPairMerge
                    ? "You win! The sides come together."
                    : "You win! The red box is alone.");
            WinSequenceStarted?.Invoke(stars, moves);
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
            if (SideAlreadyHasCancelMarker(sideName))
            {
                return;
            }

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
            if (SideAlreadyHasCancelMarker(sideName))
            {
                return;
            }

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

        private bool SideAlreadyHasCancelMarker(string sideName)
        {
            foreach (PendingCancelMarker marker in _pendingCancels)
            {
                if (marker.SideName == sideName)
                {
                    return true;
                }
            }

            return false;
        }

        private void PruneInvalidCancelMarkers()
        {
            for (int i = _pendingCancels.Count - 1; i >= 0; i--)
            {
                PendingCancelMarker marker = _pendingCancels[i];
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
            if (SideAlreadyHasCancelMarker(sideName))
            {
                return;
            }

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

        private void PopUndoWithoutApply()
        {
            if (_undoStack.Count > 0)
            {
                _undoStack.Pop();
            }
        }
    }
}
