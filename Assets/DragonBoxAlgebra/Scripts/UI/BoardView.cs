using System.Collections.Generic;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private RectTransform leftPanel;
        [SerializeField] private RectTransform rightPanel;

        private AlgebraGameController _controller;
        private CardWidget _selected;

        public void Initialize(AlgebraGameController controller, RectTransform left, RectTransform right)
        {
            _controller = controller;
            leftPanel = left;
            rightPanel = right;
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

        public void HandleCardClicked(CardWidget widget)
        {
            if (_selected == null)
            {
                _selected = widget;
                widget.SetSelected(true);
                return;
            }

            if (_selected == widget)
            {
                widget.SetSelected(false);
                _selected = null;
                return;
            }

            if (_selected.SideName != widget.SideName)
            {
                _selected.SetSelected(false);
                _selected = widget;
                widget.SetSelected(true);
                return;
            }

            _controller.TryCombine(widget.SideName, _selected.Index, widget.Index);
            _selected.SetSelected(false);
            _selected = null;
        }

        private void Refresh()
        {
            RebuildSide(leftPanel, _controller.Board.Left, "Left", _controller);
            RebuildSide(rightPanel, _controller.Board.Right, "Right", _controller);
            _selected = null;
        }

        private static void RebuildSide(RectTransform panel, BoardSide side, string sideName, AlgebraGameController controller)
        {
            for (int i = panel.childCount - 1; i >= 0; i--)
            {
                Destroy(panel.GetChild(i).gameObject);
            }

            var layout = panel.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 16f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.padding = new RectOffset(24, 24, 24, 24);
            }

            for (int i = 0; i < side.Cards.Count; i++)
            {
                CardWidget.Create(panel, side.Cards[i], i, sideName, controller);
            }
        }
    }
}
