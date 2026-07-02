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

        public void Initialize(AlgebraGameController controller, GameObject panel, Text starsText, Text creatureText = null)
        {
            _controller = controller;
            _panel = panel;
            _starsText = starsText;
            _creatureText = creatureText;
            _controller.LevelCompleted += Show;
            _panel.SetActive(false);
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
            _panel.SetActive(true);
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
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }
    }
}
