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
        public const int MaxOtherSideExtras = 7;

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
                ApplyDiceBoardWithOtherSide(level, handCount, otherSideCount, value);
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
                CardKind solverKind = level.HandCards[0];
                CardKind diceObstacle = solverKind == CardKind.NegativeConstant
                    ? CardKind.PositiveConstant
                    : CardKind.NegativeConstant;

                level.LeftCards = new List<CardKind> { CardKind.Box, diceObstacle };
                level.LeftValues = new List<int> { 1, value };
                level.LeftVisualThemes = new List<int> { -1, -1 };
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
            var usedThemes = new HashSet<int>(redThemes);
            List<int> otherThemes =
                CoordinatedCreatureThemes.BuildOtherSideThemes(otherSideCount, usedThemes, level.CreatureTheme);
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

            for (int i = 0; i < otherSideCount; i++)
            {
                level.HandCards.Add(CoordinatedCreatureThemes.OppositeCreature(otherObstacleKinds[i]));
                level.HandValues.Add(value);
            }

            CoordinatedCreatureThemes.ApplyRedSideAndOtherHand(level, redThemes, otherThemes);
        }

        private static void ApplyDiceBoardWithOtherSide(LevelDefinition level, int handCount, int otherSideCount,
            int value)
        {
            CardKind redSolverKind = level.HandCards[0];
            CardKind redObstacleKind = redSolverKind == CardKind.NegativeConstant
                ? CardKind.PositiveConstant
                : CardKind.NegativeConstant;
            CardKind otherObstacleKind = redObstacleKind;
            CardKind otherSolverKind = redSolverKind;

            var leftCards = new List<CardKind> { CardKind.Box };
            var leftValues = new List<int> { 1 };
            var leftVisualThemes = new List<int> { -1 };

            for (int i = 0; i < handCount; i++)
            {
                leftCards.Add(redObstacleKind);
                leftValues.Add(value);
                leftVisualThemes.Add(-1);
            }

            var rightCards = new List<CardKind>();
            var rightValues = new List<int>();
            var rightVisualThemes = new List<int>();

            for (int i = 0; i < otherSideCount; i++)
            {
                rightCards.Add(otherObstacleKind);
                rightValues.Add(value);
                rightVisualThemes.Add(-1);
            }

            level.LeftCards = leftCards;
            level.LeftValues = leftValues;
            level.LeftVisualThemes = leftVisualThemes;
            level.RightCards = rightCards;
            level.RightValues = rightValues;
            level.RightVisualThemes = rightVisualThemes;

            level.HandCards.Clear();
            level.HandValues.Clear();
            for (int i = 0; i < handCount + otherSideCount; i++)
            {
                level.HandCards.Add(otherSolverKind);
                level.HandValues.Add(value);
            }

            level.HandVisualThemes.Clear();
            for (int i = 0; i < handCount + otherSideCount; i++)
            {
                level.HandVisualThemes.Add(-1);
            }
        }
    }
}
