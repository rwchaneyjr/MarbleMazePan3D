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

            if (tile.SideName == "Hand")
            {
                AlgebraGameController controller = tile.Controller;
                return controller != null
                    && controller.CanPlayHandOntoBoardCard(tile.Index, Widget.SideName, Widget.Index);
            }

            return tile.SideName == Widget.SideName;
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
