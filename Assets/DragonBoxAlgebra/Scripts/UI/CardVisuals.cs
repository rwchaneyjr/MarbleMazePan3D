using DragonBoxAlgebra.Core;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    public static class CardVisuals
    {
        public static Color Background(CardKind kind) => kind switch
        {
            CardKind.Box => new Color(0.78f, 0.28f, 0.22f),
            CardKind.DayCreature => new Color(0.45f, 0.72f, 0.92f),
            CardKind.NightCreature => new Color(0.18f, 0.16f, 0.28f),
            CardKind.PositiveConstant => new Color(0.92f, 0.86f, 0.55f),
            CardKind.NegativeConstant => new Color(0.55f, 0.55f, 0.62f),
            _ => Color.white
        };

        public static string Label(BoardCard card) => card.Kind switch
        {
            CardKind.Box => "BOX",
            CardKind.DayCreature => $"DAY x{card.Value}",
            CardKind.NightCreature => $"NIGHT x{card.Value}",
            CardKind.PositiveConstant => $"+{card.Value}",
            CardKind.NegativeConstant => $"-{card.Value}",
            _ => "?"
        };

        public static string Emoji(BoardCard card) => card.Kind switch
        {
            CardKind.Box => "📦",
            CardKind.DayCreature => "🐟",
            CardKind.NightCreature => "🌙",
            CardKind.PositiveConstant => "🎲",
            CardKind.NegativeConstant => "🎲",
            _ => "?"
        };
    }
}
