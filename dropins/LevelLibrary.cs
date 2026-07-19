using System.Collections.Generic;
using UnityEngine;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelLibrary
    {
        public static IReadOnlyList<LevelDefinition> Levels { get; } = BuildLevels();

        private static IReadOnlyList<LevelDefinition> BuildLevels()
        {
            var levels = ChapterLevelGenerator.GenerateAll();
            int expected = ChapterLevelGenerator.TotalLevels;
            if (levels.Count != expected)
            {
                Debug.LogError(
                    $"[DragonBox] Level count mismatch: got {levels.Count}, expected {expected}. " +
                    $"Curriculum={ChapterLevelGenerator.CurriculumVersion}. " +
                    "Pull branch cursor/ch5-gradual-from-save-3fe3 or run: bash scripts/sync-dropins.sh import --here");
            }
            else
            {
                Debug.LogWarning(
                    $"[DragonBox] {levels.Count} levels loaded ({ChapterLevelGenerator.CurriculumVersion}). " +
                    $"Ch7 starts at level {ChapterLevelGenerator.Chapter7StartLevel}. " +
                    $"Ch8 multiply+add starts at level {ChapterLevelGenerator.Chapter8StartLevel}.");
            }

            return levels;
        }
    }
}
