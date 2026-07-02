using UnityEngine;

namespace MarbleMazePan3D
{
    /// <summary>
    /// One-click scene setup. Add this to an empty scene and press Play.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Marble")]
        [SerializeField] private float marbleRadius = 0.22f;
        [SerializeField] private Vector3 marbleStartLocalPosition = new Vector3(-3f, 0.35f, -3f);

        private void Awake()
        {
            BuildScene();
        }

        private void BuildScene()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.65f, 0.72f, 0.85f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.55f, 0.55f);
            RenderSettings.ambientGroundColor = new Color(0.35f, 0.3f, 0.25f);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var mazeBoard = new GameObject("MazeBoard");
            mazeBoard.transform.SetParent(transform, false);
            var tilt = mazeBoard.AddComponent<MazeTiltController>();
            var boardBuilder = mazeBoard.AddComponent<MazeBoardBuilder>();
            boardBuilder.BuildMaze();

            var spawn = new GameObject("MarbleSpawn");
            spawn.transform.SetParent(mazeBoard.transform, false);
            spawn.transform.localPosition = marbleStartLocalPosition;

            var marbleGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marbleGo.name = "Marble";
            marbleGo.tag = "Marble";
            marbleGo.transform.SetParent(mazeBoard.transform, false);
            marbleGo.transform.localPosition = marbleStartLocalPosition;
            marbleGo.transform.localScale = Vector3.one * marbleRadius * 2f;

            var marbleBody = marbleGo.AddComponent<Rigidbody>();
            marbleBody.mass = 0.12f;
            marbleBody.linearDamping = 0.15f;
            marbleBody.angularDamping = 0.2f;

            var marble = marbleGo.AddComponent<MarbleController>();
            marble.SetSpawnPoint(spawn.transform);

            var marbleRenderer = marbleGo.GetComponent<Renderer>();
            var marbleShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var marbleMat = new Material(marbleShader);
            marbleMat.color = new Color(0.92f, 0.18f, 0.18f);
            if (marbleMat.HasProperty("_Glossiness"))
            {
                marbleMat.SetFloat("_Glossiness", 0.85f);
            }

            marbleRenderer.sharedMaterial = marbleMat;

            var physMat = new PhysicMaterial("MarblePhys")
            {
                dynamicFriction = 0.08f,
                staticFriction = 0.08f,
                bounciness = 0.05f,
                frictionCombine = PhysicMaterialCombine.Minimum,
                bounceCombine = PhysicMaterialCombine.Minimum
            };
            marbleGo.GetComponent<Collider>().material = physMat;

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.55f, 0.65f, 0.75f);
            var mazeCamera = cameraGo.AddComponent<MazeCamera>();
            mazeCamera.SetTarget(mazeBoard.transform);

            var managerGo = new GameObject("GameManager");
            var manager = managerGo.AddComponent<GameManager>();
            manager.Configure(tilt, marble, mazeBoard.GetComponentInChildren<GoalTrigger>());

            Debug.Log("MarbleMazePan3D ready. Drag with mouse or use WASD / arrow keys to tilt the board.");
        }
    }
}
