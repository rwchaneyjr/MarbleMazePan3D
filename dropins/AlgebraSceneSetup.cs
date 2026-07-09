using DragonBoxAlgebra.UI;
using UnityEngine;

namespace DragonBoxAlgebra
{
    /// <summary>
    /// Boots the game when DragonBox.unity is open but AlgebraBootstrap is missing
    /// (e.g. the scene object was renamed to "Algebra" or the script reference broke).
    /// </summary>
    internal static class AlgebraSceneSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrap()
        {
            if (Object.FindObjectOfType<AlgebraBootstrap>() != null)
                return;

            if (Object.FindObjectOfType<AlgebraUI>() != null)
                return;

            var go = new GameObject("AlgebraBootstrap");
            go.AddComponent<AlgebraBootstrap>();
        }
    }
}
