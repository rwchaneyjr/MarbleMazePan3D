using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelSolvabilityRules
    {
        public const int ExtraPuzzleFromIndex = 12;
        public const int ExtraPuzzleToIndex = 22;
        public const int ExtraPuzzleCount = ExtraPuzzleToIndex - ExtraPuzzleFromIndex;
        public const int MinOtherSideExtras = 4;
        public const int MaxOtherSideExtras = 4;

        public static bool ShouldConfigureBoxSide(int handCount) => handCount >= 2;

        public static bool IsExtraPuzzleLevel(int levelIndex) =>
            levelIndex >= ExtraPuzzleFromIndex && levelIndex < ExtraPuzzleToIndex;

        public static int OtherSideCountForExtraLevel(int extraIndex) =>
            MinOtherSideExtras + (extraIndex % (MaxOtherSideExtras - MinOtherSideExtras + 1));

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

            List<int> redThemes = CoordinatedCreatureThemes.BuildRedSideThemes(handCount, level.CreatureTheme);
            List<int> otherThemes = new List<int>(otherSideCount);
            for (int i = 0; i < otherSideCount; i++)
            {
                otherThemes.Add(redThemes[i % handCount]);
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
            for (int i = 0; i < handCount; i++)
            {
                level.HandCards.Add(redSolverKind);
                level.HandValues.Add(value);
            }

            CoordinatedCreatureThemes.ApplyRedSideAndHand(level, redThemes);
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
