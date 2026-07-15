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
            rect.sizeDelta = new Vector2(140f, 140f);

            var image = go.GetComponent<Image>();
            image.sprite = SpriteFactory.RoundedCard;
            image.raycastTarget = false;
            image.color = new Color(0.2f, 0.95f, 0.35f, 0.9f);

            var textGo = new GameObject("Spiral", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.GetComponent<Text>();
            text.font = EmojiFont.Get() ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 56;
            text.text = "🌀";
            text.raycastTarget = false;
            text.color = new Color(0.08f, 0.45f, 0.18f);

            go.GetComponent<VortexEffect>().StartCoroutine(go.GetComponent<VortexEffect>().Animate());
        }

        private IEnumerator Animate()
        {
            Transform t = transform;
            Image image = GetComponent<Image>();
            Text spiral = GetComponentInChildren<Text>();
            float elapsed = 0f;
            const float duration = 0.55f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float n = elapsed / duration;
                t.localScale = Vector3.one * Mathf.Lerp(0.45f, 1.75f, n);
                t.Rotate(0f, 0f, 720f * Time.deltaTime);
                if (image != null)
                {
                    Color c = image.color;
                    c.a = Mathf.Lerp(0.9f, 0f, n);
                    image.color = c;
                }

                if (spiral != null)
                {
                    Color c = spiral.color;
                    c.a = Mathf.Lerp(1f, 0f, n);
                    spiral.color = c;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
