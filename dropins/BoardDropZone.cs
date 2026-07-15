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

            // Prefer merging onto the opposite under the pointer — never dump onto the side end.
            if (TryMergeOntoOppositeUnderPointer(controller, dragged, eventData))
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

            if (controller.TryPlayFromHand(dragged.Index, SideName))
            {
                dragged.MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
            }
        }

        private static bool TryMergeOntoOppositeUnderPointer(AlgebraGameController controller,
            CardWidget dragged, PointerEventData eventData)
        {
            CardWidget opposite = FindOppositeUnderPointer(controller, dragged, eventData);
            if (opposite == null)
            {
                return false;
            }

            if (controller.TryPlayHandOntoOpposite(dragged.Index, opposite.SideName, opposite.Index))
            {
                dragged.MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                return true;
            }

            return false;
        }

        private static CardWidget FindOppositeUnderPointer(AlgebraGameController controller,
            CardWidget dragged, PointerEventData eventData)
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
                if (widget == null || widget == dragged || widget.SideName == "Hand")
                {
                    continue;
                }

                if (CombineRules.GetCombineAction(dragged.Card, widget.Card)
                    != CombineActionType.OppositeCancel)
                {
                    continue;
                }

                if (!controller.CanPlayHandOntoBoardCard(dragged.Index, widget.SideName, widget.Index))
                {
                    continue;
                }

                return widget;
            }

            return null;
        }
    }

    public class CardDropZone : MonoBehaviour
    {
    }
}
