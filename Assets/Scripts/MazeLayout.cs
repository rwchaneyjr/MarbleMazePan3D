namespace MarbleMaze
{
    public static class MazeLayout
    {
        public const float CellSize = 4f;
        public const float FloorPadding = 10f;
        public const float FloorThickness = 0.2f;
        public const float WallHeight = 1.2f;
        public const float BallRadius = 0.225f;
        public const float GoalSize = 0.8f;
        public const float MoveSpeed = 8f;
        public const float WinDistance = 1.5f;
        public const float CollisionPadding = 0.15f;

        public static readonly string[] Layout =
        {
            "##########",
            "#S       #",
            "# ### ## #",
            "# #    # #",
            "# # ## # #",
            "# # #  # #",
            "# # # ## #",
            "#   #    #",
            "# ####  G#",
            "##########",
        };

        public const string HelpText = "Reach the yellow cube!  WASD = move  R = restart";
        public const string WinText = "YOU WIN! Press R to play again";
    }
}
