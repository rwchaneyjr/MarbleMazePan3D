using DragonBoxAlgebra.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragonBoxAlgebra
{
    /// <summary>
    /// Boots the game when the scene is empty or AlgebraBootstrap is missing.
    /// </summary>
    internal static class AlgebraSceneSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureScene()
        {
            EnsureMainCamera();
            EnsureEventSystem();

            if (Object.FindObjectOfType<AlgebraBootstrap>() != null)
                return;

            if (Object.FindObjectOfType<AlgebraUI>() != null)
                return;

            var go = new GameObject("AlgebraBootstrap");
            go.AddComponent<AlgebraBootstrap>();
        }

        private static void EnsureMainCamera()
        {
            var cameras = Object.FindObjectsOfType<Camera>();
            Camera main = Camera.main;

            if (main == null && cameras.Length > 0)
            {
                cameras[0].gameObject.tag = "MainCamera";
                main = cameras[0];
            }

            if (main != null)
            {
                foreach (var camera in cameras)
                {
                    if (camera != main)
                    {
                        Object.Destroy(camera.gameObject);
                    }
                }

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
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
