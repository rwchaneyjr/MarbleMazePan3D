using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// DragonBox-style intro: Ch1–Ch4 through level 62; Ch5 (63–80) variable images + red box;
    /// Ch6 (81–100) x + variables; Ch7 (101–139) sea + variables + number images
    /// (+ between tiles from 113; 129–139 = copy of 113–128 with number/variable images).
    /// </summary>
    public static class ChapterLevelGenerator
    {
        public const int LevelsPerChapter = 20;
        public const int Chapter1LevelCount = 12;
        public const int Chapter2LevelCount = 16;
        public const int Chapter3LevelCount = 15;
        public const int Chapter4LevelCount = 19;
        public const int Chapter5LevelCount = 18;
        public const int Chapter6LevelCount = 20;
        public const int Chapter7LevelCount = 39;
        public const int ChapterCount = 7;
        public const int TotalLevels = Chapter1LevelCount + Chapter2LevelCount + Chapter3LevelCount
            + Chapter4LevelCount + Chapter5LevelCount + Chapter6LevelCount + Chapter7LevelCount;

        /// <summary>First global level number (1-based) for Chapter 4 / Move Cards.</summary>
        public const int Chapter4StartLevel = Chapter1LevelCount + Chapter2LevelCount + Chapter3LevelCount + 1;

        /// <summary>First global level number (1-based) for Chapter 5 / Letter Variables.</summary>
        public const int Chapter5StartLevel = Chapter4StartLevel + Chapter4LevelCount;

        /// <summary>First global level number (1-based) for Chapter 6 / Multi Variables.</summary>
        public const int Chapter6StartLevel = Chapter5StartLevel + Chapter5LevelCount;

        /// <summary>First global level number (1-based) for Chapter 7 / Sea Creatures.</summary>
        public const int Chapter7StartLevel = Chapter6StartLevel + Chapter6LevelCount;

        /// <summary>Ch7 levels 1–6: x + sea creature light/dark images.</summary>
        public const int Chapter7SeaXLevelCount = 6;

        /// <summary>Ch7 levels 7–12: x + variable letter images.</summary>
        public const int Chapter7VariableLevelCount = 6;

        /// <summary>Ch7 levels 13–28 (global 113–128): sea + variable mix, + between tiles.</summary>
        public const int Chapter7MixedPlusStartDisplay = 13;

        /// <summary>Ch7 levels 29–39 (global 129–139): copy of 113–128 with number + variable images.</summary>
        public const int Chapter7AdditionStartDisplay = 29;

        /// <summary>Bump when curriculum changes — shown in Unity Console on Play.</summary>
        public const string CurriculumVersion = "2026-07-ch7-copy-113-128-as-129-139";

        /// <summary>From global level 64: alternate 1 vs 2 variable letters (one tile each, never duplicates).</summary>
        public const int VariableLetterCountStartLevel = 64;

        /// <summary>Up to and including level 85: only 1 or 2 variable letters.</summary>
        public const int VariableLetterCountEndLevel = 85;

        /// <summary>From global level 86: random 2 or 3 variable letters (one tile each).</summary>
        public const int HighVariableLetterCountStartLevel = 86;

        /// <summary>Levels 113–139 show a + sign between each board tile image.</summary>
        public const int PlusBetweenTilesStartLevel = 113;
        public const int PlusBetweenTilesEndLevel = 139;

        public static bool UsesPlusBetweenBoardTiles(int globalLevel) =>
            globalLevel >= PlusBetweenTilesStartLevel && globalLevel <= PlusBetweenTilesEndLevel;

        /// <summary>Levels 40–63 get one random creature on the side opposite the red box.</summary>
        public const int OppositeExtraTileStartLevel = 40;
        public const int OppositeExtraTileEndLevel = Chapter4StartLevel + Chapter4LevelCount - 1;

        private const int OppositeExtraTileStartIndex = OppositeExtraTileStartLevel - 1;
        private const int OppositeExtraTileEndIndex = OppositeExtraTileEndLevel - 1;

        private static readonly int[] ChapterLevelCounts =
        {
            Chapter1LevelCount,
            Chapter2LevelCount,
            Chapter3LevelCount,
            Chapter4LevelCount,
            Chapter5LevelCount,
            Chapter6LevelCount,
            Chapter7LevelCount
        };

        private static readonly string[] ChapterNames =
        {
            "Matching Pairs",
            "Opposite Cards",
            "Balance Sides",
            "Move Cards",
            "Variable Images",
            "x and Variables",
            "Sea Creatures"
        };

        private static readonly string[] SeaCreatureNames =
        {
            "Fish", "Turtle", "Clam", "Dolphin", "Eel", "Lobster", "Sea Horse", "Starfish"
        };

        public static IReadOnlyList<LevelDefinition> GenerateAll()
        {
            var levels = new List<LevelDefinition>(TotalLevels);
            levels.AddRange(GenerateChapter(1));
            levels.AddRange(GenerateChapter(2));
            levels.AddRange(GenerateChapter(3));
            levels.AddRange(GenerateChapter(4));
            levels.AddRange(GenerateChapter5());
            levels.AddRange(GenerateChapter6());
            levels.AddRange(GenerateChapter7());

            for (int i = OppositeExtraTileStartIndex;
                 i <= OppositeExtraTileEndIndex && i < levels.Count;
                 i++)
            {
                AddRandomExtraTileOppositeBox(levels[i], i);
            }

            HandRules.AssertAllHandCardsFlippable(levels);
            HandRules.AssertVariableHandCardsFlippable(levels);
            return levels;
        }

        public static int ChapterForLevelIndex(int levelIndex)
        {
            if (levelIndex < 0)
            {
                return 1;
            }

            int cursor = 0;
            for (int chapter = 0; chapter < ChapterCount; chapter++)
            {
                if (levelIndex < cursor + ChapterLevelCounts[chapter])
                {
                    return chapter + 1;
                }

                cursor += ChapterLevelCounts[chapter];
            }

            return ChapterCount;
        }

        public static int IndexWithinChapter(int levelIndex)
        {
            if (levelIndex < 0)
            {
                return 0;
            }

            int cursor = 0;
            for (int chapter = 0; chapter < ChapterCount; chapter++)
            {
                if (levelIndex < cursor + ChapterLevelCounts[chapter])
                {
                    return levelIndex - cursor;
                }

                cursor += ChapterLevelCounts[chapter];
            }

            return 0;
        }

        private static readonly char[] VariablePairLetters = { 'a', 'b', 'c', 'r' };

        private static IEnumerable<LevelDefinition> GenerateChapter5()
        {
            for (int i = 0; i < Chapter5LevelCount; i++)
            {
                int displayNumber = i + 1;
                int globalLevel = Chapter5StartLevel + displayNumber - 1;
                int theme = (5 * 3 + i) % 10;
                yield return BuildChapter5Level(globalLevel, theme, displayNumber);
            }
        }

        /// <summary>
        /// Ch5 (63–80): red box goal + variable images; one negative per letter in hand (reusable until cleared).
        /// </summary>
        private static LevelDefinition BuildChapter5Level(int globalLevel, int theme, int displayNumber)
        {
            int letterSeed = globalLevel * 7919 + 31;
            int countSeed = globalLevel * 7919 + 47;
            int letterCount = VariableLetterCountForGlobalLevel(globalLevel, countSeed);
            List<char> letters = PickDistinctVariableLetters(letterSeed, letterCount);
            string title =
                $"Ch5 • {ChapterNames[4]} {displayNumber} ({FormatVariableLettersLabel(letters)})";

            if (displayNumber is 7 or 8)
            {
                char first = letters[0];
                char second = letters.Count > 1 ? letters[1] : first;
                return MakeCh5MergedBoxLevel(title, theme, first, second, boxOnLeft: displayNumber == 7);
            }

            bool boxOnLeft = displayNumber <= 6
                ? displayNumber % 2 == 1
                : displayNumber % 2 == 0;
            return MakeCh5ImageVariableLevel(title, theme, letters, boxOnLeft);
        }

        private static LevelDefinition MakeCh5ImageVariableLevel(string title, int theme,
            IReadOnlyList<char> letters, bool boxOnLeft)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 5,
                CreatureTheme = theme,
                ParMoves = ParMovesForVariableLetterCount(letters.Count),
                ParCards = letters.Count
            };

            if (boxOnLeft)
            {
                AddBoxTile(level.LeftCards, level.LeftVariableLetters);
                AddOnePerLetterToSide(level.LeftCards, level.LeftVariableLetters, letters);
                AddOnePerLetterToSide(level.RightCards, level.RightVariableLetters, letters);
            }
            else
            {
                AddOnePerLetterToSide(level.LeftCards, level.LeftVariableLetters, letters);
                AddBoxTile(level.RightCards, level.RightVariableLetters);
                AddOnePerLetterToSide(level.RightCards, level.RightVariableLetters, letters);
            }

            AddHandNegativesForLetters(level, letters);
            level.LeftValues = ValuesFor(level.LeftCards);
            level.RightValues = ValuesFor(level.RightCards);
            level.HandValues = ValuesFor(level.HandCards);
            return level;
        }

        private static LevelDefinition MakeCh5MergedBoxLevel(string title, int theme, char firstLetter,
            char secondLetter, bool boxOnLeft)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 5,
                CreatureTheme = theme,
                ParMoves = firstLetter == secondLetter ? 1 : 2,
                ParCards = 0
            };

            var boxSideCards = new List<CardKind>();
            var boxSideLetters = new List<char>();
            AddBoxTile(boxSideCards, boxSideLetters);
            AddPairOnSide(boxSideCards, boxSideLetters, firstLetter);
            if (secondLetter != firstLetter)
            {
                AddPairOnSide(boxSideCards, boxSideLetters, secondLetter);
            }

            if (boxOnLeft)
            {
                level.LeftCards.AddRange(boxSideCards);
                level.LeftVariableLetters.AddRange(boxSideLetters);
            }
            else
            {
                level.RightCards.AddRange(boxSideCards);
                level.RightVariableLetters.AddRange(boxSideLetters);
            }

            level.LeftValues = ValuesFor(level.LeftCards);
            level.RightValues = ValuesFor(level.RightCards);
            level.HandValues = ValuesFor(level.HandCards);
            return level;
        }

        /// <summary>
        /// Level 63: two variables. 64–85: alternate 1- and 2-variable levels. 86–100: random 2 or 3 variables.
        /// Each letter appears at most once per side (never 2 a's or 2 b's).
        /// </summary>
        private static int VariableLetterCountForGlobalLevel(int globalLevel, int seed)
        {
            if (globalLevel >= HighVariableLetterCountStartLevel)
            {
                return new System.Random(seed).Next(2, 4);
            }

            if (globalLevel >= VariableLetterCountStartLevel && globalLevel <= VariableLetterCountEndLevel)
            {
                return (globalLevel - VariableLetterCountStartLevel) % 2 == 0 ? 1 : 2;
            }

            return 2;
        }

        private static List<char> PickDistinctVariableLetters(int seed, int count)
        {
            var rng = new System.Random(seed);
            var pool = new List<char>(VariablePairLetters);
            for (int i = 0; i < count && i < pool.Count; i++)
            {
                int swap = rng.Next(i, pool.Count);
                (pool[i], pool[swap]) = (pool[swap], pool[i]);
            }

            return pool.GetRange(0, System.Math.Min(count, pool.Count));
        }

        private static string FormatVariableLettersLabel(IReadOnlyList<char> letters)
        {
            if (letters.Count == 0)
            {
                return "images";
            }

            if (letters.Count == 1)
            {
                return $"{letters[0]} images";
            }

            return string.Join(", ", letters) + " images";
        }

        private static void AddOnePerLetterToSide(List<CardKind> cards, List<char> letters,
            IReadOnlyList<char> variableLetters)
        {
            foreach (char letter in variableLetters)
            {
                AddPositiveVariables(cards, letters, letter, 1);
            }
        }

        private static void AddHandNegativesForLetters(LevelDefinition level, IReadOnlyList<char> letters)
        {
            foreach (char letter in letters)
            {
                level.HandCards.Add(CardKind.NightCreature);
                level.HandVariableLetters.Add(letter);
            }
        }

        private static int ParMovesForVariableLetterCount(int letterCount) => 2 + letterCount * 2;

        private static void AddBoxTile(List<CardKind> cards, List<char> letters)
        {
            cards.Add(CardKind.Box);
            letters.Add('\0');
        }

        private static void AddPositiveVariables(List<CardKind> cards, List<char> letters, char pairLetter,
            int count)
        {
            for (int i = 0; i < count; i++)
            {
                cards.Add(CardKind.DayCreature);
                letters.Add(pairLetter);
            }
        }

        private static IEnumerable<LevelDefinition> GenerateChapter6()
        {
            for (int i = 0; i < Chapter6LevelCount; i++)
            {
                int displayNumber = i + 1;
                int theme = (6 * 3 + i) % 10;
                yield return BuildChapter6Level(i, theme, displayNumber);
            }
        }

        /// <summary>
        /// Ch6 (81–100): x goal + variable images; two negatives in hand; x alone wins.
        /// </summary>
        private static LevelDefinition BuildChapter6Level(int index, int theme, int displayNumber)
        {
            int globalLevel = Chapter6StartLevel + displayNumber - 1;
            int letterSeed = globalLevel * 7919 + 31;
            int countSeed = globalLevel * 7919 + 47;
            int letterCount = VariableLetterCountForGlobalLevel(globalLevel, countSeed);
            List<char> letters = PickDistinctVariableLetters(letterSeed, letterCount);
            string title =
                $"Ch6 • {ChapterNames[5]} {displayNumber} (x + {FormatVariableLettersLabel(letters)})";

            if (displayNumber is 7 or 8)
            {
                char first = letters[0];
                char second = letters.Count > 1 ? letters[1] : first;
                return MakeCh6MergedWinLevel(title, theme, first, second, xOnLeft: displayNumber == 7);
            }

            bool xOnLeft = displayNumber <= 6
                ? displayNumber % 2 == 1
                : displayNumber % 2 == 0;
            return MakeCh6MultiHandBalanceLevel(title, theme, letters, xOnLeft);
        }

        /// <summary>
        /// x side: x + one positive per variable letter; hand: one reusable negative per letter.
        /// </summary>
        private static LevelDefinition MakeCh6MultiHandBalanceLevel(string title, int theme,
            IReadOnlyList<char> letters, bool xOnLeft)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 6,
                CreatureTheme = theme,
                ParMoves = ParMovesForVariableLetterCount(letters.Count),
                ParCards = letters.Count
            };

            if (xOnLeft)
            {
                level.LeftCards.Add(CardKind.DayCreature);
                level.LeftVariableLetters.Add(VariableGoalRules.GoalLetter);
                AddOnePerLetterToSide(level.LeftCards, level.LeftVariableLetters, letters);
                AddOnePerLetterToSide(level.RightCards, level.RightVariableLetters, letters);
            }
            else
            {
                AddOnePerLetterToSide(level.LeftCards, level.LeftVariableLetters, letters);
                level.RightCards.Add(CardKind.DayCreature);
                level.RightVariableLetters.Add(VariableGoalRules.GoalLetter);
                AddOnePerLetterToSide(level.RightCards, level.RightVariableLetters, letters);
            }

            AddHandNegativesForLetters(level, letters);
            level.LeftValues = ValuesFor(level.LeftCards);
            level.RightValues = ValuesFor(level.RightCards);
            level.HandValues = ValuesFor(level.HandCards);
            return level;
        }

        /// <summary>
        /// Both pair vars already on the x side as +/-; merge each pair, dismiss swirls, x stands alone.
        /// </summary>
        private static LevelDefinition MakeCh6MergedWinLevel(string title, int theme, char firstLetter,
            char secondLetter, bool xOnLeft)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 6,
                CreatureTheme = theme,
                ParMoves = firstLetter == secondLetter ? 1 : 2,
                ParCards = 0
            };

            var xSideCards = new List<CardKind> { CardKind.DayCreature };
            var xSideLetters = new List<char> { VariableGoalRules.GoalLetter };
            AddPairOnSide(xSideCards, xSideLetters, firstLetter);
            if (secondLetter != firstLetter)
            {
                AddPairOnSide(xSideCards, xSideLetters, secondLetter);
            }

            if (xOnLeft)
            {
                level.LeftCards.AddRange(xSideCards);
                level.LeftVariableLetters.AddRange(xSideLetters);
            }
            else
            {
                level.RightCards.AddRange(xSideCards);
                level.RightVariableLetters.AddRange(xSideLetters);
            }

            level.LeftValues = ValuesFor(level.LeftCards);
            level.RightValues = ValuesFor(level.RightCards);
            level.HandValues = ValuesFor(level.HandCards);
            return level;
        }

        private static IEnumerable<LevelDefinition> GenerateChapter7()
        {
            for (int i = 0; i < Chapter7LevelCount; i++)
            {
                int displayNumber = i + 1;
                int globalLevel = Chapter7StartLevel + displayNumber - 1;
                int theme = (7 * 3 + i) % 10;
                yield return BuildChapter7Level(globalLevel, theme, displayNumber);
            }
        }

        /// <summary>
        /// Ch7 (101–139): 6 sea+x, 6 variables, 16 mixed + (113–128),
        /// then 11 addition levels (129–139) from the 3-hand 113–128 template with number + variable images.
        /// </summary>
        private static LevelDefinition BuildChapter7Level(int globalLevel, int theme, int displayNumber)
        {
            if (displayNumber <= Chapter7SeaXLevelCount)
            {
                int creatureTheme = displayNumber - 1;
                bool xLeft = displayNumber % 2 == 1;
                string creature = SeaCreatureNames[creatureTheme];
                string levelTitle = $"Ch7 • {ChapterNames[6]} {displayNumber} (x + {creature})";
                return MakeCh7SeaXBalanceLevel(levelTitle, creatureTheme, xLeft);
            }

            if (displayNumber <= Chapter7SeaXLevelCount + Chapter7VariableLevelCount)
            {
                int variableIndex = displayNumber - Chapter7SeaXLevelCount;
                int letterSeed = globalLevel * 7919 + 31;
                int countSeed = globalLevel * 7919 + 47;
                int letterCount = VariableLetterCountForGlobalLevel(globalLevel, countSeed);
                List<char> letters = PickDistinctVariableLetters(letterSeed, letterCount);
                int variableTheme = (variableIndex - 1) % SeaCreatureNames.Length;
                string variableTitle =
                    $"Ch7 • {ChapterNames[6]} {displayNumber} (x + {FormatVariableLettersLabel(letters)})";
                bool variableXLeft = variableIndex % 2 == 1;
                return MakeCh7VariableBalanceLevel(variableTitle, variableTheme, letters, variableXLeft);
            }

            if (displayNumber < Chapter7AdditionStartDisplay)
            {
                int mixedIndex = displayNumber - Chapter7MixedPlusStartDisplay;
                int mixedTheme = mixedIndex % SeaCreatureNames.Length;
                int tileCount = mixedIndex % 2 == 0 ? 2 : 3;
                bool mixedXLeft = mixedIndex % 2 == 0;
                string mixedTitle =
                    $"Ch7 • {ChapterNames[6]} {displayNumber} (x + sea + vars, {tileCount} each side)";
                return MakeCh7MixedSeaVariableLevel(mixedTitle, globalLevel, mixedTheme, mixedXLeft, tileCount);
            }

            // 129–139: exact copy of the 113–128 mixed template — only images change
            // (sea creature slots → number PNGs; variable slots stay letter PNGs).
            int additionIndex = displayNumber - Chapter7AdditionStartDisplay;
            int additionTheme = additionIndex % SeaCreatureNames.Length;
            int additionTileCount = additionIndex % 2 == 0 ? 2 : 3;
            bool additionXLeft = additionIndex % 2 == 0;
            string additionTitle =
                $"Ch7 • {ChapterNames[6]} {displayNumber} (x + numbers + vars, {additionTileCount} each side)";
            return MakeCh7NumberVariableFromMixedTemplate(additionTitle, globalLevel, additionTheme,
                additionXLeft, additionTileCount);
        }

        /// <summary>x on one side; light sea creature on both sides; dark sea creature in hand.</summary>
        private static LevelDefinition MakeCh7SeaXBalanceLevel(string title, int seaTheme, bool xOnLeft)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 7,
                CreatureTheme = seaTheme,
                ParMoves = 3,
                ParCards = 1
            };

            if (xOnLeft)
            {
                level.LeftCards.Add(CardKind.DayCreature);
                level.LeftVariableLetters.Add(VariableGoalRules.GoalLetter);
                level.LeftCards.Add(CardKind.DayCreature);
                level.LeftVariableLetters.Add('\0');
                level.RightCards.Add(CardKind.DayCreature);
                level.RightVariableLetters.Add('\0');
            }
            else
            {
                level.LeftCards.Add(CardKind.DayCreature);
                level.LeftVariableLetters.Add('\0');
                level.RightCards.Add(CardKind.DayCreature);
                level.RightVariableLetters.Add(VariableGoalRules.GoalLetter);
                level.RightCards.Add(CardKind.DayCreature);
                level.RightVariableLetters.Add('\0');
            }

            level.HandCards.Add(CardKind.NightCreature);
            level.HandVariableLetters.Add('\0');
            level.LeftValues = ValuesFor(level.LeftCards);
            level.RightValues = ValuesFor(level.RightCards);
            level.HandValues = ValuesFor(level.HandCards);
            return level;
        }

        /// <summary>Ch7 variable block: x + letter images; reusable negatives in hand.</summary>
        private static LevelDefinition MakeCh7VariableBalanceLevel(string title, int seaTheme,
            IReadOnlyList<char> letters, bool xOnLeft)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 7,
                CreatureTheme = seaTheme,
                ParMoves = ParMovesForVariableLetterCount(letters.Count),
                ParCards = letters.Count
            };

            if (xOnLeft)
            {
                level.LeftCards.Add(CardKind.DayCreature);
                level.LeftVariableLetters.Add(VariableGoalRules.GoalLetter);
                AddOnePerLetterToSide(level.LeftCards, level.LeftVariableLetters, letters);
                AddOnePerLetterToSide(level.RightCards, level.RightVariableLetters, letters);
            }
            else
            {
                AddOnePerLetterToSide(level.LeftCards, level.LeftVariableLetters, letters);
                level.RightCards.Add(CardKind.DayCreature);
                level.RightVariableLetters.Add(VariableGoalRules.GoalLetter);
                AddOnePerLetterToSide(level.RightCards, level.RightVariableLetters, letters);
            }

            AddHandNegativesForLetters(level, letters);
            level.LeftValues = ValuesFor(level.LeftCards);
            level.RightValues = ValuesFor(level.RightCards);
            level.HandValues = ValuesFor(level.HandCards);
            return level;
        }

        /// <summary>
        /// Global 113–128: x + mix of sea creature images and variable symbols (2–3 per side);
        /// hand shows each dark tile needed to balance (sea or variable).
        /// </summary>
        private static LevelDefinition MakeCh7MixedSeaVariableLevel(string title, int globalLevel, int seaTheme,
            bool xOnLeft, int tileCount)
        {
            int letterSeed = globalLevel * 7919 + 31;
            int variableSlotCount = tileCount == 2 ? 1 : (globalLevel % 2 == 0 ? 2 : 1);
            int seaSlotCount = tileCount - variableSlotCount;
            List<char> letters = PickDistinctVariableLetters(letterSeed, variableSlotCount);

            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 7,
                CreatureTheme = seaTheme,
                ParMoves = 2 + tileCount * 2,
                ParCards = tileCount
            };

            var slots = new List<bool>();
            for (int i = 0; i < variableSlotCount; i++)
            {
                slots.Add(true);
            }

            for (int i = 0; i < seaSlotCount; i++)
            {
                slots.Add(false);
            }

            ShuffleSlots(slots, globalLevel * 7919 + 59);

            int letterIndex = 0;
            if (xOnLeft)
            {
                level.LeftCards.Add(CardKind.DayCreature);
                level.LeftVariableLetters.Add(VariableGoalRules.GoalLetter);
                letterIndex = 0;
                AddMixedSlotsToSide(level.LeftCards, level.LeftVariableLetters, slots, letters, ref letterIndex);
                letterIndex = 0;
                AddMixedSlotsToSide(level.RightCards, level.RightVariableLetters, slots, letters, ref letterIndex);
            }
            else
            {
                letterIndex = 0;
                AddMixedSlotsToSide(level.LeftCards, level.LeftVariableLetters, slots, letters, ref letterIndex);
                level.RightCards.Add(CardKind.DayCreature);
                level.RightVariableLetters.Add(VariableGoalRules.GoalLetter);
                letterIndex = 0;
                AddMixedSlotsToSide(level.RightCards, level.RightVariableLetters, slots, letters, ref letterIndex);
            }

            letterIndex = 0;
            foreach (bool isVariable in slots)
            {
                level.HandCards.Add(CardKind.NightCreature);
                level.HandVariableLetters.Add(isVariable ? letters[letterIndex++] : '\0');
            }

            level.LeftValues = ValuesFor(level.LeftCards);
            level.RightValues = ValuesFor(level.RightCards);
            level.HandValues = ValuesFor(level.HandCards);
            return level;
        }

        private static void AddMixedSlotsToSide(List<CardKind> cards, List<char> letters, List<bool> slots,
            IReadOnlyList<char> variableLetters, ref int letterIndex)
        {
            foreach (bool isVariable in slots)
            {
                cards.Add(CardKind.DayCreature);
                letters.Add(isVariable ? variableLetters[letterIndex++] : '\0');
            }
        }

        /// <summary>
        /// Global 129–139: copy of <see cref="MakeCh7MixedSeaVariableLevel"/> (113–128).
        /// Same layout, tile counts, x side, shuffle, hand count, and + between tiles.
        /// Only change: sea slots become number images; variable slots stay letter images.
        /// </summary>
        private static LevelDefinition MakeCh7NumberVariableFromMixedTemplate(string title, int globalLevel,
            int seaTheme, bool xOnLeft, int tileCount)
        {
            int letterSeed = globalLevel * 7919 + 31;
            // Identical slot split to MakeCh7MixedSeaVariableLevel.
            int variableSlotCount = tileCount == 2 ? 1 : (globalLevel % 2 == 0 ? 2 : 1);
            int numberSlotCount = tileCount - variableSlotCount;
            List<char> letters = PickDistinctVariableLetters(letterSeed, variableSlotCount);
            int numberSeed = globalLevel * 7919 + 101;

            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 7,
                CreatureTheme = seaTheme,
                ParMoves = 2 + tileCount * 2,
                ParCards = tileCount
            };

            // true = variable (same as mixed), false = number (was sea in 113–128).
            var slots = new List<bool>();
            for (int i = 0; i < variableSlotCount; i++)
            {
                slots.Add(true);
            }

            for (int i = 0; i < numberSlotCount; i++)
            {
                slots.Add(false);
            }

            ShuffleSlots(slots, globalLevel * 7919 + 59);

            var numberValues = new List<int>();
            for (int i = 0; i < numberSlotCount; i++)
            {
                numberValues.Add(AdditionNumberValue(numberSeed, i));
            }

            if (xOnLeft)
            {
                level.LeftCards.Add(CardKind.DayCreature);
                level.LeftVariableLetters.Add(VariableGoalRules.GoalLetter);
                level.LeftValues.Add(1);
                AddNumberVariableSlotsToSide(level.LeftCards, level.LeftVariableLetters, level.LeftValues,
                    slots, letters, numberValues);
                AddNumberVariableSlotsToSide(level.RightCards, level.RightVariableLetters, level.RightValues,
                    slots, letters, numberValues);
            }
            else
            {
                AddNumberVariableSlotsToSide(level.LeftCards, level.LeftVariableLetters, level.LeftValues,
                    slots, letters, numberValues);
                level.RightCards.Add(CardKind.DayCreature);
                level.RightVariableLetters.Add(VariableGoalRules.GoalLetter);
                level.RightValues.Add(1);
                AddNumberVariableSlotsToSide(level.RightCards, level.RightVariableLetters, level.RightValues,
                    slots, letters, numberValues);
            }

            int letterIndex = 0;
            int numberIndex = 0;
            foreach (bool isVariable in slots)
            {
                if (isVariable)
                {
                    level.HandCards.Add(CardKind.NightCreature);
                    level.HandVariableLetters.Add(letters[letterIndex++]);
                    level.HandValues.Add(1);
                }
                else
                {
                    // Was dark sea in 113–128; now negative number image.
                    level.HandCards.Add(CardKind.NegativeConstant);
                    level.HandVariableLetters.Add('\0');
                    level.HandValues.Add(numberValues[numberIndex++]);
                }
            }

            return level;
        }

        private static void AddNumberVariableSlotsToSide(List<CardKind> cards, List<char> letters,
            List<int> values, List<bool> slots, IReadOnlyList<char> variableLetters,
            IReadOnlyList<int> numberValues)
        {
            int letterIndex = 0;
            int numberIndex = 0;
            foreach (bool isVariable in slots)
            {
                if (isVariable)
                {
                    cards.Add(CardKind.DayCreature);
                    letters.Add(variableLetters[letterIndex++]);
                    values.Add(1);
                }
                else
                {
                    // Was light sea in 113–128; now positive number image.
                    cards.Add(CardKind.PositiveConstant);
                    letters.Add('\0');
                    values.Add(numberValues[numberIndex++]);
                }
            }
        }

        private static int AdditionNumberValue(int numberSeed, int numberIndex)
        {
            int baseValue = 1 + ((numberSeed + numberIndex * 17) % 5);
            return Math.Clamp(baseValue, 1, 9);
        }

        private static void ShuffleSlots(List<bool> slots, int seed)
        {
            var rng = new System.Random(seed);
            for (int i = slots.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (slots[i], slots[j]) = (slots[j], slots[i]);
            }
        }

        private static void AddPairOnSide(List<CardKind> cards, List<char> letters, char pairLetter)
        {
            cards.Add(CardKind.DayCreature);
            cards.Add(CardKind.NightCreature);
            letters.Add(pairLetter);
            letters.Add(pairLetter);
        }

        private static IEnumerable<LevelDefinition> GenerateChapter(int chapter)
        {
            if (chapter == 1)
            {
                foreach (LevelDefinition level in GenerateChapter1())
                {
                    yield return level;
                }

                yield break;
            }

            int displayNumber = 0;
            for (int i = 0; i < LevelsPerChapter; i++)
            {
                if (chapter == 2 && ShouldSkipChapter2Slot(i))
                {
                    continue;
                }

                if (chapter == 3 && ShouldSkipChapter3Slot(i))
                {
                    continue;
                }

                if (chapter == 4 && ShouldSkipChapter4Slot(i))
                {
                    continue;
                }

                displayNumber++;
                int theme = (chapter * 3 + i) % 10;
                yield return chapter switch
                {
                    2 => BuildChapter2Level(i, theme, displayNumber),
                    3 => BuildChapter3Level(i, theme, displayNumber),
                    _ => BuildChapter4Level(i, theme)
                };
            }
        }

        private static IEnumerable<LevelDefinition> GenerateChapter1()
        {
            int displayNumber = 0;
            for (int i = 0; i < LevelsPerChapter; i++)
            {
                if (ShouldSkipChapter1Slot(i))
                {
                    continue;
                }

                displayNumber++;
                int theme = (3 + i) % 10;
                yield return BuildChapter1Level(i, theme, displayNumber);
            }
        }

        private static bool ShouldSkipChapter1Slot(int index) =>
            index is >= 6 and <= 9 or >= 16 and <= 19;

        private static bool ShouldSkipChapter2Slot(int index) =>
            index is 16 or 17 or 18 or 19;

        private static bool ShouldSkipChapter3Slot(int index) =>
            index is 0 or 8 or 9 or 10 or 11;

        private static bool ShouldSkipChapter4Slot(int index) => index == 19;

        /// <summary>Ch1: drag day/night together on one side → *; box alone on the other.</summary>
        private static LevelDefinition BuildChapter1Level(int index, int theme, int displayNumber)
        {
            string title = $"Ch1 • {ChapterNames[0]} {displayNumber}";

            if (index < 10)
            {
                return Make(
                    title,
                    chapter: 1,
                    theme,
                    left: new[] { CardKind.DayCreature, CardKind.NightCreature },
                    right: new[] { CardKind.Box },
                    hand: System.Array.Empty<CardKind>(),
                    parMoves: 2,
                    parCards: 0,
                    dragToMergePairs: true);
            }

            return Make(
                title,
                chapter: 1,
                theme,
                left: new[] { CardKind.Box },
                right: new[] { CardKind.DayCreature, CardKind.NightCreature },
                hand: System.Array.Empty<CardKind>(),
                parMoves: 2,
                parCards: 0,
                dragToMergePairs: true);
        }

        /// <summary>Ch2: dual-pair drag levels only (opposite-hand intro levels 29–30 removed).</summary>
        private static LevelDefinition BuildChapter2Level(int index, int theme, int displayNumber)
        {
            string title = $"Ch2 • {ChapterNames[1]} {displayNumber}";

            return MakeDualPairDragLevel(
                title,
                chapter: 2,
                theme,
                index,
                differentSideThemes: index >= 9);
        }

        /// <summary>Ch3: balance hand tile → ? → merge * on each side.</summary>
        private static LevelDefinition BuildChapter3Level(int index, int theme, int displayNumber)
        {
            string title = $"Ch3 • {ChapterNames[2]} {displayNumber}";

            // Levels 31–37: intro balance — box + day, day on other side, night in hand.
            if (index is >= 1 and <= 7)
            {
                return MakeBalanceIntroLevel(title, theme, displayNumber, boxOnLeft: index % 2 == 1);
            }

            // Levels 38–42: same pattern, box alternates.
            if (index is >= 12 and <= 16)
            {
                return MakeBalanceIntroLevel(title, theme, displayNumber, boxOnLeft: index % 2 == 0);
            }

            // Levels 43–45: board already has opposite pair — merge to win.
            if (index < 19)
            {
                return index % 2 == 1
                    ? Make(
                        title,
                        chapter: 3,
                        theme,
                        left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                        right: System.Array.Empty<CardKind>(),
                        hand: System.Array.Empty<CardKind>(),
                        parMoves: 1,
                        parCards: 0)
                    : Make(
                        title,
                        chapter: 3,
                        theme,
                        left: System.Array.Empty<CardKind>(),
                        right: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                        hand: System.Array.Empty<CardKind>(),
                        parMoves: 1,
                        parCards: 0);
            }

            return MakeBalanceIntroLevel(title, theme, displayNumber, boxOnLeft: true);
        }

        /// <summary>
        /// Solvable balance puzzle: place night, fill ?, then cancel day+night on each side.
        /// Box on left: drag night to left. Box on right: drag night to left (creates ? on right).
        /// </summary>
        private static LevelDefinition MakeBalanceIntroLevel(string title, int theme, int displayNumber,
            bool boxOnLeft, int parMoves = 3)
        {
            return boxOnLeft
                ? Make(
                    title,
                    chapter: 3,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature },
                    right: new[] { CardKind.DayCreature },
                    hand: new[] { CardKind.NightCreature },
                    parMoves: parMoves,
                    parCards: 1)
                : Make(
                    title,
                    chapter: 3,
                    theme,
                    left: new[] { CardKind.DayCreature },
                    right: new[] { CardKind.Box, CardKind.DayCreature },
                    hand: new[] { CardKind.NightCreature },
                    parMoves: parMoves,
                    parCards: 1);
        }

        /// <summary>Ch4: multi-hand balance — same solvable board as Ch3, extra hand slots for sequencing.</summary>
        private static LevelDefinition BuildChapter4Level(int index, int theme)
        {
            int handCount = index < 10 ? 2 : 3;
            bool boxOnLeft = index % 2 == 0;
            string title = $"Ch4 • {ChapterNames[3]} {index + 1}";
            return MakeCh4BalanceLevel(title, theme, handCount, boxOnLeft);
        }

        /// <summary>
        /// Solvable Ch4 puzzle: balance one hand tile (box+day / day), merge both sides, dismiss swirls.
        /// Extra hand tiles teach sequential play; the first tile completes the board.
        /// </summary>
        private static LevelDefinition MakeCh4BalanceLevel(string title, int theme, int handCount, bool boxOnLeft)
        {
            var hand = new CardKind[handCount];
            for (int i = 0; i < handCount; i++)
            {
                hand[i] = i % 2 == 0 ? CardKind.NightCreature : CardKind.DayCreature;
            }

            int parMoves = 2 + handCount;
            return boxOnLeft
                ? Make(
                    title,
                    chapter: 4,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature },
                    right: new[] { CardKind.DayCreature },
                    hand: hand,
                    parMoves: parMoves,
                    parCards: 1)
                : Make(
                    title,
                    chapter: 4,
                    theme,
                    left: new[] { CardKind.DayCreature },
                    right: new[] { CardKind.Box, CardKind.DayCreature },
                    hand: hand,
                    parMoves: parMoves,
                    parCards: 1);
        }

        private static LevelDefinition MakeDualPairDragLevel(string title, int chapter, int theme, int index,
            bool differentSideThemes = false)
        {
            bool boxOnLeft = index % 2 == 0;
            LevelDefinition level = boxOnLeft
                ? Make(
                    title,
                    chapter,
                    theme,
                    left: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                    right: new[] { CardKind.DayCreature, CardKind.NightCreature },
                    hand: System.Array.Empty<CardKind>(),
                    parMoves: 4,
                    parCards: 0,
                    dragToMergePairs: true)
                : Make(
                    title,
                    chapter,
                    theme,
                    left: new[] { CardKind.DayCreature, CardKind.NightCreature },
                    right: new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature },
                    hand: System.Array.Empty<CardKind>(),
                    parMoves: 4,
                    parCards: 0,
                    dragToMergePairs: true);

            if (differentSideThemes)
            {
                List<int> themes = ThemeAssignment.DistinctThemes(2, theme);
                level.LeftCreatureTheme = themes[0];
                level.RightCreatureTheme = themes[1];
            }

            return level;
        }

        private static LevelDefinition Make(string title, int chapter, int theme,
            CardKind[] left, CardKind[] right, CardKind[] hand, int parMoves, int parCards,
            bool dragToMergePairs = false)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = chapter,
                CreatureTheme = theme,
                DragToMergePairs = dragToMergePairs,
                LeftCards = new List<CardKind>(left),
                RightCards = new List<CardKind>(right),
                HandCards = new List<CardKind>(hand),
                ParMoves = parMoves,
                ParCards = parCards
            };

            level.LeftValues = ValuesFor(level.LeftCards);
            level.RightValues = ValuesFor(level.RightCards);
            level.HandValues = ValuesFor(level.HandCards);
            return level;
        }

        private static List<int> ValuesFor(List<CardKind> cards)
        {
            var values = new List<int>();
            foreach (CardKind kind in cards)
            {
                values.Add(kind is CardKind.DayCreature or CardKind.NightCreature
                    or CardKind.PositiveConstant or CardKind.NegativeConstant
                    ? 1
                    : 1);
            }

            return values;
        }

        /// <summary>One random day or night on the side away from the red box.</summary>
        private static void AddRandomExtraTileOppositeBox(LevelDefinition level, int globalLevelIndex)
        {
            if (!TryGetOppositeSideLists(level, out List<CardKind> oppositeCards, out List<int> oppositeValues))
            {
                return;
            }

            var rng = new System.Random(globalLevelIndex * 7919 + 31);
            CardKind extra = PickExtraCreature(oppositeCards, rng);
            oppositeCards.Add(extra);
            oppositeValues.Add(1);
        }

        private static CardKind PickExtraCreature(List<CardKind> oppositeCards, System.Random rng)
        {
            bool hasDay = false;
            bool hasNight = false;
            foreach (CardKind kind in oppositeCards)
            {
                if (kind == CardKind.DayCreature)
                {
                    hasDay = true;
                }
                else if (kind == CardKind.NightCreature)
                {
                    hasNight = true;
                }
            }

            if (hasDay && !hasNight)
            {
                return CardKind.NightCreature;
            }

            if (hasNight && !hasDay)
            {
                return CardKind.DayCreature;
            }

            return rng.Next(2) == 0 ? CardKind.DayCreature : CardKind.NightCreature;
        }

        private static bool TryGetOppositeSideLists(LevelDefinition level, out List<CardKind> oppositeCards,
            out List<int> oppositeValues)
        {
            oppositeCards = null;
            oppositeValues = null;

            bool boxOnLeft = level.LeftCards.Contains(CardKind.Box);
            bool boxOnRight = level.RightCards.Contains(CardKind.Box);
            if (boxOnLeft == boxOnRight)
            {
                return false;
            }

            if (boxOnLeft)
            {
                oppositeCards = level.RightCards;
                oppositeValues = level.RightValues;
            }
            else
            {
                oppositeCards = level.LeftCards;
                oppositeValues = level.LeftValues;
            }

            return true;
        }
    }
}
