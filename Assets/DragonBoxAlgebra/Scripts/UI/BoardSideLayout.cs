using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public static class BoardSideLayout
    {
        private const float DefaultCardWidth = 110f;
        private const float DefaultCardHeight = 120f;
        private const float MinCardWidth = 52f;
        private const float DefaultSpacing = 12f;
        private const float MinSpacing = 4f;
        private const float HorizontalPadding = 32f;

        public static void FitPanelToShowAllTiles(RectTransform panel)
        {
            int tileCount = CountLayoutTiles(panel);
            var layout = panel.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                return;
            }

            layout.enabled = true;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(16, 16, 16, 16);

            float cardWidth = DefaultCardWidth;
            float cardHeight = DefaultCardHeight;
            float spacing = DefaultSpacing;

            if (tileCount >= 2)
            {
                Canvas.ForceUpdateCanvases();
                float panelWidth = panel.rect.width;
                if (panelWidth > 0f)
                {
                    float available = Mathf.Max(0f, panelWidth - HorizontalPadding);
                    float needed = tileCount * DefaultCardWidth + (tileCount - 1) * DefaultSpacing;
                    if (needed > available)
                    {
                        cardWidth = Mathf.Max(
                            MinCardWidth,
                            (available - (tileCount - 1) * MinSpacing) / tileCount);
                        spacing = tileCount > 1
                            ? Mathf.Max(MinSpacing, (available - tileCount * cardWidth) / (tileCount - 1))
                            : 0f;
                        float scale = cardWidth / DefaultCardWidth;
                        cardHeight = DefaultCardHeight * scale;
                    }
                }
            }

            layout.spacing = spacing;

            for (int i = 0; i < panel.childCount; i++)
            {
                Transform child = panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() != null)
                {
                    continue;
                }

                var rect = child as RectTransform;
                if (rect == null)
                {
                    continue;
                }

                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;

                LayoutElement layoutElement = rect.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = rect.gameObject.AddComponent<LayoutElement>();
                }

                layoutElement.ignoreLayout = false;
                layoutElement.minWidth = cardWidth;
                layoutElement.preferredWidth = cardWidth;
                layoutElement.minHeight = cardHeight;
                layoutElement.preferredHeight = cardHeight;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        }

        private static int CountLayoutTiles(RectTransform panel)
        {
            int count = 0;
            for (int i = 0; i < panel.childCount; i++)
            {
                Transform child = panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() != null)
                {
                    continue;
                }

                count++;
            }

            return count;
        }
    }
}
