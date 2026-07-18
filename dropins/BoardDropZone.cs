using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragonBoxAlgebra.UI
{
    public class BoardDropZone : MonoBehaviour, IDropHandler
    {
        public string SideName;

        public void OnDrop(PointerEventData eventData)
        {
            CardWidget dragged = eventData.pointerDrag?.GetComponent<CardWidget>();
            if (dragged == null || dragged.SideName != "Hand")
            {
                return;
            }

            var ui = FindObjectOfType<AlgebraUI>();
            if (ui?.Controller == null)
            {
                return;
            }

            AlgebraGameController controller = ui.Controller;

            if (controller.HasPendingBalance)
            {
                if (controller.PendingBalance.HoleSide == SideName
                    && controller.TryPlayFromHand(dragged.Index, SideName))
                {
                    dragged.MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            // 1) Merge onto nearest correct opposite (distance snap) — this is the intended drop.
            if (TryMergeNearestOpposite(controller, dragged, eventData))
            {
                return;
            }

            if (controller.UsesOppositeHandPlay)
            {
                if (controller.TryPlayHandOntoOppositeOnSide(dragged.Index, SideName))
                {
                    dragged.MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            // 2) Over a non-opposite board tile (or empty panel) → start / continue balance
            //    so a ? (+ small card image) appears on the other side.
            CardWidget under = FindAnyBoardCardUnderPointer(eventData, dragged);
            if (under != null
                && CombineRules.GetCombineAction(dragged.Card, under.Card)
                == CombineActionType.OppositeCancel)
            {
                // Aimed at an opposite — merge path above should have handled; don't side-dump.
                return;
            }

            if (controller.TryPlayFromHand(dragged.Index, SideName))
            {
                dragged.MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
            }
        }

        private static bool TryMergeNearestOpposite(AlgebraGameController controller, CardWidget dragged,
            PointerEventData eventData)
        {
            TileSnapTarget best = null;
            float bestDistance = Mathf.Max(160f, dragged.snapDistance);

            Vector2 screen = eventData.position;
            Camera cam = eventData.pressEventCamera;
            Vector3 dragPos = dragged.transform.position;

            foreach (TileSnapTarget target in Object.FindObjectsOfType<TileSnapTarget>())
            {
                if (target == null || !target.IsCorrectTile(dragged))
                {
                    continue;
                }

                float screenDist = Vector2.Distance(screen,
                    RectTransformUtility.WorldToScreenPoint(cam, target.GetSnapPosition()));
                float worldDist = Vector3.Distance(dragPos, target.GetSnapPosition()) * 100f;
                float distance = Mathf.Min(screenDist, worldDist);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
            }

            if (best?.Widget == null)
            {
                return false;
            }

            if (controller.TryPlayHandOntoOpposite(dragged.Index, best.Widget.SideName, best.Widget.Index))
            {
                dragged.MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                return true;
            }

            return false;
        }

        private static CardWidget FindAnyBoardCardUnderPointer(PointerEventData eventData, CardWidget dragged)
        {
            if (eventData == null || EventSystem.current == null)
            {
                return null;
            }

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            foreach (RaycastResult result in results)
            {
                if (result.gameObject == null)
                {
                    continue;
                }

                CardWidget widget = result.gameObject.GetComponent<CardWidget>()
                    ?? result.gameObject.GetComponentInParent<CardWidget>();
                if (widget != null && widget != dragged && widget.SideName != "Hand")
                {
                    return widget;
                }
            }

            return null;
        }
    }

    public class CardDropZone : MonoBehaviour
    {
    }
}
