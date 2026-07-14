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
        private const float TogetherTileSeparation = 58f;
        private const float TogetherTileMeetGap = 6f;
        private static readonly Vector2 TogetherLeftAnchorMin = new(0.25f, 0f);
        private static readonly Vector2 TogetherLeftAnchorMax = new(0.5f, 1f);
        private static readonly Vector2 TogetherRightAnchorMin = new(0.5f, 0f);
        private static readonly Vector2 TogetherRightAnchorMax = new(0.75f, 1f);
        private const float DefaultTileWidth = 110f;
        private const float DefaultTileHeight = 120f;
        private const float DefaultTileSpacing = 16f;
        private const float MinTileWidth = 52f;
        private const float MinTileSpacing = 4f;
        private const int CompactPadding = 12;
        private const int DefaultPadding = 24;

        private RectTransform _leftPanel;
        private RectTransform _rightPanel;
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
        private readonly HashSet<RectTransform> _winSlideRects = new();

        public void Initialize(AlgebraGameController controller, RectTransform left, RectTransform right,
            Canvas canvas, RectTransform dragRoot)
        {
            _controller = controller;
            _leftPanel = left;
            _rightPanel = right;
            _canvas = canvas;
            _dragRoot = dragRoot;

            _leftAnchorMinDefault = left.anchorMin;
            _leftAnchorMaxDefault = left.anchorMax;
            _rightAnchorMinDefault = right.anchorMin;
            _rightAnchorMaxDefault = right.anchorMax;

            left.gameObject.AddComponent<BoardDropZone>().SideName = "Left";
            right.gameObject.AddComponent<BoardDropZone>().SideName = "Right";

            _controller.BoardChanged += Refresh;
            _controller.CombineOccurred += OnCombine;
            _controller.LevelLoaded += OnLevelLoaded;
            _controller.WinSequenceStarted += OnWinSequenceStarted;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.BoardChanged -= Refresh;
                _controller.CombineOccurred -= OnCombine;
                _controller.LevelLoaded -= OnLevelLoaded;
                _controller.WinSequenceStarted -= OnWinSequenceStarted;
            }
        }

        private void OnLevelLoaded(int current, int total)
        {
            if (_winSequenceCoroutine != null)
            {
                StopCoroutine(_winSequenceCoroutine);
                _winSequenceCoroutine = null;
            }

            _playingWinSequence = false;
            _winSlideRects.Clear();
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

            TryPrepareWinTileSlides(out List<WinTileSlide> tileSlides);

            yield return new WaitForSeconds(WinPreDelay);
            yield return AnimateSidesTogether(WinSlideDuration, tileSlides);
            yield return new WaitForSeconds(WinPostDelay);

            _controller.ClearOppositeSideAfterSidesTogether();
            _playingWinSequence = false;
            _winSlideRects.Clear();
            _winSequenceCoroutine = null;
            _controller.CompleteWinPresentation(stars, moves);
        }

        private IEnumerator AnimateSidesTogether(float duration, List<WinTileSlide> tileSlides)
        {
            Vector2 leftTargetMin = new Vector2(TogetherLeftAnchorMin.x, _leftAnchorMinDefault.y);
            Vector2 leftTargetMax = new Vector2(TogetherLeftAnchorMax.x, _leftAnchorMaxDefault.y);
            Vector2 rightTargetMin = new Vector2(TogetherRightAnchorMin.x, _rightAnchorMinDefault.y);
            Vector2 rightTargetMax = new Vector2(TogetherRightAnchorMax.x, _rightAnchorMaxDefault.y);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                float meet = Mathf.Lerp(TogetherTileSeparation, TogetherTileMeetGap, t);

                _leftPanel.anchorMin = Vector2.Lerp(_leftAnchorMinDefault, leftTargetMin, t);
                _leftPanel.anchorMax = Vector2.Lerp(_leftAnchorMaxDefault, leftTargetMax, t);
                _rightPanel.anchorMin = Vector2.Lerp(_rightAnchorMinDefault, rightTargetMin, t);
                _rightPanel.anchorMax = Vector2.Lerp(_rightAnchorMaxDefault, rightTargetMax, t);

                if (tileSlides != null)
                {
                    foreach (WinTileSlide slide in tileSlides)
                    {
                        if (slide.Rect == null)
                        {
                            continue;
                        }

                        Vector2 end = slide.IsBox
                            ? new Vector2(-meet, 0f)
                            : new Vector2(meet + slide.OppositeOffset, 0f);
                        slide.Rect.localPosition = Vector2.Lerp(slide.StartLocal, end, t);
                    }
                }

                Canvas.ForceUpdateCanvases();
                yield return null;
            }

            _leftPanel.anchorMin = leftTargetMin;
            _leftPanel.anchorMax = leftTargetMax;
            _rightPanel.anchorMin = rightTargetMin;
            _rightPanel.anchorMax = rightTargetMax;

            if (tileSlides != null)
            {
                float meet = TogetherTileMeetGap;
                foreach (WinTileSlide slide in tileSlides)
                {
                    if (slide.Rect == null)
                    {
                        continue;
                    }

                    slide.Rect.localPosition = slide.IsBox
                        ? new Vector2(-meet, 0f)
                        : new Vector2(meet + slide.OppositeOffset, 0f);
                }
            }
        }

        private readonly struct WinTileSlide
        {
            public readonly RectTransform Rect;
            public readonly Vector2 StartLocal;
            public readonly bool IsBox;
            public readonly float OppositeOffset;

            public WinTileSlide(RectTransform rect, Vector2 startLocal, bool isBox, float oppositeOffset)
            {
                Rect = rect;
                StartLocal = startLocal;
                IsBox = isBox;
                OppositeOffset = oppositeOffset;
            }
        }

        /// <summary>
        /// Float the red box and any opposite-side tiles on the drag layer so all stay visible
        /// while both sides slide together to the center.
        /// </summary>
        private bool TryPrepareWinTileSlides(out List<WinTileSlide> slides)
        {
            slides = new List<WinTileSlide>();
            _winSlideRects.Clear();

            if (!_controller.TryGetBoxSideNames(out string boxSide, out string oppositeSide))
            {
                return false;
            }

            int oppositeIndex = 0;
            foreach (CardWidget widget in _widgets)
            {
                if (widget.SideName != boxSide && widget.SideName != oppositeSide)
                {
                    continue;
                }

                var rect = widget.transform as RectTransform;
                if (rect == null)
                {
                    continue;
                }

                rect.SetParent(_dragRoot, true);
                rect.SetAsLastSibling();
                _winSlideRects.Add(rect);

                bool isBox = widget.Card.Kind == CardKind.Box;
                float oppositeOffset = isBox ? 0f : oppositeIndex * 12f;
                if (!isBox)
                {
                    oppositeIndex++;
                }

                slides.Add(new WinTileSlide(rect, rect.localPosition, isBox, oppositeOffset));
            }

            return slides.Count > 0;
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
            TileLayout leftLayout = ComputeTileLayout(_leftPanel, CountSlotsForSide("Left", _controller.Board.Left));
            TileLayout rightLayout = ComputeTileLayout(_rightPanel, CountSlotsForSide("Right", _controller.Board.Right));
            RebuildSide(_leftPanel, _controller.Board.Left, "Left", leftLayout);
            RebuildSide(_rightPanel, _controller.Board.Right, "Right", rightLayout);

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

            BuildCancelMarkers(_leftPanel, "Left", leftLayout);
            BuildCancelMarkers(_rightPanel, "Right", rightLayout);
        }

        private int CountSlotsForSide(string sideName, BoardSide side)
        {
            int count = 0;
            for (int i = 0; i < side.Cards.Count; i++)
            {
                if (!_controller.IsCardPendingCancelOnSide(side.Cards[i].Id, sideName))
                {
                    count++;
                }
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

        private TileLayout ComputeTileLayout(RectTransform panel, int slotCount)
        {
            int padding = slotCount >= 5 ? CompactPadding : DefaultPadding;
            float available = Mathf.Max(0f, panel.rect.width - padding * 2f);

            if (slotCount <= 0 || available <= 0f)
            {
                return new TileLayout(DefaultTileWidth, DefaultTileHeight, DefaultTileSpacing, DefaultPadding);
            }

            float spacing = slotCount >= 5 ? 8f : DefaultTileSpacing;
            float needed = slotCount * DefaultTileWidth + (slotCount - 1) * spacing;

            if (needed <= available)
            {
                return new TileLayout(DefaultTileWidth, DefaultTileHeight, DefaultTileSpacing, padding);
            }

            float width = (available - (slotCount - 1) * MinTileSpacing) / slotCount;
            width = Mathf.Clamp(width, MinTileWidth, DefaultTileWidth);
            spacing = slotCount > 1
                ? Mathf.Max(MinTileSpacing, (available - slotCount * width) / (slotCount - 1))
                : 0f;

            float total = slotCount * width + (slotCount - 1) * spacing;
            if (total > available)
            {
                width = (available - (slotCount - 1) * MinTileSpacing) / slotCount;
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

        private void BuildCancelMarkers(RectTransform panel, string sideName, TileLayout layout)
        {
            BoardSide side = sideName == "Left" ? _controller.Board.Left : _controller.Board.Right;
            IReadOnlyList<PendingCancelMarker> markers = _controller.PendingCancels;
            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i].SideName != sideName)
                {
                    continue;
                }

                AsteriskCancelWidget.Create(panel, _controller, i, layout.Width, layout.Height);
            }
        }

        private void ClearOrphanedBoardDragWidgets()
        {
            if (_dragRoot == null)
            {
                return;
            }

            var toRemove = new List<GameObject>();
            for (int i = 0; i < _dragRoot.childCount; i++)
            {
                CardWidget widget = _dragRoot.GetChild(i).GetComponent<CardWidget>();
                if (widget == null || widget.SideName == "Hand")
                {
                    continue;
                }

                var rect = widget.transform as RectTransform;
                if (rect != null && _winSlideRects.Contains(rect))
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

            for (int i = 0; i < side.Cards.Count; i++)
            {
                BoardCard card = side.Cards[i];
                if (_controller.IsCardPendingCancelOnSide(card.Id, sideName))
                {
                    continue;
                }

                CardWidget widget = CardWidget.Create(panel, card, i, sideName, _controller, _canvas, _dragRoot,
                    layout.Width, layout.Height);
                widget.gameObject.AddComponent<CardDropZone>();
                _widgets.Add(widget);
            }
        }
    }
}
