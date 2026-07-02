using UnityEngine;
using UnityEngine.Events;

namespace MarbleMazePan3D
{
    /// <summary>
    /// Win zone at the end of the maze.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GoalTrigger : MonoBehaviour
    {
        [SerializeField] private string marbleTag = "Marble";
        [SerializeField] private UnityEvent onGoalReached;

        private bool _completed;

        private void Reset()
        {
            var collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_completed || !other.CompareTag(marbleTag))
            {
                return;
            }

            _completed = true;
            onGoalReached?.Invoke();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel();
            }
        }

        public void ResetGoal()
        {
            _completed = false;
        }
    }
}
