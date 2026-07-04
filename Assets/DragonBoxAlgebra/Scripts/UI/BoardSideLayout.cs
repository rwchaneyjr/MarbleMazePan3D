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
        private const float HorizontalPadding = 32f;
        private const float VerticalPadding = 32f;
        private const float TileGap = 10f;

        // Creature slots for the pattern:
        //   + +
        // x   + +
        //     + +
        private static readonly GridCell[] BoxSideCreatureSlots =
        {
            new GridCell(0, 1), new GridCell(0, 2),
            new GridCell(1, 2), new GridCell(1, 3),
            new GridCell(2, 1), new GridCell(2, 2)
        };

        // Same stagger without the box column:
        // + +
        //   + +
        // + +
        private static readonly GridCell[] CreatureOnlySlots =
        {
            new GridCell(0, 0), new GridCell(0, 1),
            new GridCell(1, 1), new GridCell(1, 2),
            new GridCell(2, 0), new GridCell(2, 1)
        };

        private static readonly GridCell BoxSlot = new GridCell(1, 0);

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
            var creatures = new List<RectTransform>();
            foreach (LayoutItem item in items)
            {
                if (item.IsBox)
                {
                    box = item.Rect;
                }
                else
                {
                    creatures.Add(item.Rect);
                }
            }

            float cardWidth = DefaultCardWidth;
            float cardHeight = DefaultCardHeight;
            GridCell[] slots = box != null ? BoxSideCreatureSlots : CreatureOnlySlots;
            int maxCol = box != null ? BoxSlot.Col : 0;
            foreach (GridCell slot in slots)
            {
                maxCol = Mathf.Max(maxCol, slot.Col);
            }

            float scale = ComputeFitScale(panelWidth, panelHeight, maxCol, cardWidth, cardHeight);
            cardWidth *= scale;
            cardHeight *= scale;

            float colPitch = cardWidth + TileGap * scale;
            float rowPitch = cardHeight + TileGap * scale;
            int rowCount = 3;
            float gridWidth = (maxCol + 1) * colPitch - TileGap * scale;
            float gridHeight = rowCount * rowPitch - TileGap * scale;
            float originX = -gridWidth * 0.5f + cardWidth * 0.5f;
            float originY = gridHeight * 0.5f - cardHeight * 0.5f;

            if (box != null)
            {
                ApplyTileSize(box, cardWidth, cardHeight, ignoreLayout: true);
                box.anchoredPosition = CellCenter(BoxSlot, originX, originY, colPitch, rowPitch);
            }

            for (int i = 0; i < creatures.Count; i++)
            {
                GridCell slot = i < slots.Length ? slots[i] : ExtraSlot(i, slots.Length, box != null);
                ApplyTileSize(creatures[i], cardWidth, cardHeight, ignoreLayout: true);
                creatures[i].anchoredPosition = CellCenter(slot, originX, originY, colPitch, rowPitch);
            }
        }

        private static Vector2 CellCenter(GridCell cell, float originX, float originY, float colPitch, float rowPitch)
        {
            return new Vector2(originX + cell.Col * colPitch, originY - cell.Row * rowPitch);
        }

        private static GridCell ExtraSlot(int index, int baseSlotCount, bool hasBox)
        {
            int overflow = index - baseSlotCount;
            int row = overflow / 2;
            int col = (overflow % 2) + (hasBox ? 4 : 3);
            return new GridCell(row % 3, col);
        }

        private static float ComputeFitScale(float panelWidth, float panelHeight, int maxCol, float cardWidth,
            float cardHeight)
        {
            float colPitch = cardWidth + TileGap;
            float rowPitch = cardHeight + TileGap;
            float gridWidth = (maxCol + 1) * colPitch - TileGap;
            float gridHeight = 3f * rowPitch - TileGap;
            float availableWidth = Mathf.Max(cardWidth, panelWidth - HorizontalPadding);
            float availableHeight = Mathf.Max(cardHeight, panelHeight - VerticalPadding);
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
        }
    }
}
