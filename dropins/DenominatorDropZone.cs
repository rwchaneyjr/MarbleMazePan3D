using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    /// <summary>
    /// Drop zone under a board side's division line (DragonBox fraction denominator).
    /// </summary>
    public class DenominatorDropZone : MonoBehaviour, IDropHandler
    {
        public string SideName;
        private AlgebraGameController _controller;

        public void Initialize(AlgebraGameController controller, string sideName)
        {
            _controller = controller;
            SideName = sideName;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_controller == null || !_controller.UsesMultiplyAdditionLevels)
            {
                return;
            }

            CardWidget dragged = eventData.pointerDrag?.GetComponent<CardWidget>();
            if (dragged == null || dragged.SideName != "Hand")
            {
                return;
            }

            if (_controller.TryPlaceDenominatorFromHand(dragged.Index, SideName))
            {
                dragged.MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
            }
        }

        public static DenominatorDropZone Create(Transform parent, AlgebraGameController controller, string sideName)
        {
            var go = new GameObject($"DenomZone_{sideName}", typeof(RectTransform), typeof(Image),
                typeof(LayoutElement), typeof(DenominatorDropZone));
            go.transform.SetParent(parent, false);

            // Sit under the whole side as an overlay — never participate in the card row layout
            // (otherwise the shared line/blank collapses to zero width between tiles).
            var layoutElement = go.GetComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.03f, 0f);
            rect.anchorMax = new Vector2(0.97f, 0f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 96f);
            rect.anchoredPosition = new Vector2(0f, -6f);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.1f, 0.12f, 0.18f, 0.01f);
            image.raycastTarget = true;

            var zone = go.GetComponent<DenominatorDropZone>();
            zone.Initialize(controller, sideName);

            // Visible fraction bar across the grouped expression.
            var lineGo = new GameObject("FractionLine", typeof(RectTransform), typeof(Image));
            lineGo.transform.SetParent(go.transform, false);
            var lineRect = lineGo.GetComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0.02f, 0.78f);
            lineRect.anchorMax = new Vector2(0.98f, 0.88f);
            lineRect.offsetMin = Vector2.zero;
            lineRect.offsetMax = Vector2.zero;
            var lineImage = lineGo.GetComponent<Image>();
            lineImage.color = new Color(0.95f, 0.95f, 0.9f, 0.98f);
            lineImage.raycastTarget = false;

            var slotGo = new GameObject("DenomSlot", typeof(RectTransform));
            slotGo.transform.SetParent(go.transform, false);
            var slotRect = slotGo.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.32f, 0.02f);
            slotRect.anchorMax = new Vector2(0.68f, 0.72f);
            slotRect.offsetMin = Vector2.zero;
            slotRect.offsetMax = Vector2.zero;

            return zone;
        }

        public void RefreshVisual(AlgebraGameController controller)
        {
            var layoutElement = GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            Transform slot = transform.Find("DenomSlot");
            if (slot == null)
            {
                return;
            }

            for (int i = slot.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(slot.GetChild(i).gameObject);
            }

            if (controller == null || !controller.UsesMultiplyAdditionLevels)
            {
                gameObject.SetActive(false);
                return;
            }

            var side = controller.Board.GetSide(SideName);
            bool groupedLetter = controller.UsesGroupedLetterFraction(SideName);
            bool showZone = side.HasDenominator
                || controller.GetActiveFractionDivisor() != null
                || (controller.HasPendingDivide
                    && (controller.PendingDivide.HoleSide == SideName
                        || controller.PendingDivide.PlacedSide == SideName));

            // Grouped letter side: always show the shared line + blank while a divisor is active
            // on either side (left may already have 3/3 while right still needs the drop).
            if (!showZone && groupedLetter && controller.GetActiveFractionDivisor() != null)
            {
                showZone = true;
            }

            if (!showZone)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // Ch8/Ch9 single tiles: per-card guides own the line. Ch10 multi-term: this zone draws
            // one shared bar + one blank/denominator under the whole grouped expression.
            Transform line = transform.Find("FractionLine");
            if (line != null)
            {
                line.gameObject.SetActive(groupedLetter);
            }

            var image = GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.1f, 0.12f, 0.18f, groupedLetter ? 0.04f : 0.01f);
                image.raycastTarget = true;
            }

            if (!groupedLetter)
            {
                ApplyDefaultBottomOverlay();
                return;
            }

            if (side.HasDenominator)
            {
                CreateDenomCard(slot, side.Denominator.Value);
                return;
            }

            // Blank waiting for the matching coefficient (e.g. 3).
            CreateDenomHint(slot, "?");
        }

        /// <summary>
        /// Place the shared fraction bar flush under the equation tiles on this side.
        /// localLeft/Right/Bottom are in the parent panel's local space (center origin).
        /// </summary>
        public void SnapFlushUnderEquation(float localLeft, float localRight, float localBottom)
        {
            var layoutElement = GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            var rect = transform as RectTransform;
            if (rect == null)
            {
                return;
            }

            float width = Mathf.Max(140f, localRight - localLeft);
            float centerX = (localLeft + localRight) * 0.5f;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(width, 86f);
            // Pivot at top → top edge sits just under the tiles.
            rect.anchoredPosition = new Vector2(centerX, localBottom - 2f);

            Transform line = transform.Find("FractionLine");
            if (line != null)
            {
                var lineRect = line as RectTransform;
                lineRect.anchorMin = new Vector2(0.02f, 0.84f);
                lineRect.anchorMax = new Vector2(0.98f, 0.94f);
                lineRect.offsetMin = Vector2.zero;
                lineRect.offsetMax = Vector2.zero;
                line.gameObject.SetActive(true);
            }

            Transform slot = transform.Find("DenomSlot");
            if (slot != null)
            {
                var slotRect = slot as RectTransform;
                slotRect.anchorMin = new Vector2(0.34f, 0.02f);
                slotRect.anchorMax = new Vector2(0.66f, 0.78f);
                slotRect.offsetMin = Vector2.zero;
                slotRect.offsetMax = Vector2.zero;
            }
        }

        private void ApplyDefaultBottomOverlay()
        {
            var rect = transform as RectTransform;
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.03f, 0f);
            rect.anchorMax = new Vector2(0.97f, 0f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 96f);
            rect.anchoredPosition = new Vector2(0f, -6f);
        }

        private static void CreateDenomHint(Transform slot, string label)
        {
            // Match the per-card fraction slot under a·x (dark rounded box + ?).
            var boxGo = new GameObject("DenomBox", typeof(RectTransform), typeof(Image));
            boxGo.transform.SetParent(slot, false);
            var boxRect = boxGo.GetComponent<RectTransform>();
            boxRect.anchorMin = Vector2.zero;
            boxRect.anchorMax = Vector2.one;
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;
            var boxBg = boxGo.GetComponent<Image>();
            boxBg.sprite = SpriteFactory.RoundedCard;
            boxBg.type = Image.Type.Sliced;
            boxBg.color = new Color(0.16f, 0.2f, 0.3f, 0.92f);
            boxBg.raycastTarget = false;

            var textGo = new GameObject("DenomHint", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(boxGo.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label;
            text.fontSize = 42;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.92f, 0.94f, 0.98f, 0.95f);
            text.raycastTarget = false;
        }

        private static void CreateDenomCard(Transform slot, BoardCard denom)
        {
            // Same chrome as the fraction slot under a·x once a number is dropped.
            var boxGo = new GameObject("DenomCard", typeof(RectTransform), typeof(Image));
            boxGo.transform.SetParent(slot, false);
            var boxRect = boxGo.GetComponent<RectTransform>();
            boxRect.anchorMin = Vector2.zero;
            boxRect.anchorMax = Vector2.one;
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;

            var cardBg = boxGo.GetComponent<Image>();
            cardBg.sprite = SpriteFactory.RoundedCard;
            cardBg.type = Image.Type.Sliced;
            cardBg.color = new Color(0.96f, 0.97f, 1f, 0.98f);
            cardBg.raycastTarget = false;

            Sprite numberSprite = CardSpriteLoader.GetNumberSprite(denom.Value, positive: true);
            if (numberSprite != null)
            {
                var spriteGo = new GameObject("Art", typeof(RectTransform), typeof(Image));
                spriteGo.transform.SetParent(boxGo.transform, false);
                var spriteRect = spriteGo.GetComponent<RectTransform>();
                spriteRect.anchorMin = new Vector2(0.1f, 0.1f);
                spriteRect.anchorMax = new Vector2(0.9f, 0.9f);
                spriteRect.offsetMin = Vector2.zero;
                spriteRect.offsetMax = Vector2.zero;
                var spriteImage = spriteGo.GetComponent<Image>();
                spriteImage.sprite = numberSprite;
                spriteImage.preserveAspect = true;
                spriteImage.raycastTarget = false;
            }
            else
            {
                var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
                textGo.transform.SetParent(boxGo.transform, false);
                var textRect = textGo.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                var text = textGo.GetComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.text = denom.Value.ToString();
                text.fontSize = 36;
                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.black;
                text.raycastTarget = false;
            }
        }
    }
}
