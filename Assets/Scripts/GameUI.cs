using UnityEngine;
using UnityEngine.UI;

namespace MarbleMazePan3D
{
    /// <summary>
    /// Minimal HUD for timer, win message, and reset button.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Text timerText;
        [SerializeField] private Text statusText;
        [SerializeField] private GameObject winPanel;

        private void Update()
        {
            if (gameManager == null || timerText == null)
            {
                return;
            }

            timerText.text = $"Time: {gameManager.ElapsedSeconds:0.0}s";
        }

        public void ShowWin()
        {
            if (statusText != null)
            {
                statusText.text = "You reached the goal!";
            }

            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }
        }

        public void OnResetClicked()
        {
            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }

            if (statusText != null)
            {
                statusText.text = "Tilt the board to guide the marble.";
            }

            gameManager?.ResetLevel();
        }
    }
}
