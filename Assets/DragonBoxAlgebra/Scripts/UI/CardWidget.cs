using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class CardWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public BoardCard Card { get; private set; }
        public int Index { get; private set; }
        public string SideName { get; private set; }

        private AlgebraGameController _controller;
        private RectTransform _rect;
        private RectTransform _dragRoot;
        private Canvas _canvas;
        private Image _background;
        private Image _border;
        private CreatureReaction _reaction;
        private Vector2 _dragOffset;
        private Transform _originalParent;
        private int _originalSiblingIndex;
        private bool _isDragging;

        public void Bind(BoardCard card, int index, string sideName, AlgebraGameController controller, Canvas canvas,
            RectTransform dragRoot)
        {
            Card = card;
            Index = index;
            SideName = sideName;
            _controller = controller;
            _canvas = canvas;
            _dragRoot = dragRoot;
            RefreshVisual();
        }

        public void RefreshVisual()
        {
            if (_background != null)
            {
                _background.color = CardVisuals.Background(Card.Kind);
            }

            if (_border != null)
            {
                _border.color = CardVisuals.Border(Card.Kind);
            }
        }

        public void SetHighlight(bool on)
        {
            if (_background != null)
            {
                _background.color = on
                    ? Color.Lerp(CardVisuals.Background(Card.Kind), Color.white, 0.35f)
                    : CardVisuals.Background(Card.Kind);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanDrag())
            {
                return;
            }

            _isDragging = true;
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();
            transform.SetParent(_dragRoot, true);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, eventData.position,
                eventData.pressEventCamera, out _dragOffset);
            _dragOffset = (Vector2)transform.localPosition - _dragOffset;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, eventData.position,
                    eventData.pressEventCamera, out Vector2 localPoint))
            {
                _rect.localPosition = localPoint + _dragOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                return;
            }

            _isDragging = false;

            if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<CardDropZone>() == null)
            {
                CardWidget target = FindDropTarget(eventData);
                if (target != null && target != this)
                {
                    HandleDropOnCard(target);
                }
                else if (SideName == "Hand")
                {
                    BoardDropZone boardZone = FindBoardZone(eventData);
                    if (boardZone != null)
                    {
                        _controller.TryPlayFromHand(Index);
                        DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                    }
                }
            }

            transform.SetParent(_originalParent, false);
            transform.SetSiblingIndex(_originalSiblingIndex);
        }

        public void OnDrop(PointerEventData eventData)
        {
            CardWidget dragged = eventData.pointerDrag?.GetComponent<CardWidget>();
            if (dragged == null || dragged == this)
            {
                return;
            }

            dragged.HandleDropOnCard(this);
        }

        public void HandleDropOnCard(CardWidget target)
        {
            if (SideName == "Hand")
            {
                _controller.TryPlayFromHand(Index);
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                return;
            }

            if (SideName != target.SideName)
            {
                return;
            }

            _controller.TryCombine(SideName, Index, target.Index);
        }

        private BoardDropZone FindBoardZone(PointerEventData eventData)
        {
            foreach (RaycastResult result in eventData.hovered)
            {
                BoardDropZone zone = result.gameObject.GetComponent<BoardDropZone>();
                if (zone != null)
                {
                    return zone;
                }
            }

            return null;
        }

        private CardWidget FindDropTarget(PointerEventData eventData)
        {
            foreach (RaycastResult result in eventData.hovered)
            {
                CardWidget widget = result.gameObject.GetComponent<CardWidget>();
                if (widget != null && widget != this)
                {
                    return widget;
                }
            }

            return null;
        }

        private bool CanDrag()
        {
            if (_controller == null)
            {
                return false;
            }

            if (SideName == "Hand")
            {
                return Card.IsPlayableFromHand;
            }

            return Card.IsDraggableFromBoard;
        }

        public static CardWidget Create(Transform parent, BoardCard card, int index, string sideName,
            AlgebraGameController controller, Canvas canvas, RectTransform dragRoot)
        {
            var root = new GameObject($"Card_{sideName}_{index}", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            root.transform.SetParent(parent, false);

            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(110f, 120f);

            var borderGo = new GameObject("Border", typeof(RectTransform), typeof(Image));
            borderGo.transform.SetParent(root.transform, false);
            var borderRect = borderGo.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-3f, -3f);
            borderRect.offsetMax = new Vector2(3f, 3f);
            var borderImage = borderGo.GetComponent<Image>();
            borderImage.sprite = SpriteFactory.RoundedCard;
            borderImage.type = Image.Type.Sliced;
            borderImage.color = CardVisuals.Border(card.Kind);

            var image = root.GetComponent<Image>();
            image.sprite = SpriteFactory.RoundedCard;
            image.type = Image.Type.Sliced;
            image.color = CardVisuals.Background(card.Kind);

            var widget = root.AddComponent<CardWidget>();
            widget._rect = rect;
            widget._background = image;
            widget._border = borderImage;
            widget.Bind(card, index, sideName, controller, canvas, dragRoot);

            var emojiGo = new GameObject("Emoji", typeof(RectTransform), typeof(Text), typeof(CreatureReaction));
            emojiGo.transform.SetParent(root.transform, false);
            var emojiRect = emojiGo.GetComponent<RectTransform>();
            emojiRect.anchorMin = Vector2.zero;
            emojiRect.anchorMax = Vector2.one;
            emojiRect.offsetMin = new Vector2(8f, 28f);
            emojiRect.offsetMax = new Vector2(-8f, -8f);

            var emojiText = emojiGo.GetComponent<Text>();
            emojiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            emojiText.alignment = TextAnchor.MiddleCenter;
            emojiText.fontSize = 38;
            emojiText.text = CardVisuals.Emoji(card);
            widget._reaction = emojiGo.GetComponent<CreatureReaction>();

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
            labelText.fontSize = 13;
            labelText.color = Color.white;
            labelText.text = CardVisuals.Label(card);

            if (card.StackCount > 1)
            {
                labelText.text += $" x{card.StackCount}";
            }

            root.GetComponent<CanvasGroup>().blocksRaycasts = true;
            return widget;
        }

        public void ReactCombine() => _reaction?.PlayCombine();
        public void ReactCelebrate() => _reaction?.PlayCelebrate();
        public void ReactUndo() => _reaction?.PlayUndo();
    }
}
