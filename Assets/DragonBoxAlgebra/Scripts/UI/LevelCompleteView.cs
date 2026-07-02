using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class LevelCompleteView : MonoBehaviour
    {
        private GameObject _panel;
        private Text _starsText;
        private AlgebraGameController _controller;

        public void Initialize(AlgebraGameController controller, GameObject panel, Text starsText)
        {
            _controller = controller;
            _panel = panel;
            _starsText = starsText;
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
            _starsText.text =
                $"The box is alone!\nRight number of moves: {(moves <= _controller.CurrentLevel.ParMoves ? "✓" : "✗")}\n" +
                $"Right number of cards: {(_controller.Moves.CardsPlayed <= _controller.CurrentLevel.ParCards ? "✓" : "✗")}\n" +
                $"Stars: {new string('★', stars)}{new string('☆', 3 - stars)}\nMoves: {moves}";
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }
    }
}
