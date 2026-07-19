namespace DragonBoxAlgebra.Gameplay
{
    public class PendingCancelMarker
    {
        public string SideName;
        public string CardIdA;
        public string CardIdB;
        /// <summary>Card dropped onto — swirl replaces this slot so it does not appear beside a leftover tile.</summary>
        public string AnchorCardId;
        /// <summary>Balance pair already removed — marker is tap-to-dismiss swirl only.</summary>
        public bool SwirlOnly;
    }
}
