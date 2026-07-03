using System.Collections;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    public class CreatureReaction : MonoBehaviour
    {
        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private Coroutine _spinLoop;

        public void StartOppositeSpin()
        {
            StopOppositeSpin();
            _spinLoop = StartCoroutine(SpinLoop());
        }

        public void StopOppositeSpin()
        {
            if (_spinLoop != null)
            {
                StopCoroutine(_spinLoop);
                _spinLoop = null;
            }

            transform.localRotation = Quaternion.identity;
            transform.localScale = _baseScale;
        }

        private IEnumerator SpinLoop()
        {
            while (true)
            {
                transform.Rotate(0f, 0f, 220f * Time.deltaTime);
                yield return null;
            }
        public void PlayCombine()
        {
            StopOppositeSpin();
            StopAllCoroutines();
            StartCoroutine(Punch(1.25f, 0.18f));
        }

        public void PlayCelebrate()
        {
            StopAllCoroutines();
            StartCoroutine(Punch(1.4f, 0.28f));
        }

        public void PlayUndo()
        {
            StopAllCoroutines();
            StartCoroutine(Wobble());
        }

        private IEnumerator Punch(float peak, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float n = elapsed / duration;
                float scale = n < 0.5f
                    ? Mathf.Lerp(1f, peak, n * 2f)
                    : Mathf.Lerp(peak, 1f, (n - 0.5f) * 2f);
                transform.localScale = _baseScale * scale;
                yield return null;
            }

            transform.localScale = _baseScale;
        }

        private IEnumerator Wobble()
        {
            float elapsed = 0f;
            const float duration = 0.25f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float angle = Mathf.Sin(elapsed * 40f) * 8f * (1f - elapsed / duration);
                transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                yield return null;
            }

            transform.localRotation = Quaternion.identity;
        }
    }
}
