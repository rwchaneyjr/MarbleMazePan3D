using DragonBoxAlgebra.Core;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    public static class EmojiSprites
    {
        private static Sprite _fish;
        private static Sprite _turtle;
        private static Sprite _dice;
        private static Sprite _smiley;
        private static Sprite _box;
        private static Sprite _divide;

        public static Sprite For(BoardCard card)
        {
            Sprite custom = CardSpriteLoader.ForCard(card);
            if (custom != null)
            {
                return custom;
            }

            return card.Kind switch
            {
                CardKind.DayCreature => _fish ??= SpriteFactory.CreateFishSprite(),
                CardKind.NightCreature => _turtle ??= SpriteFactory.CreateTurtleSprite(),
                CardKind.PositiveConstant => _dice ??= SpriteFactory.CreateDiceSprite(false),
                CardKind.NegativeConstant => _dice ??= SpriteFactory.CreateDiceSprite(true),
                CardKind.One => _smiley ??= SpriteFactory.CreateSmileySprite(),
                CardKind.Box => _box ??= SpriteFactory.CreateBoxSprite(),
                CardKind.DivideTool => _divide ??= SpriteFactory.CreateDivideSprite(),
                _ => null
            };
        }
    }
}
