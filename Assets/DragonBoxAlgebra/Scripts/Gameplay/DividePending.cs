using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// DragonBox divide: place the same card below the line on one side, then the other.
    /// </summary>
    public class DividePending
    {
        public BoardCard Card;
        public string PlacedSide;
        public int HandIndex;

        public string HoleSide => PlacedSide == "Left" ? "Right" : "Left";

        public bool Matches(BoardCard other) =>
            Card.Kind == other.Kind && Card.Value == other.Value;
    }
}
