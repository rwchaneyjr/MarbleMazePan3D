using MarbleMaze;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarbleMaze.Editor
{
    public static class MarbleMazeSetup
    {
        const string ScenePath = "Assets/Scenes/MainScene.unity";

        [MenuItem("Marble Maze/Create Main Scene")]
        public static void CreateMainScene()
        {
            EnsureScenesFolder();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var mazeRoot = new GameObject("Maze");
            mazeRoot.AddComponent<MazeBuilder>();

            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Ball";
            ball.transform.localScale = Vector3.one * (MazeLayout.BallRadius * 2f);
            var ballBody = ball.AddComponent<Rigidbody>();
            ballBody.useGravity = false;
            ballBody.isKinematic = true;
            ball.AddComponent<BallController>();

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.6f);

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.45f, 0.45f, 0.45f);
            cameraGo.AddComponent<AudioListener>();

            var canvasGo = new GameObject("UI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var textGo = new GameObject("StatusText");
            textGo.transform.SetParent(canvasGo.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = MazeLayout.HelpText;
            text.fontSize = 24f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -20f);
            rect.sizeDelta = new Vector2(900f, 80f);

            var gameGo = new GameObject("Game");
            var game = gameGo.AddComponent<MarbleMazeGame>();

            var serializedGame = new SerializedObject(game);
            serializedGame.FindProperty("mazeBuilder").objectReferenceValue = mazeRoot.GetComponent<MazeBuilder>();
            serializedGame.FindProperty("ball").objectReferenceValue = ball.GetComponent<BallController>();
            serializedGame.FindProperty("statusText").objectReferenceValue = text;
            serializedGame.FindProperty("gameCamera").objectReferenceValue = camera;
            serializedGame.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Marble Maze",
                "Main scene created at Assets/Scenes/MainScene.unity.\n\nOpen it and press Play.",
                "OK");
        }

        static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }
    }
}
