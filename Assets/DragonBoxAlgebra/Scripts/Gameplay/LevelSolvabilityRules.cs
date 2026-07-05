using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelSolvabilityRules
    {
        public const int ExtraPuzzleFromIndex = 12;
        public const int ExtraPuzzleToIndex = 22;
        public const int ExtraPuzzleCount = ExtraPuzzleToIndex - ExtraPuzzleFromIndex;
        public const int DistinctAnimalsForExtraLevel = 5;
        public const int MinOtherSideExtras = 2;
        public const int MaxOtherSideExtras = 2;

        public static bool ShouldConfigureBoxSide(int handCount) => handCount >= 2;

        public static bool IsExtraPuzzleLevel(int levelIndex) =>
            levelIndex >= ExtraPuzzleFromIndex && levelIndex < ExtraPuzzleToIndex;

        /// <summary>
        /// Tile count for the side opposite the red box (display levels 13–22 only).
        /// Change MinOtherSideExtras / MaxOtherSideExtras above to adjust.
        /// </summary>
        public static int OtherSideCountForExtraLevel(int extraIndex) =>
            MinOtherSideExtras + (extraIndex % (MaxOtherSideExtras - MinOtherSideExtras + 1));

        /// <summary>
        /// Single entry point from LevelGenerator — picks standard vs extra layout.
        /// </summary>
        public static void ConfigureLevel(LevelDefinition level, int levelIndex, int handCount, bool diceLevel,
            int value)
        {
            if (!ShouldConfigureBoxSide(handCount))
            {
                return;
            }

            if (IsExtraPuzzleLevel(levelIndex))
            {
                int extraIndex = levelIndex - ExtraPuzzleFromIndex;
                int otherSideCount = OtherSideCountForExtraLevel(extraIndex);
                ConfigureExtraPuzzleLevel(level, handCount, diceLevel, value, otherSideCount);
                return;
            }

            ConfigureStandardSolvableLevel(level, handCount, diceLevel, value);
        }

        public static int OtherSideCountForLevelIndex(int levelIndex) =>
            IsExtraPuzzleLevel(levelIndex)
                ? OtherSideCountForExtraLevel(levelIndex - ExtraPuzzleFromIndex)
                : 0;

        public static void ConfigureExtraPuzzleLevel(LevelDefinition level, int handCount, bool diceLevel, int value,
            int otherSideCount)
        {
            if (handCount < 2 || otherSideCount <= 0)
            {
                ConfigureStandardSolvableLevel(level, handCount, diceLevel, value);
                return;
            }

            if (diceLevel)
            {
                ApplyDiceBoard(level, handCount, otherSideCount, value);
                return;
            }

            ApplyCreatureBoardWithOtherSide(level, handCount, otherSideCount, value);
        }

        public static void ConfigureStandardSolvableLevel(LevelDefinition level, int handCount, bool diceLevel, int value)
        {
            level.RightCards.Clear();
            level.RightValues.Clear();
            level.RightVisualThemes.Clear();

            if (diceLevel)
            {
                ApplyDiceBoard(level, handCount, rightCount: 0, value);
                return;
            }

            ApplyCreatureBoard(level, handCount, rightCount: 0, value, handCount);
        }

        private static void ApplyCreatureBoard(LevelDefinition level, int leftBesideBox, int rightCount, int value,
            int handCount)
        {
            CardKind solverKind = level.HandCards[0];
            CardKind obstacleKind = CoordinatedCreatureThemes.OppositeCreature(solverKind);

            List<int> handThemes = CoordinatedCreatureThemes.BuildRedSideThemes(handCount, level.CreatureTheme);
            List<int> leftThemes = leftBesideBox <= handCount
                ? handThemes
                : CoordinatedCreatureThemes.BuildRedSideThemes(leftBesideBox, level.CreatureTheme);

            var usedThemes = new HashSet<int>(leftThemes);
            List<int> rightThemes = rightCount > 0
                ? CoordinatedCreatureThemes.BuildOtherSideThemes(rightCount, usedThemes, level.CreatureTheme)
                : new List<int>();

            var leftCards = new List<CardKind> { CardKind.Box };
            var leftValues = new List<int> { 1 };
            var leftVisualThemes = new List<int> { -1 };

            for (int i = 0; i < leftBesideBox; i++)
            {
                leftCards.Add(obstacleKind);
                leftValues.Add(value);
                leftVisualThemes.Add(leftThemes[i]);
            }

            var rightCards = new List<CardKind>();
            var rightValues = new List<int>();
            var rightVisualThemes = new List<int>();

            for (int i = 0; i < rightCount; i++)
            {
                rightCards.Add(obstacleKind);
                rightValues.Add(value);
                rightVisualThemes.Add(rightThemes[i]);
            }

            level.LeftCards = leftCards;
            level.LeftValues = leftValues;
            level.LeftVisualThemes = leftVisualThemes;
            level.RightCards = rightCards;
            level.RightValues = rightValues;
            level.RightVisualThemes = rightVisualThemes;

            CoordinatedCreatureThemes.ApplyRedSideAndHand(level, handThemes);
        }

        private static void ApplyCreatureBoardWithOtherSide(LevelDefinition level, int handCount, int otherSideCount,
            int value)
        {
            CardKind redSolverKind = level.HandCards[0];
            CardKind redObstacleKind = CoordinatedCreatureThemes.OppositeCreature(redSolverKind);

            List<int> allThemes = CoordinatedCreatureThemes.BuildRedSideThemes(
                DistinctAnimalsForExtraLevel, level.CreatureTheme);
            List<int> redThemes = allThemes.GetRange(0, handCount);

            var usedThemes = new HashSet<int>(redThemes);
            int uniqueOtherCount = DistinctAnimalsForExtraLevel - handCount;
            List<int> uniqueOtherThemes = CoordinatedCreatureThemes.BuildOtherSideThemes(
                uniqueOtherCount, usedThemes, level.CreatureTheme);

            List<int> otherThemes = new List<int>(otherSideCount);
            for (int i = 0; i < otherSideCount; i++)
            {
                otherThemes.Add(uniqueOtherThemes[i % uniqueOtherCount]);
            }

            List<CardKind> otherObstacleKinds =
                CoordinatedCreatureThemes.BuildAlternatingCreatureKinds(otherSideCount);

            var leftCards = new List<CardKind> { CardKind.Box };
            var leftValues = new List<int> { 1 };
            var leftVisualThemes = new List<int> { -1 };

            for (int i = 0; i < handCount; i++)
            {
                leftCards.Add(redObstacleKind);
                leftValues.Add(value);
                leftVisualThemes.Add(redThemes[i]);
            }

            var rightCards = new List<CardKind>();
            var rightValues = new List<int>();
            var rightVisualThemes = new List<int>();

            for (int i = 0; i < otherSideCount; i++)
            {
                rightCards.Add(otherObstacleKinds[i]);
                rightValues.Add(value);
                rightVisualThemes.Add(otherThemes[i]);
            }

            level.LeftCards = leftCards;
            level.LeftValues = leftValues;
            level.LeftVisualThemes = leftVisualThemes;
            level.RightCards = rightCards;
            level.RightValues = rightValues;
            level.RightVisualThemes = rightVisualThemes;

            level.HandCards.Clear();
            level.HandValues.Clear();
            for (int i = 0; i < DistinctAnimalsForExtraLevel; i++)
            {
                if (i < handCount)
                {
                    level.HandCards.Add(redSolverKind);
                }
                else
                {
                    int otherIndex = i - handCount;
                    CardKind otherKind = otherIndex < otherObstacleKinds.Count
                        ? otherObstacleKinds[otherIndex]
                        : otherObstacleKinds[otherIndex % otherObstacleKinds.Count];
                    level.HandCards.Add(CoordinatedCreatureThemes.OppositeCreature(otherKind));
                }

                level.HandValues.Add(value);
            }

            CoordinatedCreatureThemes.ApplyRedSideAndOtherHand(level, redThemes, uniqueOtherThemes);
        }

        private static void ApplyDiceBoard(LevelDefinition level, int leftBesideBox, int rightCount, int baseValue)
        {
            CardKind solverKind = level.HandCards[0];
            CardKind obstacleKind = solverKind == CardKind.NegativeConstant
                ? CardKind.PositiveConstant
                : CardKind.NegativeConstant;

            var leftCards = new List<CardKind> { CardKind.Box };
            var leftValues = new List<int> { 1 };
            var leftVisualThemes = new List<int> { -1 };

            for (int i = 0; i < leftBesideBox; i++)
            {
                leftCards.Add(obstacleKind);
                leftValues.Add(baseValue + i);
                leftVisualThemes.Add(-1);
            }

            var rightCards = new List<CardKind>();
            var rightValues = new List<int>();
            var rightVisualThemes = new List<int>();

            for (int i = 0; i < rightCount; i++)
            {
                rightCards.Add(obstacleKind);
                rightValues.Add(baseValue + (i % leftBesideBox));
                rightVisualThemes.Add(-1);
            }

            level.LeftCards = leftCards;
            level.LeftValues = leftValues;
            level.LeftVisualThemes = leftVisualThemes;
            level.RightCards = rightCards;
            level.RightValues = rightValues;
            level.RightVisualThemes = rightVisualThemes;

            int handTileCount = leftBesideBox;
            level.HandCards.Clear();
            level.HandValues.Clear();
            level.HandVisualThemes.Clear();

            for (int i = 0; i < handTileCount; i++)
            {
                level.HandCards.Add(solverKind);
                level.HandValues.Add(baseValue + i);
                level.HandVisualThemes.Add(-1);
            }
        }
    }
}
