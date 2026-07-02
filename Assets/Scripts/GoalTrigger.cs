using UnityEngine;

namespace MarbleMaze
{
    public class GoalTrigger : MonoBehaviour
    {
        float triggerRadius;

        public void Initialize(float radius)
        {
            triggerRadius = radius;
            var collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        public bool Contains(Vector3 position)
        {
            var flat = position - transform.position;
            flat.y = 0f;
            return flat.magnitude < triggerRadius;
        }
    }
}
