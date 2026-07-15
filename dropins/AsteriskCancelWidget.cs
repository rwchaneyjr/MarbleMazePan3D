using System.Collections;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class AsteriskCancelWidget : MonoBehaviour, IPointerClickHandler, IDropHandler
    {
        private const float MergeDuration = 1.15f;
        private const float MergeHalfOffset = 28f;
        private const float SwirlClickableAlpha = 0.25f;
        private const string SwirlingLightResourcePath = "CreatureSprites/SwirlingLight";

        private AlgebraGameController _controller;
        private int _markerIndex;
        private RectTransform _symbolRect;
        private CanvasGroup _symbolGroup;
        private Image _symbolImage;
        private Text _symbolFallbackText;
        private bool _readyToClick;
        private bool _mergeInProgress;
        private Coroutine _symbolAnimation;

        private static Sprite _swirlingLightSprite;

        private void OnDestroy()
        {
            if (_mergeInProgress && _controller != null)
            {
                _controller.NotifyMergeAnimationCompleted();
                _mergeInProgress = false;
            }
        }

        public void Initialize(AlgebraGameController controller, int markerIndex,
            float tileWidth = 110f, float tileHeight = 120f)
        {
            _controller = controller;
            _markerIndex = markerIndex;
            _readyToClick = true;
            Build(tileWidth, tileHeight);
            _symbolGroup.alpha = 1f;
            _symbolRect.localScale = Vector3.one;
            StartSymbolAnimation();
        }

        public void InitializeMergePair(AlgebraGameController controller, int markerIndex,
            BoardCard cardA, BoardCard cardB, float tileWidth = 110f, float tileHeight = 120f)
        {
            _controller = controller;
            _markerIndex = markerIndex;
            _readyToClick = false;
            Build(tileWidth, tileHeight);
            BoardCard lightCard = CardFlipRules.IsLight(cardA) ? cardA : cardB;
            BoardCard darkCard = CardFlipRules.IsDark(cardA) ? cardA : cardB;
            BuildMergePair(lightCard, darkCard);
            StartCoroutine(PlayMergeAnimation());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_readyToClick)
            {
                return;
            }

            if (_controller.TryDismissCancelMarker(_markerIndex))
            {
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCombine();
            }
        }

        /// <summary>
        /// Swirls must not eat hand drops — forward onto the side drop zone so the other
        /// (and same) side stays playable while asterisks spin.
        /// </summary>
        public void OnDrop(PointerEventData eventData)
        {
            BoardDropZone zone = GetComponentInParent<BoardDropZone>();
            zone?.OnDrop(eventData);
        }

        private static Sprite GetSwirlingLightSprite()
        {
            if (_swirlingLightSprite == null)
            {
                _swirlingLightSprite = Resources.Load<Sprite>(SwirlingLightResourcePath);
            }

            return _swirlingLightSprite;
        }

        private static bool UsesSwirlingLight => GetSwirlingLightSprite() != null;

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
            image.color = UsesSwirlingLight
                ? new Color(0.04f, 0.05f, 0.14f, 0.94f)
                : new Color(0.98f, 0.84f, 0.14f, 0.92f);
            // Keep the tile visible but let drops pass to the side / other cards.
            // Clicks still hit the swirl symbol below.
            image.raycastTarget = false;

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
            borderImage.color = UsesSwirlingLight
                ? new Color(0.35f, 0.55f, 1f, 0.95f)
                : new Color(0.72f, 0.48f, 0.04f, 1f);

            var symbolGo = new GameObject("SwirlSymbol", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            symbolGo.transform.SetParent(transform, false);
            _symbolRect = symbolGo.GetComponent<RectTransform>();
            _symbolRect.anchorMin = Vector2.zero;
            _symbolRect.anchorMax = Vector2.one;
            _symbolRect.offsetMin = UsesSwirlingLight ? new Vector2(6f, 6f) : Vector2.zero;
            _symbolRect.offsetMax = UsesSwirlingLight ? new Vector2(-6f, -6f) : Vector2.zero;
            _symbolGroup = symbolGo.GetComponent<CanvasGroup>();
            _symbolGroup.alpha = 0f;
            var symbolHit = symbolGo.GetComponent<Image>();
            symbolHit.color = new Color(1f, 1f, 1f, 0.01f);
            symbolHit.raycastTarget = true;

            Sprite swirl = GetSwirlingLightSprite();
            if (swirl != null)
            {
                var imageGo = new GameObject("SwirlingLight", typeof(RectTransform), typeof(Image));
                imageGo.transform.SetParent(symbolGo.transform, false);
                var imageRect = imageGo.GetComponent<RectTransform>();
                imageRect.anchorMin = Vector2.zero;
                imageRect.anchorMax = Vector2.one;
                imageRect.offsetMin = Vector2.zero;
                imageRect.offsetMax = Vector2.zero;
                _symbolImage = imageGo.GetComponent<Image>();
                _symbolImage.sprite = swirl;
                _symbolImage.preserveAspect = true;
                _symbolImage.raycastTarget = false;
                _symbolImage.color = Color.white;
            }
            else
            {
                var textGo = new GameObject("Asterisk", typeof(RectTransform), typeof(Text));
                textGo.transform.SetParent(symbolGo.transform, false);
                var textRect = textGo.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                _symbolFallbackText = textGo.GetComponent<Text>();
                _symbolFallbackText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                _symbolFallbackText.alignment = TextAnchor.MiddleCenter;
                _symbolFallbackText.fontSize = 88;
                _symbolFallbackText.fontStyle = FontStyle.Bold;
                _symbolFallbackText.color = Color.black;
                _symbolFallbackText.text = "*";
                _symbolFallbackText.raycastTarget = false;
            }
        }

        private void BuildMergePair(BoardCard lightCard, BoardCard darkCard)
        {
            Image lightHalf = CreateMergeHalf(transform, "LightHalf", lightCard, true);
            Image darkHalf = CreateMergeHalf(transform, "DarkHalf", darkCard, false);
            lightHalf.rectTransform.anchoredPosition = new Vector2(-MergeHalfOffset, 0f);
            darkHalf.rectTransform.anchoredPosition = new Vector2(MergeHalfOffset, 0f);
        }

        private static Image CreateMergeHalf(Transform parent, string name, BoardCard card, bool light)
        {
            var halfGo = new GameObject(name, typeof(RectTransform), typeof(Image));
            halfGo.transform.SetParent(parent, false);
            var halfRect = halfGo.GetComponent<RectTransform>();
            halfRect.anchorMin = new Vector2(0.5f, 0.5f);
            halfRect.anchorMax = new Vector2(0.5f, 0.5f);
            halfRect.pivot = new Vector2(0.5f, 0.5f);
            halfRect.sizeDelta = new Vector2(54f, 96f);

            var bg = halfGo.GetComponent<Image>();
            bg.sprite = SpriteFactory.RoundedCard;
            bg.type = Image.Type.Sliced;
            bg.color = light
                ? new Color(0.98f, 0.84f, 0.14f, 1f)
                : new Color(0.08f, 0.08f, 0.12f, 1f);
            bg.raycastTarget = false;

            var spriteGo = new GameObject("Creature", typeof(RectTransform), typeof(Image));
            spriteGo.transform.SetParent(halfGo.transform, false);
            var spriteRect = spriteGo.GetComponent<RectTransform>();
            spriteRect.anchorMin = Vector2.zero;
            spriteRect.anchorMax = Vector2.one;
            spriteRect.offsetMin = new Vector2(4f, 4f);
            spriteRect.offsetMax = new Vector2(-4f, -4f);
            var spriteImage = spriteGo.GetComponent<Image>();
            spriteImage.sprite = CardVisuals.IconSprite(card);
            spriteImage.preserveAspect = true;
            spriteImage.raycastTarget = false;

            return bg;
        }

        private IEnumerator PlayMergeAnimation()
        {
            _mergeInProgress = true;
            _controller?.NotifyMergeAnimationStarted();

            Image lightHalf = transform.Find("LightHalf")?.GetComponent<Image>();
            Image darkHalf = transform.Find("DarkHalf")?.GetComponent<Image>();
            RectTransform lightRect = lightHalf?.rectTransform;
            RectTransform darkRect = darkHalf?.rectTransform;

            Vector2 lightStart = lightRect != null ? lightRect.anchoredPosition : new Vector2(-MergeHalfOffset, 0f);
            Vector2 darkStart = darkRect != null ? darkRect.anchoredPosition : new Vector2(MergeHalfOffset, 0f);
            Color lightStartColor = lightHalf != null ? lightHalf.color : Color.white;
            Color darkStartColor = darkHalf != null ? darkHalf.color : Color.white;

            float elapsed = 0f;
            while (elapsed < MergeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / MergeDuration));

                if (lightRect != null)
                {
                    lightRect.anchoredPosition = Vector2.Lerp(lightStart, Vector2.zero, t);
                    lightRect.localScale = Vector3.one * Mathf.Lerp(1f, 0.35f, t);
                }

                if (darkRect != null)
                {
                    darkRect.anchoredPosition = Vector2.Lerp(darkStart, Vector2.zero, t);
                    darkRect.localScale = Vector3.one * Mathf.Lerp(1f, 0.35f, t);
                }

                if (lightHalf != null)
                {
                    lightHalf.color = new Color(lightStartColor.r, lightStartColor.g, lightStartColor.b,
                        Mathf.Lerp(1f, 0f, t));
                }

                if (darkHalf != null)
                {
                    darkHalf.color = new Color(darkStartColor.r, darkStartColor.g, darkStartColor.b,
                        Mathf.Lerp(1f, 0f, t));
                }

                if (_symbolGroup != null)
                {
                    float symbolAlpha = Mathf.Lerp(0f, 1f, t);
                    _symbolGroup.alpha = symbolAlpha;
                    if (!_readyToClick && symbolAlpha >= SwirlClickableAlpha)
                    {
                        _readyToClick = true;
                    }
                }

                if (_symbolRect != null)
                {
                    _symbolRect.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, t);
                }

                yield return null;
            }

            if (lightHalf != null)
            {
                lightHalf.gameObject.SetActive(false);
            }

            if (darkHalf != null)
            {
                darkHalf.gameObject.SetActive(false);
            }

            if (_symbolGroup != null)
            {
                _symbolGroup.alpha = 1f;
            }

            _readyToClick = true;
            _mergeInProgress = false;
            _controller?.NotifyMergeAnimationCompleted();
            StartSymbolAnimation();
        }

        private void StartSymbolAnimation()
        {
            if (_symbolAnimation != null)
            {
                StopCoroutine(_symbolAnimation);
            }

            _symbolAnimation = StartCoroutine(UsesSwirlingLight ? AnimateSwirlingLight() : SpinAsteriskFallback());
        }

        private IEnumerator AnimateSwirlingLight()
        {
            float pulsePhase = 0f;
            while (_symbolRect != null)
            {
                _symbolRect.Rotate(0f, 0f, -165f * Time.deltaTime);

                pulsePhase += Time.deltaTime * 2.8f;
                float pulse = 0.5f + 0.5f * Mathf.Sin(pulsePhase);
                float scale = Mathf.Lerp(0.9f, 1.1f, pulse);
                _symbolRect.localScale = Vector3.one * scale;

                if (_symbolGroup != null)
                {
                    _symbolGroup.alpha = Mathf.Lerp(0.78f, 1f, pulse);
                }

                if (_symbolImage != null)
                {
                    float glow = Mathf.Lerp(0.88f, 1.18f, pulse);
                    _symbolImage.color = new Color(glow, glow, glow * 1.08f, 1f);
                }

                yield return null;
            }
        }

        private IEnumerator SpinAsteriskFallback()
        {
            while (_symbolRect != null)
            {
                _symbolRect.Rotate(0f, 0f, 220f * Time.deltaTime);
                yield return null;
            }
        }

        public static AsteriskCancelWidget Create(Transform parent, AlgebraGameController controller, int markerIndex,
            float tileWidth = 110f, float tileHeight = 120f)
        {
            var go = new GameObject($"CancelMarker_{markerIndex}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var widget = go.AddComponent<AsteriskCancelWidget>();
            widget.Initialize(controller, markerIndex, tileWidth, tileHeight);
            return widget;
        }

        public static AsteriskCancelWidget CreateMergePair(Transform parent, AlgebraGameController controller,
            int markerIndex, BoardCard cardA, BoardCard cardB, float tileWidth = 110f, float tileHeight = 120f)
        {
            var go = new GameObject($"CancelMarker_{markerIndex}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var widget = go.AddComponent<AsteriskCancelWidget>();
            widget.InitializeMergePair(controller, markerIndex, cardA, cardB, tileWidth, tileHeight);
            return widget;
        }
    }
}
