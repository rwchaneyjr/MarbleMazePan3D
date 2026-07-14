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

            if (ui.Controller.HasPendingBalance)
            {
                if (ui.Controller.PendingBalance.HoleSide == SideName
                    && ui.Controller.TryPlayFromHand(dragged.Index, SideName))
                {
                    dragged.MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            if (ui.Controller.UsesOppositeHandPlay)
            {
                if (ui.Controller.TryPlayHandOntoOppositeOnSide(dragged.Index, SideName))
                {
                    dragged.MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            if (ui.Controller.TryPlayFromHand(dragged.Index, SideName))
            {
                dragged.MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
            }
        }
    }

    public class CardDropZone : MonoBehaviour
    {
    }
}
