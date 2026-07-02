using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class HandView : MonoBehaviour
    {
        private RectTransform _panel;
        private AlgebraGameController _controller;

        public void Initialize(AlgebraGameController controller, RectTransform panel)
        {
            _controller = controller;
            _panel = panel;
            _controller.BoardChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.BoardChanged -= Refresh;
            }
        }

        private void Refresh()
        {
            for (int i = _panel.childCount - 1; i >= 0; i--)
            {
                Destroy(_panel.GetChild(i).gameObject);
            }

            var layout = _panel.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = _panel.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 12f;
                layout.childAlignment = TextAnchor.MiddleCenter;
            }

            for (int i = 0; i < _controller.Hand.Count; i++)
            {
                int handIndex = i;
                BoardCard card = _controller.Hand[i];
                CardWidget widget = CardWidget.Create(_panel, card, handIndex, "Hand", _controller);
                var button = widget.gameObject.AddComponent<Button>();
                button.targetGraphic = widget.GetComponent<Image>();
                button.onClick.AddListener(() => _controller.TryPlayFromHand(handIndex, true));
            }
        }
    }
}
