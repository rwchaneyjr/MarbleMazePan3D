using System.Collections.Generic;
using DragonBoxAlgebra.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public static class BoardSideLayout
    {
        private const float DefaultCardWidth = 110f;
        private const float DefaultCardHeight = 120f;
        private const float MinCardWidth = 48f;
        private const float HorizontalPadding = 28f;
        private const float VerticalPadding = 28f;
        private const float BoxGapFactor = 0.18f;
        private const float ColOverlap = 0.52f;
        private const float RowOverlap = 0.52f;

        public static void FitPanelToShowAllTiles(RectTransform panel)
        {
            List<LayoutItem> items = CollectLayoutItems(panel);
            var layout = panel.GetComponent<HorizontalLayoutGroup>();

            if (items.Count <= 1)
            {
                if (layout != null)
                {
                    layout.enabled = true;
                    layout.childAlignment = TextAnchor.MiddleCenter;
                }

                if (items.Count == 1)
                {
                    ApplyTileSize(items[0].Rect, DefaultCardWidth, DefaultCardHeight, ignoreLayout: false);
                }

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
            float panelHeight = panel.rect.height;
            if (panelWidth <= 0f || panelHeight <= 0f)
            {
                return;
            }

            RectTransform box = null;
            var stack = new List<RectTransform>();
            foreach (LayoutItem item in items)
            {
                if (item.IsBox)
                {
                    box = item.Rect;
                }
                else
                {
                    stack.Add(item.Rect);
                }
            }

            float cardWidth = DefaultCardWidth;
            float cardHeight = DefaultCardHeight;
            float scale = ComputeFitScale(panelWidth, panelHeight, box != null, stack.Count, cardWidth, cardHeight);
            cardWidth *= scale;
            cardHeight *= scale;

            if (box != null && stack.Count > 0)
            {
                LayoutBoxWithBrickStack(box, stack, cardWidth, cardHeight);
                return;
            }

            float colStep = cardWidth * ColOverlap;
            float brickHalf = colStep * 0.5f;
            float stackWidth = MaxRowWidth(stack.Count, cardWidth, colStep, brickHalf);
            LayoutBrickStack(stack, cardWidth, cardHeight, -stackWidth * 0.5f);
        }

        private static float ComputeFitScale(float panelWidth, float panelHeight, bool hasBox, int stackCount,
            float cardWidth, float cardHeight)
        {
            float colStep = cardWidth * ColOverlap;
            float rowStep = cardHeight * RowOverlap;
            float brickHalf = colStep * 0.5f;
            int rowCount = RowCountFor(stackCount);
            float stackWidth = MaxRowWidth(stackCount, cardWidth, colStep, brickHalf);
            float stackHeight = cardHeight + Mathf.Max(0, rowCount - 1) * rowStep;
            float boxGap = cardWidth * BoxGapFactor;
            float totalWidth = (hasBox ? cardWidth + boxGap : 0f) + stackWidth;
            float availableWidth = Mathf.Max(cardWidth, panelWidth - HorizontalPadding);
            float availableHeight = Mathf.Max(cardHeight, panelHeight - VerticalPadding);
            float widthScale = totalWidth > availableWidth ? availableWidth / totalWidth : 1f;
            float heightScale = stackHeight > availableHeight ? availableHeight / stackHeight : 1f;
            float scale = Mathf.Min(widthScale, heightScale, 1f);
            return Mathf.Max(scale, MinCardWidth / DefaultCardWidth);
        }

        private static void LayoutBoxWithBrickStack(RectTransform box, List<RectTransform> stack, float cardWidth,
            float cardHeight)
        {
            float colStep = cardWidth * ColOverlap;
            float rowStep = cardHeight * RowOverlap;
            float brickHalf = colStep * 0.5f;
            float boxGap = cardWidth * BoxGapFactor;
            int rowCount = RowCountFor(stack.Count);
            float stackWidth = MaxRowWidth(stack.Count, cardWidth, colStep, brickHalf);
            float totalWidth = cardWidth + boxGap + stackWidth;
            float boxX = -totalWidth * 0.5f + cardWidth * 0.5f;
            float stackOriginX = boxX + cardWidth * 0.5f + boxGap;

            ApplyTileSize(box, cardWidth, cardHeight, ignoreLayout: true);
            box.anchoredPosition = new Vector2(boxX, 0f);

            LayoutBrickStack(stack, cardWidth, cardHeight, stackOriginX);
        }

        private static void LayoutBrickStack(List<RectTransform> stack, float cardWidth, float cardHeight,
            float originX)
        {
            if (stack.Count == 0)
            {
                return;
            }

            float colStep = cardWidth * ColOverlap;
            float rowStep = cardHeight * RowOverlap;
            float brickHalf = colStep * 0.5f;
            int rowCount = RowCountFor(stack.Count);
            float topY = (rowCount - 1) * rowStep * 0.5f;

            int tileIndex = 0;
            for (int row = 0; row < rowCount && tileIndex < stack.Count; row++)
            {
                int tilesInRow = Mathf.Min(2, stack.Count - tileIndex);
                float rowOffsetX = row % 2 == 0 ? brickHalf : 0f;
                float baseCenterX = originX + rowOffsetX + cardWidth * 0.5f;
                float y = topY - row * rowStep;

                for (int col = 0; col < tilesInRow; col++)
                {
                    RectTransform rect = stack[tileIndex];
                    ApplyTileSize(rect, cardWidth, cardHeight, ignoreLayout: true);
                    rect.anchoredPosition = new Vector2(baseCenterX + col * colStep, y);
                    tileIndex++;
                }
            }
        }

        private static int RowCountFor(int tileCount)
        {
            if (tileCount <= 0)
            {
                return 0;
            }

            return (tileCount + 1) / 2;
        }

        private static float MaxRowWidth(int tileCount, float cardWidth, float colStep, float brickHalf)
        {
            int rowCount = RowCountFor(tileCount);
            float maxWidth = 0f;
            int tileIndex = 0;
            for (int row = 0; row < rowCount && tileIndex < tileCount; row++)
            {
                int tilesInRow = Mathf.Min(2, tileCount - tileIndex);
                float rowOffsetX = row % 2 == 0 ? brickHalf : 0f;
                float rowWidth = cardWidth + (tilesInRow - 1) * colStep + rowOffsetX;
                maxWidth = Mathf.Max(maxWidth, rowWidth);
                tileIndex += tilesInRow;
            }

            return maxWidth;
        }

        private static void ApplyTileSize(RectTransform rect, float cardWidth, float cardHeight, bool ignoreLayout)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(cardWidth, cardHeight);

            LayoutElement layoutElement = rect.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = rect.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.ignoreLayout = ignoreLayout;
            layoutElement.minWidth = cardWidth;
            layoutElement.preferredWidth = cardWidth;
            layoutElement.minHeight = cardHeight;
            layoutElement.preferredHeight = cardHeight;
        }

        private static List<LayoutItem> CollectLayoutItems(RectTransform panel)
        {
            var items = new List<LayoutItem>();
            for (int i = 0; i < panel.childCount; i++)
            {
                Transform child = panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() != null
                    || child.GetComponent<AsteriskCancelWidget>() != null)
                {
                    continue;
                }

                CardWidget cardWidget = child.GetComponent<CardWidget>();
                if (cardWidget != null)
                {
                    items.Add(new LayoutItem
                    {
                        Rect = child as RectTransform,
                        IsBox = cardWidget.Card.Kind == CardKind.Box
                    });
                    continue;
                }

                if (child.GetComponent<BalanceHoleWidget>() != null)
                {
                    items.Add(new LayoutItem
                    {
                        Rect = child as RectTransform,
                        IsBox = false
                    });
                }
            }

            return items;
        }

        private struct LayoutItem
        {
            public RectTransform Rect;
            public bool IsBox;
        }
    }
}
