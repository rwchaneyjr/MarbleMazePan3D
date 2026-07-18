using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    /// <summary>
    /// Drop socket on a board tile. Only the correct opposite tile may snap here.
    /// </summary>
    public class TileSnapTarget : MonoBehaviour
    {
        [Header("Where the tile should snap")]
        public Transform snapPoint;

        [Header("Bound board card (set at runtime)")]
        public CardWidget Widget;

        public void Bind(CardWidget widget)
        {
            Widget = widget;
            if (snapPoint == null)
            {
                snapPoint = transform;
            }
        }

        public bool IsCorrectTile(CardWidget tile)
        {
            if (tile == null || Widget == null || tile == Widget || Widget.SideName == "Hand")
            {
                return false;
            }

            if (CombineRules.GetCombineAction(tile.Card, Widget.Card) != CombineActionType.OppositeCancel)
            {
                return false;
            }

            // Same creature image family only (blocks cross-theme snaps on split-theme levels).
            if (tile.Card.VisualTheme >= 0 && Widget.Card.VisualTheme >= 0
                && tile.Card.VisualTheme != Widget.Card.VisualTheme)
            {
                return false;
            }

            if (tile.SideName == "Hand")
            {
                AlgebraGameController controller = tile.Controller;
                return controller != null
                    && controller.CanPlayHandOntoBoardCard(tile.Index, Widget.SideName, Widget.Index);
            }

            if (tile.SideName != Widget.SideName)
            {
                return false;
            }

            AlgebraGameController boardController = tile.Controller ?? Widget.Controller;
            if (boardController != null
                && (boardController.IsCardPendingCancelOnSide(tile.Card.Id, tile.SideName)
                    || boardController.IsCardPendingCancelOnSide(Widget.Card.Id, Widget.SideName)))
            {
                return false;
            }

            return true;
        }

        public bool IsCorrectTile(GameObject tileObject)
        {
            if (tileObject == null)
            {
                return false;
            }

            CardWidget widget = tileObject.GetComponent<CardWidget>();
            return widget != null && IsCorrectTile(widget);
        }

        public Vector3 GetSnapPosition()
        {
            return snapPoint != null ? snapPoint.position : transform.position;
        }
    }
}
