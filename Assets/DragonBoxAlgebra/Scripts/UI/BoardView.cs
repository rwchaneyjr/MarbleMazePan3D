using System;
using System.Collections;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class BoardView : MonoBehaviour
    {
        private const float WinPreDelay = 0.4f;
        private const float WinSlideDuration = 1.15f;
        private const float WinPostDelay = 0.35f;
        private const float DefaultTileWidth = 110f;
        private const float DefaultTileHeight = 120f;
        private const float DefaultTileSpacing = 16f;
        private const float PlusSeparatorWidth = 28f;
        private const float TimesSeparatorWidth = 22f;
        private const float MinTileWidth = 52f;
        private const float MinTileSpacing = 4f;
        private const int CompactPadding = 12;
        private const int DefaultPadding = 24;

        private RectTransform _leftPanel;
        private RectTransform _rightPanel;
        private RectTransform _boardRow;
        private DenominatorDropZone _leftDenomZone;
        private DenominatorDropZone _rightDenomZone;
        private RectTransform _dragRoot;
        private Canvas _canvas;
        private AlgebraGameController _controller;
        private readonly List<CardWidget> _widgets = new();

        private Vector2 _leftAnchorMinDefault;
        private Vector2 _leftAnchorMaxDefault;
        private Vector2 _rightAnchorMinDefault;
        private Vector2 _rightAnchorMaxDefault;
        private bool _playingWinSequence;
        private Coroutine _winSequenceCoroutine;

        public void Initialize(AlgebraGameController controller, RectTransform left, RectTransform right,
            Canvas canvas, RectTransform dragRoot, RectTransform boardRow = null)
        {
            _controller = controller;
            _leftPanel = left;
            _rightPanel = right;
            _boardRow = boardRow;
            _canvas = canvas;
            _dragRoot = dragRoot;

            _leftAnchorMinDefault = left.anchorMin;
            _leftAnchorMaxDefault = left.anchorMax;
            _rightAnchorMinDefault = right.anchorMin;
            _rightAnchorMaxDefault = right.anchorMax;

            left.gameObject.AddComponent<BoardDropZone>().SideName = "Left";
            right.gameObject.AddComponent<BoardDropZone>().SideName = "Right";

            EnsureDenominatorZones();

            _controller.BoardChanged += Refresh;
            _controller.CombineOccurred += OnCombine;
            _controller.LevelLoaded += OnLevelLoaded;
            _controller.WinSequenceStarted += OnWinSequenceStarted;
            _controller.FractionGuideChanged += OnFractionGuideChanged;
            Refresh();
        }

        private void EnsureDenominatorZones()
        {
            if (_leftPanel == null || _rightPanel == null)
            {
                return;
            }

            if (_leftDenomZone == null)
            {
                _leftDenomZone = DenominatorDropZone.Create(_leftPanel, _controller, "Left");
            }

            if (_rightDenomZone == null)
            {
                _rightDenomZone = DenominatorDropZone.Create(_rightPanel, _controller, "Right");
            }
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.BoardChanged -= Refresh;
                _controller.CombineOccurred -= OnCombine;
                _controller.LevelLoaded -= OnLevelLoaded;
                _controller.WinSequenceStarted -= OnWinSequenceStarted;
                _controller.FractionGuideChanged -= OnFractionGuideChanged;
            }
        }

        private void OnFractionGuideChanged()
        {
            for (int i = 0; i < _widgets.Count; i++)
            {
                _widgets[i]?.RefreshFractionGuide();
            }

            RefreshDenominatorZones();
        }

        private void OnLevelLoaded(int current, int total)
        {
            if (_winSequenceCoroutine != null)
            {
                StopCoroutine(_winSequenceCoroutine);
                _winSequenceCoroutine = null;
            }

            _playingWinSequence = false;
            ResetPanelAnchors();
        }

        private void ResetPanelAnchors()
        {
            _leftPanel.anchorMin = _leftAnchorMinDefault;
            _leftPanel.anchorMax = _leftAnchorMaxDefault;
            _rightPanel.anchorMin = _rightAnchorMinDefault;
            _rightPanel.anchorMax = _rightAnchorMaxDefault;
        }

        public string SideAtScreenPosition(Vector2 screenPosition)
        {
            Camera cam = _canvas != null ? _canvas.worldCamera : null;
            if (_rightPanel != null
                && RectTransformUtility.RectangleContainsScreenPoint(_rightPanel, screenPosition, cam))
            {
                return "Right";
            }

            if (_leftPanel != null
                && RectTransformUtility.RectangleContainsScreenPoint(_leftPanel, screenPosition, cam))
            {
                return "Left";
            }

            return screenPosition.x >= Screen.width * 0.5f ? "Right" : "Left";
        }

        private void OnWinSequenceStarted(int stars, int moves)
        {
            if (_winSequenceCoroutine != null)
            {
                StopCoroutine(_winSequenceCoroutine);
                _winSequenceCoroutine = null;
            }

            _playingWinSequence = true;
            _winSequenceCoroutine = StartCoroutine(PlayWinSequence(stars, moves));
        }

        private IEnumerator PlayWinSequence(int stars, int moves)
        {
            while (!_controller.CanPresentWin())
            {
                yield return null;
            }

            yield return new WaitForSeconds(WinPreDelay);

            bool animateLeft = SideHasVisibleCards("Left");
            bool animateRight = SideHasVisibleCards("Right");

            Vector2 leftTargetMin = animateLeft
                ? new Vector2(0.36f, _leftAnchorMinDefault.y)
                : _leftAnchorMinDefault;
            Vector2 leftTargetMax = animateLeft
                ? new Vector2(0.49f, _leftAnchorMaxDefault.y)
                : _leftAnchorMaxDefault;
            Vector2 rightTargetMin = animateRight
                ? new Vector2(0.51f, _rightAnchorMinDefault.y)
                : _rightAnchorMinDefault;
            Vector2 rightTargetMax = animateRight
                ? new Vector2(0.64f, _rightAnchorMaxDefault.y)
                : _rightAnchorMaxDefault;

            float elapsed = 0f;
            while (elapsed < WinSlideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / WinSlideDuration));

                _leftPanel.anchorMin = Vector2.Lerp(_leftAnchorMinDefault, leftTargetMin, animateLeft ? t : 0f);
                _leftPanel.anchorMax = Vector2.Lerp(_leftAnchorMaxDefault, leftTargetMax, animateLeft ? t : 0f);
                _rightPanel.anchorMin = Vector2.Lerp(_rightAnchorMinDefault, rightTargetMin, animateRight ? t : 0f);
                _rightPanel.anchorMax = Vector2.Lerp(_rightAnchorMaxDefault, rightTargetMax, animateRight ? t : 0f);

                yield return null;
            }

            _leftPanel.anchorMin = leftTargetMin;
            _leftPanel.anchorMax = leftTargetMax;
            _rightPanel.anchorMin = rightTargetMin;
            _rightPanel.anchorMax = rightTargetMax;

            yield return new WaitForSeconds(WinPostDelay);

            _controller.ClearOppositeSideAfterSidesTogether();
            _playingWinSequence = false;
            _winSequenceCoroutine = null;
            _controller.CompleteWinPresentation(stars, moves);
        }

        private bool SideHasVisibleCards(string sideName)
        {
            BoardSide side = sideName == "Left" ? _controller.Board.Left : _controller.Board.Right;
            foreach (BoardCard card in side.Cards)
            {
                if (!_controller.IsCardPendingCancelOnSide(card.Id, sideName))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnCombine(CombineEvent evt)
        {
            if (evt.Action != CombineActionType.OppositeCancel)
            {
                return;
            }

            if (DragonBoxAlgebra.Audio.AudioManager.Instance != null)
            {
                DragonBoxAlgebra.Audio.AudioManager.Instance.PlayCombine();
            }

            Vector3 worldPos = ResolveCombineWorldPosition(evt);
            if (_dragRoot != null)
            {
                _dragRoot.SetAsLastSibling();
                VortexEffect.Play(_dragRoot, worldPos);
            }
            else if (_canvas != null)
            {
                VortexEffect.Play(_canvas.transform, worldPos);
            }
        }

        private Vector3 ResolveCombineWorldPosition(CombineEvent evt)
        {
            Vector3 sum = Vector3.zero;
            int found = 0;
            foreach (CardWidget widget in _widgets)
            {
                if (widget == null || widget.SideName != evt.SideName)
                {
                    continue;
                }

                if (widget.Index == evt.IndexA || widget.Index == evt.IndexB)
                {
                    sum += widget.transform.position;
                    found++;
                }
            }

            if (found > 0)
            {
                return sum / found;
            }

            RectTransform panel = evt.SideName == "Left" ? _leftPanel : _rightPanel;
            return panel != null ? panel.position : Vector3.zero;
        }

        private void Refresh()
        {
            if (_playingWinSequence)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();

            ClearOrphanedBoardDragWidgets();
            _widgets.Clear();
            int leftSlots = CountSlotsForSide("Left", _controller.Board.Left);
            int rightSlots = CountSlotsForSide("Right", _controller.Board.Right);
            int leftPlus = CountPlusSeparatorsForSide("Left", _controller.Board.Left);
            int rightPlus = CountPlusSeparatorsForSide("Right", _controller.Board.Right);
            TileLayout leftLayout = ComputeTileLayout(_leftPanel, leftSlots, leftPlus);
            TileLayout rightLayout = ComputeTileLayout(_rightPanel, rightSlots, rightPlus);
            RebuildSide(_leftPanel, _controller.Board.Left, "Left", leftLayout);
            RebuildSide(_rightPanel, _controller.Board.Right, "Right", rightLayout);
            RefreshDenominatorZones();

            if (_controller.HasPendingBalance)
            {
                BalancePending pending = _controller.PendingBalance;
                RectTransform holePanel = pending.HoleSide == "Left" ? _leftPanel : _rightPanel;
                TileLayout holeLayout = pending.HoleSide == "Left" ? leftLayout : rightLayout;
                BalanceHoleWidget hole = BalanceHoleWidget.Create(holePanel, _controller, pending.HoleSide, pending.Card,
                    holeLayout.Width, holeLayout.Height);
                int holeSlot = Mathf.Clamp(pending.HoleInsertIndex, 0, holePanel.childCount - 1);
                hole.transform.SetSiblingIndex(holeSlot);
            }

            // SwirlOnly markers (cards already gone) still append at end of their side.
            BuildSwirlOnlyMarkers(_leftPanel, "Left", leftLayout);
            BuildSwirlOnlyMarkers(_rightPanel, "Right", rightLayout);
        }

        private int CountSlotsForSide(string sideName, BoardSide side)
        {
            int visibleCards = 0;
            for (int i = 0; i < side.Cards.Count; i++)
            {
                if (!_controller.IsCardPendingCancelOnSide(side.Cards[i].Id, sideName))
                {
                    visibleCards++;
                }
            }

            int count = visibleCards;
            if (_controller.UsesPlusBetweenBoardTiles && visibleCards > 1)
            {
                count += visibleCards - 1;
            }

            if (_controller.HasPendingBalance && _controller.PendingBalance.HoleSide == sideName)
            {
                count++;
            }

            IReadOnlyList<PendingCancelMarker> markers = _controller.PendingCancels;
            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i].SideName == sideName)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountPlusSeparatorsForSide(string sideName, BoardSide side)
        {
            if (!_controller.UsesPlusBetweenBoardTiles)
            {
                return 0;
            }

            int visibleCards = 0;
            for (int i = 0; i < side.Cards.Count; i++)
            {
                if (!_controller.IsCardPendingCancelOnSide(side.Cards[i].Id, sideName))
                {
                    visibleCards++;
                }
            }

            return visibleCards > 1 ? visibleCards - 1 : 0;
        }

        private TileLayout ComputeTileLayout(RectTransform panel, int slotCount, int plusSeparatorCount)
        {
            int padding = slotCount >= 5 ? CompactPadding : DefaultPadding;
            float available = Mathf.Max(0f, panel.rect.width - padding * 2f);

            if (slotCount <= 0 || available <= 0f)
            {
                return new TileLayout(DefaultTileWidth, DefaultTileHeight, DefaultTileSpacing, DefaultPadding);
            }

            int cardSlots = Mathf.Max(1, slotCount - plusSeparatorCount);
            float spacing = slotCount >= 5 ? 8f : DefaultTileSpacing;
            float needed = cardSlots * DefaultTileWidth
                + plusSeparatorCount * PlusSeparatorWidth
                + (slotCount - 1) * spacing;

            if (needed <= available)
            {
                return new TileLayout(DefaultTileWidth, DefaultTileHeight, spacing, padding);
            }

            float width = (available - plusSeparatorCount * PlusSeparatorWidth - (slotCount - 1) * MinTileSpacing)
                / cardSlots;
            width = Mathf.Clamp(width, MinTileWidth, DefaultTileWidth);
            spacing = slotCount > 1
                ? Mathf.Max(MinTileSpacing,
                    (available - cardSlots * width - plusSeparatorCount * PlusSeparatorWidth) / (slotCount - 1))
                : 0f;

            float total = cardSlots * width + plusSeparatorCount * PlusSeparatorWidth + (slotCount - 1) * spacing;
            if (total > available)
            {
                width = (available - plusSeparatorCount * PlusSeparatorWidth - (slotCount - 1) * MinTileSpacing)
                    / cardSlots;
                spacing = MinTileSpacing;
            }

            float height = width * (DefaultTileHeight / DefaultTileWidth);
            return new TileLayout(width, height, spacing, padding);
        }

        private readonly struct TileLayout
        {
            public readonly float Width;
            public readonly float Height;
            public readonly float Spacing;
            public readonly int Padding;

            public TileLayout(float width, float height, float spacing, int padding)
            {
                Width = width;
                Height = height;
                Spacing = spacing;
                Padding = padding;
            }
        }

        private void BuildSwirlOnlyMarkers(RectTransform panel, string sideName, TileLayout layout)
        {
            IReadOnlyList<PendingCancelMarker> markers = _controller.PendingCancels;
            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i].SideName != sideName || !markers[i].SwirlOnly)
                {
                    continue;
                }

                AsteriskCancelWidget.Create(panel, _controller, i, layout.Width, layout.Height);
            }
        }

        private void RebuildSide(RectTransform panel, BoardSide side, string sideName, TileLayout layout)
        {
            for (int i = panel.childCount - 1; i >= 0; i--)
            {
                Transform child = panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() == null)
                {
                    Destroy(child.gameObject);
                }
            }

            var horizontalLayout = panel.GetComponent<HorizontalLayoutGroup>();
            if (horizontalLayout == null)
            {
                horizontalLayout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
                horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
                horizontalLayout.childControlWidth = false;
                horizontalLayout.childControlHeight = false;
                horizontalLayout.childForceExpandWidth = false;
                horizontalLayout.childForceExpandHeight = false;
                horizontalLayout.padding = new RectOffset(24, 24, 24, 24);
            }

            horizontalLayout.spacing = layout.Spacing;
            horizontalLayout.padding = new RectOffset(layout.Padding, layout.Padding, layout.Padding, layout.Padding);

            bool usePlus = _controller.UsesPlusBetweenBoardTiles;
            bool useMultiply = _controller.UsesMultiplyAdditionLevels;
            bool placedCard = false;
            BoardCard? previousCard = null;
            var placedMarkerIndexes = new HashSet<int>();

            for (int i = 0; i < side.Cards.Count; i++)
            {
                BoardCard card = side.Cards[i];
                int markerIndex = FindPendingMarkerIndex(sideName, card.Id);
                if (markerIndex >= 0)
                {
                    // Put the merge/swirl in the first pending card's slot so it stays where you dropped.
                    if (!placedMarkerIndexes.Contains(markerIndex)
                        && IsEarlierCardOfPendingPair(side, _controller.PendingCancels[markerIndex], card.Id))
                    {
                        if (usePlus && placedCard)
                        {
                            CreateOperatorSeparator(panel, layout.Height, previousCard, card, useMultiply);
                        }

                        PendingCancelMarker marker = _controller.PendingCancels[markerIndex];
                        BoardCard? cardA = FindCardById(side, marker.CardIdA);
                        BoardCard? cardB = FindCardById(side, marker.CardIdB);
                        if (cardA.HasValue && cardB.HasValue)
                        {
                            AsteriskCancelWidget.CreateMergePair(panel, _controller, markerIndex,
                                cardA.Value, cardB.Value, layout.Width, layout.Height);
                        }
                        else
                        {
                            AsteriskCancelWidget.Create(panel, _controller, markerIndex, layout.Width, layout.Height);
                        }

                        placedMarkerIndexes.Add(markerIndex);
                        placedCard = true;
                        previousCard = card;
                    }

                    continue;
                }

                if (usePlus && placedCard)
                {
                    CreateOperatorSeparator(panel, layout.Height, previousCard, card, useMultiply);
                }

                CardWidget widget = CardWidget.Create(panel, card, i, sideName, _controller, _canvas, _dragRoot,
                    layout.Width, layout.Height);
                widget.gameObject.AddComponent<CardDropZone>();
                _widgets.Add(widget);
                placedCard = true;
                previousCard = card;
            }
        }

        private void RefreshDenominatorZones()
        {
            EnsureDenominatorZones();
            bool chapterActive = _controller.UsesMultiplyAdditionLevels && !_playingWinSequence;
            bool guideActive = chapterActive
                && (_controller.GetActiveFractionDivisor() != null
                    || _controller.Board.Left.HasDenominator
                    || _controller.Board.Right.HasDenominator
                    || _controller.HasPendingDivide);

            if (_leftDenomZone != null)
            {
                _leftDenomZone.gameObject.SetActive(guideActive);
                if (guideActive)
                {
                    _leftDenomZone.RefreshVisual(_controller);
                    _leftDenomZone.transform.SetAsLastSibling();
                }
            }

            if (_rightDenomZone != null)
            {
                _rightDenomZone.gameObject.SetActive(guideActive);
                if (guideActive)
                {
                    _rightDenomZone.RefreshVisual(_controller);
                    _rightDenomZone.transform.SetAsLastSibling();
                }
            }

            if (chapterActive)
            {
                for (int i = 0; i < _widgets.Count; i++)
                {
                    _widgets[i]?.RefreshFractionGuide();
                }
            }
        }

        private int FindPendingMarkerIndex(string sideName, string cardId)
        {
            IReadOnlyList<PendingCancelMarker> markers = _controller.PendingCancels;
            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i].SideName != sideName || markers[i].SwirlOnly)
                {
                    continue;
                }

                if (markers[i].CardIdA == cardId || markers[i].CardIdB == cardId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsEarlierCardOfPendingPair(BoardSide side, PendingCancelMarker marker, string cardId)
        {
            int indexA = IndexOfCardId(side, marker.CardIdA);
            int indexB = IndexOfCardId(side, marker.CardIdB);
            if (indexA < 0 || indexB < 0)
            {
                return false;
            }

            int earlier = Math.Min(indexA, indexB);
            return side.Cards[earlier].Id == cardId;
        }

        private static int IndexOfCardId(BoardSide side, string cardId)
        {
            for (int i = 0; i < side.Cards.Count; i++)
            {
                if (side.Cards[i].Id == cardId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static BoardCard? FindCardById(BoardSide side, string cardId)
        {
            for (int i = 0; i < side.Cards.Count; i++)
            {
                if (side.Cards[i].Id == cardId)
                {
                    return side.Cards[i];
                }
            }

            return null;
        }

        private void ClearOrphanedBoardDragWidgets()
        {
            if (_dragRoot == null || _playingWinSequence)
            {
                return;
            }

            var toRemove = new List<GameObject>();
            for (int i = 0; i < _dragRoot.childCount; i++)
            {
                CardWidget widget = _dragRoot.GetChild(i).GetComponent<CardWidget>();
                if (widget == null || widget.SideName == "Hand" || widget.IsActivelyDragging)
                {
                    continue;
                }

                toRemove.Add(widget.gameObject);
            }

            foreach (GameObject go in toRemove)
            {
                Destroy(go);
            }
        }

        private static void CreateOperatorSeparator(Transform parent, float tileHeight, BoardCard? previous,
            BoardCard next, bool useMultiply)
        {
            bool times = useMultiply
                && previous.HasValue
                && DivisionRules.IsCoefficientTimesXPair(previous.Value, next);
            if (times)
            {
                CreateTimesSeparator(parent, tileHeight);
            }
            else
            {
                CreatePlusSeparator(parent, tileHeight);
            }
        }

        private static void CreatePlusSeparator(Transform parent, float tileHeight)
        {
            CreateSymbolSeparator(parent, tileHeight, "PlusSeparator", "+", PlusSeparatorWidth, 34);
        }

        private static void CreateTimesSeparator(Transform parent, float tileHeight)
        {
            CreateSymbolSeparator(parent, tileHeight, "TimesSeparator", "·", TimesSeparatorWidth, 36);
        }

        private static void CreateSymbolSeparator(Transform parent, float tileHeight, string name, string symbol,
            float width, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var layoutElement = go.GetComponent<LayoutElement>();
            layoutElement.minWidth = width;
            layoutElement.preferredWidth = width;
            layoutElement.minHeight = tileHeight;
            layoutElement.preferredHeight = tileHeight;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, tileHeight);

            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = symbol;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.95f, 0.95f, 0.88f);
            text.raycastTarget = false;
        }
    }
}
