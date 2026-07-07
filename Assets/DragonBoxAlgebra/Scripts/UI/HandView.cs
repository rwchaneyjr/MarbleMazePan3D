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
            Refresh();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.HandChanged -= RefreshHandInPlace;
            }
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

            if (widgets.Count == _controller.Hand.Count)
            {
                for (int i = 0; i < widgets.Count; i++)
                {
                    widgets[i].Bind(_controller.Hand[i], i, "Hand", _controller, _canvas, _dragRoot);
                }

                return;
            }

            Refresh();
        }

        private void Refresh()
        {
            for (int i = _dragRoot.childCount - 1; i >= 0; i--)
            {
                CardWidget stray = _dragRoot.GetChild(i).GetComponent<CardWidget>();
                if (stray != null && stray.SideName == "Hand")
                {
                    Destroy(stray.gameObject);
                }
            }

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
                BoardCard card = _controller.Hand[i];
                CardWidget.Create(_panel, card, i, "Hand", _controller, _canvas, _dragRoot);
            }
        }
    }
}
