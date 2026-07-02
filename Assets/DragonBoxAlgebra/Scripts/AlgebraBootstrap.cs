using DragonBoxAlgebra.Gameplay;
using DragonBoxAlgebra.UI;
using UnityEngine;

namespace DragonBoxAlgebra
{
    public class AlgebraBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var cameraGo = new GameObject("Main Camera");
                cameraGo.tag = "MainCamera";
                mainCamera = cameraGo.AddComponent<Camera>();
                mainCamera.orthographic = true;
                mainCamera.backgroundColor = new Color(0.12f, 0.34f, 0.42f);
            }

            var controller = new AlgebraGameController();
            var ui = gameObject.AddComponent<AlgebraUI>();
            ui.Initialize(controller);
        }
    }
}
