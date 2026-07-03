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

            CardSpriteLoader.EnsureLoaded();

            var controller = new AlgebraGameController();
            var ui = gameObject.AddComponent<AlgebraUI>();
            ui.Initialize(controller);
        }
    }
}
