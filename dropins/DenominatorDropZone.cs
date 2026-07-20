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
                typeof(DenominatorDropZone));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.06f, -0.22f);
            rect.anchorMax = new Vector2(0.94f, 0.02f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.1f, 0.12f, 0.18f, 0.01f);
            image.raycastTarget = true;

            var zone = go.GetComponent<DenominatorDropZone>();
            zone.Initialize(controller, sideName);

            // Visible fraction bar.
            var lineGo = new GameObject("FractionLine", typeof(RectTransform), typeof(Image));
            lineGo.transform.SetParent(go.transform, false);
            var lineRect = lineGo.GetComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0.05f, 0.72f);
            lineRect.anchorMax = new Vector2(0.95f, 0.82f);
            lineRect.offsetMin = Vector2.zero;
            lineRect.offsetMax = Vector2.zero;
            var lineImage = lineGo.GetComponent<Image>();
            lineImage.color = new Color(0.95f, 0.95f, 0.9f, 0.95f);
            lineImage.raycastTarget = false;

            var slotGo = new GameObject("DenomSlot", typeof(RectTransform));
            slotGo.transform.SetParent(go.transform, false);
            var slotRect = slotGo.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.25f, 0.05f);
            slotRect.anchorMax = new Vector2(0.75f, 0.68f);
            slotRect.offsetMin = Vector2.zero;
            slotRect.offsetMax = Vector2.zero;

            return zone;
        }

        public void RefreshVisual(AlgebraGameController controller)
        {
            Transform slot = transform.Find("DenomSlot");
            if (slot == null)
            {
                return;
            }

            for (int i = slot.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(slot.GetChild(i).gameObject);
            }

            if (controller == null || !controller.UsesMultiplyAdditionLevels)
            {
                gameObject.SetActive(false);
                return;
            }

            var side = controller.Board.GetSide(SideName);
            bool showZone = side.HasDenominator
                || controller.GetActiveFractionDivisor() != null
                || (controller.HasPendingDivide
                    && (controller.PendingDivide.HoleSide == SideName
                        || controller.PendingDivide.PlacedSide == SideName));
            if (!showZone)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            bool groupedLetter = controller.UsesGroupedLetterFraction(SideName);
            // Ch8/Ch9 single tiles: per-card guides own the line. Ch10 multi-term: this zone draws
            // one shared bar + one denominator under the whole grouped expression.
            Transform line = transform.Find("FractionLine");
            if (line != null)
            {
                line.gameObject.SetActive(groupedLetter);
            }

            var image = GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.1f, 0.12f, 0.18f, 0.01f);
                image.raycastTarget = true;
            }

            if (controller.HasPendingDivide && controller.PendingDivide.HoleSide == SideName)
            {
                if (groupedLetter)
                {
                    CreateDenomHint(slot, "?");
                }

                return;
            }

            if (!side.HasDenominator)
            {
                if (groupedLetter && controller.GetActiveFractionDivisor() != null)
                {
                    CreateDenomHint(slot, "?");
                }

                return;
            }

            CreateDenomCard(slot, side.Denominator.Value);
        }

        private static void CreateDenomHint(Transform slot, string label)
        {
            var textGo = new GameObject("DenomHint", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(slot, false);
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
            // Image and Text cannot share one GameObject in Unity UI.
            var cardGo = new GameObject("DenomCard", typeof(RectTransform), typeof(Image));
            cardGo.transform.SetParent(slot, false);
            var cardRect = cardGo.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.15f, 0f);
            cardRect.anchorMax = new Vector2(0.85f, 1f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            var cardBg = cardGo.GetComponent<Image>();
            cardBg.sprite = SpriteFactory.RoundedCard;
            cardBg.type = Image.Type.Sliced;
            cardBg.color = new Color(0.96f, 0.97f, 1f, 0.98f);
            cardBg.raycastTarget = false;

            Sprite numberSprite = CardSpriteLoader.GetNumberSprite(denom.Value, positive: true);
            if (numberSprite != null)
            {
                var spriteGo = new GameObject("Art", typeof(RectTransform), typeof(Image));
                spriteGo.transform.SetParent(cardGo.transform, false);
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
                textGo.transform.SetParent(cardGo.transform, false);
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
