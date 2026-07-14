using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    [Serializable]
    public class LevelDefinition
    {
        public string Title;
        public int Chapter;
        public int CreatureTheme;
        public int LeftCreatureTheme = -1;
        public int RightCreatureTheme = -1;
        public List<CardKind> LeftCards = new();
        public List<CardKind> RightCards = new();
        public List<int> LeftValues = new();
        public List<int> RightValues = new();
        public List<CardKind> HandCards = new();
        public List<int> HandValues = new();
        public int ParMoves = 6;
        public int ParCards = 2;
        public bool DragToMergePairs;
        /// <summary>Letter variable for Ch5+ only. '\0' = creatures use light/dark art (levels 1–63).</summary>
        public char VariableLetter = '\0';

        public BoardSide BuildSide(List<CardKind> kinds, List<int> values, int sideTheme = -1)
        {
            var side = new BoardSide();
            int resolvedTheme = sideTheme >= 0 ? sideTheme : CreatureTheme;
            for (int i = 0; i < kinds.Count; i++)
            {
                int value = values != null && i < values.Count ? values[i] : 1;
                int visualTheme = kinds[i] is CardKind.DayCreature or CardKind.NightCreature
                    ? resolvedTheme
                    : -1;
                char variableLetter = VariableLetter != '\0'
                    && kinds[i] is CardKind.DayCreature or CardKind.NightCreature
                    ? VariableLetter
                    : '\0';
                side.Cards.Add(new BoardCard(kinds[i], value, 1, visualTheme, variableLetter));
            }

            return side;
        }

        public List<BoardCard> BuildHand()
        {
            var hand = new List<BoardCard>();
            for (int i = 0; i < HandCards.Count; i++)
            {
                int value = HandValues != null && i < HandValues.Count ? HandValues[i] : 1;
                int visualTheme = HandCards[i] is CardKind.DayCreature or CardKind.NightCreature
                    ? CreatureTheme
                    : -1;
                char variableLetter = VariableLetter != '\0'
                    && HandCards[i] is CardKind.DayCreature or CardKind.NightCreature
                    ? VariableLetter
                    : '\0';
                hand.Add(new BoardCard(HandCards[i], value, 1, visualTheme, variableLetter));
            }

            return hand;
        }
    }
}
