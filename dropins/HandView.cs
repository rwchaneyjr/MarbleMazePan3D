using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class HandView : MonoBehaviour
    {
        private RectTransform _panel;
        private Canvas _canvas;
        private RectTransform _dragRoot;
        private AlgebraGameController _controller;

        public void Initialize(AlgebraGameController controller, RectTransform panel, Canvas canvas,
            RectTransform dragRoot)
        {
            _controller = controller;
            _panel = panel;
            _canvas = canvas;
            _dragRoot = dragRoot;
            _panel.gameObject.AddComponent<BoardDropZone>();
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
                Transform child = _panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() == null)
                {
                    Destroy(child.gameObject);
                }
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
                BoardCard card = _controller.Hand[i];
                CardWidget.Create(_panel, card, i, "Hand", _controller, _canvas, _dragRoot);
            }
        }
    }
}
