using System.Collections.Generic;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class HandView : MonoBehaviour
    {
        private RectTransform _panel;
        private Canvas _canvas;
        private RectTransform _dragRoot;
        private AlgebraGameController _controller;

        public void Initialize(AlgebraGameController controller, RectTransform panel, Canvas canvas,
            RectTransform dragRoot)
        {
            _controller = controller;
            _panel = panel;
            _canvas = canvas;
            _dragRoot = dragRoot;
            _controller.HandChanged += RefreshHandInPlace;
            _controller.WinSequenceStarted += OnWinSequenceStarted;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.HandChanged -= RefreshHandInPlace;
                _controller.WinSequenceStarted -= OnWinSequenceStarted;
            }
        }

        private void OnWinSequenceStarted(int stars, int moves)
        {
            ClearHandOnly();
        }

        private void RefreshHandInPlace()
        {
            if (_controller.IsLevelComplete)
            {
                ClearHandOnly();
                return;
            }

            // Always refresh. Skipping while a hand tile is on DragRoot left stale/duplicate
            // tiles and broke the opposite-snap drag flow.
            bool preserveDragRoot = HasHandWidgetOnDragRoot() && _controller.KeepHandSlotVisibleDuringDrag();
            Refresh(preserveDragRoot);
        }

        private bool HasHandWidgetOnDragRoot()
        {
            for (int i = 0; i < _dragRoot.childCount; i++)
            {
                CardWidget widget = _dragRoot.GetChild(i).GetComponent<CardWidget>();
                if (widget != null && widget.SideName == "Hand")
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsHandIndexOnDragRoot(int handIndex)
        {
            for (int i = 0; i < _dragRoot.childCount; i++)
            {
                CardWidget widget = _dragRoot.GetChild(i).GetComponent<CardWidget>();
                if (widget != null && widget.SideName == "Hand" && widget.Index == handIndex)
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearHandOnly()
        {
            ClearHandWidgets(_dragRoot);
            ClearHandPanel(_panel);
        }

        private void Refresh(bool preserveDragRoot = false)
        {
            if (_controller.IsLevelComplete)
            {
                ClearHandOnly();
                return;
            }

            // Always rebuild the hand panel from the controller hand list.
            // Incremental sync was leaving duplicate tiles (same image 2–3 times).
            if (!preserveDragRoot)
            {
                ClearHandWidgets(_dragRoot);
            }

            ClearHandPanel(_panel);
            EnsureHandLayout();

            var createdIndexes = new HashSet<int>();
            for (int i = 0; i < _controller.Hand.Count; i++)
            {
                if (!_controller.ShouldDisplayHandCard(i))
                {
                    continue;
                }

                if (!createdIndexes.Add(i))
                {
                    continue;
                }

                if (preserveDragRoot && IsHandIndexOnDragRoot(i))
                {
                    continue;
                }

                BoardCard card = _controller.GetHandDisplayCard(i);
                CardWidget widget = CardWidget.Create(_panel, card, i, "Hand", _controller, _canvas, _dragRoot);
                widget.SetHandCard(card);
            }
        }

        private void EnsureHandLayout()
        {
            var layout = _panel.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = _panel.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 12f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
            }
        }

        private static void ClearHandWidgets(RectTransform panel)
        {
            var toRemove = new List<GameObject>();
            for (int i = 0; i < panel.childCount; i++)
            {
                Transform child = panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() != null)
                {
                    continue;
                }

                CardWidget widget = child.GetComponent<CardWidget>();
                if (widget != null && widget.SideName == "Hand")
                {
                    toRemove.Add(child.gameObject);
                }
            }

            foreach (GameObject go in toRemove)
            {
                Object.DestroyImmediate(go);
            }
        }

        private static void ClearHandPanel(RectTransform panel)
        {
            var toRemove = new List<GameObject>();
            for (int i = 0; i < panel.childCount; i++)
            {
                Transform child = panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() != null)
                {
                    continue;
                }

                toRemove.Add(child.gameObject);
            }

            foreach (GameObject go in toRemove)
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
