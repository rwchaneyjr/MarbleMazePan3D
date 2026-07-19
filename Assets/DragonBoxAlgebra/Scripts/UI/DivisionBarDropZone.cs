using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragonBoxAlgebra.UI
{
    /// <summary>Drop a hand number here to divide both sides (multiply chapters).</summary>
    public class DivisionBarDropZone : MonoBehaviour, IDropHandler
    {
        private AlgebraGameController _controller;

        public void Initialize(AlgebraGameController controller)
        {
            _controller = controller;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_controller == null || !_controller.UsesMultiplyAdditionLevels)
            {
                return;
            }

            CardWidget dragged = eventData.pointerDrag?.GetComponent<CardWidget>();
            if (dragged == null || dragged.SideName != "Hand")
            {
                return;
            }

            if (_controller.TryDivideBothSidesFromHand(dragged.Index))
            {
                dragged.MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
            }
        }
    }
}
