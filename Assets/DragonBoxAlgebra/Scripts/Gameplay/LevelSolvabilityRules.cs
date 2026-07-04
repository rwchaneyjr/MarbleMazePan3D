using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelSolvabilityRules
    {
        public const int ExtraPuzzleFromIndex = 12;
        public const int ExtraPuzzleToIndex = 22;

        public static bool ShouldConfigureBoxSide(int handCount) => handCount >= 2;

        public static bool IsDualSideChallenge(int levelIndex) =>
            levelIndex >= ExtraPuzzleFromIndex
            && levelIndex < ExtraPuzzleFromIndex + 5;

        public static bool IsExtraPuzzleLevel(int levelIndex) =>
            levelIndex >= ExtraPuzzleFromIndex && levelIndex < ExtraPuzzleToIndex;

        public static void ConfigureSolvableLevel(LevelDefinition level, int handCount, bool diceLevel, int value,
            int extraTileCount = 0, bool extraTilesOnOtherSide = true)
        {
            if (handCount < 2)
            {
                return;
            }

            int leftBesideBox = handCount;
            int rightCount = 0;
            if (extraTileCount > 0)
            {
                if (extraTilesOnOtherSide)
                {
                    rightCount = extraTileCount;
                }
                else if (handCount + extraTileCount <= 3)
                {
                    leftBesideBox = handCount + extraTileCount;
                }
                else
                {
                    rightCount = extraTileCount;
                }
            }

            if (diceLevel)
            {
                ApplyDiceBoard(level, leftBesideBox, rightCount, value);
                return;
            }

            ApplyCreatureBoard(level, leftBesideBox, rightCount, value);
        }

        public static void ConfigureDualSideChallenge(LevelDefinition level, bool diceLevel, int value)
        {
            if (diceLevel)
            {
                ApplyDiceBoard(level, leftBesideBox: 2, rightCount: 2, value);
                return;
            }

            ApplyCreatureBoard(level, leftBesideBox: 2, rightCount: 2, value);
        }

        private static void ApplyCreatureBoard(LevelDefinition level, int leftBesideBox, int rightCount, int value)
        {
            CardKind solverKind = level.HandCards[0];
            CardKind obstacleKind = solverKind == CardKind.NightCreature
                ? CardKind.DayCreature
                : CardKind.NightCreature;

            var usedThemes = HandVisualRules.CollectHandCreatureThemes(level);
            List<int> leftThemes = ThemeAssignment.DistinctThemesExcluding(
                leftBesideBox, usedThemes, level.CreatureTheme);
            foreach (int theme in leftThemes)
            {
                usedThemes.Add(theme);
            }

            List<int> rightThemes = ThemeAssignment.DistinctThemesExcluding(
                rightCount, usedThemes, level.CreatureTheme);

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
        }

        private static void ApplyDiceBoard(LevelDefinition level, int leftBesideBox, int rightCount, int value)
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
                leftValues.Add(value);
                leftVisualThemes.Add(-1);
            }

            var rightCards = new List<CardKind>();
            var rightValues = new List<int>();
            var rightVisualThemes = new List<int>();

            for (int i = 0; i < rightCount; i++)
            {
                rightCards.Add(obstacleKind);
                rightValues.Add(value);
                rightVisualThemes.Add(-1);
            }

            level.LeftCards = leftCards;
            level.LeftValues = leftValues;
            level.LeftVisualThemes = leftVisualThemes;
            level.RightCards = rightCards;
            level.RightValues = rightValues;
            level.RightVisualThemes = rightVisualThemes;
        }
    }
}
