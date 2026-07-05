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

            var creatureIndices = new List<int>();
            bool hasDay = false;
            bool hasNight = false;
            for (int i = 0; i < level.HandCards.Count; i++)
            {
                CardKind kind = level.HandCards[i];
                if (kind is CardKind.DayCreature or CardKind.NightCreature)
                {
                    creatureIndices.Add(i);
                    if (kind == CardKind.DayCreature)
                    {
                        hasDay = true;
                    }

                    if (kind == CardKind.NightCreature)
                    {
                        hasNight = true;
                    }
                }
            }

            for (int i = 0; i < level.HandCards.Count; i++)
            {
                level.HandVisualThemes.Add(-1);
            }

            if (creatureIndices.Count == 0)
            {
                return;
            }

            bool sharePairTheme = hasDay && hasNight && creatureIndices.Count == 2
                && !AllSameCreatureKind(level.HandCards, creatureIndices);
            List<int> themes = sharePairTheme
                ? new List<int> { ThemeAssignment.DistinctThemes(1, level.CreatureTheme)[0] }
                : ThemeAssignment.DistinctThemes(creatureIndices.Count, level.CreatureTheme);

            for (int i = 0; i < creatureIndices.Count; i++)
            {
                int handIndex = creatureIndices[i];
                int theme = sharePairTheme ? themes[0] : themes[i];
                level.HandVisualThemes[handIndex] = theme;
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
            bool hasDay = false;
            bool hasNight = false;

            for (int i = 0; i < hand.Count; i++)
            {
                BoardCard card = hand[i];
                if (card.Kind is not (CardKind.DayCreature or CardKind.NightCreature))
                {
                    continue;
                }

                if (card.Kind == CardKind.DayCreature)
                {
                    hasDay = true;
                }

                if (card.Kind == CardKind.NightCreature)
                {
                    hasNight = true;
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

            if (creatureIndices.Count == 0)
            {
                return;
            }

            if (hasDay && hasNight && creatureIndices.Count == 2
                && AllSameCreatureKind(hand, creatureIndices) == false)
            {
                if (!hasUnset)
                {
                    return;
                }

                int pairTheme = ThemeAssignment.DistinctThemes(1, boardTheme)[0];
                for (int i = 0; i < creatureIndices.Count; i++)
                {
                    int index = creatureIndices[i];
                    BoardCard card = hand[index];
                    card.VisualTheme = pairTheme;
                    hand[index] = card;
                }

                return;
            }

            if (!hasUnset && !hasDuplicates)
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

        private static bool AllSameCreatureKind(IReadOnlyList<CardKind> cards, IReadOnlyList<int> indices)
        {
            if (indices.Count == 0)
            {
                return true;
            }

            CardKind kind = cards[indices[0]];
            for (int i = 1; i < indices.Count; i++)
            {
                if (cards[indices[i]] != kind)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AllSameCreatureKind(List<BoardCard> hand, IReadOnlyList<int> indices)
        {
            if (indices.Count == 0)
            {
                return true;
            }

            CardKind kind = hand[indices[0]].Kind;
            for (int i = 1; i < indices.Count; i++)
            {
                if (hand[indices[i]].Kind != kind)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
