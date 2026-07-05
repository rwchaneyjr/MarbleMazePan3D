using System.Collections;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class AsteriskCancelWidget : MonoBehaviour, IPointerClickHandler
    {
        private AlgebraGameController _controller;
        private int _markerIndex;
        private RectTransform _symbolRect;

        public void Initialize(AlgebraGameController controller, int markerIndex,
            float tileWidth = 110f, float tileHeight = 120f)
        {
            _controller = controller;
            _markerIndex = markerIndex;
            Build(tileWidth, tileHeight);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
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

            var symbolGo = new GameObject("Asterisk", typeof(RectTransform), typeof(Text));
            symbolGo.transform.SetParent(transform, false);
            _symbolRect = symbolGo.GetComponent<RectTransform>();
            _symbolRect.anchorMin = Vector2.zero;
            _symbolRect.anchorMax = Vector2.one;
            _symbolRect.offsetMin = Vector2.zero;
            _symbolRect.offsetMax = Vector2.zero;

            var symbolText = symbolGo.GetComponent<Text>();
            symbolText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            symbolText.alignment = TextAnchor.MiddleCenter;
            symbolText.fontSize = 88;
            symbolText.fontStyle = FontStyle.Bold;
            symbolText.color = Color.black;
            symbolText.text = "*";
            symbolText.raycastTarget = false;

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
    }
}
