using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragonBoxAlgebra.UI
{
    /// <summary>
    /// Legacy shared bar — forwards to per-side denominator placement (Left first).
    /// Prefer <see cref="DenominatorDropZone"/> under each board panel.
    /// </summary>
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
