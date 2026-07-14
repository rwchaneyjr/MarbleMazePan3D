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
        private const float BoxCenterSlideDuration = 0.85f;
        private const float WinPostDelay = 0.35f;
        private static readonly Vector2 CenteredPanelAnchorMin = new(0.25f, 0f);
        private static readonly Vector2 CenteredPanelAnchorMax = new(0.75f, 1f);
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
        private Coroutine _boxSlideCoroutine;
        private string _activeBoxSlideSide;

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
            _activeBoxSlideSide = null;
            if (_boxSlideCoroutine != null)
            {
                StopCoroutine(_boxSlideCoroutine);
                _boxSlideCoroutine = null;
            }

            ResetPanelAnchors();
        }

        private void ResetPanelAnchors()
        {
            _leftPanel.anchorMin = _leftAnchorMinDefault;
            _leftPanel.anchorMax = _leftAnchorMaxDefault;
            _rightPanel.anchorMin = _rightAnchorMinDefault;
            _rightPanel.anchorMax = _rightAnchorMaxDefault;
        }

        private void OnWinSequenceStarted(int stars, int moves)
        {
            if (_winSequenceCoroutine != null)
            {
                StopCoroutine(_winSequenceCoroutine);
            }

            _winSequenceCoroutine = StartCoroutine(PlayWinSequence(stars, moves));
        }

        private IEnumerator PlayWinSequence(int stars, int moves)
        {
            _playingWinSequence = true;

            while (!_controller.CanPresentWin())
            {
                yield return null;
            }

            yield return new WaitForSeconds(WinPreDelay);

            if (_controller.TryGetBoxSlideToCenterSide(out string boxSide))
            {
                yield return SlidePanelToAnchors(
                    boxSide == "Left" ? _leftPanel : _rightPanel,
                    GetCenteredAnchors(boxSide == "Left" ? _leftPanel : _rightPanel),
                    WinSlideDuration);
            }
            else
            {
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
            }

            yield return new WaitForSeconds(WinPostDelay);

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
            UpdateBoxCenterSlide();
        }

        private void UpdateBoxCenterSlide()
        {
            if (_playingWinSequence)
            {
                return;
            }

            if (_controller.TryGetBoxSlideToCenterSide(out string boxSide))
            {
                if (_activeBoxSlideSide == boxSide && _boxSlideCoroutine != null)
                {
                    return;
                }

                if (_boxSlideCoroutine != null)
                {
                    StopCoroutine(_boxSlideCoroutine);
                }

                _activeBoxSlideSide = boxSide;
                RectTransform panel = boxSide == "Left" ? _leftPanel : _rightPanel;
                (Vector2 targetMin, Vector2 targetMax) = GetCenteredAnchors(panel);
                _boxSlideCoroutine = StartCoroutine(SlidePanelToAnchors(panel, (targetMin, targetMax), BoxCenterSlideDuration,
                    () => _boxSlideCoroutine = null));
                return;
            }

            if (_boxSlideCoroutine != null)
            {
                StopCoroutine(_boxSlideCoroutine);
                _boxSlideCoroutine = null;
            }

            if (_activeBoxSlideSide != null)
            {
                ResetPanelAnchors();
                _activeBoxSlideSide = null;
            }
        }

        private static (Vector2 min, Vector2 max) GetCenteredAnchors(RectTransform panel)
        {
            Vector2 targetMin = CenteredPanelAnchorMin;
            Vector2 targetMax = CenteredPanelAnchorMax;
            targetMin.y = panel.anchorMin.y;
            targetMax.y = panel.anchorMax.y;
            return (targetMin, targetMax);
        }

        private IEnumerator SlidePanelToAnchors(RectTransform panel, (Vector2 min, Vector2 max) targets, float duration,
            System.Action onComplete = null)
        {
            Vector2 startMin = panel.anchorMin;
            Vector2 startMax = panel.anchorMax;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                panel.anchorMin = Vector2.Lerp(startMin, targets.min, t);
                panel.anchorMax = Vector2.Lerp(startMax, targets.max, t);
                yield return null;
            }

            panel.anchorMin = targets.min;
            panel.anchorMax = targets.max;
            onComplete?.Invoke();
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
                if (widget != null && widget.SideName != "Hand")
                {
                    toRemove.Add(widget.gameObject);
                }
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
