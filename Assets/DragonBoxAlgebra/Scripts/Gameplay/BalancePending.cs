using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public class BalancePending
    {
        public BoardCard Card;
        public string PlacedSide;

        public string HoleSide => PlacedSide == "Left" ? "Right" : "Left";
    }
}
