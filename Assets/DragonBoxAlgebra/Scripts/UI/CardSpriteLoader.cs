using DragonBoxAlgebra.Core;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    public static class CardSpriteLoader
    {
        private static Sprite _dayCreature;
        private static Sprite _nightCreature;
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
                CardKind.DayCreature => _dayCreature,
                CardKind.NightCreature => _nightCreature,
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
            int dayScore = DayScore(name);
            int nightScore = NightScore(name);

            if (dayScore > 0 && (_dayCreature == null || dayScore > DayScore(_dayCreature.name.ToLowerInvariant())))
            {
                _dayCreature = sprite;
            }

            if (nightScore > 0 && (_nightCreature == null || nightScore > NightScore(_nightCreature.name.ToLowerInvariant())))
            {
                _nightCreature = sprite;
            }

            if (IsBox(name))
            {
                _box ??= sprite;
            }
        }

        private static int DayScore(string name)
        {
            if (name == "lightfish")
            {
                return 100;
            }

            if (name == "lightturtle")
            {
                return 90;
            }

            if (name.Contains("light") && name.Contains("fish"))
            {
                return 80;
            }

            if (name.Contains("light") && name.Contains("turtle"))
            {
                return 70;
            }

            if (name.Contains("fish") && !name.Contains("dark"))
            {
                return 50;
            }

            if (name.Contains("day"))
            {
                return 40;
            }

            return 0;
        }

        private static int NightScore(string name)
        {
            if (name == "darkturtle")
            {
                return 100;
            }

            if (name == "darkfish")
            {
                return 90;
            }

            if (name.Contains("dark") && name.Contains("turtle"))
            {
                return 80;
            }

            if (name.Contains("dark") && name.Contains("fish"))
            {
                return 70;
            }

            if (name.Contains("turtle") && !name.Contains("light"))
            {
                return 50;
            }

            if (name.Contains("night"))
            {
                return 40;
            }

            return 0;
        }

        private static bool IsBox(string name) =>
            name.Contains("box") || name.Contains("dragon");
    }
}
