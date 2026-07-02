using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class AlgebraUI : MonoBehaviour
    {
        public AlgebraGameController Controller { get; private set; }

        private Text _titleText;
        private Text _progressText;
        private Text _messageText;
        private LevelCompleteView _completeView;

        public void Initialize(AlgebraGameController controller)
        {
            Controller = controller;
            BuildUI();
            controller.LevelLoaded += OnLevelLoaded;
            controller.MessageChanged += OnMessageChanged;
            controller.LoadLevel(0);
        }

        private void OnDestroy()
        {
            if (Controller != null)
            {
                Controller.LevelLoaded -= OnLevelLoaded;
                Controller.MessageChanged -= OnMessageChanged;
            }
        }

        private void OnLevelLoaded(int current, int total)
        {
            _progressText.text = $"{current}/{total}";
            _titleText.text = Controller.CurrentLevel.Title;
            _completeView.Hide();
        }

        private void OnMessageChanged(string message)
        {
            _messageText.text = message;
        }

        public void OnRestartClicked()
        {
            Controller.RestartLevel();
        }

        public void OnNextClicked()
        {
            _completeView.Hide();
            Controller.LoadNextLevel();
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            var background = CreatePanel(canvasGo.transform, "Background", new Color(0.12f, 0.34f, 0.42f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            _titleText = CreateText(background.transform, "Title", "DragonBox Algebra",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), 28, TextAnchor.MiddleCenter);
            _progressText = CreateText(background.transform, "Progress", "1/4",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f), 20, TextAnchor.MiddleCenter);

            var boardRow = CreatePanel(background.transform, "BoardRow", new Color(0f, 0f, 0f, 0.15f),
                new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.82f), Vector2.zero, Vector2.zero);

            var leftPanel = CreatePanel(boardRow.transform, "LeftPanel", new Color(0.45f, 0.72f, 0.78f, 0.55f),
                new Vector2(0f, 0f), new Vector2(0.49f, 1f), Vector2.zero, Vector2.zero);
            var rightPanel = CreatePanel(boardRow.transform, "RightPanel", new Color(0.45f, 0.72f, 0.78f, 0.55f),
                new Vector2(0.51f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            var boardView = gameObject.AddComponent<BoardView>();
            boardView.Initialize(Controller, leftPanel.GetComponent<RectTransform>(),
                rightPanel.GetComponent<RectTransform>());

            var handPanel = CreatePanel(background.transform, "Hand", new Color(0.08f, 0.18f, 0.24f, 0.85f),
                new Vector2(0.15f, 0.06f), new Vector2(0.85f, 0.22f), Vector2.zero, Vector2.zero);
            var handView = gameObject.AddComponent<HandView>();
            handView.Initialize(Controller, handPanel.GetComponent<RectTransform>());

            _messageText = CreateText(background.transform, "Message",
                "Tap two opposite cards on the same side to combine them.",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), 18, TextAnchor.LowerCenter);

            CreateButton(background.transform, "Restart", new Vector2(0.08f, 0.92f), OnRestartClicked);
            var nextButton = CreateButton(background.transform, "Next", new Vector2(0.92f, 0.92f), OnNextClicked);

            var completePanel = CreatePanel(background.transform, "CompletePanel", new Color(0.05f, 0.12f, 0.18f, 0.92f),
                new Vector2(0.2f, 0.25f), new Vector2(0.8f, 0.75f), Vector2.zero, Vector2.zero);
            var starsText = CreateText(completePanel.transform, "Stars", "Level Complete",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, 24, TextAnchor.MiddleCenter);

            _completeView = gameObject.AddComponent<LevelCompleteView>();
            _completeView.Initialize(Controller, completePanel, starsText);

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }
        }

        private static RectTransform CreatePanel(Transform parent, string name, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            go.GetComponent<Image>().color = color;
            return rect;
        }

        private static Text CreateText(Transform parent, string name, string value, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 anchoredPosition, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(600f, 120f);
            rect.anchoredPosition = anchoredPosition;

            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(120f, 44f);
            go.GetComponent<Image>().color = new Color(0.82f, 0.32f, 0.18f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            CreateText(go.transform, "Label", label, Vector2.zero, Vector2.one, Vector2.zero, 18, TextAnchor.MiddleCenter);
            return button;
        }
    }
}
