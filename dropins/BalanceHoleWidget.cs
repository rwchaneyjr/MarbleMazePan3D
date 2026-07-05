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
            Build(tileWidth, tileHeight);
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

        private void Build(float tileWidth, float tileHeight)
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
            questionText.color = Color.black;
            questionText.text = "?";
            questionText.raycastTarget = false;
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
