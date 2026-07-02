using UnityEngine;

namespace MarbleMazePan3D
{
    /// <summary>
    /// Smooth follow camera for the tilted maze board.
    /// </summary>
    public class MazeCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 9f, -7f);
        [SerializeField] private float followSmoothing = 6f;
        [SerializeField] private float lookAhead = 0.35f;

        private Vector3 _velocity;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + target.TransformDirection(offset);
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _velocity,
                1f / followSmoothing);

            Vector3 lookTarget = target.position + target.up * lookAhead;
            transform.LookAt(lookTarget);
        }
    }
}
