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
        private BoardCard _card;

        public string SideName => _sideName;

        public void Initialize(AlgebraGameController controller, string sideName, BoardCard card)
        {
            _controller = controller;
            _sideName = sideName;
            _card = card;
            Build();
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
                dragged.MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
            }
        }

        private void Build()
        {
            var rect = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(110f, 120f);

            var layout = gameObject.AddComponent<LayoutElement>();
            layout.minWidth = 110f;
            layout.minHeight = 120f;
            layout.preferredWidth = 110f;
            layout.preferredHeight = 120f;

            bool isDice = _card.Kind is CardKind.PositiveConstant or CardKind.NegativeConstant;
            Color face = isDice
                ? CardVisuals.Background(_card.Kind)
                : new Color(0.98f, 0.84f, 0.14f, 1f);
            Color border = isDice
                ? CardVisuals.Border(_card.Kind)
                : new Color(0.72f, 0.48f, 0.04f, 1f);

            var image = gameObject.AddComponent<Image>();
            image.sprite = SpriteFactory.RoundedCard;
            image.type = Image.Type.Sliced;
            image.color = face;
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
            borderImage.color = border;

            if (isDice)
            {
                Sprite icon = CardVisuals.IconSprite(_card);
                if (icon != null)
                {
                    var iconGo = new GameObject("DiceHint", typeof(RectTransform), typeof(Image));
                    iconGo.transform.SetParent(transform, false);
                    var iconRect = iconGo.GetComponent<RectTransform>();
                    iconRect.anchorMin = Vector2.zero;
                    iconRect.anchorMax = Vector2.one;
                    iconRect.offsetMin = new Vector2(10f, 10f);
                    iconRect.offsetMax = new Vector2(-10f, -10f);
                    var iconImage = iconGo.GetComponent<Image>();
                    iconImage.sprite = icon;
                    iconImage.preserveAspect = true;
                    iconImage.color = new Color(1f, 1f, 1f, 0.45f);
                    iconImage.raycastTarget = false;
                }
            }

            var questionGo = new GameObject("QuestionMark", typeof(RectTransform), typeof(Text));
            questionGo.transform.SetParent(transform, false);
            var questionRect = questionGo.GetComponent<RectTransform>();
            questionRect.anchorMin = Vector2.zero;
            questionRect.anchorMax = Vector2.one;
            questionRect.offsetMin = Vector2.zero;
            questionRect.offsetMax = Vector2.zero;

            var questionText = questionGo.GetComponent<Text>();
            questionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            questionText.alignment = TextAnchor.MiddleCenter;
            questionText.fontSize = isDice ? 64 : 88;
            questionText.fontStyle = FontStyle.Bold;
            questionText.color = isDice ? new Color(0f, 0f, 0f, 0.75f) : Color.black;
            questionText.text = "?";
            questionText.raycastTarget = false;
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
