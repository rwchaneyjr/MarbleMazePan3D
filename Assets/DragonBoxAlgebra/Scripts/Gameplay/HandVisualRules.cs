using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class HandVisualRules
    {
        public static void AssignLevelHandVisualThemes(LevelDefinition level)
        {
            level.HandVisualThemes.Clear();
            if (level.HandCards.Count == 0)
            {
                return;
            }

            int creatureCount = 0;
            foreach (CardKind kind in level.HandCards)
            {
                if (kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    creatureCount++;
                }
            }

            List<int> themes = ThemeAssignment.DistinctThemes(creatureCount, level.CreatureTheme);
            int creatureIndex = 0;

            for (int i = 0; i < level.HandCards.Count; i++)
            {
                CardKind kind = level.HandCards[i];
                if (kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    level.HandVisualThemes.Add(themes[creatureIndex]);
                    creatureIndex++;
                    continue;
                }

                level.HandVisualThemes.Add(-1);
            }
        }

        public static HashSet<int> CollectHandCreatureThemes(LevelDefinition level)
        {
            var used = new HashSet<int>();
            for (int i = 0; i < level.HandCards.Count; i++)
            {
                if (level.HandCards[i] is not (CardKind.DayCreature or CardKind.NightCreature))
                {
                    continue;
                }

                if (level.HandVisualThemes != null
                    && i < level.HandVisualThemes.Count
                    && level.HandVisualThemes[i] >= 0)
                {
                    used.Add(level.HandVisualThemes[i]);
                }
            }

            return used;
        }

        public static void EnsureDistinctHandVisuals(List<BoardCard> hand, int boardTheme)
        {
            var creatureIndices = new List<int>();
            var used = new HashSet<int>();
            bool hasUnset = false;
            bool hasDuplicates = false;

            for (int i = 0; i < hand.Count; i++)
            {
                BoardCard card = hand[i];
                if (card.Kind is not (CardKind.DayCreature or CardKind.NightCreature))
                {
                    continue;
                }

                creatureIndices.Add(i);
                if (card.VisualTheme < 0)
                {
                    hasUnset = true;
                    continue;
                }

                if (!used.Add(card.VisualTheme))
                {
                    hasDuplicates = true;
                }
            }

            if (creatureIndices.Count == 0 || (!hasUnset && !hasDuplicates))
            {
                return;
            }

            List<int> themes = ThemeAssignment.DistinctThemes(creatureIndices.Count, boardTheme);
            for (int i = 0; i < creatureIndices.Count; i++)
            {
                int index = creatureIndices[i];
                BoardCard card = hand[index];
                card.VisualTheme = themes[i];
                hand[index] = card;
            }
        }
    }
}
