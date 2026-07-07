using System.Collections;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class AsteriskCancelWidget : MonoBehaviour, IPointerClickHandler
    {
        private const float MergeDuration = 1.15f;
        private const float MergeHalfOffset = 28f;

        private AlgebraGameController _controller;
        private int _markerIndex;
        private RectTransform _symbolRect;
        private CanvasGroup _symbolGroup;
        private bool _readyToClick;

        public void Initialize(AlgebraGameController controller, int markerIndex,
            float tileWidth = 110f, float tileHeight = 120f)
        {
            _controller = controller;
            _markerIndex = markerIndex;
            _readyToClick = true;
            Build(tileWidth, tileHeight);
            _symbolGroup.alpha = 1f;
            _symbolRect.localScale = Vector3.one;
            StartCoroutine(SpinSymbol());
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
            image.color = new Color(0.98f, 0.84f, 0.14f, 0.92f);
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

            var symbolGo = new GameObject("Asterisk", typeof(RectTransform), typeof(Text), typeof(CanvasGroup));
            symbolGo.transform.SetParent(transform, false);
            _symbolRect = symbolGo.GetComponent<RectTransform>();
            _symbolRect.anchorMin = Vector2.zero;
            _symbolRect.anchorMax = Vector2.one;
            _symbolRect.offsetMin = Vector2.zero;
            _symbolRect.offsetMax = Vector2.zero;
            _symbolGroup = symbolGo.GetComponent<CanvasGroup>();
            _symbolGroup.alpha = 0f;

            var symbolText = symbolGo.GetComponent<Text>();
            symbolText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            symbolText.alignment = TextAnchor.MiddleCenter;
            symbolText.fontSize = 88;
            symbolText.fontStyle = FontStyle.Bold;
            symbolText.color = Color.black;
            symbolText.text = "*";
            symbolText.raycastTarget = false;
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
                    _symbolGroup.alpha = Mathf.Lerp(0f, 1f, t);
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
            StartCoroutine(SpinSymbol());
        }

        private IEnumerator SpinSymbol()
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
