using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class CardWidget : MonoBehaviour, IPointerClickHandler
    {
        public BoardCard Card { get; private set; }
        public int Index { get; private set; }
        public string SideName { get; private set; }

        private AlgebraGameController _controller;
        private CardWidget _selectedPartner;
        private Image _background;
        private Text _label;

        public void Bind(BoardCard card, int index, string sideName, AlgebraGameController controller)
        {
            Card = card;
            Index = index;
            SideName = sideName;
            _controller = controller;
            RefreshVisual();
        }

        public void SetSelected(bool selected)
        {
            if (_background != null)
            {
                _background.color = selected
                    ? Color.Lerp(CardVisuals.Background(Card.Kind), Color.white, 0.35f)
                    : CardVisuals.Background(Card.Kind);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_controller == null || Card.Kind == CardKind.Box || SideName == "Hand")
            {
                return;
            }

            var boardView = GetComponentInParent<BoardView>();
            if (boardView == null)
            {
                return;
            }

            boardView.HandleCardClicked(this);
        }

        public static CardWidget Create(Transform parent, BoardCard card, int index, string sideName,
            AlgebraGameController controller)
        {
            var root = new GameObject($"Card_{sideName}_{index}", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);

            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(110f, 110f);

            var image = root.GetComponent<Image>();
            image.color = CardVisuals.Background(card.Kind);

            var widget = root.AddComponent<CardWidget>();
            widget._background = image;
            widget.Bind(card, index, sideName, controller);

            var emojiGo = new GameObject("Emoji", typeof(RectTransform), typeof(Text));
            emojiGo.transform.SetParent(root.transform, false);
            var emojiRect = emojiGo.GetComponent<RectTransform>();
            emojiRect.anchorMin = Vector2.zero;
            emojiRect.anchorMax = Vector2.one;
            emojiRect.offsetMin = new Vector2(8f, 28f);
            emojiRect.offsetMax = new Vector2(-8f, -8f);

            var emojiText = emojiGo.GetComponent<Text>();
            emojiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            emojiText.alignment = TextAnchor.MiddleCenter;
            emojiText.fontSize = 34;
            emojiText.text = CardVisuals.Emoji(card);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(root.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(0f, 24f);

            var labelText = labelGo.GetComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.fontSize = 14;
            labelText.color = Color.white;
            labelText.text = CardVisuals.Label(card);
            widget._label = labelText;

            return widget;
        }
    }
}
