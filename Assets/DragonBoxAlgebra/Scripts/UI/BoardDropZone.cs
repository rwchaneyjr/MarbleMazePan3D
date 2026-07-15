using System.Collections.Generic;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragonBoxAlgebra.UI
{
    public class BoardDropZone : MonoBehaviour, IDropHandler
    {
        public string SideName;

        private const float OppositeSnapRadius = 160f;

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

            // 1) Merge onto nearest correct opposite (distance snap) — intended drop.
            if (TryMergeNearestOpposite(controller, dragged, eventData))
            {
                return;
            }

            // 2) If the pointer is over ANY board tile, do nothing.
            //    Falling through to TryPlayFromHand is what "pops the tile to the side".
            if (FindAnyBoardCardUnderPointer(eventData, dragged) != null)
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

            // 3) Empty panel only → start balance play.
            if (controller.TryPlayFromHand(dragged.Index, SideName))
            {
                dragged.MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
            }
        }

        private static bool TryMergeNearestOpposite(AlgebraGameController controller, CardWidget dragged,
            PointerEventData eventData)
        {
            CardWidget best = null;
            float bestDistance = OppositeSnapRadius;
            Camera cam = eventData != null ? eventData.pressEventCamera : null;
            Vector2 screen = eventData != null ? eventData.position : Vector2.zero;

            foreach (CardWidget other in Object.FindObjectsOfType<CardWidget>())
            {
                if (other == null || other == dragged || other.SideName == "Hand")
                {
                    continue;
                }

                if (controller.IsCardPendingCancelOnSide(other.Card.Id, other.SideName))
                {
                    continue;
                }

                if (CombineRules.GetCombineAction(dragged.Card, other.Card) != CombineActionType.OppositeCancel)
                {
                    continue;
                }

                float screenDist = Vector2.Distance(screen,
                    RectTransformUtility.WorldToScreenPoint(cam, other.transform.position));
                if (screenDist < bestDistance)
                {
                    bestDistance = screenDist;
                    best = other;
                }
            }

            if (best == null)
            {
                return false;
            }

            if (controller.TryPlayHandOntoOpposite(dragged.Index, best.SideName, best.Index))
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

            var results = new List<RaycastResult>();
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
