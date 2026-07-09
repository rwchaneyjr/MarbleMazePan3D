using DragonBoxAlgebra.Audio;
using DragonBoxAlgebra.Gameplay;
using DragonBoxAlgebra.UI;
using UnityEngine;

namespace DragonBoxAlgebra
{
    public class AlgebraBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (Camera.main == null)
            {
                var cameraGo = new GameObject("Main Camera");
                cameraGo.tag = "MainCamera";
                var camera = cameraGo.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.12f, 0.34f, 0.42f);
                cameraGo.AddComponent<AudioListener>();
            }

            if (AudioManager.Instance == null)
            {
                var audioGo = new GameObject("AudioManager", typeof(AudioSource), typeof(AudioManager));
                DontDestroyOnLoad(audioGo);
            }

            CardSpriteLoader.Reset();
            CardSpriteLoader.EnsureLoaded();
            LogStartupDiagnostics();

            var controller = new AlgebraGameController();
            var ui = gameObject.AddComponent<AlgebraUI>();
            ui.Initialize(controller);
        }

        private static void LogStartupDiagnostics()
        {
            int loaded = 0;
            for (int theme = 0; theme < CreatureArt.ThemeCount; theme++)
            {
                if (CardSpriteLoader.HasCustomArt(theme, light: true))
                {
                    loaded++;
                }

                if (CardSpriteLoader.HasCustomArt(theme, light: false))
                {
                    loaded++;
                }
            }

            string firstTitle = LevelLibrary.Count > 0 ? LevelLibrary.GetLevel(0).Title : "(no levels)";
            Debug.Log(
                $"[DragonBox] Build OK — custom sprites: {loaded}/16. " +
                $"Level 1 should be 'Pair on Left 1' (not Butterfly/Bat). Actual: '{firstTitle}'");
        }
    }
}
