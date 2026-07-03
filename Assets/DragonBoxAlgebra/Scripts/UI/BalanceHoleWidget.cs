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

        public void Initialize(AlgebraGameController controller, string sideName, BoardCard card)
        {
            _controller = controller;
            _sideName = sideName;
            Build(card);
        }

        public void OnDrop(PointerEventData eventData)
        {
            CardWidget dragged = eventData.pointerDrag?.GetComponent<CardWidget>();
            if (dragged == null || dragged.SideName != "Hand")
            {
                return;
            }

            if (_controller.TryPlayFromHand(dragged.Index, _sideName))
            {
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
            }
        }

        private void Build(BoardCard card)
        {
            var rect = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(110f, 120f);

            var layout = gameObject.AddComponent<LayoutElement>();
            layout.minWidth = 110f;
            layout.minHeight = 120f;
            layout.preferredWidth = 110f;
            layout.preferredHeight = 120f;

            var image = gameObject.AddComponent<Image>();
            image.sprite = SpriteFactory.RoundedCard;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.05f, 0.05f, 0.08f, 0.55f);
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
            borderImage.color = new Color(1f, 0.78f, 0.1f, 0.95f);

            var bannerGo = new GameObject("Banner", typeof(RectTransform), typeof(Text));
            bannerGo.transform.SetParent(transform, false);
            var bannerRect = bannerGo.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0f, 1f);
            bannerRect.anchorMax = new Vector2(1f, 1f);
            bannerRect.pivot = new Vector2(0.5f, 1f);
            bannerRect.anchoredPosition = new Vector2(0f, 8f);
            bannerRect.sizeDelta = new Vector2(0f, 22f);
            var bannerText = bannerGo.GetComponent<Text>();
            bannerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bannerText.alignment = TextAnchor.MiddleCenter;
            bannerText.fontSize = 11;
            bannerText.fontStyle = FontStyle.Bold;
            bannerText.color = new Color(1f, 0.85f, 0.15f);
            bannerText.text = "BALANCE";
            bannerText.raycastTarget = false;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(transform, false);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(10f, 30f);
            iconRect.offsetMax = new Vector2(-10f, -10f);
            var iconImage = iconGo.GetComponent<Image>();
            iconImage.raycastTarget = false;
            iconImage.preserveAspect = true;
            Sprite icon = CardVisuals.IconSprite(card);
            iconImage.sprite = icon;
            iconImage.color = new Color(1f, 1f, 1f, 0.35f);
            iconImage.enabled = icon != null;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(0f, 24f);
            var labelText = labelGo.GetComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.fontSize = 13;
            labelText.fontStyle = FontStyle.Bold;
            labelText.color = new Color(1f, 0.92f, 0.4f);
            labelText.text = CardVisuals.AlgebraLabel(card);
            labelText.raycastTarget = false;

            var hintGo = new GameObject("Hint", typeof(RectTransform), typeof(Text));
            hintGo.transform.SetParent(transform, false);
            var hintRect = hintGo.GetComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0.5f);
            hintRect.anchorMax = new Vector2(0.5f, 0.5f);
            hintRect.sizeDelta = new Vector2(100f, 40f);
            hintRect.anchoredPosition = new Vector2(0f, -8f);
            var hintText = hintGo.GetComponent<Text>();
            hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.fontSize = 28;
            hintText.fontStyle = FontStyle.Bold;
            hintText.color = new Color(1f, 0.85f, 0.15f, 0.75f);
            hintText.text = "?";
            hintText.raycastTarget = false;
        }

        public static BalanceHoleWidget Create(Transform parent, AlgebraGameController controller, string sideName,
            BoardCard card)
        {
            var go = new GameObject($"BalanceHole_{sideName}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var hole = go.AddComponent<BalanceHoleWidget>();
            hole.Initialize(controller, sideName, card);
            return hole;
        }
    }
}
