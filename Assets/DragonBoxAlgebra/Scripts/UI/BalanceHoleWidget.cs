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
            questionText.fontSize = 88;
            questionText.fontStyle = FontStyle.Bold;
            questionText.color = new Color(1f, 0.85f, 0.15f, 0.9f);
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
