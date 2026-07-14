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
