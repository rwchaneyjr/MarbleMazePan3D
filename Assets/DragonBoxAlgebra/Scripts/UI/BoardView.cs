using System.Collections.Generic;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class BoardView : MonoBehaviour
    {
        private RectTransform _leftPanel;
        private RectTransform _rightPanel;
        private RectTransform _dragRoot;
        private Canvas _canvas;
        private AlgebraGameController _controller;
        private readonly List<CardWidget> _widgets = new();

        public void Initialize(AlgebraGameController controller, RectTransform left, RectTransform right,
            Canvas canvas, RectTransform dragRoot)
        {
            _controller = controller;
            _leftPanel = left;
            _rightPanel = right;
            _canvas = canvas;
            _dragRoot = dragRoot;

            left.gameObject.AddComponent<BoardDropZone>().SideName = "Left";
            right.gameObject.AddComponent<BoardDropZone>().SideName = "Right";

            _controller.BoardChanged += Refresh;
            _controller.CombineOccurred += OnCombine;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.BoardChanged -= Refresh;
                _controller.CombineOccurred -= OnCombine;
            }
        }

        private void OnCombine(CombineEvent evt)
        {
            if (evt.Action != CombineActionType.OppositeCancel)
            {
                return;
            }

            VortexEffect.Play(_dragRoot, GetSideCenter(evt.SideName));

            if (DragonBoxAlgebra.Audio.AudioManager.Instance != null)
            {
                DragonBoxAlgebra.Audio.AudioManager.Instance.PlayCombine();
            }

            foreach (CardWidget widget in _widgets)
            {
                if (widget.SideName == evt.SideName)
                {
                    widget.ReactCombine();
                }
            }
        }

        private Vector3 GetSideCenter(string sideName)
        {
            RectTransform panel = sideName == "Left" ? _leftPanel : _rightPanel;
            return panel.transform.position;
        }

        private void Refresh()
        {
            _widgets.Clear();
            RebuildSide(_leftPanel, _controller.Board.Left, "Left");
            RebuildSide(_rightPanel, _controller.Board.Right, "Right");

            if (_controller.HasPendingBalance)
            {
                BalancePending pending = _controller.PendingBalance;
                RectTransform holePanel = pending.HoleSide == "Left" ? _leftPanel : _rightPanel;
                BalanceHoleWidget.Create(holePanel, _controller, pending.HoleSide, pending.Card);
            }
        }

        private void RebuildSide(RectTransform panel, BoardSide side, string sideName)
        {
            for (int i = panel.childCount - 1; i >= 0; i--)
            {
                Transform child = panel.GetChild(i);
                if (child.GetComponent<BoardDropZone>() == null)
                {
                    Destroy(child.gameObject);
                }
            }

            var layout = panel.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 16f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.padding = new RectOffset(24, 24, 24, 24);
            }

            for (int i = 0; i < side.Cards.Count; i++)
            {
                CardWidget widget = CardWidget.Create(panel, side.Cards[i], i, sideName, _controller, _canvas, _dragRoot);
                widget.gameObject.AddComponent<CardDropZone>();
                _widgets.Add(widget);
            }
        }
    }
}
