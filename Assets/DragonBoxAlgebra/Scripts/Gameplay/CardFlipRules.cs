using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class CardFlipRules
    {
        public static bool CanFlip(CardKind kind) =>
            kind is CardKind.DayCreature or CardKind.NightCreature
                or CardKind.PositiveConstant or CardKind.NegativeConstant;

        public static BoardCard Flip(BoardCard card)
        {
            BoardCard flipped = card.Clone();
            flipped.Kind = card.Kind switch
            {
                CardKind.DayCreature => CardKind.NightCreature,
                CardKind.NightCreature => CardKind.DayCreature,
                CardKind.PositiveConstant => CardKind.NegativeConstant,
                CardKind.NegativeConstant => CardKind.PositiveConstant,
                _ => card.Kind
            };
            return flipped;
        }

        public static bool IsLight(BoardCard card) =>
            card.Kind is CardKind.DayCreature or CardKind.PositiveConstant;

        public static bool IsDark(BoardCard card) =>
            card.Kind is CardKind.NightCreature or CardKind.NegativeConstant;
    }
}
