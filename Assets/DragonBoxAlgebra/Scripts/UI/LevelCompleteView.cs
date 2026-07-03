using System.Collections;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class LevelCompleteView : MonoBehaviour
    {
        private GameObject _panel;
        private Text _starsText;
        private Text _creatureText;
        private AlgebraGameController _controller;
        private CanvasGroup _canvasGroup;
        private Coroutine _showCoroutine;

        public void Initialize(AlgebraGameController controller, GameObject panel, Text starsText, Text creatureText = null)
        {
            _controller = controller;
            _panel = panel;
            _starsText = starsText;
            _creatureText = creatureText;
            _controller.LevelCompleted += Show;
            _panel.SetActive(false);

            _canvasGroup = _panel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = _panel.AddComponent<CanvasGroup>();
            }
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.LevelCompleted -= Show;
            }
        }

        private void Show(int stars, int moves)
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
            }

            _showCoroutine = StartCoroutine(ShowAnimated(stars, moves));
        }

        private IEnumerator ShowAnimated(int stars, int moves)
        {
            bool goodMoves = moves <= _controller.CurrentLevel.ParMoves;
            bool goodCards = _controller.Moves.CardsPlayed <= _controller.CurrentLevel.ParCards;

            _starsText.text =
                $"The box is alone! {(goodMoves ? "✓" : "✗")}\n" +
                $"Right number of moves {(goodMoves ? "✓" : "✗")}\n" +
                $"Right number of cards {(goodCards ? "✓" : "✗")}\n\n" +
                $"{new string('★', stars)}{new string('☆', 3 - stars)}\n" +
                $"Moves: {moves}";

            if (_creatureText != null)
            {
                _creatureText.text = stars >= 2 ? "🐲 ✨" : "🐲";
            }

            _panel.SetActive(true);
            _canvasGroup.alpha = 0f;

            const float duration = 0.45f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _showCoroutine = null;
        }

        public void Hide()
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
                _showCoroutine = null;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }

            _panel.SetActive(false);
        }
    }
}
