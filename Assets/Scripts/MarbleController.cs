using System.Collections;
using UnityEngine;

namespace MarbleMazePan3D
{
    /// <summary>
    /// Keeps the marble inside the maze and respawns it when it falls off or into a hole.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MarbleController : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float fallResetY = -2f;
        [SerializeField] private float respawnDelay = 0.35f;
        [SerializeField] private float maxSpeed = 12f;

        private Rigidbody _rigidbody;
        private bool _respawning;

        public Transform SpawnPoint => spawnPoint;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            if (spawnPoint == null)
            {
                var spawn = new GameObject("MarbleSpawn");
                spawn.transform.SetParent(transform.parent);
                spawn.transform.position = transform.position;
                spawnPoint = spawn.transform;
            }
        }

        private void FixedUpdate()
        {
            if (_respawning)
            {
                return;
            }

            if (transform.position.y < fallResetY)
            {
                RequestRespawn();
                return;
            }

            if (_rigidbody.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            {
                _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * maxSpeed;
            }
        }

        public void RequestRespawn()
        {
            if (_respawning)
            {
                return;
            }

            StartCoroutine(RespawnRoutine());
        }

        public void SetSpawnPoint(Transform point)
        {
            spawnPoint = point;
        }

        private IEnumerator RespawnRoutine()
        {
            _respawning = true;
            _rigidbody.isKinematic = true;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            yield return new WaitForSeconds(respawnDelay);

            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            _rigidbody.isKinematic = false;
            _respawning = false;
        }
    }
}
