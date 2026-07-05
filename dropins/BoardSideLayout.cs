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
        private const float MinCardWidth = 52f;
        private const int MaxOtherSideTiles = 6;
        private const float HorizontalPadding = 20f;
        private const float VerticalPadding = 20f;
        private const float TileGap = 14f;

        //   +       (3 beside box)
        // x +
        //   +
        //
        // X +       (2 beside box)
        //   +
        //
        // X +       (1 beside box)
        private static readonly GridCell BoxSlot = new GridCell(1, 1);

        private static readonly GridCell[] BoxSideSlotsOne = { new GridCell(1, 2) };
        private static readonly GridCell[] BoxSideSlotsTwo = { new GridCell(1, 2), new GridCell(2, 2) };
        private static readonly GridCell[] BoxSideSlotsThree = { new GridCell(0, 2), new GridCell(1, 2), new GridCell(2, 2) };

        // Other side (no box): vertical column, no stacking
        private static readonly GridCell[] OtherSideSlotsOne = { new GridCell(1, 0) };
        private static readonly GridCell[] OtherSideSlotsTwo = { new GridCell(0, 0), new GridCell(2, 0) };

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
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
            float panelWidth = panel.rect.width;
            float panelHeight = panel.rect.height;
            if (panelWidth <= 0f || panelHeight <= 0f)
            {
                return;
            }

            RectTransform box = null;
            var creatures = new List<LayoutItem>();
            foreach (LayoutItem item in items)
            {
                if (item.IsBox)
                {
                    box = item.Rect;
                }
                else
                {
                    creatures.Add(item);
                }
            }

            creatures.Sort((a, b) => a.BoardIndex.CompareTo(b.BoardIndex));

            GridCell[] slots = box != null
                ? SlotsForBoxSide(creatures.Count)
                : SlotsForOtherSide(creatures.Count);

            int maxCol = box != null ? 2 : Mathf.Max(0, slots.Length - 1);
            int rowCount = box != null ? RowCountForBoxSide(creatures.Count) : OtherSideRowCount(creatures.Count);

            float cardWidth = DefaultCardWidth;
            float cardHeight = DefaultCardHeight;
            float scale = ComputeFitScale(panelWidth, panelHeight, maxCol, rowCount, cardWidth, cardHeight);
            cardWidth *= scale;
            cardHeight *= scale;

            float colPitch = cardWidth + TileGap;
            float rowPitch = cardHeight + TileGap;
            float gridHeight = rowCount * rowPitch - TileGap;
            float leftEdge = -panelWidth * 0.5f + HorizontalPadding;
            float topY = gridHeight * 0.5f - cardHeight * 0.5f;

            if (box != null)
            {
                ApplyTileSize(box, cardWidth, cardHeight, ignoreLayout: true);
                box.anchoredPosition = CellCenter(BoxSlot, leftEdge, topY, colPitch, rowPitch, cardWidth);
            }

            for (int i = 0; i < creatures.Count && i < slots.Length; i++)
            {
                ApplyTileSize(creatures[i].Rect, cardWidth, cardHeight, ignoreLayout: true);
                creatures[i].Rect.anchoredPosition = CellCenter(slots[i], leftEdge, topY, colPitch, rowPitch,
                    cardWidth);
            }
        }

        private static GridCell[] SlotsForBoxSide(int count)
        {
            if (count <= 1)
            {
                return BoxSideSlotsOne;
            }

            if (count == 2)
            {
                return BoxSideSlotsTwo;
            }

            return BoxSideSlotsThree;
        }

        private static GridCell[] SlotsForOtherSide(int count)
        {
            if (count <= 1)
            {
                return OtherSideSlotsOne;
            }

            if (count == 2)
            {
                return OtherSideSlotsTwo;
            }

            int slotsToPlace = Mathf.Clamp(count, 3, MaxOtherSideTiles);
            var slots = new GridCell[slotsToPlace];
            for (int i = 0; i < slotsToPlace; i++)
            {
                slots[i] = new GridCell(0, i);
            }

            return slots;
        }

        private static int OtherSideRowCount(int count)
        {
            if (count <= 2)
            {
                return 3;
            }

            return 1;
        }

        private static int RowCountForBoxSide(int count)
        {
            if (count <= 1)
            {
                return 2;
            }

            return 3;
        }

        private static Vector2 CellCenter(GridCell cell, float originX, float topY, float colPitch, float rowPitch,
            float cardWidth)
        {
            float x = originX + cell.Col * colPitch + cardWidth * 0.5f;
            float y = topY - cell.Row * rowPitch;
            return new Vector2(x, y);
        }

        private static float ComputeFitScale(float panelWidth, float panelHeight, int maxCol, int rowCount,
            float cardWidth, float cardHeight)
        {
            float colPitch = cardWidth + TileGap;
            float rowPitch = cardHeight + TileGap;
            float gridWidth = (maxCol + 1) * colPitch - TileGap;
            float gridHeight = rowCount * rowPitch - TileGap;
            float availableWidth = Mathf.Max(cardWidth, panelWidth - HorizontalPadding * 2f);
            float availableHeight = Mathf.Max(cardHeight, panelHeight - VerticalPadding * 2f);
            float widthScale = gridWidth > availableWidth ? availableWidth / gridWidth : 1f;
            float heightScale = gridHeight > availableHeight ? availableHeight / gridHeight : 1f;
            float scale = Mathf.Min(widthScale, heightScale, 1f);
            return Mathf.Max(scale, MinCardWidth / DefaultCardWidth);
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
            rect.localScale = Vector3.one;

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
                        IsBox = cardWidget.Card.Kind == CardKind.Box,
                        BoardIndex = cardWidget.Index
                    });
                    continue;
                }

                if (child.GetComponent<BalanceHoleWidget>() != null)
                {
                    items.Add(new LayoutItem
                    {
                        Rect = child as RectTransform,
                        IsBox = false,
                        BoardIndex = i
                    });
                }
            }

            return items;
        }

        private readonly struct GridCell
        {
            public readonly int Row;
            public readonly int Col;

            public GridCell(int row, int col)
            {
                Row = row;
                Col = col;
            }
        }

        private struct LayoutItem
        {
            public RectTransform Rect;
            public bool IsBox;
            public int BoardIndex;
        }
    }
}
