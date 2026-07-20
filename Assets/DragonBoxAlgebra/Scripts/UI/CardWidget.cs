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
        IPointerClickHandler
    {
        public BoardCard Card { get; private set; }
        public int Index { get; private set; }
        public string SideName { get; private set; }
        public AlgebraGameController Controller => _controller;

        /// <summary>Screen-pixel snap radius — forwarded to DraggableTile.snapDistance.</summary>
        public float snapDistance = 220f;

        private const float DragToMergeSnapDistance = 320f;

        private AlgebraGameController _controller;
        private RectTransform _rect;
        private RectTransform _dragRoot;
        private Canvas _canvas;
        private Image _background;
        private Image _border;
        private Image _creatureImage;
        private Text _creatureText;
        private Text _labelText;
        private GameObject _fractionGuideRoot;
        private Image _fractionLineImage;
        private GameObject _fractionSlotGo;
        private Text _fractionSlotHint;
        private CreatureReaction _reaction;
        private DraggableTile _draggable;
        private Vector2 _dragOffset;
        private Transform _originalParent;
        private int _originalSiblingIndex;
        private bool _isDragging;
        private bool _dragStarted;
        private bool _didDrag;
        private bool _dropHandled;
        private bool _handPlayHandled;
        private int _lastFlipFrame = -1;
        private Vector2 _dragPressScreenPosition;
        private CanvasGroup _canvasGroup;

        /// <summary>Pixels before a press counts as drag instead of flip — lower = drag is easier.</summary>
        private const float FlipDragThresholdPixels = 28f;
        private const float DragScale = 1.08f;

        private Vector3 _originalScale;
        private CardWidget _snapHighlight;
        private Vector2 _lastDragScreenPosition;
        private Vector2 _boardDragSize;

        /// <summary>True while this board/hand tile is mid-drag on the DragRoot.</summary>
        public bool IsActivelyDragging => _isDragging && _dragStarted;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_didDrag || _dragStarted || _controller == null)
            {
                return;
            }

            // Multiply levels: tap a number above the line (3/3 → 1, dice/3 → divide).
            if (SideName != "Hand"
                && _controller.UsesMultiplyAdditionLevels
                && _controller.Board.Left.HasDenominator
                && _controller.Board.Right.HasDenominator
                && _controller.TryResolveDivisionOnCard(SideName, Index))
            {
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCombine();
                return;
            }

            if (SideName != "Hand" || !CanFlipHand())
            {
                return;
            }

            TryFlipHandOnTap();
        }

        private void TryFlipHandOnTap()
        {
            if (Time.frameCount == _lastFlipFrame)
            {
                return;
            }

            if (!CanFlipHand() || !_controller.TryFlipHandCard(Index))
            {
                return;
            }

            _lastFlipFrame = Time.frameCount;
            Card = _controller.GetHandDisplayCard(Index);
            RefreshVisual();
            DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayUndo();
            StartCoroutine(PlayHandFlip());
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
                bool showIconOnly = CardVisuals.ShowsIconOnly(Card) && _creatureImage != null && _creatureImage.enabled;
                bool largeNumberText = !showIconOnly
                    && Card.Kind is CardKind.PositiveConstant or CardKind.NegativeConstant or CardKind.One
                    && (_creatureImage == null || !_creatureImage.enabled);

                if (showIconOnly || largeNumberText)
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
            RefreshFractionGuide();
        }

        /// <summary>Show the DragonBox fraction underline + empty slot under a·x / dice (151–165 only).</summary>
        public void RefreshFractionGuide()
        {
            if (_fractionGuideRoot == null)
            {
                return;
            }

            bool show = SideName != "Hand"
                && _controller != null
                && _controller.UsesMultiplyAdditionLevels
                && _controller.ShouldShowFractionLineUnder(SideName, Index);
            _fractionGuideRoot.SetActive(show);
            if (!show)
            {
                return;
            }

            var fractionRect = _fractionGuideRoot.transform as RectTransform;
            bool productAnchor = _controller.IsFractionProductAnchor(SideName, Index);
            bool isVariablePart = VariableGoalRules.IsVariableXGoal(Card);
            if (fractionRect != null)
            {
                // Span under a·x from the coefficient; x half only extends the hit target lightly.
                if (productAnchor)
                {
                    fractionRect.anchorMin = new Vector2(0.05f, -0.95f);
                    fractionRect.anchorMax = new Vector2(1.95f, -0.04f);
                }
                else if (isVariablePart)
                {
                    // Coefficient already draws the shared line+slot; keep a slim drop target under x.
                    fractionRect.anchorMin = new Vector2(0.05f, -0.95f);
                    fractionRect.anchorMax = new Vector2(0.95f, -0.04f);
                }
                else
                {
                    fractionRect.anchorMin = new Vector2(0.08f, -0.95f);
                    fractionRect.anchorMax = new Vector2(0.92f, -0.04f);
                }
            }

            if (_fractionLineImage != null)
            {
                _fractionLineImage.color = new Color(0.95f, 0.95f, 0.9f, 0.98f);
                _fractionLineImage.gameObject.SetActive(!isVariablePart || productAnchor);
            }

            if (_fractionSlotGo != null)
            {
                // One slot under a·x (on the coefficient) and one under the dice — not under x alone.
                bool showSlot = productAnchor || !isVariablePart;
                _fractionSlotGo.SetActive(showSlot);
            }

            if (_fractionSlotHint != null)
            {
                var side = _controller.Board.GetSide(SideName);
                if (side.HasDenominator)
                {
                    _fractionSlotHint.text = side.Denominator.Value.Value.ToString();
                    _fractionSlotHint.color = Color.white;
                }
                else if (_controller.HasPendingDivide
                         && _controller.PendingDivide.HoleSide == SideName)
                {
                    _fractionSlotHint.text = "?";
                    _fractionSlotHint.color = new Color(0.92f, 0.94f, 0.98f, 0.9f);
                }
                else
                {
                    _fractionSlotHint.text = "?";
                    _fractionSlotHint.color = new Color(0.92f, 0.94f, 0.98f, 0.9f);
                }
            }
        }

        private void ApplyHandSlotDimming()
        {
            if (_canvasGroup == null || _controller == null || SideName != "Hand")
            {
                return;
            }

            // Never dim or deactivate hand tiles — spent only blocks replay, not appearance.
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
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
                bool numberNeedsText = icon == null
                    && Card.Kind is CardKind.PositiveConstant or CardKind.NegativeConstant or CardKind.One;
                if (numberNeedsText)
                {
                    _creatureText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    _creatureText.text = CardVisuals.AlgebraLabel(Card);
                    _creatureText.fontSize = Card.Value >= 10 ? 36 : 44;
                    _creatureText.fontStyle = FontStyle.Bold;
                    _creatureText.color = Color.black;
                    _creatureText.enabled = true;
                }
                else
                {
                    _creatureText.font = EmojiFont.Get();
                    _creatureText.text = CardVisuals.Emoji(Card);
                    _creatureText.fontSize = CardVisuals.EmojiFontSize(Card);
                    _creatureText.color = Color.white;
                    _creatureText.enabled = icon == null;
                }
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
                yield break;
            }

            const float halfDuration = 0.1f;
            Vector3 scale = _rect.localScale;

            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                if (_rect == null)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                float n = elapsed / halfDuration;
                _rect.localScale = new Vector3(Mathf.Lerp(scale.x, 0.05f, n), scale.y, scale.z);
                yield return null;
            }

            if (_rect == null)
            {
                yield break;
            }

            if (_controller != null && SideName == "Hand")
            {
                Card = _controller.GetHandDisplayCard(Index);
            }

            RefreshVisual();

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                if (_rect == null)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                float n = elapsed / halfDuration;
                _rect.localScale = new Vector3(Mathf.Lerp(0.05f, scale.x, n), scale.y, scale.z);
                yield return null;
            }

            if (_rect != null)
            {
                _rect.localScale = scale;
            }
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
            _snapHighlight = null;
            _dragPressScreenPosition = eventData.pressPosition;
            _originalScale = _rect != null ? _rect.localScale : Vector3.one;
            EnsureDraggable().RememberStart();
            EnsureDraggable().snapDistance = snapDistance;

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
            if (_dragRoot != null)
            {
                _dragRoot.SetAsLastSibling();
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.alpha = 0.95f;
            }

            // Layout stretch anchors ignore localPosition — switch to center anchors so the
            // tile visibly follows the pointer, and keep the on-screen size/position.
            if (_rect != null)
            {
                _boardDragSize = _rect.rect.size;
                if (_boardDragSize.x < 1f || _boardDragSize.y < 1f)
                {
                    _boardDragSize = _rect.sizeDelta;
                }

                Vector3 worldPos = _rect.position;
                var layoutElement = GetComponent<LayoutElement>();
                if (layoutElement != null)
                {
                    layoutElement.ignoreLayout = true;
                }

                _rect.anchorMin = _rect.anchorMax = new Vector2(0.5f, 0.5f);
                _rect.pivot = new Vector2(0.5f, 0.5f);
                _rect.sizeDelta = _boardDragSize;
                _rect.localScale = _originalScale * DragScale;
                transform.SetParent(_dragRoot, true);
                _rect.position = worldPos;
            }
            else
            {
                transform.SetParent(_dragRoot, true);
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, eventData.position,
                eventData.pressEventCamera, out _dragOffset);
            _dragOffset = (Vector2)_rect.localPosition - _dragOffset;
        }

        private void BeginHandDrag(PointerEventData eventData)
        {
            _dragStarted = true;
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();
            if (_dragRoot != null)
            {
                _dragRoot.SetAsLastSibling();
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.alpha = 0.95f;
            }

            if (_rect != null)
            {
                _rect.localScale = _originalScale * DragScale;
            }

            transform.SetParent(_dragRoot, true);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, eventData.position,
                eventData.pressEventCamera, out _dragOffset);
            _dragOffset = (Vector2)transform.localPosition - _dragOffset;
            _controller?.BeginFractionDrag(Index);
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
                    if (!CanDrag() || !ExceededFlipDragThreshold(eventData))
                    {
                        return;
                    }

                    BeginHandDrag(eventData);
                }

                if (_dragStarted
                    && RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, eventData.position,
                        eventData.pressEventCamera, out Vector2 handLocalPoint))
                {
                    _didDrag = true;
                    _lastDragScreenPosition = eventData.position;
                    _rect.localPosition = handLocalPoint + _dragOffset;
                    UpdateSnapHighlight(eventData);
                }

                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, eventData.position,
                    eventData.pressEventCamera, out Vector2 localPoint))
            {
                _didDrag = true;
                _lastDragScreenPosition = eventData.position;
                _rect.localPosition = localPoint + _dragOffset;
                UpdateSnapHighlight(eventData);
            }
        }

        private bool ExceededFlipDragThreshold(PointerEventData eventData) =>
            eventData != null
            && Vector2.Distance(_dragPressScreenPosition, eventData.position) > FlipDragThresholdPixels;

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
                    if (!_handPlayHandled && !ExceededFlipDragThreshold(eventData) && CanFlipHand())
                    {
                        TryFlipHandOnTap();
                    }

                    _controller?.EndFractionDrag();
                    return;
                }

                if (!_handPlayHandled && ExceededFlipDragThreshold(eventData))
                {
                    if (_canvasGroup != null)
                    {
                        _canvasGroup.blocksRaycasts = false;
                    }

                    // DraggableTile pattern: snap to correct opposite, else return / balance.
                    if (!TryToSnap(eventData))
                    {
                        TryPlayHandDrop(eventData);
                    }
                }
                else if (!_handPlayHandled && CanFlipHand())
                {
                    TryFlipHandOnTap();
                }

                ClearSnapHighlight();
                RestoreDragVisuals();
                _controller?.EndFractionDrag();

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

                ReturnToStart();
                return;
            }

            ClearSnapHighlight();
            RestoreDragVisuals();
            RestoreDragRaycasts();

            // Board opposites (Ch1/Ch2 drag-to-merge): snap light onto dark on the same side.
            if (!TryToSnap(eventData) && !TrySnapBoardOppositeNearby(eventData))
            {
                ReturnToStart();
                return;
            }

            if (_dropHandled)
            {
                // Board refresh owns cleanup when combine succeeded; only destroy if still on DragRoot.
                if (this != null && gameObject != null && transform.parent == _dragRoot)
                {
                    Destroy(gameObject);
                }

                return;
            }

            // Snapped visually but merge did not apply — put the tile back.
            EnsureDraggable().ClearSnappedFlag();
            ReturnToStart();
        }

        /// <summary>
        /// Wider same-side opposite search so light/dark pairs snap together even when not
        /// exactly on top of each other (Ch1/Ch2 drag-to-merge levels).
        /// </summary>
        private bool TrySnapBoardOppositeNearby(PointerEventData eventData)
        {
            if (SideName == "Hand" || _controller == null || !_controller.UsesDragToMergePairs)
            {
                return false;
            }

            CardWidget best = null;
            float bestDistance = DragToMergeSnapDistance;
            Vector2 screen = eventData != null ? eventData.position : _lastDragScreenPosition;
            Camera cam = eventData != null
                ? eventData.pressEventCamera
                : (_canvas != null ? _canvas.worldCamera : null);
            Vector3 selfPos = transform.position;

            foreach (TileSnapTarget target in FindObjectsOfType<TileSnapTarget>())
            {
                if (target == null || !target.IsCorrectTile(this))
                {
                    continue;
                }

                float screenDist = Vector2.Distance(screen,
                    RectTransformUtility.WorldToScreenPoint(cam, target.GetSnapPosition()));
                float worldDist = Vector3.Distance(selfPos, target.GetSnapPosition()) * 100f;
                float distance = Mathf.Min(screenDist, worldDist);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = target.Widget;
                }
            }

            if (best == null)
            {
                return false;
            }

            transform.position = best.transform.position;
            return HandleDropOnCard(best);
        }

        /// <summary>
        /// Uses DraggableTile + TileSnapTarget: closest correct opposite within snapDistance.
        /// </summary>
        private bool TryToSnap(PointerEventData eventData)
        {
            DraggableTile drag = EnsureDraggable();
            Vector2 screen = eventData != null ? eventData.position : _lastDragScreenPosition;
            Camera cam = eventData != null
                ? eventData.pressEventCamera
                : (_canvas != null ? _canvas.worldCamera : null);

            TileSnapTarget target = drag.TryToSnap(screen, cam);
            if (target == null || target.Widget == null)
            {
                return false;
            }

            _snapHighlight = target.Widget;

            if (SideName == "Hand")
            {
                TryPlayHandOnBoardTarget(target.Widget);
            }
            else if (HandleDropOnCard(target.Widget))
            {
                _dropHandled = true;
            }

            // Only treat as handled when a real merge/play happened — not merely a visual snap.
            return _handPlayHandled || _dropHandled;
        }

        private void ReturnToStart()
        {
            ClearSnapHighlight();
            EnsureDraggable().ReturnToStart();

            if (_originalParent != null && transform.parent != _originalParent)
            {
                transform.SetParent(_originalParent, false);
                transform.SetSiblingIndex(_originalSiblingIndex);
            }

            if (_rect != null)
            {
                _rect.anchoredPosition = Vector2.zero;
                _rect.localRotation = Quaternion.identity;
                _rect.localScale = _originalScale == Vector3.zero ? Vector3.one : _originalScale;
            }

            if (_originalParent is RectTransform parentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }

            RestoreDragVisuals();
            RestoreDragRaycasts();
        }

        private void UpdateSnapHighlight(PointerEventData eventData)
        {
            ClearSnapHighlight();

            TileSnapTarget closest = EnsureDraggable()
                .FindClosestTarget(eventData.position, eventData.pressEventCamera);
            if (closest?.Widget != null)
            {
                _snapHighlight = closest.Widget;
                _snapHighlight.SetHighlight(true);
            }
        }

        private void ClearSnapHighlight()
        {
            if (_snapHighlight != null)
            {
                _snapHighlight.SetHighlight(false);
                _snapHighlight = null;
            }
        }

        private DraggableTile EnsureDraggable()
        {
            if (_draggable == null)
            {
                _draggable = GetComponent<DraggableTile>() ?? gameObject.AddComponent<DraggableTile>();
                _draggable.Bind(this);
                _draggable.snapDistance = snapDistance;
            }

            return _draggable;
        }

        private void RestoreDragVisuals()
        {
            if (_rect != null)
            {
                _rect.localScale = _originalScale == Vector3.zero ? Vector3.one : _originalScale;
            }

            if (_canvasGroup != null && SideName == "Hand")
            {
                ApplyHandSlotDimming();
            }
            else if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
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
            // 1) Opposite under the pointer → merge + swirl in that slot.
            CardWidget opposite = FindOppositeBoardCardUnderPointer(eventData);
            if (opposite == null && _snapHighlight != null
                && CombineRules.GetCombineAction(Card, _snapHighlight.Card) == CombineActionType.OppositeCancel)
            {
                opposite = _snapHighlight;
            }

            if (opposite != null)
            {
                TryPlayHandOnBoardTarget(opposite);
                return;
            }

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

                BoardDropZone boardZone = FindBoardZone(eventData);
                string pendingSide = boardZone != null ? boardZone.SideName : SideUnderPointer(eventData);
                if (pendingSide == holeSide && _controller.TryPlayFromHand(Index, holeSide))
                {
                    MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            // Over a board tile: fill ?, start balance on that side, or ignore non-opposites.
            // Addition levels (129–139) already have tiles — empty-padding-only was locking drops.
            CardWidget boardTarget = FindHandBoardTarget(eventData);
            if (boardTarget != null)
            {
                if (_controller.HasPendingBalance
                    && _controller.PendingBalance != null
                    && _controller.PendingBalance.HoleSide == boardTarget.SideName
                    && _controller.TryPlayFromHand(Index, boardTarget.SideName))
                {
                    MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                    return;
                }

                if (!_controller.UsesOppositeHandPlay
                    && CombineRules.GetCombineAction(Card, boardTarget.Card) != CombineActionType.OppositeCancel
                    && _controller.TryPlayFromHand(Index, boardTarget.SideName))
                {
                    MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            // Drop onto / under a card that has a fraction guide (5 of 5·x, or the dice).
            CardWidget fractionTarget = FindFractionLineTarget(eventData);
            if (fractionTarget != null
                && TryPlaceDenominatorUnderCard(fractionTarget))
            {
                return;
            }

            DenominatorDropZone denomZone = FindDenominatorZone(eventData);
            if (denomZone != null
                && _controller.UsesMultiplyAdditionLevels
                && _controller.TryPlaceDenominatorFromHand(Index, denomZone.SideName))
            {
                MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                return;
            }

            BoardDropZone zone = FindBoardZone(eventData);
            string dropSide = zone != null ? zone.SideName : SideUnderPointer(eventData);
            if (dropSide != null)
            {
                TryPlayHandOnSide(dropSide);
            }
        }

        private static DenominatorDropZone FindDenominatorZone(PointerEventData eventData)
        {
            if (eventData == null || EventSystem.current == null)
            {
                return null;
            }

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            foreach (RaycastResult result in results)
            {
                if (result.gameObject == null)
                {
                    continue;
                }

                DenominatorDropZone zone = result.gameObject.GetComponent<DenominatorDropZone>()
                    ?? result.gameObject.GetComponentInParent<DenominatorDropZone>();
                if (zone != null)
                {
                    return zone;
                }
            }

            return null;
        }

        private CardWidget FindOppositeBoardCardUnderPointer(PointerEventData eventData)
        {
            foreach (CardWidget widget in GetHoveredCardWidgets(eventData))
            {
                if (widget == null || widget == this || widget.SideName == "Hand")
                {
                    continue;
                }

                if (CombineRules.GetCombineAction(Card, widget.Card) != CombineActionType.OppositeCancel)
                {
                    continue;
                }

                if (_controller != null
                    && !_controller.CanPlayHandOntoBoardCard(Index, widget.SideName, widget.Index))
                {
                    continue;
                }

                return widget;
            }

            return null;
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
                    // Only merge when this tile is the opposite. Do not start balance from a card drop —
                    // that appends at the end of the side and looks like the tile "went off to the side".
                    if (CombineRules.GetCombineAction(Card, target.Card) == CombineActionType.OppositeCancel)
                    {
                        TryPlayHandOnBoardTarget(target);
                    }
                    else if (TryPlaceDenominatorUnderCard(target))
                    {
                        return false;
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

            Camera cam = _canvas != null ? _canvas.worldCamera : null;
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, targetRect.position);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, screen, cam, out Vector2 local))
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
            // Only accept a hovered free tile that can actually combine — never a dead fallback.
            foreach (CardWidget widget in GetHoveredCardWidgets(eventData))
            {
                if (widget == this || widget.SideName != SideName)
                {
                    continue;
                }

                if (_controller != null && _controller.IsCardPendingCancelOnSide(widget.Card.Id, widget.SideName))
                {
                    continue;
                }

                if (CombineRules.CanCombine(Card, widget.Card))
                {
                    return widget;
                }
            }

            return null;
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
            if (target == null)
            {
                return;
            }

            // With lines under both sides, dropping on a number resolves 3/3 → 1 or dice/3.
            if (_controller.UsesMultiplyAdditionLevels
                && _controller.Board.Left.HasDenominator
                && _controller.Board.Right.HasDenominator
                && _controller.TryResolveDivisionOnCard(target.SideName, target.Index))
            {
                MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCombine();
                return;
            }

            // Dropping on top of an opposite always merges + swirl (all chapters).
            if (CombineRules.GetCombineAction(Card, target.Card) == CombineActionType.OppositeCancel)
            {
                if (_controller.TryPlayHandOntoOpposite(Index, target.SideName, target.Index))
                {
                    MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            if (TryPlaceDenominatorUnderCard(target))
            {
                return;
            }

            if (_controller.UsesOppositeHandPlay)
            {
                if (_controller.TryPlayHandOntoOppositeOnSide(Index, target.SideName))
                {
                    MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }
            }

            // Do not call TryPlayFromHand here — that parks the tile at the end of the side.
        }

        private bool TryPlaceDenominatorUnderCard(CardWidget target)
        {
            if (target == null || _controller == null || !_controller.UsesMultiplyAdditionLevels)
            {
                return false;
            }

            if (Card.Kind is not (CardKind.PositiveConstant or CardKind.NegativeConstant))
            {
                return false;
            }

            if (!_controller.ShouldShowFractionLineUnder(target.SideName, target.Index))
            {
                return false;
            }

            if (_controller.TryPlaceDenominatorFromHand(Index, target.SideName))
            {
                MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                return true;
            }

            return false;
        }

        private CardWidget FindFractionLineTarget(PointerEventData eventData)
        {
            foreach (CardWidget widget in GetHoveredCardWidgets(eventData))
            {
                if (widget == null || widget == this || widget.SideName == "Hand")
                {
                    continue;
                }

                if (_controller != null
                    && _controller.ShouldShowFractionLineUnder(widget.SideName, widget.Index))
                {
                    return widget;
                }
            }

            return null;
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
            image.raycastTarget = true;

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
            labelText.raycastTarget = false;
            widget._labelText = labelText;

            if (sideName != "Hand")
            {
                var fractionGo = new GameObject("FractionGuide", typeof(RectTransform), typeof(Image));
                fractionGo.transform.SetParent(root.transform, false);
                var fractionRect = fractionGo.GetComponent<RectTransform>();
                fractionRect.anchorMin = new Vector2(0.08f, -0.95f);
                fractionRect.anchorMax = new Vector2(0.92f, -0.04f);
                fractionRect.offsetMin = Vector2.zero;
                fractionRect.offsetMax = Vector2.zero;
                var fractionHit = fractionGo.GetComponent<Image>();
                fractionHit.color = new Color(1f, 1f, 1f, 0.02f);
                fractionHit.raycastTarget = true;

                var lineGo = new GameObject("FractionLine", typeof(RectTransform), typeof(Image));
                lineGo.transform.SetParent(fractionGo.transform, false);
                var lineRect = lineGo.GetComponent<RectTransform>();
                lineRect.anchorMin = new Vector2(0.04f, 0.78f);
                lineRect.anchorMax = new Vector2(0.96f, 0.9f);
                lineRect.offsetMin = Vector2.zero;
                lineRect.offsetMax = Vector2.zero;
                var lineImage = lineGo.GetComponent<Image>();
                lineImage.color = new Color(0.95f, 0.95f, 0.9f, 0.98f);
                lineImage.raycastTarget = false;

                var slotGo = new GameObject("FractionSlot", typeof(RectTransform), typeof(Image), typeof(Text));
                slotGo.transform.SetParent(fractionGo.transform, false);
                var slotRect = slotGo.GetComponent<RectTransform>();
                slotRect.anchorMin = new Vector2(0.18f, 0.05f);
                slotRect.anchorMax = new Vector2(0.82f, 0.72f);
                slotRect.offsetMin = Vector2.zero;
                slotRect.offsetMax = Vector2.zero;
                var slotBg = slotGo.GetComponent<Image>();
                slotBg.sprite = SpriteFactory.RoundedCard;
                slotBg.type = Image.Type.Sliced;
                slotBg.color = new Color(0.16f, 0.2f, 0.3f, 0.92f);
                slotBg.raycastTarget = true;
                var slotHint = slotGo.GetComponent<Text>();
                slotHint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                slotHint.text = "?";
                slotHint.fontSize = 36;
                slotHint.fontStyle = FontStyle.Bold;
                slotHint.alignment = TextAnchor.MiddleCenter;
                slotHint.color = new Color(0.92f, 0.94f, 0.98f, 0.9f);
                slotHint.raycastTarget = false;

                widget._fractionGuideRoot = fractionGo;
                widget._fractionLineImage = lineImage;
                widget._fractionSlotGo = slotGo;
                widget._fractionSlotHint = slotHint;
                fractionGo.SetActive(false);
            }

            var layoutElement = root.AddComponent<LayoutElement>();
            layoutElement.minWidth = tileWidth;
            layoutElement.minHeight = tileHeight;
            layoutElement.preferredWidth = tileWidth;
            layoutElement.preferredHeight = tileHeight;

            widget.Bind(card, index, sideName, controller, canvas, dragRoot);
            widget._canvasGroup = root.GetComponent<CanvasGroup>();
            widget._canvasGroup.blocksRaycasts = true;
            widget._canvasGroup.interactable = true;

            if (sideName != "Hand" && controller != null && controller.UsesDragToMergePairs)
            {
                widget.snapDistance = DragToMergeSnapDistance;
            }

            var draggable = root.AddComponent<DraggableTile>();
            draggable.Bind(widget);
            draggable.snapDistance = widget.snapDistance;
            widget._draggable = draggable;

            if (sideName != "Hand")
            {
                var snapTarget = root.AddComponent<TileSnapTarget>();
                snapTarget.Bind(widget);
            }

            return widget;
        }

        public void ReactCombine() => _reaction?.PlayCombine();
        public void ReactCelebrate() => _reaction?.PlayCelebrate();
        public void ReactUndo() => _reaction?.PlayUndo();
    }
}
