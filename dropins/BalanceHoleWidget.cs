using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    /// <summary>
    /// Pending-balance marker: a plain "?" symbol (same family as the board "+"),
    /// placed in the middle of the opposite side's equation.
    /// </summary>
    public class BalanceHoleWidget : MonoBehaviour, IDropHandler
    {
        private AlgebraGameController _controller;
        private string _sideName;

        public string SideName => _sideName;

        public void OnDrop(PointerEventData eventData)
        {
            CardWidget dragged = eventData.pointerDrag?.GetComponent<CardWidget>();
            if (dragged == null || dragged.SideName != "Hand")
            {
                return;
            }

            if (_controller != null && _controller.TryPlayFromHand(dragged.Index, _sideName))
            {
                dragged.MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
            }
        }

        /// <summary>
        /// Slim in-row "?" like the "+" separator — sits in the middle of that side's equation.
        /// </summary>
        public static BalanceHoleWidget CreateInlineSymbol(Transform parent, AlgebraGameController controller,
            string holeSide, float tileHeight)
        {
            var go = new GameObject($"BalanceQuestion_{holeSide}", typeof(RectTransform), typeof(Text),
                typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            const float width = 28f;
            var layoutElement = go.GetComponent<LayoutElement>();
            layoutElement.minWidth = width;
            layoutElement.preferredWidth = width;
            layoutElement.minHeight = tileHeight;
            layoutElement.preferredHeight = tileHeight;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, tileHeight);

            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = "?";
            text.fontSize = 34;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.95f, 0.95f, 0.88f);
            text.raycastTarget = true;

            var hole = go.AddComponent<BalanceHoleWidget>();
            hole._controller = controller;
            hole._sideName = holeSide;
            return hole;
        }

        public static BalanceHoleWidget Create(Transform parent, AlgebraGameController controller, string sideName,
            BoardCard card, float tileWidth = 110f, float tileHeight = 120f)
        {
            return CreateInlineSymbol(parent, controller, sideName, tileHeight);
        }
    }
}
