using UnityEngine;

namespace MarbleMaze
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallController : MonoBehaviour
    {
        Rigidbody body;
        float playLimit;
        Vector3 startPosition;
        bool movementEnabled = true;

        public void Initialize(Vector3 start, float limit)
        {
            startPosition = start;
            playLimit = limit;
            ResetPosition();
        }

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        void FixedUpdate()
        {
            if (!movementEnabled)
            {
                return;
            }

            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            var delta = input * (MazeLayout.MoveSpeed * Time.fixedDeltaTime);
            var target = body.position + delta;
            target.x = Mathf.Clamp(target.x, -playLimit, playLimit);
            target.z = Mathf.Clamp(target.z, -playLimit, playLimit);
            target.y = startPosition.y;

            body.MovePosition(target);
        }

        public void ResetPosition()
        {
            movementEnabled = true;
            body.position = startPosition;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;
            if (!enabled)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }
    }
}
