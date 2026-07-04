using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public static class BoardSideLayout
    {
        private const float DefaultCardWidth = 110f;
        private const float DefaultCardHeight = 120f;
        private const float MinCardWidth = 72f;
        private const float HorizontalPadding = 40f;
        private const float MinStaggerX = 20f;
        private const float MaxStaggerX = 34f;
        private const float StaggerY = 12f;

        public static void FitPanelToShowAllTiles(RectTransform panel)
        {
            int tileCount = CountLayoutTiles(panel);
            var layout = panel.GetComponent<HorizontalLayoutGroup>();

            if (tileCount < 2)
            {
                if (layout != null)
                {
                    layout.enabled = true;
                }

                ResetTileTransforms(panel, DefaultCardWidth, DefaultCardHeight);
                if (layout != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
                }

                return;
            }

            if (layout != null)
            {
                layout.enabled = false;
            }

            Canvas.ForceUpdateCanvases();
            float panelWidth = panel.rect.width;
            if (panelWidth <= 0f)
            {
                return;
            }

            float cardWidth = DefaultCardWidth;
            float cardHeight = DefaultCardHeight;
            if (tileCount > 5)
            {
                float scale = Mathf.Clamp(5.5f / tileCount, 0.62f, 1f);
                cardWidth *= scale;
                cardHeight *= scale;
            }

            float available = Mathf.Max(cardWidth, panelWidth - HorizontalPadding);
            float staggerX = tileCount > 1
                ? (available - cardWidth) / (tileCount - 1)
                : 0f;
            staggerX = Mathf.Clamp(staggerX, MinStaggerX, MaxStaggerX);

            float totalWidth = cardWidth + (tileCount - 1) * staggerX;
            float startX = -totalWidth * 0.5f + cardWidth * 0.5f;

            int slot = 0;
            for (int i = 0; i < panel.childCount; i++)
            {
                Transform child = panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() != null)
                {
                    continue;
                }

                ApplyStaggeredTile(child as RectTransform, slot, startX, staggerX, cardWidth, cardHeight);
                slot++;
            }
        }

        private static void ApplyStaggeredTile(RectTransform rect, int slot, float startX, float staggerX,
            float cardWidth, float cardHeight)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(cardWidth, cardHeight);

            float yWave = (slot % 2 == 0 ? 1f : -1f) * Mathf.Min(StaggerY, slot * 3f);
            rect.anchoredPosition = new Vector2(startX + slot * staggerX, yWave);

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

            rect.SetSiblingIndex(slot);
        }

        private static void ResetTileTransforms(RectTransform panel, float cardWidth, float cardHeight)
        {
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

                LayoutElement layoutElement = rect.GetComponent<LayoutElement>();
                if (layoutElement != null)
                {
                    layoutElement.ignoreLayout = false;
                    layoutElement.minWidth = cardWidth;
                    layoutElement.preferredWidth = cardWidth;
                    layoutElement.minHeight = cardHeight;
                    layoutElement.preferredHeight = cardHeight;
                }
            }
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
