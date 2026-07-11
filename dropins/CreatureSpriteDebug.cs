using System.Collections.Generic;
using System.Text;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    /// <summary>
    /// Console + on-screen debug for CreatureSprites PNG loading.
    /// Filter Console by: DragonBox
    /// </summary>
    public static class CreatureSpriteDebug
    {
        public static bool Enabled = true;

        private static readonly HashSet<string> LoggedFallbacks = new();

        public static int LoadedSpriteCount => CountLoadedSprites();

        public static bool HasProblems => LoadedSpriteCount < 16;

        public static string GetOnScreenMessage()
        {
            int loaded = LoadedSpriteCount;
            string levelTitle = LevelLibrary.Levels.Count > 0 ? LevelLibrary.Levels[0].Title : "(no levels)";

            if (loaded == 0)
            {
                return $"SPRITE DEBUG 0/16 | Put PNGs in Resources/CreatureSprites (lightFish, darkFish, …)";
            }

            if (loaded < 16)
            {
                return $"SPRITE DEBUG {loaded}/16 | Missing PNGs — Console filter: DragonBox";
            }

            return $"SPRITE DEBUG {loaded}/16 OK | Ch1 saved copy | {levelTitle}";
        }

        public static void LogFallback(BoardCard card, string source)
        {
            if (!Enabled || card.Kind == CardKind.Box)
            {
                return;
            }

            int theme = card.VisualTheme >= 0 ? card.VisualTheme : CreatureArt.ThemeIndex;
            string key = $"{card.Kind}_{theme}_{source}";
            if (!LoggedFallbacks.Add(key))
            {
                return;
            }

            bool light = CardFlipRules.IsLight(card);
            string expected = light ? CreatureArt.ExpectedLightPng(theme) : CreatureArt.ExpectedDarkPng(theme);

            Debug.LogWarning(
                $"[DragonBox] {source}: FALLBACK for {card.Kind} level-theme {theme} " +
                $"(PNG pair {CreatureArt.PngCreatureNameFor(theme)}). Expected: Resources/CreatureSprites/{expected}");
        }

        public static void LogStartup()
        {
            if (!Enabled)
            {
                return;
            }

            Debug.LogWarning("[DragonBox] === SPRITE DEBUG REPORT ===");

            CardSpriteLoader.Reset();
            CardSpriteLoader.EnsureLoaded();

            var lines = new StringBuilder();
            lines.AppendLine("========== DragonBox Sprite Debug (Ch1 saved copy) ==========");
            lines.AppendLine($"Custom sprites loaded: {LoadedSpriteCount}/16");
            lines.AppendLine("Folder: Assets/DragonBoxAlgebra/Resources/CreatureSprites/");
            lines.AppendLine(CardSpriteLoader.GetLoadSummary());

            if (LevelLibrary.Levels.Count > 0)
            {
                LevelDefinition level1 = LevelLibrary.Levels[0];
                lines.AppendLine($"Level 1: '{level1.Title}'");
                lines.AppendLine(
                    $"Level 1 PNG pair: {CreatureArt.PngCreatureNameFor(level1.CreatureTheme)} " +
                    $"(light{CreatureArt.PngCreatureNameFor(level1.CreatureTheme).Replace(" ", string.Empty)} / " +
                    $"dark{CreatureArt.PngCreatureNameFor(level1.CreatureTheme).Replace(" ", string.Empty)})");
            }

            lines.AppendLine("--- PNG slots (theme index maps to row mod 8) ---");
            lines.AppendLine(CardSpriteLoader.GetSlotReport());
            lines.AppendLine("--- Files found ---");
            lines.AppendLine(CardSpriteLoader.GetFileReport());

            if (CardSpriteLoader.UnmatchedFileCount > 0)
            {
                lines.AppendLine("--- Unmatched filenames ---");
                lines.AppendLine(CardSpriteLoader.GetUnmatchedReport());
            }

            lines.AppendLine("============================================================");

            string report = lines.ToString();
            if (HasProblems)
            {
                Debug.LogError(report);
            }
            else
            {
                Debug.LogWarning(report);
            }
        }

        public static void LogLevel(AlgebraBoard board, IReadOnlyList<BoardCard> hand, LevelDefinition level)
        {
            if (!Enabled)
            {
                return;
            }

            var lines = new StringBuilder();
            lines.AppendLine($"[DragonBox] {level.Title} — sprites (PNG theme {level.CreatureTheme} = {CreatureArt.PngCreatureNameFor(level.CreatureTheme)}):");

            LogSide(lines, "Left", board.Left.Cards);
            LogSide(lines, "Right", board.Right.Cards);
            LogSide(lines, "Hand", hand);

            if (lines.ToString().Contains("FALLBACK"))
            {
                Debug.LogWarning(lines.ToString());
            }
            else
            {
                Debug.Log(lines.ToString());
            }
        }

        private static void LogSide(StringBuilder lines, string sideName, IReadOnlyList<BoardCard> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                BoardCard card = cards[i];
                if (card.Kind is not (CardKind.DayCreature or CardKind.NightCreature or CardKind.Box))
                {
                    continue;
                }

                Sprite custom = CardSpriteLoader.ForCard(card);
                int theme = card.VisualTheme >= 0 ? card.VisualTheme : CreatureArt.ThemeIndex;
                string pngName = CreatureArt.PngCreatureNameFor(theme);
                string side = CardFlipRules.IsLight(card) ? "light" : "dark";

                if (custom != null)
                {
                    lines.AppendLine(
                        $"  {sideName}[{i}] {card.Kind} theme={theme}({pngName}) {side} -> CUSTOM '{custom.name}'");
                }
                else
                {
                    lines.AppendLine(
                        $"  {sideName}[{i}] {card.Kind} theme={theme}({pngName}) {side} -> FALLBACK");
                }
            }
        }

        private static int CountLoadedSprites()
        {
            int loaded = 0;
            for (int theme = 0; theme < 8; theme++)
            {
                if (CardSpriteLoader.HasCustomArt(theme, light: true))
                {
                    loaded++;
                }

                if (CardSpriteLoader.HasCustomArt(theme, light: false))
                {
                    loaded++;
                }
            }

            return loaded;
        }
    }
}
