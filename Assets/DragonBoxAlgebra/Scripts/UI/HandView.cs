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
            _controller.HandChanged += OnHandChanged;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.HandChanged -= OnHandChanged;
            }
        }

        private void OnHandChanged()
        {
            StartCoroutine(RefreshNextFrame());
        }

        private IEnumerator RefreshNextFrame()
        {
            yield return null;
            Refresh();
        }

        private void DestroyAllHandWidgets()
        {
            DestroyHandWidgetsUnder(_panel);
            if (_dragRoot != null)
            {
                DestroyHandWidgetsUnder(_dragRoot);
            }
        }

        private static void DestroyHandWidgetsUnder(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                CardWidget widget = child.GetComponent<CardWidget>();
                if (widget != null && widget.SideName == "Hand")
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void Refresh()
        {
            DestroyAllHandWidgets();

            for (int i = _panel.childCount - 1; i >= 0; i--)
            {
                Transform child = _panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() == null && child.GetComponent<CardWidget>() == null)
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
