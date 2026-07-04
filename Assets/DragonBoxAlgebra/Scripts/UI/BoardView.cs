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

            _widgets.Clear();
            RebuildSide(_leftPanel, _controller.Board.Left, "Left");
            RebuildSide(_rightPanel, _controller.Board.Right, "Right");

            if (_controller.HasPendingBalance)
            {
                BalancePending pending = _controller.PendingBalance;
                RectTransform holePanel = pending.HoleSide == "Left" ? _leftPanel : _rightPanel;
                BalanceHoleWidget hole = BalanceHoleWidget.Create(holePanel, _controller, pending.HoleSide, pending.Card);
                int holeSlot = Mathf.Clamp(pending.HoleInsertIndex, 0, holePanel.childCount - 1);
                hole.transform.SetSiblingIndex(holeSlot);
            }

            BuildCancelMarkers(_leftPanel, "Left");
            BuildCancelMarkers(_rightPanel, "Right");

            BoardSideLayout.FitPanelToShowAllTiles(_leftPanel);
            BoardSideLayout.FitPanelToShowAllTiles(_rightPanel);
        }

        private void BuildCancelMarkers(RectTransform panel, string sideName)
        {
            IReadOnlyList<PendingCancelMarker> markers = _controller.PendingCancels;
            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i].SideName == sideName)
                {
                    AsteriskCancelWidget.Create(panel, _controller, i);
                }
            }
        }

        private void RebuildSide(RectTransform panel, BoardSide side, string sideName)
        {
            for (int i = panel.childCount - 1; i >= 0; i--)
            {
                Transform child = panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() == null)
                {
                    Destroy(child.gameObject);
                }
            }

            var layout = panel.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 16f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.padding = new RectOffset(24, 24, 24, 24);
            }

            for (int i = 0; i < side.Cards.Count; i++)
            {
                BoardCard card = side.Cards[i];
                if (_controller.IsCardPendingCancelOnSide(card.Id, sideName))
                {
                    continue;
                }

                CardWidget widget = CardWidget.Create(panel, card, i, sideName, _controller, _canvas, _dragRoot);
                widget.gameObject.AddComponent<CardDropZone>();
                _widgets.Add(widget);
            }
        }
    }
}
