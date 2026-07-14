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

        public bool Matches(BoardCard other)
        {
            if (Card.Kind != other.Kind || Card.Value != other.Value)
            {
                return false;
            }

            if (Card.VariableLetter == '\0' && other.VariableLetter == '\0')
            {
                return true;
            }

            return Card.VariableLetter == other.VariableLetter;
        }
    }
}
