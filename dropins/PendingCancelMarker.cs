namespace DragonBoxAlgebra.Gameplay
{
    public class PendingCancelMarker
    {
        public string SideName;
        public string CardIdA;
        public string CardIdB;
        /// <summary>Balance pair already removed — marker is tap-to-dismiss swirl only.</summary>
        public bool SwirlOnly;
    }
}
