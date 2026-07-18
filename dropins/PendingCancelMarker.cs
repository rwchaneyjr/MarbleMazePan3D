namespace DragonBoxAlgebra.Gameplay
{
    public class PendingCancelMarker
    {
        public string SideName;
        public string CardIdA;
        public string CardIdB;
        /// <summary>
        /// Card the player dropped onto — swirl sits in this slot (not the other side of the red box).
        /// </summary>
        public string AnchorCardId;
        /// <summary>Balance pair already removed — marker is tap-to-dismiss swirl only.</summary>
        public bool SwirlOnly;
    }
}
