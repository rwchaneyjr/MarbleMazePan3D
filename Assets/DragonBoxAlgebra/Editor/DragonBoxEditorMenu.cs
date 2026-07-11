#if UNITY_EDITOR
using DragonBoxAlgebra.UI;
using UnityEditor;
using UnityEngine;

namespace DragonBoxAlgebra.Editor
{
    public static class DragonBoxEditorMenu
    {
        [MenuItem("DragonBox Algebra/Print Sprite Debug (why no PNGs?)", false, 0)]
        public static void PrintSpriteDebug()
        {
            CardSpriteLoader.Reset();
            CardSpriteLoader.EnsureLoaded();
            CreatureSpriteDebug.LogStartup();
            Debug.LogWarning(
                "[DragonBox] If Console is empty after Play, fix red package errors first " +
                "(Packages/manifest.json — remove com.unity.multiplayer.center).");
        }

        [MenuItem("DragonBox Algebra/Fix Package Error (remove multiplayer.center)", false, 1)]
        public static void ShowPackageFixHelp()
        {
            Debug.LogWarning(
                "[DragonBox] Open Packages/manifest.json and DELETE the line:\n" +
                "  \"com.unity.multiplayer.center\": \"1.0.0\",\n" +
                "Save file. Unity will reimport. Then press Play again.");
        }
    }
}
#endif
