using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    /// <summary>
    /// Snap point on a board tile — only the correct opposite hand/board tile may snap here.
    /// Mirrors TileSnapTarget for the algebra UI.
    /// </summary>
    public class CardSnapTarget : MonoBehaviour
    {
        public CardWidget Widget { get; private set; }

        public void Bind(CardWidget widget)
        {
            Widget = widget;
        }

        public bool IsCorrectTile(CardWidget dragged)
        {
            if (dragged == null || Widget == null || dragged == Widget || Widget.SideName == "Hand")
            {
                return false;
            }

            if (CombineRules.GetCombineAction(dragged.Card, Widget.Card) != CombineActionType.OppositeCancel)
            {
                return false;
            }

            if (dragged.SideName == "Hand")
            {
                AlgebraGameController controller = dragged.Controller;
                return controller != null
                    && controller.CanPlayHandOntoBoardCard(dragged.Index, Widget.SideName, Widget.Index);
            }

            return dragged.SideName == Widget.SideName;
        }

        public Vector3 GetSnapPosition() => transform.position;
    }
}
