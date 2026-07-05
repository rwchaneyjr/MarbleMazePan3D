using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public class BalancePending
    {
        public BoardCard Card;
        public string PlacedSide;
        public int PlacedIndex;
        public int HandIndex;
        public int HoleInsertIndex;

        public string HoleSide => PlacedSide == "Left" ? "Right" : "Left";

        public bool Matches(BoardCard other) =>
            Card.Kind == other.Kind
            && Card.Value == other.Value
            && Card.VisualTheme == other.VisualTheme;
    }
}
