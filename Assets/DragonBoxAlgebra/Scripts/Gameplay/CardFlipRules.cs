using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class CardFlipRules
    {
        public static bool CanFlip(CardKind kind) =>
            kind is CardKind.DayCreature or CardKind.NightCreature
                or CardKind.PositiveConstant or CardKind.NegativeConstant;

        public static bool CanFlip(BoardCard card) => CanFlip(card.Kind);

        public static BoardCard Flip(BoardCard card)
        {
            BoardCard flipped = card.Kind switch
            {
                CardKind.DayCreature => new BoardCard(CardKind.NightCreature, card.Value, card.StackCount),
                CardKind.NightCreature => new BoardCard(CardKind.DayCreature, card.Value, card.StackCount),
                CardKind.PositiveConstant => new BoardCard(CardKind.NegativeConstant, card.Value, card.StackCount),
                CardKind.NegativeConstant => new BoardCard(CardKind.PositiveConstant, card.Value, card.StackCount),
                _ => card
            };

            flipped.Id = card.Id;
            return flipped;
        }

        public static bool IsLight(BoardCard card) =>
            card.Kind is CardKind.DayCreature or CardKind.PositiveConstant;

        public static bool IsDark(BoardCard card) =>
            card.Kind is CardKind.NightCreature or CardKind.NegativeConstant;
    }
}
