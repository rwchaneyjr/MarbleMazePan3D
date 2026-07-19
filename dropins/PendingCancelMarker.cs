namespace DragonBoxAlgebra.Gameplay
{
    public enum CancelResultSymbol
    {
        Swirl,
        Zero,
        One
    }

    public class PendingCancelMarker
    {
        public string SideName;
        public string CardIdA;
        public string CardIdB;
        /// <summary>Balance pair already removed — marker is tap-to-dismiss swirl only.</summary>
        public bool SwirlOnly;
        /// <summary>Addition → 0, division → 1, otherwise swirl.</summary>
        public CancelResultSymbol ResultSymbol = CancelResultSymbol.Swirl;
    }
}
