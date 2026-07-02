using UnityEngine;

namespace MarbleMaze
{
    public class MazeBuilder : MonoBehaviour
    {
        [SerializeField] Material wallMaterial;
        [SerializeField] Material floorMaterial;
        [SerializeField] Material goalMaterial;

        public Vector3 StartPosition { get; private set; }
        public Vector3 GoalPosition { get; private set; }
        public float PlayLimit { get; private set; }

        public void Build()
        {
            ClearChildren();

            var rows = MazeLayout.Layout.Length;
            var cols = MazeLayout.Layout[0].Length;
            var mazeWidth = cols * MazeLayout.CellSize;
            var mazeHeight = rows * MazeLayout.CellSize;
            var wallHalf = MazeLayout.CellSize * 0.5f;
            var floorTop = MazeLayout.FloorThickness * 0.5f;

            PlayLimit = mazeWidth * 0.5f - wallHalf - MazeLayout.BallRadius - MazeLayout.CollisionPadding;

            CreateFloor(mazeWidth, mazeHeight, floorTop);

            for (var row = 0; row < rows; row++)
            {
                var line = MazeLayout.Layout[row];
                for (var col = 0; col < line.Length; col++)
                {
                    var center = CellCenter(col, row, cols, rows);
                    switch (line[col])
                    {
                        case '#':
                            CreateWall(center, floorTop);
                            break;
                        case 'S':
                            StartPosition = new Vector3(center.x + 1f, floorTop + MazeLayout.BallRadius, center.z - 1f);
                            break;
                        case 'G':
                            GoalPosition = new Vector3(center.x, floorTop + MazeLayout.GoalSize * 0.5f, center.z);
                            CreateGoal(GoalPosition);
                            break;
                    }
                }
            }
        }

        void ClearChildren()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        Vector3 CellCenter(int col, int row, int cols, int rows)
        {
            var x = -cols * MazeLayout.CellSize * 0.5f + col * MazeLayout.CellSize + MazeLayout.CellSize * 0.5f;
            var z = rows * MazeLayout.CellSize * 0.5f - row * MazeLayout.CellSize - MazeLayout.CellSize * 0.5f;
            return new Vector3(x, 0f, z);
        }

        void CreateFloor(float mazeWidth, float mazeHeight, float floorTop)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(transform, false);
            floor.transform.localScale = new Vector3(
                mazeWidth + MazeLayout.FloorPadding,
                MazeLayout.FloorThickness,
                mazeHeight + MazeLayout.FloorPadding);
            floor.transform.position = new Vector3(0f, floorTop - MazeLayout.FloorThickness * 0.5f, 0f);
            ApplyMaterial(floor, floorMaterial, new Color(0.4f, 0.7f, 0.4f));
        }

        void CreateWall(Vector3 center, float floorTop)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.SetParent(transform, false);
            wall.transform.localScale = new Vector3(MazeLayout.CellSize, MazeLayout.WallHeight, MazeLayout.CellSize);
            wall.transform.position = new Vector3(center.x, floorTop + MazeLayout.WallHeight * 0.5f, center.z);
            ApplyMaterial(wall, wallMaterial, new Color(0.45f, 0.45f, 0.45f));
        }

        void CreateGoal(Vector3 position)
        {
            var goal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goal.name = "Goal";
            goal.transform.SetParent(transform, false);
            goal.transform.localScale = Vector3.one * MazeLayout.GoalSize;
            goal.transform.position = position;
            ApplyMaterial(goal, goalMaterial, Color.yellow);

            var trigger = goal.AddComponent<GoalTrigger>();
            trigger.Initialize(MazeLayout.GoalSize * 0.6f);
        }

        static void ApplyMaterial(GameObject obj, Material material, Color fallbackColor)
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
