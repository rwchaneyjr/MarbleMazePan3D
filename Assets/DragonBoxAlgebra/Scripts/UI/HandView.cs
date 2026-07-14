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
            if (HasHandWidgetOnDragRoot() && !_controller.KeepHandSlotVisibleDuringDrag())
            {
                return;
            }

            Refresh();
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

        private void Refresh()
        {
            ClearHandWidgets(_dragRoot);
            ClearHandPanel(_panel);

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
                if (!_controller.ShouldDisplayHandCard(i))
                {
                    continue;
                }

                BoardCard card = _controller.GetHandDisplayCard(i);
                CardWidget.Create(_panel, card, i, "Hand", _controller, _canvas, _dragRoot);
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
