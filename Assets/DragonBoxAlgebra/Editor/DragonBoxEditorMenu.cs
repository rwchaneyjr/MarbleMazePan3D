#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragonBoxAlgebra.Editor
{
    public static class DragonBoxEditorMenu
    {
        private const string ScenePath = "Assets/DragonBoxAlgebra/Scenes/DragonBox.unity";

        [MenuItem("DragonBox Algebra/Open Game Scene and Setup", false, 0)]
        public static void OpenAndSetup()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(ScenePath);
            SetupScene();
        }

        [MenuItem("DragonBox Algebra/Setup Scene (Camera + Bootstrap)", false, 1)]
        public static void SetupScene()
        {
            EnsureMainCamera();
            EnsureEventSystem();
            EnsureAlgebraBootstrap();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(
                "DragonBox Algebra: Scene ready. Hierarchy should show AlgebraBootstrap, Main Camera, EventSystem. Press Play.");
        }

        private static void EnsureAlgebraBootstrap()
        {
            if (Object.FindObjectOfType<AlgebraBootstrap>() != null)
            {
                return;
            }

            var go = new GameObject("AlgebraBootstrap");
            go.AddComponent<AlgebraBootstrap>();
            Undo.RegisterCreatedObjectUndo(go, "Create AlgebraBootstrap");
            Selection.activeGameObject = go;
        }

        private static void EnsureMainCamera()
        {
            if (Camera.main != null)
            {
                return;
            }

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.34f, 0.42f);
            cameraGo.AddComponent<AudioListener>();
            Undo.RegisterCreatedObjectUndo(cameraGo, "Create Main Camera");
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventGo, "Create EventSystem");
        }
    }
}
#endif
