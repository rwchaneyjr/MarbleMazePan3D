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
        private const float DefaultCardWidth = 100f;
        private const float DefaultCardHeight = 110f;
        private const float MinCardWidth = 56f;
        private const float TileGap = 10f;
        private const float HorizontalPadding = 12f;

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
            int handCount = _controller.Hand.Count;
            bool useManualLayout = handCount > 5;

            if (layout == null)
            {
                layout = _panel.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = TileGap;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
            }

            layout.enabled = !useManualLayout;
            layout.spacing = TileGap;

            for (int i = 0; i < handCount; i++)
            {
                BoardCard card = _controller.Hand[i];
                CardWidget.Create(_panel, card, i, "Hand", _controller, _canvas, _dragRoot);
            }

            if (useManualLayout)
            {
                FitHandToPanel();
            }
        }

        private void FitHandToPanel()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);

            float panelWidth = _panel.rect.width;
            if (panelWidth <= 0f)
            {
                return;
            }

            int count = _controller.Hand.Count;
            if (count == 0)
            {
                return;
            }

            float cardWidth = DefaultCardWidth;
            float cardHeight = DefaultCardHeight;
            float totalWidth = count * cardWidth + (count - 1) * TileGap;
            float availableWidth = panelWidth - HorizontalPadding * 2f;
            if (totalWidth > availableWidth)
            {
                float scale = availableWidth / totalWidth;
                cardWidth = Mathf.Max(MinCardWidth, cardWidth * scale);
                cardHeight = cardHeight * (cardWidth / DefaultCardWidth);
            }

            float pitch = cardWidth + TileGap;
            float rowWidth = count * pitch - TileGap;
            float startX = -rowWidth * 0.5f + cardWidth * 0.5f;

            int widgetIndex = 0;
            for (int i = 0; i < _panel.childCount; i++)
            {
                Transform child = _panel.GetChild(i);
                CardWidget widget = child.GetComponent<CardWidget>();
                if (widget == null || widget.SideName != "Hand")
                {
                    continue;
                }

                var rect = child as RectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(cardWidth, cardHeight);
                rect.anchoredPosition = new Vector2(startX + widgetIndex * pitch, 0f);
                rect.localScale = Vector3.one;

                LayoutElement layoutElement = rect.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = rect.gameObject.AddComponent<LayoutElement>();
                }

                layoutElement.ignoreLayout = true;
                layoutElement.minWidth = cardWidth;
                layoutElement.preferredWidth = cardWidth;
                layoutElement.minHeight = cardHeight;
                layoutElement.preferredHeight = cardHeight;
                widgetIndex++;
            }
        }
    }
}
