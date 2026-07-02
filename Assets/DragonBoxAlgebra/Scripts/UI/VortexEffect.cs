using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class VortexEffect : MonoBehaviour
    {
        public static void Play(Transform parent, Vector3 worldPosition)
        {
            var go = new GameObject("Vortex", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VortexEffect));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.position = worldPosition;
            rect.sizeDelta = new Vector2(120f, 120f);

            var image = go.GetComponent<Image>();
            image.sprite = SpriteFactory.RoundedCard;
            image.color = new Color(0.2f, 0.95f, 0.35f, 0.85f);

            var textGo = new GameObject("Spiral", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 48;
            text.text = "🌀";
            text.color = new Color(0.1f, 0.55f, 0.2f);

            go.GetComponent<VortexEffect>().StartCoroutine(go.GetComponent<VortexEffect>().Animate());
        }

        private IEnumerator Animate()
        {
            Transform t = transform;
            Image image = GetComponent<Image>();
            float elapsed = 0f;
            const float duration = 0.55f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float n = elapsed / duration;
                t.localScale = Vector3.one * Mathf.Lerp(0.4f, 1.6f, n);
                t.Rotate(0f, 0f, 720f * Time.deltaTime);
                if (image != null)
                {
                    Color c = image.color;
                    c.a = Mathf.Lerp(0.85f, 0f, n);
                    image.color = c;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
