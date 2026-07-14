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
        public List<char> LeftVariableLetters = new();
        public List<char> RightVariableLetters = new();
        public List<char> HandVariableLetters = new();
        public int ParMoves = 6;
        public int ParCards = 2;
        public bool DragToMergePairs;

        public BoardSide BuildSide(List<CardKind> kinds, List<int> values, int sideTheme = -1,
            List<char> variableLetters = null)
        {
            var side = new BoardSide();
            int resolvedTheme = sideTheme >= 0 ? sideTheme : CreatureTheme;
            for (int i = 0; i < kinds.Count; i++)
            {
                int value = values != null && i < values.Count ? values[i] : 1;
                int visualTheme = kinds[i] is CardKind.DayCreature or CardKind.NightCreature
                    ? resolvedTheme
                    : -1;
                char variableLetter = LetterForCard(kinds[i], i, variableLetters);
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
                char variableLetter = LetterForCard(HandCards[i], i, HandVariableLetters);
                hand.Add(new BoardCard(HandCards[i], value, 1, visualTheme, variableLetter));
            }

            return hand;
        }

        public BoardSide BuildLeftSide() =>
            BuildSide(LeftCards, LeftValues, LeftCreatureTheme, LeftVariableLetters);

        public BoardSide BuildRightSide() =>
            BuildSide(RightCards, RightValues, RightCreatureTheme, RightVariableLetters);

        private static char LetterForCard(CardKind kind, int index, List<char> letters)
        {
            if (kind is not (CardKind.DayCreature or CardKind.NightCreature))
            {
                return '\0';
            }

            if (letters != null && index < letters.Count && letters[index] != '\0')
            {
                return letters[index];
            }

            return '\0';
        }
    }
}
