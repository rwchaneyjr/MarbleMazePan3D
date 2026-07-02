using UnityEngine;

namespace MarbleMazePan3D
{
    /// <summary>
    /// Triggers when the marble rolls into a hole on the maze board.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HoleTrigger : MonoBehaviour
    {
        [SerializeField] private string marbleTag = "Marble";

        private void Reset()
        {
            var collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(marbleTag))
            {
                return;
            }

            if (other.TryGetComponent(out MarbleController marble))
            {
                marble.RequestRespawn();
            }
        }
    }
}
