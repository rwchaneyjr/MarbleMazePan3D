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
        [Tooltip("Max screen-pixel distance to snap onto a correct opposite. Farther = snap back.")]
        public float snapDistance = 140f;

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

            TileSnapTarget[] targets = FindObjectsOfType<TileSnapTarget>();
            foreach (TileSnapTarget target in targets)
            {
                if (target == null || _owner == null || !target.IsCorrectTile(_owner))
                {
                    continue;
                }

                // Screen pixels only so a far tile past the red box cannot look "close".
                Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(eventCamera, target.GetSnapPosition());
                float distance = Vector2.Distance(screenPosition, targetScreen);
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

                if (_startingParent is RectTransform parentRect)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
                }
            }
            else
            {
                transform.position = _startingPosition;
            }
        }

        public void ClearSnappedFlag() => _snapped = false;
    }
}
