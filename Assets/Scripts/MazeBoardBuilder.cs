using UnityEngine;

namespace MarbleMazePan3D
{
    /// <summary>
    /// Procedurally builds a simple marble maze pan board at runtime.
    /// Attach to an empty GameObject under the tilting MazeBoard root.
    /// </summary>
    public class MazeBoardBuilder : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField] private Vector2 boardSize = new Vector2(8f, 8f);
        [SerializeField] private float wallHeight = 0.6f;
        [SerializeField] private float wallThickness = 0.25f;
        [SerializeField] private Material boardMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material holeMaterial;

        [Header("Holes")]
        [SerializeField] private Vector2[] holePositions =
        {
            new Vector2(-2f, 1f),
            new Vector2(0f, -1.5f),
            new Vector2(2.2f, 0.5f),
            new Vector2(1.5f, 2f),
        };

        [SerializeField] private float holeRadius = 0.45f;

        [Header("Goal")]
        [SerializeField] private Vector2 goalPosition = new Vector2(3f, 3f);
        [SerializeField] private float goalRadius = 0.55f;

        private bool _built;

        public void BuildMaze()
        {
            if (_built)
            {
                return;
            }

            _built = true;
            BuildBoard();
            BuildWalls();
            BuildHoles();
            BuildGoal();
        }

        private void Awake()
        {
            BuildMaze();
        }

        private void BuildBoard()
        {
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "BoardSurface";
            board.transform.SetParent(transform, false);
            board.transform.localScale = new Vector3(boardSize.x, 0.2f, boardSize.y);
            board.transform.localPosition = new Vector3(0f, -0.1f, 0f);

            ApplyMaterial(board, boardMaterial, new Color(0.78f, 0.62f, 0.38f));
        }

        private void BuildWalls()
        {
            // Outer frame
            CreateWall("WallNorth", new Vector3(0f, wallHeight * 0.5f, boardSize.y * 0.5f),
                new Vector3(boardSize.x, wallHeight, wallThickness));
            CreateWall("WallSouth", new Vector3(0f, wallHeight * 0.5f, -boardSize.y * 0.5f),
                new Vector3(boardSize.x, wallHeight, wallThickness));
            CreateWall("WallEast", new Vector3(boardSize.x * 0.5f, wallHeight * 0.5f, 0f),
                new Vector3(wallThickness, wallHeight, boardSize.y));
            CreateWall("WallWest", new Vector3(-boardSize.x * 0.5f, wallHeight * 0.5f, 0f),
                new Vector3(wallThickness, wallHeight, boardSize.y));

            // Inner maze walls (simple labyrinth layout)
            CreateWall("InnerA", new Vector3(-1.5f, wallHeight * 0.5f, 0.5f), new Vector3(3f, wallHeight, wallThickness));
            CreateWall("InnerB", new Vector3(0.5f, wallHeight * 0.5f, -1f), new Vector3(wallThickness, wallHeight, 3f));
            CreateWall("InnerC", new Vector3(1.8f, wallHeight * 0.5f, 1.2f), new Vector3(2.5f, wallHeight, wallThickness));
            CreateWall("InnerD", new Vector3(-0.5f, wallHeight * 0.5f, 2.2f), new Vector3(wallThickness, wallHeight, 2f));
            CreateWall("InnerE", new Vector3(2.5f, wallHeight * 0.5f, -1.5f), new Vector3(wallThickness, wallHeight, 2.5f));
        }

        private void CreateWall(string name, Vector3 localPosition, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(transform, false);
            wall.transform.localPosition = localPosition;
            wall.transform.localScale = scale;
            ApplyMaterial(wall, wallMaterial, new Color(0.55f, 0.38f, 0.22f));
        }

        private void BuildHoles()
        {
            for (int i = 0; i < holePositions.Length; i++)
            {
                var hole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                hole.name = $"Hole_{i}";
                hole.transform.SetParent(transform, false);
                hole.transform.localPosition = new Vector3(holePositions[i].x, -0.05f, holePositions[i].y);
                hole.transform.localScale = new Vector3(holeRadius * 2f, 0.02f, holeRadius * 2f);

                var collider = hole.GetComponent<Collider>();
                collider.isTrigger = true;
                hole.AddComponent<HoleTrigger>();

                ApplyMaterial(hole, holeMaterial, new Color(0.12f, 0.12f, 0.12f));
            }
        }

        private void BuildGoal()
        {
            var goal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            goal.name = "Goal";
            goal.transform.SetParent(transform, false);
            goal.transform.localPosition = new Vector3(goalPosition.x, 0.01f, goalPosition.y);
            goal.transform.localScale = new Vector3(goalRadius * 2f, 0.02f, goalRadius * 2f);

            var collider = goal.GetComponent<Collider>();
            collider.isTrigger = true;
            goal.AddComponent<GoalTrigger>();

            ApplyMaterial(goal, null, new Color(0.2f, 0.85f, 0.35f));
        }

        private static void ApplyMaterial(GameObject obj, Material material, Color fallbackColor)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (material != null)
            {
                renderer.sharedMaterial = material;
                return;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var runtimeMaterial = new Material(shader);
            runtimeMaterial.color = fallbackColor;
            renderer.sharedMaterial = runtimeMaterial;
        }
    }
}
