using DragonBoxAlgebra.Core;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    public static class CardSpriteLoader
    {
        private static Sprite _fish;
        private static Sprite _turtle;
        private static Sprite _box;
        private static bool _initialized;

        public static void EnsureLoaded()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            LoadFromResources("Sprites");
            LoadFromResources("CardSprites");
            LoadFromResources("Cards");
        }

        public static Sprite ForCard(BoardCard card)
        {
            EnsureLoaded();
            return card.Kind switch
            {
                CardKind.DayCreature => _fish,
                CardKind.NightCreature => _turtle,
                CardKind.Box => _box,
                _ => null
            };
        }

        private static void LoadFromResources(string folder)
        {
            foreach (Sprite sprite in Resources.LoadAll<Sprite>(folder))
            {
                RegisterByName(sprite);
            }
        }

        private static void RegisterByName(Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            string name = sprite.name.ToLowerInvariant();
            if (IsFish(name))
            {
                _fish ??= sprite;
            }
            else if (IsTurtle(name))
            {
                _turtle ??= sprite;
            }
            else if (IsBox(name))
            {
                _box ??= sprite;
            }
        }

        private static bool IsFish(string name) =>
            name.Contains("fish") || name.Contains("day") || name == "a";

        private static bool IsTurtle(string name) =>
            name.Contains("turtle") || name.Contains("night") || name == "neg";

        private static bool IsBox(string name) =>
            name.Contains("box") || name.Contains("dragon");
    }
}
