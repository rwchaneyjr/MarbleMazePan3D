using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public static class BoardSideLayout
    {
        private const float DefaultCardWidth = 110f;
        private const float DefaultCardHeight = 120f;
        private const float MinCardWidth = 56f;
        private const float DefaultSpacing = 16f;
        private const float MinSpacing = 4f;
        private const float HorizontalPadding = 48f;

        public static void FitPanelToShowAllTiles(RectTransform panel)
        {
            int tileCount = CountLayoutTiles(panel);
            if (tileCount < 2)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            float panelWidth = panel.rect.width;
            if (panelWidth <= 0f)
            {
                return;
            }

            float available = Mathf.Max(0f, panelWidth - HorizontalPadding);
            float needed = tileCount * DefaultCardWidth + (tileCount - 1) * DefaultSpacing;
            float cardWidth = DefaultCardWidth;
            float spacing = DefaultSpacing;

            if (needed > available)
            {
                cardWidth = Mathf.Max(
                    MinCardWidth,
                    (available - (tileCount - 1) * MinSpacing) / tileCount);
                spacing = tileCount > 1
                    ? Mathf.Max(MinSpacing, (available - tileCount * cardWidth) / (tileCount - 1))
                    : 0f;
            }

            var layout = panel.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
            {
                layout.spacing = spacing;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
            }

            float scale = cardWidth / DefaultCardWidth;
            float cardHeight = DefaultCardHeight * scale;

            for (int i = 0; i < panel.childCount; i++)
            {
                Transform child = panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() != null)
                {
                    continue;
                }

                LayoutElement layoutElement = child.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = child.gameObject.AddComponent<LayoutElement>();
                }

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
