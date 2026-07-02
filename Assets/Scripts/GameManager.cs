using UnityEngine;
using UnityEngine.Events;

namespace MarbleMazePan3D
{
    /// <summary>
    /// Tracks level state and exposes events for UI/audio.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private MazeTiltController mazeTilt;
        [SerializeField] private MarbleController marble;
        [SerializeField] private GoalTrigger goal;

        [Header("Events")]
        [SerializeField] private UnityEvent onLevelComplete;
        [SerializeField] private UnityEvent onLevelReset;

        private float _startTime;
        private bool _running = true;

        public float ElapsedSeconds => _running ? Time.time - _startTime : 0f;

        public void Configure(MazeTiltController tilt, MarbleController marbleController, GoalTrigger goalTrigger = null)
        {
            mazeTilt = tilt;
            marble = marbleController;
            goal = goalTrigger;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _startTime = Time.time;
        }

        public void CompleteLevel()
        {
            if (!_running)
            {
                return;
            }

            _running = false;
            onLevelComplete?.Invoke();
        }

        public void ResetLevel()
        {
            _running = true;
            _startTime = Time.time;

            mazeTilt?.ResetTilt();
            marble?.RequestRespawn();
            goal?.ResetGoal();

            onLevelReset?.Invoke();
        }
    }
}
