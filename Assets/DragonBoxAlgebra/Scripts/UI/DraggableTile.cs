using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    /// <summary>
    /// Drag + snap helper (UI version of world-space DraggableTile).
    /// Finds the closest correct <see cref="TileSnapTarget"/> within snapDistance and locks onto it.
    /// </summary>
    public class DraggableTile : MonoBehaviour
    {
        [Header("Snapping")]
        [Tooltip("Max screen-pixel distance to snap onto a correct opposite.")]
        public float snapDistance = 220f;

        private CardWidget _owner;
        private Vector3 _startingPosition;
        private Transform _startingParent;
        private int _startingSiblingIndex;
        private bool _snapped;

        public bool IsSnapped => _snapped;
        public CardWidget Owner => _owner;

        public void Bind(CardWidget owner)
        {
            _owner = owner;
        }

        public void RememberStart()
        {
            _startingPosition = transform.position;
            _startingParent = transform.parent;
            _startingSiblingIndex = transform.GetSiblingIndex();
            _snapped = false;
        }

        /// <summary>
        /// Highlight the closest valid snap target under the pointer (does not move the tile).
        /// </summary>
        public TileSnapTarget FindClosestTarget(Vector2 screenPosition, Camera eventCamera)
        {
            TileSnapTarget closestTarget = null;
            float closestDistance = snapDistance;

            Vector3 ownerPos = transform.position;
            TileSnapTarget[] targets = FindObjectsOfType<TileSnapTarget>();
            foreach (TileSnapTarget target in targets)
            {
                if (target == null || _owner == null || !target.IsCorrectTile(_owner))
                {
                    continue;
                }

                Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(eventCamera, target.GetSnapPosition());
                float screenDist = Vector2.Distance(screenPosition, targetScreen);
                // Also allow center-to-center proximity so edge drops still count as "on" the tile.
                float worldDist = Vector3.Distance(ownerPos, target.GetSnapPosition()) * 100f;
                float distance = Mathf.Min(screenDist, worldDist);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }

            return closestTarget;
        }

        /// <summary>
        /// Same idea as the world DraggableTile.TryToSnap.
        /// Returns the target when snapped; null when nothing was in range.
        /// </summary>
        public TileSnapTarget TryToSnap(Vector2 screenPosition, Camera eventCamera)
        {
            if (_snapped)
            {
                return null;
            }

            TileSnapTarget closestTarget = FindClosestTarget(screenPosition, eventCamera);
            if (closestTarget != null)
            {
                SnapToTarget(closestTarget);
                return closestTarget;
            }

            return null;
        }

        public void SnapToTarget(TileSnapTarget target)
        {
            if (target == null)
            {
                return;
            }

            // Only move in place — never reparent under the target.
            // Nesting under another card breaks layout and can leave a ghost tile
            // covering the board so later drags never receive pointer events.
            // It also destroys hand→opposite swirls when BoardChanged rebuilds the target.
            transform.position = target.GetSnapPosition();
            _snapped = true;

            if (target.Widget != null)
            {
                target.Widget.SetHighlight(true);
            }
        }

        public void ReturnToStart()
        {
            _snapped = false;

            if (this == null || gameObject == null)
            {
                return;
            }

            // Unity fake-null: parent may have been destroyed by a hand refresh mid-drop.
            if (_startingParent != null)
            {
                transform.SetParent(_startingParent, false);
                transform.SetSiblingIndex(_startingSiblingIndex);
                if (transform is RectTransform rect)
                {
                    rect.anchoredPosition = Vector2.zero;
                    rect.localRotation = Quaternion.identity;
                    rect.localScale = Vector3.one;
                }

                if (_startingParent is RectTransform parentRect && parentRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
                }
            }
            else if (transform is RectTransform)
            {
                // Parent gone — at least clear drag offset so the tile is not left mid-screen.
                transform.localPosition = Vector3.zero;
            }
            else
            {
                transform.position = _startingPosition;
            }
        }

        public void ClearSnappedFlag() => _snapped = false;
    }
}
