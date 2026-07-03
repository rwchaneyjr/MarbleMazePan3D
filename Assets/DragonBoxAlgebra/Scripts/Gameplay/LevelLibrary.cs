using System.Collections.Generic;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelLibrary
    {
        public static IReadOnlyList<LevelDefinition> Levels { get; } = LevelGenerator.GenerateAll();
    }
}
