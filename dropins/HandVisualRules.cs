using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class HandVisualRules
    {
        public static void EnsureDistinctHandVisuals(List<BoardCard> hand, int boardTheme)
        {
            if (hand.Count <= 1)
            {
                return;
            }

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
                && !AllSameCreatureKind(hand, creatureIndices))
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
