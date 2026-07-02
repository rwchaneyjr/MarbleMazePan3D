using TMPro;
using UnityEngine;

namespace MarbleMaze
{
    public class MarbleMazeGame : MonoBehaviour
    {
        [SerializeField] MazeBuilder mazeBuilder;
        [SerializeField] BallController ball;
        [SerializeField] GoalTrigger goal;
        [SerializeField] TextMeshProUGUI statusText;
        [SerializeField] Camera gameCamera;

        bool hasWon;

        void Start()
        {
            mazeBuilder.Build();
            ball.Initialize(mazeBuilder.StartPosition, mazeBuilder.PlayLimit);
            goal = FindObjectOfType<GoalTrigger>();
            SetupCamera();
            UpdateStatus(MazeLayout.HelpText);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }

            if (!hasWon && goal != null && goal.Contains(ball.transform.position))
            {
                hasWon = true;
                ball.SetMovementEnabled(false);
                UpdateStatus(MazeLayout.WinText);
            }
        }

        void Restart()
        {
            hasWon = false;
            ball.ResetPosition();
            UpdateStatus(MazeLayout.HelpText);
        }

        void SetupCamera()
        {
            if (gameCamera == null)
            {
                gameCamera = Camera.main;
            }

            if (gameCamera == null)
            {
                return;
            }

            var rows = MazeLayout.Layout.Length;
            var mazeHeight = rows * MazeLayout.CellSize;
            gameCamera.transform.position = new Vector3(0f, mazeHeight * 0.85f, -mazeHeight * 1.1f);
            gameCamera.transform.LookAt(Vector3.zero);
        }

        void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
    }
}
