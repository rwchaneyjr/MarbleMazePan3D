using System.Collections;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class CardWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
        IPointerClickHandler, IPointerUpHandler
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
        private Image _creatureImage;
        private Text _creatureText;
        private Text _labelText;
        private CreatureReaction _reaction;
        private Vector2 _dragOffset;
        private Transform _originalParent;
        private int _originalSiblingIndex;
        private bool _isDragging;
        private bool _dragStarted;
        private bool _didDrag;
        private bool _dropHandled;
        private bool _handPlayHandled;
        private bool _handFlipHandled;
        private Vector2 _dragPressScreenPosition;
        private CanvasGroup _canvasGroup;

        /// <summary>Hand drag only starts after this much movement — keeps taps as flips.</summary>
        private const float DragStartThresholdPixels = 50f;

        public void OnPointerClick(PointerEventData eventData) => TryFlipHandOnClick(eventData);

        public void OnPointerUp(PointerEventData eventData) => TryFlipHandOnClick(eventData);

        private void TryFlipHandOnClick(PointerEventData eventData)
        {
            if (SideName != "Hand" || _controller == null || _handPlayHandled || _handFlipHandled || !CanFlipHand())
            {
                return;
            }

            if (_dragStarted && ExceededDragStartThreshold(eventData))
            {
                return;
            }

            TryFlipHandOnTap();
        }

        private void TryFlipHandOnTap()
        {
            if (_handFlipHandled)
            {
                return;
            }

            if (!_controller.TryFlipHandCard(Index))
            {
                return;
            }

            _handFlipHandled = true;
            DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayUndo();
            // HandChanged rebuilds hand widgets — do not StartCoroutine on this instance.
        }

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

        public void SetHandCard(BoardCard card)
        {
            Card = card;
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
            }

            RefreshVisual();
        }

        public void RefreshVisual()
        {
            if (_background != null)
            {
                _background.color = CardVisuals.FaceBackground(Card, SideName);
            }

            if (_border != null)
            {
                _border.color = CardVisuals.FaceBorder(Card, SideName);
            }

            ApplyCreatureVisual();
            if (_labelText != null)
            {
                bool showHandSlotLabel = SideName == "Hand"
                    && _controller != null
                    && _controller.Hand.Count > 1
                    && Card.Kind is CardKind.DayCreature or CardKind.NightCreature;
                bool showIconOnly = CardVisuals.ShowsIconOnly(Card) && _creatureImage != null && _creatureImage.enabled
                    && !showHandSlotLabel;

                if (showIconOnly)
                {
                    _labelText.text = string.Empty;
                }
                else
                {
                    _labelText.text = CardVisuals.AlgebraLabel(Card);
                    if (Card.StackCount > 1)
                    {
                        _labelText.text += $" x{Card.StackCount}";
                    }
                }
            }

            ApplyHandSlotDimming();
        }

        private void ApplyHandSlotDimming()
        {
            if (_canvasGroup == null || _controller == null || SideName != "Hand")
            {
                return;
            }

            bool completed = _controller.IsHandBalanceComplete(Index);
            bool waitingTurn = _controller.UsesDualHandPanelDisplay
                && !completed
                && !_controller.IsHandSlotPlayable(Index);
            _canvasGroup.alpha = completed || waitingTurn ? 0.55f : 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        private void ApplyCreatureVisual()
        {
            Sprite icon = CardVisuals.IconSprite(Card);
            bool isCreature = Card.Kind is CardKind.DayCreature or CardKind.NightCreature or CardKind.Box;
            bool showHandSlotLabel = SideName == "Hand"
                && _controller != null
                && _controller.Hand.Count > 1
                && Card.Kind is CardKind.DayCreature or CardKind.NightCreature;
            bool usesFullIcon = CardVisuals.ShowsIconOnly(Card) && !showHandSlotLabel;

            if (_creatureImage != null)
            {
                _creatureImage.sprite = icon;
                _creatureImage.enabled = icon != null;
                _creatureImage.color = Color.white;

                var creatureRect = _creatureImage.rectTransform.parent as RectTransform;
                if (creatureRect != null)
                {
                    if (usesFullIcon && isCreature)
                    {
                        creatureRect.offsetMin = new Vector2(3f, 3f);
                        creatureRect.offsetMax = new Vector2(-3f, -3f);
                    }
                    else
                    {
                        creatureRect.offsetMin = new Vector2(8f, 28f);
                        creatureRect.offsetMax = new Vector2(-8f, -8f);
                    }
                }

                if (icon != null && _creatureImage.rectTransform != null)
                {
                    if (usesFullIcon && isCreature && creatureRect != null)
                    {
                        ApplyCoverSpriteLayout(_creatureImage, icon, creatureRect);
                    }
                    else
                    {
                        var inset = usesFullIcon ? new Vector2(6f, 6f) : new Vector2(8f, 28f);
                        StretchSpriteLayout(_creatureImage.rectTransform, inset);
                        _creatureImage.preserveAspect = true;
                    }
                }
            }

            if (_creatureText != null)
            {
                _creatureText.font = EmojiFont.Get();
                _creatureText.text = CardVisuals.Emoji(Card);
                _creatureText.fontSize = CardVisuals.EmojiFontSize(Card);
                _creatureText.enabled = icon == null;
            }
        }

        private static void StretchSpriteLayout(RectTransform imageRect, Vector2 inset)
        {
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = Vector2.zero;
            imageRect.offsetMin = inset;
            imageRect.offsetMax = new Vector2(-inset.x, -inset.y);
            imageRect.sizeDelta = Vector2.zero;
        }

        private static void ApplyCoverSpriteLayout(Image image, Sprite sprite, RectTransform container)
        {
            Canvas.ForceUpdateCanvases();
            float containerWidth = container.rect.width;
            float containerHeight = container.rect.height;
            if (containerWidth <= 0f || containerHeight <= 0f || sprite.rect.height <= 0f)
            {
                StretchSpriteLayout(image.rectTransform, Vector2.zero);
                image.preserveAspect = true;
                return;
            }

            float spriteAspect = sprite.rect.width / sprite.rect.height;
            float containerAspect = containerWidth / containerHeight;

            float imageWidth;
            float imageHeight;
            if (spriteAspect > containerAspect)
            {
                imageHeight = containerHeight;
                imageWidth = containerHeight * spriteAspect;
            }
            else
            {
                imageWidth = containerWidth;
                imageHeight = containerWidth / spriteAspect;
            }

            var imageRect = image.rectTransform;
            imageRect.anchorMin = imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = Vector2.zero;
            imageRect.sizeDelta = new Vector2(imageWidth, imageHeight);
            image.preserveAspect = true;
        }

        public void SetHighlight(bool on)
        {
            if (_background != null)
            {
                Color baseColor = CardVisuals.FaceBackground(Card, SideName);
                _background.color = on
                    ? Color.Lerp(baseColor, Color.white, 0.35f)
                    : baseColor;
            }
        }

        private IEnumerator PlayHandFlip()
        {
            if (_rect == null)
            {
                RefreshVisual();
                yield break;
            }

            const float halfDuration = 0.1f;
            Vector3 scale = _rect.localScale;

            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float n = elapsed / halfDuration;
                _rect.localScale = new Vector3(Mathf.Lerp(scale.x, 0.05f, n), scale.y, scale.z);
                yield return null;
            }

            RefreshVisual();

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float n = elapsed / halfDuration;
                _rect.localScale = new Vector3(Mathf.Lerp(0.05f, scale.x, n), scale.y, scale.z);
                yield return null;
            }

            _rect.localScale = scale;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (SideName == "Hand")
            {
                if (!CanFlipHand() && !CanDrag())
                {
                    return;
                }
            }
            else if (!CanDrag())
            {
                return;
            }

            _isDragging = true;
            _dragStarted = false;
            _didDrag = false;
            _dropHandled = false;
            _handPlayHandled = false;
            _handFlipHandled = false;
            _dragPressScreenPosition = eventData.pressPosition;

            if (SideName != "Hand")
            {
                BeginBoardDrag(eventData);
            }
        }

        private void BeginBoardDrag(PointerEventData eventData)
        {
            _dragStarted = true;
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
            }

            transform.SetParent(_dragRoot, true);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, eventData.position,
                eventData.pressEventCamera, out _dragOffset);
            _dragOffset = (Vector2)transform.localPosition - _dragOffset;
        }

        private void BeginHandDrag(PointerEventData eventData)
        {
            _dragStarted = true;
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
            }

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

            if (SideName == "Hand")
            {
                if (!_dragStarted)
                {
                    if (CanDrag() && ExceededDragStartThreshold(eventData))
                    {
                        BeginHandDrag(eventData);
                    }

                    return;
                }

                if (_dragStarted
                    && RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, eventData.position,
                        eventData.pressEventCamera, out Vector2 handLocalPoint))
                {
                    _didDrag = true;
                    _rect.localPosition = handLocalPoint + _dragOffset;
                }

                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, eventData.position,
                    eventData.pressEventCamera, out Vector2 localPoint))
            {
                _didDrag = true;
                _rect.localPosition = localPoint + _dragOffset;
            }
        }

        private float PointerMovementPixels(PointerEventData eventData) =>
            eventData != null ? Vector2.Distance(_dragPressScreenPosition, eventData.position) : 0f;

        private bool ExceededDragStartThreshold(PointerEventData eventData) =>
            PointerMovementPixels(eventData) > DragStartThresholdPixels;

        public void MarkHandPlayHandled() => _handPlayHandled = true;

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                return;
            }

            _isDragging = false;

            if (SideName == "Hand")
            {
                if (!_dragStarted)
                {
                    if (!_handPlayHandled && !_handFlipHandled && CanFlipHand())
                    {
                        TryFlipHandOnTap();
                    }

                    _isDragging = false;
                    return;
                }

                if (!_handPlayHandled)
                {
                    if (_canvasGroup != null)
                    {
                        _canvasGroup.blocksRaycasts = false;
                    }

                    if (ExceededDragStartThreshold(eventData))
                    {
                        TryPlayHandDrop(eventData);
                    }

                    if (!_handPlayHandled && !_handFlipHandled && CanFlipHand() && !ExceededDragStartThreshold(eventData))
                    {
                        TryFlipHandOnTap();
                    }
                }

                if (_handPlayHandled)
                {
                    if (_controller.ShouldKeepHandCardInPanel(Index))
                    {
                        transform.SetParent(_originalParent, false);
                        transform.SetSiblingIndex(_originalSiblingIndex);
                        SetHandCard(_controller.GetHandDisplayCard(Index));
                    }
                    else
                    {
                        DestroyImmediate(gameObject);
                    }

                    _controller.RefreshHandPresentation();
                    return;
                }

                transform.SetParent(_originalParent, false);
                transform.SetSiblingIndex(_originalSiblingIndex);
                RestoreDragRaycasts();
                return;
            }

            CardWidget target = FindBoardMergeTarget(eventData);
            if (target != null)
            {
                if (HandleDropOnCard(target))
                {
                    RestoreDragRaycasts();
                    Destroy(gameObject);
                    return;
                }
            }

            transform.SetParent(_originalParent, false);
            transform.SetSiblingIndex(_originalSiblingIndex);
            RestoreDragRaycasts();
        }

        private void RestoreDragRaycasts()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
            }
        }

        private void TryPlayHandDrop(PointerEventData eventData)
        {
            if (_controller.HasPendingBalance)
            {
                string holeSide = _controller.PendingBalance.HoleSide;

                BalanceHoleWidget balanceHole = FindBalanceHole(eventData);
                if (balanceHole != null && balanceHole.SideName == holeSide)
                {
                    if (_controller.TryPlayFromHand(Index, holeSide))
                    {
                        MarkHandPlayHandled();
                        DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                    }

                    return;
                }

                BoardDropZone zone = FindBoardZone(eventData);
                string dropSide = zone != null ? zone.SideName : SideUnderPointer(eventData);
                if (dropSide == holeSide && _controller.TryPlayFromHand(Index, holeSide))
                {
                    MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            CardWidget targetCard = FindHandBoardTarget(eventData);
            if (targetCard != null)
            {
                TryPlayHandOnBoardTarget(targetCard);
            }
            else
            {
                BoardDropZone zone = FindBoardZone(eventData);
                string dropSide = zone != null ? zone.SideName : SideUnderPointer(eventData);
                if (dropSide != null)
                {
                    TryPlayHandOnSide(dropSide);
                }
            }
        }

        private void TryPlayHandOnSide(string sideName)
        {
            if (_controller.UsesOppositeHandPlay)
            {
                if (_controller.TryPlayHandOntoOppositeOnSide(Index, sideName))
                {
                    MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            if (_controller.TryPlayFromHand(Index, sideName))
            {
                MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
            }
        }

        private string SideUnderPointer(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return null;
            }

            var boardView = FindObjectOfType<BoardView>();
            if (boardView != null)
            {
                return boardView.SideAtScreenPosition(eventData.position);
            }

            return eventData.position.x >= Screen.width * 0.5f ? "Right" : "Left";
        }

        public void OnDrop(PointerEventData eventData)
        {
            CardWidget dragged = eventData.pointerDrag?.GetComponent<CardWidget>();
            if (dragged == null || dragged == this || dragged._dropHandled)
            {
                return;
            }

            dragged.HandleDropOnCard(this);
        }

        public bool HandleDropOnCard(CardWidget target)
        {
            if (_dropHandled)
            {
                return false;
            }

            if (SideName == "Hand")
            {
                if (_controller.HasPendingBalance)
                {
                    if (target.SideName == _controller.PendingBalance.HoleSide
                        && _controller.TryPlayFromHand(Index, target.SideName))
                    {
                        MarkHandPlayHandled();
                        DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                    }

                    return false;
                }

                if (target.SideName != "Hand")
                {
                    TryPlayHandOnBoardTarget(target);
                    if (!_handPlayHandled)
                    {
                        TryPlayHandOnSide(target.SideName);
                    }
                }

                return false;
            }

            if (SideName != target.SideName)
            {
                return false;
            }

            SnapOnto(target);
            if (_controller.TryCombine(SideName, Index, target.Index))
            {
                _dropHandled = true;
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                return true;
            }

            return false;
        }

        private void SnapOnto(CardWidget target)
        {
            if (_rect == null || _dragRoot == null)
            {
                return;
            }

            var targetRect = target.transform as RectTransform;
            if (targetRect == null)
            {
                return;
            }

            Vector3 world = targetRect.position;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, world,
                    _canvas != null ? _canvas.worldCamera : null, out Vector2 local))
            {
                _rect.localPosition = local;
            }
        }

        private BalanceHoleWidget FindBalanceHole(PointerEventData eventData)
        {
            foreach (GameObject go in GetRaycastHits(eventData))
            {
                BalanceHoleWidget hole = go.GetComponent<BalanceHoleWidget>();
                if (hole == null)
                {
                    hole = go.GetComponentInParent<BalanceHoleWidget>();
                }

                if (hole != null)
                {
                    return hole;
                }
            }

            return null;
        }

        private BoardDropZone FindBoardZone(PointerEventData eventData)
        {
            foreach (GameObject go in GetRaycastHits(eventData))
            {
                BoardDropZone zone = go.GetComponent<BoardDropZone>();
                if (zone == null)
                {
                    zone = go.GetComponentInParent<BoardDropZone>();
                }

                if (zone != null)
                {
                    return zone;
                }
            }

            return null;
        }

        private CardWidget FindDropTarget(PointerEventData eventData) => FindBoardMergeTarget(eventData);

        private CardWidget FindHandBoardTarget(PointerEventData eventData)
        {
            foreach (CardWidget widget in GetHoveredCardWidgets(eventData))
            {
                if (widget == this || widget.SideName == "Hand")
                {
                    continue;
                }

                if (_controller.CanPlayHandOntoBoardCard(Index, widget.SideName, widget.Index))
                {
                    return widget;
                }
            }

            return null;
        }

        private CardWidget FindBoardMergeTarget(PointerEventData eventData)
        {
            CardWidget fallback = null;
            foreach (CardWidget widget in GetHoveredCardWidgets(eventData))
            {
                if (widget == this || widget.SideName != SideName)
                {
                    continue;
                }

                if (CombineRules.CanCombine(Card, widget.Card))
                {
                    return widget;
                }

                if (fallback == null && widget.Card.Kind != CardKind.Box)
                {
                    fallback = widget;
                }
            }

            return fallback;
        }

        private IEnumerable<CardWidget> GetHoveredCardWidgets(PointerEventData eventData)
        {
            var seen = new HashSet<CardWidget>();
            foreach (GameObject go in GetRaycastHits(eventData))
            {
                CardWidget widget = go.GetComponent<CardWidget>();
                if (widget == null)
                {
                    widget = go.GetComponentInParent<CardWidget>();
                }

                if (widget != null && seen.Add(widget))
                {
                    yield return widget;
                }
            }
        }

        private static IEnumerable<GameObject> GetRaycastHits(PointerEventData eventData)
        {
            if (EventSystem.current == null)
            {
                yield break;
            }

            RaycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, RaycastResults);
            foreach (RaycastResult result in RaycastResults)
            {
                if (result.gameObject != null)
                {
                    yield return result.gameObject;
                }
            }
        }

        private static readonly List<RaycastResult> RaycastResults = new();

        private bool CanDrag()
        {
            if (_controller == null)
            {
                return false;
            }

            if (SideName == "Hand")
            {
                return Card.IsPlayableFromHand && _controller.IsHandSlotPlayable(Index);
            }

            return Card.IsDraggableFromBoard;
        }

        private bool CanFlipHand() =>
            _controller != null && SideName == "Hand" && _controller.CanFlipHandCard(Index);

        private void TryPlayHandOnBoardTarget(CardWidget target)
        {
            if (_controller.UsesOppositeHandPlay)
            {
                if (_controller.TryPlayHandOntoOpposite(Index, target.SideName, target.Index))
                {
                    MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                    return;
                }

                if (_controller.TryPlayHandOntoOppositeOnSide(Index, target.SideName))
                {
                    MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            if (_controller.TryPlayFromHand(Index, target.SideName))
            {
                MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                return;
            }
        }

        public static CardWidget Create(Transform parent, BoardCard card, int index, string sideName,
            AlgebraGameController controller, Canvas canvas, RectTransform dragRoot,
            float tileWidth = 110f, float tileHeight = 120f)
        {
            var root = new GameObject($"Card_{sideName}_{index}", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            root.transform.SetParent(parent, false);

            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(tileWidth, tileHeight);

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
            borderImage.raycastTarget = false;
            borderImage.color = CardVisuals.Border(card.Kind);

            var image = root.GetComponent<Image>();
            image.sprite = SpriteFactory.RoundedCard;
            image.type = Image.Type.Sliced;
            image.color = CardVisuals.Background(card.Kind);

            var widget = root.AddComponent<CardWidget>();
            widget._rect = rect;
            widget._background = image;
            widget._border = borderImage;

            var creatureGo = new GameObject("Creature", typeof(RectTransform), typeof(CreatureReaction), typeof(RectMask2D));
            creatureGo.transform.SetParent(root.transform, false);
            var creatureRect = creatureGo.GetComponent<RectTransform>();
            creatureRect.anchorMin = Vector2.zero;
            creatureRect.anchorMax = Vector2.one;
            creatureRect.offsetMin = new Vector2(3f, 3f);
            creatureRect.offsetMax = new Vector2(-3f, -3f);
            widget._reaction = creatureGo.GetComponent<CreatureReaction>();

            var creatureImageGo = new GameObject("Sprite", typeof(RectTransform), typeof(Image));
            creatureImageGo.transform.SetParent(creatureGo.transform, false);
            var creatureImageRect = creatureImageGo.GetComponent<RectTransform>();
            creatureImageRect.anchorMin = Vector2.zero;
            creatureImageRect.anchorMax = Vector2.one;
            creatureImageRect.offsetMin = Vector2.zero;
            creatureImageRect.offsetMax = Vector2.zero;
            var creatureImage = creatureImageGo.GetComponent<Image>();
            creatureImage.raycastTarget = false;
            creatureImage.preserveAspect = true;
            widget._creatureImage = creatureImage;

            var creatureTextGo = new GameObject("Emoji", typeof(RectTransform), typeof(Text));
            creatureTextGo.transform.SetParent(creatureGo.transform, false);
            var creatureTextRect = creatureTextGo.GetComponent<RectTransform>();
            creatureTextRect.anchorMin = Vector2.zero;
            creatureTextRect.anchorMax = Vector2.one;
            creatureTextRect.offsetMin = Vector2.zero;
            creatureTextRect.offsetMax = Vector2.zero;
            var emojiText = creatureTextGo.GetComponent<Text>();
            emojiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            emojiText.alignment = TextAnchor.MiddleCenter;
            emojiText.fontSize = CardVisuals.EmojiFontSize(card);
            emojiText.raycastTarget = false;
            widget._creatureText = emojiText;

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
            widget._labelText = labelText;

            var layoutElement = root.AddComponent<LayoutElement>();
            layoutElement.minWidth = tileWidth;
            layoutElement.minHeight = tileHeight;
            layoutElement.preferredWidth = tileWidth;
            layoutElement.preferredHeight = tileHeight;

            widget.Bind(card, index, sideName, controller, canvas, dragRoot);
            widget._canvasGroup = root.GetComponent<CanvasGroup>();
            widget._canvasGroup.blocksRaycasts = true;
            return widget;
        }

        public void ReactCombine() => _reaction?.PlayCombine();
        public void ReactCelebrate() => _reaction?.PlayCelebrate();
        public void ReactUndo() => _reaction?.PlayUndo();
    }
}
