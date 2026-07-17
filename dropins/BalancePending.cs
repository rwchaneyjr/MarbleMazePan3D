using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public class BalancePending
    {
        public BoardCard Card;
        public string PlacedSide;
        public int PlacedIndex;
        public int HandIndex;
        public int HoleInsertIndex;

        public string HoleSide => PlacedSide == "Left" ? "Right" : "Left";

        public bool Matches(BoardCard other)
        {
            if (Card.Kind != other.Kind || Card.Value != other.Value)
            {
                return false;
            }

            if (Card.VariableLetter == '\0' && other.VariableLetter == '\0')
            {
                return true;
            }

            return Card.VariableLetter == other.VariableLetter;
        }

        /// <summary>
        /// Same flippable identity (creature/theme/letter or +/- value). Light/dark may differ.
        /// Used so flipping the hand tile after placing one side still allows filling the ? hole.
        /// </summary>
        public bool MatchesFamily(BoardCard other)
        {
            if (Card.Value != other.Value)
            {
                return false;
            }

            bool cardCreature = Card.Kind is CardKind.DayCreature or CardKind.NightCreature;
            bool otherCreature = other.Kind is CardKind.DayCreature or CardKind.NightCreature;
            if (cardCreature && otherCreature)
            {
                if (Card.VariableLetter != '\0' || other.VariableLetter != '\0')
                {
                    return Card.VariableLetter == other.VariableLetter;
                }

                return Card.VisualTheme == other.VisualTheme;
            }

            bool cardConst = Card.Kind is CardKind.PositiveConstant or CardKind.NegativeConstant;
            bool otherConst = other.Kind is CardKind.PositiveConstant or CardKind.NegativeConstant;
            if (cardConst && otherConst)
            {
                return true;
            }

            return Matches(other);
        }
    }
}
