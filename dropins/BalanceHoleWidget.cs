using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class BalanceHoleWidget : MonoBehaviour, IDropHandler
    {
        private AlgebraGameController _controller;
        private string _sideName;

        public string SideName => _sideName;

        public void Initialize(AlgebraGameController controller, string sideName, BoardCard card,
            float tileWidth = 110f, float tileHeight = 120f)
        {
            _controller = controller;
            _sideName = sideName;
            Build(card, tileWidth, tileHeight);
        }

        public void OnDrop(PointerEventData eventData)
        {
            CardWidget dragged = eventData.pointerDrag?.GetComponent<CardWidget>();
            if (dragged == null || dragged.SideName != "Hand")
            {
                return;
            }

            // Filling the hole clears PendingBalance → BoardView rebuild removes this widget.
            if (_controller.TryPlayFromHand(dragged.Index, _sideName))
            {
                dragged.MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
            }
        }

        private void Build(BoardCard card, float tileWidth, float tileHeight)
        {
            var rect = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(tileWidth, tileHeight);

            var layout = gameObject.AddComponent<LayoutElement>();
            layout.minWidth = tileWidth;
            layout.minHeight = tileHeight;
            layout.preferredWidth = tileWidth;
            layout.preferredHeight = tileHeight;

            var image = gameObject.AddComponent<Image>();
            image.sprite = SpriteFactory.RoundedCard;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.98f, 0.84f, 0.14f, 1f);
            image.raycastTarget = true;

            var borderGo = new GameObject("Border", typeof(RectTransform), typeof(Image));
            borderGo.transform.SetParent(transform, false);
            var borderRect = borderGo.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-5f, -5f);
            borderRect.offsetMax = new Vector2(5f, 5f);
            var borderImage = borderGo.GetComponent<Image>();
            borderImage.sprite = SpriteFactory.RoundedCard;
            borderImage.type = Image.Type.Sliced;
            borderImage.raycastTarget = false;
            borderImage.color = new Color(0.72f, 0.48f, 0.04f, 1f);

            // Big "?" — leaves room at the bottom for the small card preview.
            var questionGo = new GameObject("QuestionMark", typeof(RectTransform), typeof(Text));
            questionGo.transform.SetParent(transform, false);
            var questionRect = questionGo.GetComponent<RectTransform>();
            questionRect.anchorMin = new Vector2(0f, 0.32f);
            questionRect.anchorMax = new Vector2(1f, 1f);
            questionRect.offsetMin = Vector2.zero;
            questionRect.offsetMax = Vector2.zero;

            var questionText = questionGo.GetComponent<Text>();
            questionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            questionText.alignment = TextAnchor.MiddleCenter;
            questionText.fontSize = Mathf.Clamp(Mathf.RoundToInt(tileHeight * 0.45f), 36, 72);
            questionText.fontStyle = FontStyle.Bold;
            questionText.color = Color.black;
            questionText.text = "?";
            questionText.raycastTarget = false;

            AddCardPreview(card, tileWidth, tileHeight);
        }

        /// <summary>
        /// Small image of the card that was played to the opposite side (what must fill this hole).
        /// </summary>
        private void AddCardPreview(BoardCard card, float tileWidth, float tileHeight)
        {
            Sprite previewSprite = CardVisuals.IconSprite(card);
            if (previewSprite == null)
            {
                previewSprite = CardVisuals.CreatureSprite(card);
            }

            float previewSize = Mathf.Clamp(Mathf.Min(tileWidth, tileHeight) * 0.38f, 28f, 48f);

            var previewGo = new GameObject("CardPreview", typeof(RectTransform), typeof(Image));
            previewGo.transform.SetParent(transform, false);
            var previewRect = previewGo.GetComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.5f, 0.08f);
            previewRect.anchorMax = new Vector2(0.5f, 0.08f);
            previewRect.pivot = new Vector2(0.5f, 0f);
            previewRect.sizeDelta = new Vector2(previewSize, previewSize);

            var previewImage = previewGo.GetComponent<Image>();
            previewImage.raycastTarget = false;
            previewImage.preserveAspect = true;
            if (previewSprite != null)
            {
                previewImage.sprite = previewSprite;
                previewImage.color = Color.white;
            }
            else
            {
                // Fallback: tiny tinted chip + algebra label so the hole still shows what to match.
                previewImage.sprite = SpriteFactory.RoundedCard;
                previewImage.type = Image.Type.Sliced;
                previewImage.color = CardVisuals.FaceBackground(card, "Hand");

                var labelGo = new GameObject("PreviewLabel", typeof(RectTransform), typeof(Text));
                labelGo.transform.SetParent(previewGo.transform, false);
                var labelRect = labelGo.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                var label = labelGo.GetComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.alignment = TextAnchor.MiddleCenter;
                label.fontSize = Mathf.Clamp(Mathf.RoundToInt(previewSize * 0.35f), 10, 16);
                label.fontStyle = FontStyle.Bold;
                label.color = Color.black;
                label.text = CardVisuals.AlgebraLabel(card);
                label.raycastTarget = false;
            }
        }

        public static BalanceHoleWidget Create(Transform parent, AlgebraGameController controller, string sideName,
            BoardCard card, float tileWidth = 110f, float tileHeight = 120f)
        {
            var go = new GameObject($"BalanceHole_{sideName}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var hole = go.AddComponent<BalanceHoleWidget>();
            hole.Initialize(controller, sideName, card, tileWidth, tileHeight);
            return hole;
        }
    }
}
