using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    /// <summary>
    /// DragonBox-style intro: Ch1–Ch4 through level 62; Ch5 (63–80) variable images + red box;
    /// Ch6 (81–100) x + variables; Ch7 (101–150) sea + variables + numbers (+ between tiles from 113);
    /// 129–139 exact copies of 118–128; 140–150 copy 129–139 with sea slots → number images
    /// and a letter opposite x (b,c,b,r,a,r,a,b,a,c,b) instead of scene 0;
    /// Ch8 (151–165) multiplication with addition (a·x + b = c) and divide-both-sides;
    /// Ch9 (166–180) same as Ch8 with a letter RHS (a·x + b = letter);
    /// Ch10 (181–200) mix: multi-letter both sides (e.g. 3·x + b + 5 = 6·a + r)
    /// and simpler letter−const (e.g. 2·x + a = c − 7).
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
        public const int Chapter7LevelCount = 50;
        public const int Chapter8LevelCount = 15;
        public const int Chapter9LevelCount = 15;
        public const int Chapter10LevelCount = 20;
        public const int ChapterCount = 10;
        public const int TotalLevels = Chapter1LevelCount + Chapter2LevelCount + Chapter3LevelCount
            + Chapter4LevelCount + Chapter5LevelCount + Chapter6LevelCount + Chapter7LevelCount
            + Chapter8LevelCount + Chapter9LevelCount + Chapter10LevelCount;

        /// <summary>First global level number (1-based) for Chapter 4 / Move Cards.</summary>
        public const int Chapter4StartLevel = Chapter1LevelCount + Chapter2LevelCount + Chapter3LevelCount + 1;

        /// <summary>First global level number (1-based) for Chapter 5 / Letter Variables.</summary>
        public const int Chapter5StartLevel = Chapter4StartLevel + Chapter4LevelCount;

        /// <summary>First global level number (1-based) for Chapter 6 / Multi Variables.</summary>
        public const int Chapter6StartLevel = Chapter5StartLevel + Chapter5LevelCount;

        /// <summary>First global level number (1-based) for Chapter 7 / Sea Creatures.</summary>
        public const int Chapter7StartLevel = Chapter6StartLevel + Chapter6LevelCount;

        /// <summary>First global level number (1-based) for Chapter 8 / Multiply + Addition.</summary>
        public const int Chapter8StartLevel = Chapter7StartLevel + Chapter7LevelCount;

        /// <summary>Last global level number (1-based) for Chapter 8 (151–165).</summary>
        public const int Chapter8EndLevel = Chapter8StartLevel + Chapter8LevelCount - 1;

        /// <summary>First global level number (1-based) for Chapter 9 / Multiply + Letter.</summary>
        public const int Chapter9StartLevel = Chapter8EndLevel + 1;

        /// <summary>Last global level number (1-based) for Chapter 9 (166–180).</summary>
        public const int Chapter9EndLevel = Chapter9StartLevel + Chapter9LevelCount - 1;

        /// <summary>First global level number (1-based) for Chapter 10 / Multi-letter both sides.</summary>
        public const int Chapter10StartLevel = Chapter9EndLevel + 1;

        /// <summary>Last global level number (1-based) for Chapter 10 (181–200).</summary>
        public const int Chapter10EndLevel = Chapter10StartLevel + Chapter10LevelCount - 1;

        /// <summary>Ch7 levels 1–6: x + sea creature light/dark images.</summary>
        public const int Chapter7SeaXLevelCount = 6;

        /// <summary>Ch7 levels 7–12: x + variable letter images.</summary>
        public const int Chapter7VariableLevelCount = 6;

        /// <summary>Ch7 levels 13–28 (global 113–128): sea + variable mix, + between tiles.</summary>
        public const int Chapter7MixedPlusStartDisplay = 13;

        /// <summary>Ch7 levels 29–39 (global 129–139): exact copies of 118–128.</summary>
        public const int Chapter7CopyStartDisplay = 29;

        /// <summary>Ch7 levels 40–50 (global 140–150): copies of 129–139 with numbers instead of sea.</summary>
        public const int Chapter7Copy140StartDisplay = 40;

        /// <summary>Global 1-based: first level that uses number tiles (and 0 cancel symbol).</summary>
        public const int NumberLevelsStartLevel = 140;

        /// <summary>Bump when curriculum changes — shown in Unity Console on Play.</summary>
        public const string CurriculumVersion = "2026-07-151-180-order-intro";

        /// <summary>
        /// Levels 151–180: show "First undo addition… / Second undo multiplication"
        /// until the player's first drag.
        /// </summary>
        public static bool UsesOrderOfOperationsIntro(int globalLevel) =>
            globalLevel >= Chapter8StartLevel && globalLevel <= Chapter9EndLevel;

        /// <summary>
        /// First global level (1-based) of each distinct problem type.
        /// Next/Skip jumps to these instead of every individual puzzle.
        /// </summary>
        public static readonly int[] ProblemTypeStartLevels =
        {
            1,   // Ch1 Matching Pairs
            13,  // Ch2 Opposite Cards
            29,  // Ch3 Balance Sides
            44,  // Ch4 Move Cards
            63,  // Ch5 Variable Images
            81,  // Ch6 x and Variables
            101, // Ch7 sea + x
            107, // Ch7 variable letters
            113, // Ch7 mixed + between tiles
            129, // Ch7 copies of mixed
            140, // Ch7 numbers + letter opposite x
            151, // Ch8 multiply + add (number RHS)
            166, // Ch9 multiply + add (letter RHS)
            181  // Ch10 multi-letter both sides
        };

        /// <summary>
        /// Next problem-type start after the current global level (1-based).
        /// If already on/after the last type, wraps to level 1.
        /// </summary>
        public static int GetNextProblemTypeStartLevel(int currentGlobalLevel1Based)
        {
            for (int i = 0; i < ProblemTypeStartLevels.Length; i++)
            {
                if (ProblemTypeStartLevels[i] > currentGlobalLevel1Based)
                {
                    return ProblemTypeStartLevels[i];
                }
            }

            return ProblemTypeStartLevels[0];
        }

        /// <summary>0-based level index for the next problem type after current 0-based index.</summary>
        public static int GetNextProblemTypeLevelIndex(int currentLevelIndex0Based)
        {
            int nextGlobal = GetNextProblemTypeStartLevel(currentLevelIndex0Based + 1);
            int index = nextGlobal - 1;
            if (index < 0)
            {
                return 0;
            }

            if (index > TotalLevels - 1)
            {
                return TotalLevels - 1;
            }

            return index;
        }

        /// <summary>From global level 64: alternate 1 vs 2 variable letters (one tile each, never duplicates).</summary>
        public const int VariableLetterCountStartLevel = 64;

        /// <summary>Up to and including level 85: only 1 or 2 variable letters.</summary>
        public const int VariableLetterCountEndLevel = 85;

        /// <summary>From global level 86: random 2 or 3 variable letters (one tile each).</summary>
        public const int HighVariableLetterCountStartLevel = 86;

        /// <summary>Ch3 Balance Sides begins at global level 29 (after Ch1+Ch2).</summary>
        public const int Chapter3BalanceStartLevel = 29;

        /// <summary>Levels 113–150 show a + sign between each board tile image.</summary>
        public const int PlusBetweenTilesStartLevel = 113;
        public const int PlusBetweenTilesEndLevel = 150;

        public static bool UsesPlusBetweenBoardTiles(int globalLevel) =>
            (globalLevel >= PlusBetweenTilesStartLevel && globalLevel <= PlusBetweenTilesEndLevel)
            || (globalLevel >= Chapter8StartLevel && globalLevel <= Chapter10EndLevel);

        /// <summary>Multiply+add divide UI/rules — levels 151–200 (Ch8–Ch10).</summary>
        public static bool UsesMultiplyAddition(int globalLevel) =>
            globalLevel >= Chapter8StartLevel && globalLevel <= Chapter10EndLevel;

        /// <summary>Levels 166–200: letter answer expression (Ch9 single letter, Ch10 multi-letter).</summary>
        public static bool UsesMultiplyLetterRhs(int globalLevel) =>
            globalLevel >= Chapter9StartLevel && globalLevel <= Chapter10EndLevel;

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
            Chapter7LevelCount,
            Chapter8LevelCount,
            Chapter9LevelCount,
            Chapter10LevelCount
        };

        private static readonly string[] ChapterNames =
        {
            "Matching Pairs",
            "Opposite Cards",
            "Balance Sides",
            "Move Cards",
            "Variable Images",
            "x and Variables",
            "Sea Creatures",
            "Multiply and Add",
            "Multiply and Letter",
            "Letters Both Sides"
        };

        /// <summary>
        /// Ch8 (151–165): (coeff, addend, rhs). RHS is single-digit dice opposite x.
        /// Cancel addend → 0; keep RHS; divide by coeff → x = rhs/coeff (e.g. 9/2).
        /// Level 151: 2·x + 3 = 9, hand 2 and 3.
        /// </summary>
        private static readonly int[,] MultiplyAdditionSpecs =
        {
            { 2, 3, 9 }, // 151: 2x+3=9 → x=9/2
            { 3, 1, 7 }, // 152: 3x+1=7 → x=7/3
            { 2, 1, 7 }, // 153: 2x+1=7 → x=7/2
            { 4, 1, 9 }, // 154: 4x+1=9 → x=9/4
            { 3, 2, 8 }, // 155: 3x+2=8 → x=8/3
            { 2, 5, 9 }, // 156: 2x+5=9 → x=9/2
            { 5, 1, 6 }, // 157: 5x+1=6 → x=6/5
            { 4, 3, 7 }, // 158: 4x+3=7 → x=7/4
            { 3, 4, 7 }, // 159: 3x+4=7 → x=7/3
            { 2, 1, 9 }, // 160: 2x+1=9 → x=9/2
            { 4, 1, 5 }, // 161: 4x+1=5 → x=5/4
            { 5, 2, 7 }, // 162: 5x+2=7 → x=7/5
            { 3, 1, 4 }, // 163: 3x+1=4 → x=4/3
            { 2, 3, 9 }, // 164: 2x+3=9 → x=9/2
            { 4, 5, 9 }, // 165: 4x+5=9 → x=9/4
        };

        /// <summary>
        /// Ch9 (166–180): (coeff, addend, letterCode) letterCode is char as int ('a','b','c','r').
        /// Example: 3·x + 2 = b.
        /// </summary>
        private static readonly int[,] MultiplyLetterSpecs =
        {
            { 3, 2, 'b' }, // 166: 3x+2=b
            { 2, 1, 'a' }, // 167: 2x+1=a
            { 4, 3, 'c' }, // 168: 4x+3=c
            { 2, 5, 'b' }, // 169: 2x+5=b
            { 5, 1, 'r' }, // 170: 5x+1=r
            { 3, 4, 'a' }, // 171: 3x+4=a
            { 4, 1, 'b' }, // 172: 4x+1=b
            { 2, 3, 'c' }, // 173: 2x+3=c
            { 3, 1, 'r' }, // 174: 3x+1=r
            { 5, 2, 'a' }, // 175: 5x+2=a
            { 2, 4, 'b' }, // 176: 2x+4=b
            { 4, 5, 'c' }, // 177: 4x+5=c
            { 3, 5, 'r' }, // 178: 3x+5=r
            { 5, 3, 'b' }, // 179: 5x+3=b
            { 2, 2, 'a' }, // 180: 2x+2=a
        };

        /// <summary>
        /// Ch10 (181–200) type A — multi-letter both sides:
        /// (coeffX, letterB, constAdd, coeffA, letterA, letterR)
        /// → coeffX·x + letterB + constAdd = coeffA·letterA + letterR
        /// </summary>
        private static readonly int[,] MultiplyMultiLetterSpecs =
        {
            { 3, 'b', 5, 6, 'a', 'r' },
            { 2, 'a', 3, 4, 'b', 'c' },
            { 4, 'c', 2, 3, 'a', 'b' },
            { 5, 'r', 1, 2, 'b', 'a' },
            { 2, 'b', 4, 5, 'c', 'r' },
            { 3, 'a', 2, 4, 'r', 'b' },
            { 4, 'b', 3, 2, 'a', 'c' },
            { 5, 'c', 4, 3, 'b', 'r' },
            { 2, 'r', 5, 6, 'a', 'b' },
            { 3, 'c', 1, 5, 'b', 'a' },
        };

        /// <summary>
        /// Ch10 type B — simpler: (coeff, letterA, letterC, constSub)
        /// → coeff·x + letterA = letterC − constSub
        /// e.g. 2·x + a = c − 7. About half of 181–200 use this form.
        /// </summary>
        private static readonly int[,] MultiplyLetterMinusConstSpecs =
        {
            { 2, 'a', 'c', 7 },
            { 3, 'b', 'a', 5 },
            { 4, 'c', 'r', 3 },
            { 5, 'r', 'b', 2 },
            { 2, 'b', 'c', 4 },
            { 3, 'a', 'r', 6 },
            { 4, 'r', 'a', 1 },
            { 5, 'c', 'b', 7 },
            { 2, 'c', 'a', 5 },
            { 3, 'b', 'c', 4 },
        };

        /// <summary>
        /// Which Ch10 slots (0–19) use type B (letter − const). Fixed mix ≈ half, shuffled.
        /// </summary>
        private static readonly bool[] Chapter10UsesLetterMinusConst =
        {
            false, true, false, true, true, false, true, false, false, true,
            true, false, true, false, true, false, false, true, false, true
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
            levels.AddRange(GenerateChapter8());
            levels.AddRange(GenerateChapter9());
            levels.AddRange(GenerateChapter10());

            for (int i = OppositeExtraTileStartIndex;
                 i <= OppositeExtraTileEndIndex && i < levels.Count;
                 i++)
            {
                AddRandomExtraTileOppositeBox(levels[i], i);
            }

            // 113–139: scene 0 opposite x/box (x = 0).
            // 140–150: letter opposite x in order b,c,b,r,a,r,a,b,a,c,b (x = letter).
            for (int globalLevel = PlusBetweenTilesStartLevel; globalLevel <= PlusBetweenTilesEndLevel; globalLevel++)
            {
                int index = globalLevel - 1;
                if (index < 0 || index >= levels.Count)
                {
                    continue;
                }

                if (globalLevel >= NumberLevelsStartLevel)
                {
                    int letterIndex = globalLevel - NumberLevelsStartLevel;
                    AddLetterOppositeGoal(levels[index], OppositeLetterFor140To150[letterIndex]);
                }
                else
                {
                    AddZeroOppositeGoal(levels[index]);
                }
            }

            // One unique image per hand — drop duplicates and light/dark (or +/-) pairs.
            HandRules.DedupeLevelHandDefinitions(levels);
            HandRules.AssertAllHandCardsFlippable(levels);
            HandRules.AssertVariableHandCardsFlippable(levels);
            AssertGoalXOnOneSideForNumberLevels(levels);
            AssertOppositeAnswerForAdditionLevels(levels);
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

        /// <summary>Global 140–150: replace scene 0 with this letter opposite x (x = letter).</summary>
        private static readonly char[] OppositeLetterFor140To150 =
        {
            'b', 'c', 'b', 'r', 'a', 'r', 'a', 'b', 'a', 'c', 'b'
        };

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

        private static IEnumerable<LevelDefinition> GenerateChapter8()
        {
            for (int i = 0; i < Chapter8LevelCount; i++)
            {
                int displayNumber = i + 1;
                int coeff = MultiplyAdditionSpecs[i, 0];
                int addend = MultiplyAdditionSpecs[i, 1];
                int rhs = MultiplyAdditionSpecs[i, 2];
                string title =
                    $"Ch8 • {ChapterNames[7]} {displayNumber} ({coeff}·x + {addend} = {rhs})";
                yield return MakeMultiplyAdditionLevel(title, coeff, addend, rhs);
            }
        }

        private static IEnumerable<LevelDefinition> GenerateChapter9()
        {
            for (int i = 0; i < Chapter9LevelCount; i++)
            {
                int displayNumber = i + 1;
                int coeff = MultiplyLetterSpecs[i, 0];
                int addend = MultiplyLetterSpecs[i, 1];
                char letter = (char)MultiplyLetterSpecs[i, 2];
                string title =
                    $"Ch9 • {ChapterNames[8]} {displayNumber} ({coeff}·x + {addend} = {letter})";
                yield return MakeMultiplyLetterLevel(title, coeff, addend, letter);
            }
        }

        private static IEnumerable<LevelDefinition> GenerateChapter10()
        {
            int multiIndex = 0;
            int minusIndex = 0;
            for (int i = 0; i < Chapter10LevelCount; i++)
            {
                int displayNumber = i + 1;
                if (Chapter10UsesLetterMinusConst[i])
                {
                    int coeff = MultiplyLetterMinusConstSpecs[minusIndex, 0];
                    char letterA = (char)MultiplyLetterMinusConstSpecs[minusIndex, 1];
                    char letterC = (char)MultiplyLetterMinusConstSpecs[minusIndex, 2];
                    int constSub = MultiplyLetterMinusConstSpecs[minusIndex, 3];
                    minusIndex++;
                    string title =
                        $"Ch10 • {ChapterNames[9]} {displayNumber} ({coeff}·x + {letterA} = {letterC} − {constSub})";
                    yield return MakeMultiplyLetterMinusConstLevel(title, coeff, letterA, letterC, constSub);
                }
                else
                {
                    int coeffX = MultiplyMultiLetterSpecs[multiIndex, 0];
                    char letterB = (char)MultiplyMultiLetterSpecs[multiIndex, 1];
                    int constAdd = MultiplyMultiLetterSpecs[multiIndex, 2];
                    int coeffA = MultiplyMultiLetterSpecs[multiIndex, 3];
                    char letterA = (char)MultiplyMultiLetterSpecs[multiIndex, 4];
                    char letterR = (char)MultiplyMultiLetterSpecs[multiIndex, 5];
                    multiIndex++;
                    string title =
                        $"Ch10 • {ChapterNames[9]} {displayNumber} ({coeffX}·x + {letterB} + {constAdd} = {coeffA}·{letterA} + {letterR})";
                    yield return MakeMultiplyMultiLetterLevel(title, coeffX, letterB, constAdd, coeffA, letterA,
                        letterR);
                }
            }
        }

        /// <summary>
        /// a·x + b = c with hand a and b. Cancel b → 0; keep c; divide by a → x = c/a (e.g. 9/2).
        /// </summary>
        private static LevelDefinition MakeMultiplyAdditionLevel(string title, int coeff, int addend, int rhs)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 8,
                CreatureTheme = 0,
                ParMoves = 6,
                ParCards = 2
            };

            // Left: coeff · x + addend
            level.LeftCards.Add(CardKind.PositiveConstant);
            level.LeftVariableLetters.Add('\0');
            level.LeftValues.Add(coeff);

            level.LeftCards.Add(CardKind.DayCreature);
            level.LeftVariableLetters.Add(VariableGoalRules.GoalLetter);
            level.LeftValues.Add(1);

            level.LeftCards.Add(CardKind.PositiveConstant);
            level.LeftVariableLetters.Add('\0');
            level.LeftValues.Add(addend);

            // Right: rhs (stays after cancel — answer is rhs/coeff)
            level.RightCards.Add(CardKind.PositiveConstant);
            level.RightVariableLetters.Add('\0');
            level.RightValues.Add(rhs);

            // Hand: opposites of board numbers (flippable +/-). Flip addend to cancel;
            // flip coefficient to + before dropping on the divide line.
            level.HandCards.Add(CardKind.NegativeConstant);
            level.HandVariableLetters.Add('\0');
            level.HandValues.Add(coeff);

            level.HandCards.Add(CardKind.NegativeConstant);
            level.HandVariableLetters.Add('\0');
            level.HandValues.Add(addend);

            return level;
        }

        /// <summary>
        /// a·x + b = letter with hand a and b. Same play as Ch8; RHS is a variable letter.
        /// </summary>
        private static LevelDefinition MakeMultiplyLetterLevel(string title, int coeff, int addend, char letter)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 9,
                CreatureTheme = 0,
                ParMoves = 6,
                ParCards = 2
            };

            // Left: coeff · x + addend
            level.LeftCards.Add(CardKind.PositiveConstant);
            level.LeftVariableLetters.Add('\0');
            level.LeftValues.Add(coeff);

            level.LeftCards.Add(CardKind.DayCreature);
            level.LeftVariableLetters.Add(VariableGoalRules.GoalLetter);
            level.LeftValues.Add(1);

            level.LeftCards.Add(CardKind.PositiveConstant);
            level.LeftVariableLetters.Add('\0');
            level.LeftValues.Add(addend);

            // Right: letter (e.g. b)
            level.RightCards.Add(CardKind.DayCreature);
            level.RightVariableLetters.Add(letter);
            level.RightValues.Add(1);

            // Hand: opposites of board numbers (flippable +/-).
            level.HandCards.Add(CardKind.NegativeConstant);
            level.HandVariableLetters.Add('\0');
            level.HandValues.Add(coeff);

            level.HandCards.Add(CardKind.NegativeConstant);
            level.HandVariableLetters.Add('\0');
            level.HandValues.Add(addend);

            return level;
        }

        /// <summary>
        /// coeffX·x + letterB + constAdd = coeffA·letterA + letterR.
        /// Hand: coeffX, constAdd, letterB (flip to cancel addends; divide by coeffX).
        /// </summary>
        private static LevelDefinition MakeMultiplyMultiLetterLevel(string title, int coeffX, char letterB,
            int constAdd, int coeffA, char letterA, char letterR)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 10,
                CreatureTheme = 0,
                ParMoves = 10,
                ParCards = 3
            };

            // Left: coeffX · x + letterB + constAdd
            level.LeftCards.Add(CardKind.PositiveConstant);
            level.LeftVariableLetters.Add('\0');
            level.LeftValues.Add(coeffX);

            level.LeftCards.Add(CardKind.DayCreature);
            level.LeftVariableLetters.Add(VariableGoalRules.GoalLetter);
            level.LeftValues.Add(1);

            level.LeftCards.Add(CardKind.DayCreature);
            level.LeftVariableLetters.Add(letterB);
            level.LeftValues.Add(1);

            level.LeftCards.Add(CardKind.PositiveConstant);
            level.LeftVariableLetters.Add('\0');
            level.LeftValues.Add(constAdd);

            // Right: coeffA · letterA + letterR
            level.RightCards.Add(CardKind.PositiveConstant);
            level.RightVariableLetters.Add('\0');
            level.RightValues.Add(coeffA);

            level.RightCards.Add(CardKind.DayCreature);
            level.RightVariableLetters.Add(letterA);
            level.RightValues.Add(1);

            level.RightCards.Add(CardKind.DayCreature);
            level.RightVariableLetters.Add(letterR);
            level.RightValues.Add(1);

            // Hand: cancel tools + divisor (flippable).
            level.HandCards.Add(CardKind.NegativeConstant);
            level.HandVariableLetters.Add('\0');
            level.HandValues.Add(coeffX);

            level.HandCards.Add(CardKind.NegativeConstant);
            level.HandVariableLetters.Add('\0');
            level.HandValues.Add(constAdd);

            level.HandCards.Add(CardKind.NightCreature);
            level.HandVariableLetters.Add(letterB);
            level.HandValues.Add(1);

            return level;
        }

        /// <summary>
        /// coeff·x + letterA = letterC − constSub (e.g. 2·x + a = c − 7).
        /// Hand: coeff, letterA (cancel), +constSub (cancel the −const on the right).
        /// </summary>
        private static LevelDefinition MakeMultiplyLetterMinusConstLevel(string title, int coeff, char letterA,
            char letterC, int constSub)
        {
            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 10,
                CreatureTheme = 0,
                ParMoves = 8,
                ParCards = 3
            };

            // Left: coeff · x + letterA
            level.LeftCards.Add(CardKind.PositiveConstant);
            level.LeftVariableLetters.Add('\0');
            level.LeftValues.Add(coeff);

            level.LeftCards.Add(CardKind.DayCreature);
            level.LeftVariableLetters.Add(VariableGoalRules.GoalLetter);
            level.LeftValues.Add(1);

            level.LeftCards.Add(CardKind.DayCreature);
            level.LeftVariableLetters.Add(letterA);
            level.LeftValues.Add(1);

            // Right: letterC − constSub  (letter + negative constant)
            level.RightCards.Add(CardKind.DayCreature);
            level.RightVariableLetters.Add(letterC);
            level.RightValues.Add(1);

            level.RightCards.Add(CardKind.NegativeConstant);
            level.RightVariableLetters.Add('\0');
            level.RightValues.Add(constSub);

            // Hand: divisor, cancel letterA, cancel −const with +const
            level.HandCards.Add(CardKind.NegativeConstant);
            level.HandVariableLetters.Add('\0');
            level.HandValues.Add(coeff);

            level.HandCards.Add(CardKind.NightCreature);
            level.HandVariableLetters.Add(letterA);
            level.HandValues.Add(1);

            level.HandCards.Add(CardKind.PositiveConstant);
            level.HandVariableLetters.Add('\0');
            level.HandValues.Add(constSub);

            return level;
        }

        /// <summary>
        /// Ch7 (101–150): 6 sea+x, 6 variables, 16 mixed + (113–128),
        /// 11 exact copies of 118–128 as 129–139, then 11 copies of 129–139 as 140–150 with sea→numbers.
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

            if (displayNumber < Chapter7CopyStartDisplay)
            {
                int mixedIndex = displayNumber - Chapter7MixedPlusStartDisplay;
                int mixedTheme = mixedIndex % SeaCreatureNames.Length;
                int tileCount = mixedIndex % 2 == 0 ? 2 : 3;
                bool mixedXLeft = mixedIndex % 2 == 0;
                string mixedTitle =
                    $"Ch7 • {ChapterNames[6]} {displayNumber} (x + sea + vars, {tileCount} each side)";
                return MakeCh7MixedSeaVariableLevel(mixedTitle, globalLevel, mixedTheme, mixedXLeft, tileCount);
            }

            if (displayNumber < Chapter7Copy140StartDisplay)
            {
                // 129–139: exact copies of global 118–128 (same layout/seeds).
                int sourceDisplay = displayNumber - 11; // 29→18 … 39→28
                int sourceGlobalLevel = globalLevel - 11; // 129→118 … 139→128
                int sourceMixedIndex = sourceDisplay - Chapter7MixedPlusStartDisplay;
                int sourceTheme = sourceMixedIndex % SeaCreatureNames.Length;
                int sourceTileCount = sourceMixedIndex % 2 == 0 ? 2 : 3;
                bool sourceXLeft = sourceMixedIndex % 2 == 0;
                string copyTitle =
                    $"Ch7 • {ChapterNames[6]} {displayNumber} (x + sea + vars, {sourceTileCount} each side; from {sourceGlobalLevel})";
                return MakeCh7MixedSeaVariableLevel(copyTitle, sourceGlobalLevel, sourceTheme, sourceXLeft,
                    sourceTileCount);
            }

            // 140–150: copies of 129–139 with sea creature slots replaced by number PNGs.
            int from129Display = displayNumber - 11; // 40→29 … 50→39
            int from129Global = globalLevel - 11; // 140→129 … 150→139
            // 129–139 themselves copy 118–128, so reuse that underlying layout seed.
            int layoutDisplay = from129Display - 11; // 40→18 … 50→28
            int layoutGlobal = from129Global - 11; // 140→118 … 150→128
            int layoutMixedIndex = layoutDisplay - Chapter7MixedPlusStartDisplay;
            int layoutTheme = layoutMixedIndex % SeaCreatureNames.Length;
            int layoutTileCount = layoutMixedIndex % 2 == 0 ? 2 : 3;
            bool layoutXLeft = layoutMixedIndex % 2 == 0;
            string copy140Title =
                $"Ch7 • {ChapterNames[6]} {displayNumber} (x + numbers + vars, {layoutTileCount} each side; from {from129Global})";
            LevelDefinition numberLevel = MakeCh7NumberVariableFromMixedTemplate(copy140Title, layoutGlobal,
                layoutTheme, layoutXLeft, layoutTileCount);
            // 140–150: x must always appear, and only on one side.
            EnsureGoalXOnOneSide(numberLevel, preferLeft: layoutXLeft);
            return numberLevel;
        }

        /// <summary>
        /// Guarantees exactly one isolation x on exactly one board side (never both, never missing).
        /// </summary>
        private static void EnsureGoalXOnOneSide(LevelDefinition level, bool preferLeft)
        {
            int leftX = CountGoalX(level.LeftCards, level.LeftVariableLetters);
            int rightX = CountGoalX(level.RightCards, level.RightVariableLetters);

            if (leftX == 1 && rightX == 0)
            {
                return;
            }

            if (leftX == 0 && rightX == 1)
            {
                return;
            }

            RemoveAllGoalX(level.LeftCards, level.LeftVariableLetters, level.LeftValues);
            RemoveAllGoalX(level.RightCards, level.RightVariableLetters, level.RightValues);

            if (preferLeft)
            {
                InsertGoalXAtStart(level.LeftCards, level.LeftVariableLetters, level.LeftValues);
            }
            else
            {
                InsertGoalXAtStart(level.RightCards, level.RightVariableLetters, level.RightValues);
            }
        }

        private static int CountGoalX(List<CardKind> cards, List<char> letters)
        {
            int count = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] == CardKind.DayCreature
                    && i < letters.Count
                    && letters[i] == VariableGoalRules.GoalLetter)
                {
                    count++;
                }
            }

            return count;
        }

        private static void RemoveAllGoalX(List<CardKind> cards, List<char> letters, List<int> values)
        {
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                bool isGoalX = cards[i] == CardKind.DayCreature
                    && i < letters.Count
                    && letters[i] == VariableGoalRules.GoalLetter;
                if (!isGoalX)
                {
                    continue;
                }

                cards.RemoveAt(i);
                if (i < letters.Count)
                {
                    letters.RemoveAt(i);
                }

                if (i < values.Count)
                {
                    values.RemoveAt(i);
                }
            }
        }

        private static void InsertGoalXAtStart(List<CardKind> cards, List<char> letters, List<int> values)
        {
            cards.Insert(0, CardKind.DayCreature);
            letters.Insert(0, VariableGoalRules.GoalLetter);
            values.Insert(0, 1);
        }

        private static void AssertGoalXOnOneSideForNumberLevels(IReadOnlyList<LevelDefinition> levels)
        {
            for (int globalLevel = NumberLevelsStartLevel; globalLevel <= TotalLevels; globalLevel++)
            {
                int index = globalLevel - 1;
                if (index < 0 || index >= levels.Count)
                {
                    continue;
                }

                LevelDefinition level = levels[index];
                int leftX = CountGoalX(level.LeftCards, level.LeftVariableLetters);
                int rightX = CountGoalX(level.RightCards, level.RightVariableLetters);
                if (leftX + rightX != 1)
                {
                    throw new System.InvalidOperationException(
                        $"Level {globalLevel} ({level.Title}) must have x on exactly one side " +
                        $"(left={leftX}, right={rightX}).");
                }
            }
        }

        /// <summary>
        /// Levels 113–150: put a scene 0 on the board side opposite the isolation goal (x or box).
        /// </summary>
        private static void AddZeroOppositeGoal(LevelDefinition level)
        {
            if (!TryGetSideOppositeGoal(level, out List<CardKind> cards, out List<char> letters, out List<int> values))
            {
                return;
            }

            if (SideHasZeroConstant(cards, values))
            {
                return;
            }

            EnsureSideListLengths(cards, letters, values);
            cards.Add(CardKind.PositiveConstant);
            letters.Add('\0');
            values.Add(0);
        }

        /// <summary>
        /// Levels 140–150: put a variable letter opposite x (replaces scene 0) so the answer is x = letter.
        /// </summary>
        private static void AddLetterOppositeGoal(LevelDefinition level, char letter)
        {
            if (!TryGetSideOppositeGoal(level, out List<CardKind> cards, out List<char> letters, out List<int> values))
            {
                return;
            }

            EnsureSideListLengths(cards, letters, values);

            // Replace an existing opposite 0 if present; otherwise append the letter.
            for (int i = 0; i < cards.Count; i++)
            {
                int value = i < values.Count ? values[i] : 1;
                if (cards[i] is (CardKind.PositiveConstant or CardKind.NegativeConstant) && value == 0)
                {
                    cards[i] = CardKind.DayCreature;
                    letters[i] = letter;
                    values[i] = 1;
                    return;
                }
            }

            cards.Add(CardKind.DayCreature);
            letters.Add(letter);
            values.Add(1);
        }

        private static bool TryGetSideOppositeGoal(LevelDefinition level,
            out List<CardKind> cards, out List<char> letters, out List<int> values)
        {
            bool goalOnLeft = SideHasIsolationGoal(level.LeftCards, level.LeftVariableLetters);
            bool goalOnRight = SideHasIsolationGoal(level.RightCards, level.RightVariableLetters);

            if (goalOnLeft && !goalOnRight)
            {
                cards = level.RightCards;
                letters = level.RightVariableLetters;
                values = level.RightValues;
                return true;
            }

            if (goalOnRight && !goalOnLeft)
            {
                cards = level.LeftCards;
                letters = level.LeftVariableLetters;
                values = level.LeftValues;
                return true;
            }

            // Goal missing/ambiguous — still place on the right scene.
            cards = level.RightCards;
            letters = level.RightVariableLetters;
            values = level.RightValues;
            return true;
        }

        private static void EnsureSideListLengths(List<CardKind> cards, List<char> letters, List<int> values)
        {
            while (letters.Count < cards.Count)
            {
                letters.Add('\0');
            }

            while (values.Count < cards.Count)
            {
                values.Add(1);
            }
        }

        private static bool SideHasIsolationGoal(List<CardKind> cards, List<char> letters)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] == CardKind.Box)
                {
                    return true;
                }

                if (cards[i] == CardKind.DayCreature
                    && i < letters.Count
                    && letters[i] == VariableGoalRules.GoalLetter)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SideHasZeroConstant(List<CardKind> cards, List<int> values)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] is not (CardKind.PositiveConstant or CardKind.NegativeConstant))
                {
                    continue;
                }

                int value = i < values.Count ? values[i] : 1;
                if (value == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SideHasLetter(List<CardKind> cards, List<char> letters, char letter)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] is not (CardKind.DayCreature or CardKind.NightCreature))
                {
                    continue;
                }

                if (i < letters.Count && letters[i] == letter)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AssertOppositeAnswerForAdditionLevels(IReadOnlyList<LevelDefinition> levels)
        {
            for (int globalLevel = PlusBetweenTilesStartLevel; globalLevel <= PlusBetweenTilesEndLevel; globalLevel++)
            {
                int index = globalLevel - 1;
                if (index < 0 || index >= levels.Count)
                {
                    continue;
                }

                LevelDefinition level = levels[index];
                bool goalOnLeft = SideHasIsolationGoal(level.LeftCards, level.LeftVariableLetters);
                bool goalOnRight = SideHasIsolationGoal(level.RightCards, level.RightVariableLetters);

                if (globalLevel >= NumberLevelsStartLevel)
                {
                    char expected = OppositeLetterFor140To150[globalLevel - NumberLevelsStartLevel];
                    bool letterOpposite = goalOnLeft && !goalOnRight
                        ? SideHasLetter(level.RightCards, level.RightVariableLetters, expected)
                        : goalOnRight && !goalOnLeft
                            ? SideHasLetter(level.LeftCards, level.LeftVariableLetters, expected)
                            : SideHasLetter(level.LeftCards, level.LeftVariableLetters, expected)
                              || SideHasLetter(level.RightCards, level.RightVariableLetters, expected);

                    if (!letterOpposite)
                    {
                        throw new System.InvalidOperationException(
                            $"Level {globalLevel} ({level.Title}) must have letter '{expected}' opposite x/box.");
                    }

                    continue;
                }

                bool zeroOpposite = goalOnLeft && !goalOnRight
                    ? SideHasZeroConstant(level.RightCards, level.RightValues)
                    : goalOnRight && !goalOnLeft
                        ? SideHasZeroConstant(level.LeftCards, level.LeftValues)
                        : SideHasZeroConstant(level.LeftCards, level.LeftValues)
                          || SideHasZeroConstant(level.RightCards, level.RightValues);

                if (!zeroOpposite)
                {
                    throw new System.InvalidOperationException(
                        $"Level {globalLevel} ({level.Title}) must have a scene 0 opposite x/box.");
                }
            }
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
        /// Global 140–150: copy of a 129–139 / 118–128 mixed layout. Sea slots → number images;
        /// variable slots stay letter images.
        /// </summary>
        private static LevelDefinition MakeCh7NumberVariableFromMixedTemplate(string title, int sourceGlobalLevel,
            int seaTheme, bool xOnLeft, int tileCount)
        {
            int letterSeed = sourceGlobalLevel * 7919 + 31;
            int variableSlotCount = tileCount == 2 ? 1 : (sourceGlobalLevel % 2 == 0 ? 2 : 1);
            int numberSlotCount = tileCount - variableSlotCount;
            List<char> letters = PickDistinctVariableLetters(letterSeed, variableSlotCount);
            int numberSeed = sourceGlobalLevel * 7919 + 101;

            var level = new LevelDefinition
            {
                Title = title,
                Chapter = 7,
                CreatureTheme = seaTheme,
                ParMoves = 2 + tileCount * 2,
                ParCards = tileCount
            };

            // true = variable (same as mixed), false = number (was sea in 129–139).
            var slots = new List<bool>();
            for (int i = 0; i < variableSlotCount; i++)
            {
                slots.Add(true);
            }

            for (int i = 0; i < numberSlotCount; i++)
            {
                slots.Add(false);
            }

            ShuffleSlots(slots, sourceGlobalLevel * 7919 + 59);

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
                    cards.Add(CardKind.PositiveConstant);
                    letters.Add('\0');
                    values.Add(numberValues[numberIndex++]);
                }
            }
        }

        private static int AdditionNumberValue(int numberSeed, int numberIndex)
        {
            int baseValue = 1 + ((numberSeed + numberIndex * 17) % 5);
            if (baseValue < 1)
            {
                return 1;
            }

            return baseValue > 9 ? 9 : baseValue;
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
