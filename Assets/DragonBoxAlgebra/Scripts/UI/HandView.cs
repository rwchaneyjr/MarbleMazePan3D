using System.Collections;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using System.Collections.Generic;
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
            _controller.BoardChanged += OnBoardChanged;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.HandChanged -= RefreshHandInPlace;
                _controller.BoardChanged -= OnBoardChanged;
            }
        }

        private void OnBoardChanged()
        {
            if (_controller.HasPendingBalance && _controller.Hand.Count > 0)
            {
                StartCoroutine(EnsureHandVisibleNextFrame());
            }
        }

        private IEnumerator EnsureHandVisibleNextFrame()
        {
            yield return null;

            if (_controller.Hand.Count == 0)
            {
                yield break;
            }

            if (CountHandWidgets() == VisibleHandCount())
            {
                RefreshHandInPlace();
                yield break;
            }

            Refresh();
        }

        private int CountHandWidgets()
        {
            int count = 0;
            for (int i = 0; i < _panel.childCount; i++)
            {
                CardWidget widget = _panel.GetChild(i).GetComponent<CardWidget>();
                if (widget != null && widget.SideName == "Hand")
                {
                    count++;
                }
            }

            return count;
        }

        private int VisibleHandCount()
        {
            if (_controller.HasPendingBalance && _controller.Hand.Count > 0)
            {
                return 1;
            }

            return _controller.Hand.Count;
        }

        private bool ShouldShowHandIndex(int index)
        {
            if (!_controller.HasPendingBalance)
            {
                return true;
            }

            return index == _controller.PendingBalance.HandIndex;
        }

        private void RefreshHandInPlace()
        {
            var widgets = new List<CardWidget>();
            for (int i = 0; i < _panel.childCount; i++)
            {
                CardWidget widget = _panel.GetChild(i).GetComponent<CardWidget>();
                if (widget != null && widget.SideName == "Hand")
                {
                    widgets.Add(widget);
                }
            }

            var visibleIndices = new List<int>();
            for (int i = 0; i < _controller.Hand.Count; i++)
            {
                if (ShouldShowHandIndex(i))
                {
                    visibleIndices.Add(i);
                }
            }

            if (widgets.Count == visibleIndices.Count)
            {
                for (int i = 0; i < widgets.Count; i++)
                {
                    int handIndex = visibleIndices[i];
                    widgets[i].Bind(_controller.Hand[handIndex], handIndex, "Hand", _controller, _canvas, _dragRoot);
                }

                return;
            }

            Refresh();
        }

        private void Refresh()
        {
            for (int i = _panel.childCount - 1; i >= 0; i--)
            {
                Transform child = _panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() == null)
                {
                    Destroy(child.gameObject);
                }
            }

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

            for (int i = 0; i < _controller.Hand.Count; i++)
            {
                if (!ShouldShowHandIndex(i))
                {
                    continue;
                }

                BoardCard card = _controller.Hand[i];
                CardWidget.Create(_panel, card, i, "Hand", _controller, _canvas, _dragRoot);
            }
        }
    }
}
