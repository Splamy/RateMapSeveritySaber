using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RateMapSeveritySaber;
using LevelScores =
    System.Collections.Generic.Dictionary<string,
        System.Collections.Generic.Dictionary<BeatmapDifficulty, RateMapSeveritySaber.Score>>;

namespace RamsesBeatsaberPlugin
{
    public static class RamsesFileParser
    {
        private static readonly Dictionary<string, LevelScores> scoreCache = new Dictionary<string, LevelScores>();

        public static void LoadCachedFiles()
        {
            try
            {
                var scoreCacheDir = Path.Combine(CustomLevelPathHelper.baseProjectPath, "UserData", "RamsesSongCache");
                Directory.CreateDirectory(scoreCacheDir);
                // load into score cache
            }
            catch (Exception e)
            {
                RamsesPlugin.Log("Failed to load ramses cache.");
                Console.WriteLine(e);
            }
        }

        public static Score GetRamsesScore(IBeatmapLevel level, IDifficultyBeatmap difficulty)
        {
            if (!scoreCache.ContainsKey(level.levelID))
            {
                scoreCache[level.levelID] = new LevelScores();
            }

            var levelTypes = scoreCache[level.levelID];

            if (!levelTypes.ContainsKey("yeet"))
            {
                levelTypes["yeet"] = new Dictionary<BeatmapDifficulty, Score>();
            }

            var levelDifficulties = levelTypes["yeet"];

            if (levelDifficulties.TryGetValue(difficulty.difficulty, out var cachedScore))
            {
                return cachedScore;
            }

            var score = Analyzer.AnalyzeMap(LevelDataToMap(level, difficulty));
            levelDifficulties[difficulty.difficulty] = score;
            return score;
        }

        private static BSMap LevelDataToMap(IBeatmapLevel level, IDifficultyBeatmap difficulty)
        {
            var map = new BSMap {Data = new JsonMap {Notes = new List<JsonNote>(), Version = ""}};
            foreach (BeatmapObjectData objectData in difficulty.beatmapData.beatmapLinesData.SelectMany(bld =>
                bld.beatmapObjectsData))
            {
                switch (objectData)
                {
                    case ObstacleData _:
                    case LongNoteData _:
                        break;
                    case NoteData noteData:
                        map.Data.Notes.Add(new JsonNote
                        {
                            Direction = (NoteDir) (int) noteData.cutDirection,
                            Time = noteData.time,
                            Type = (NoteColor) (int) noteData.noteType,
                            X = noteData.lineIndex,
                            Y = (int) noteData.noteLineLayer
                        });
                        break;
                    default:
                        RamsesPlugin.Log($"Unknown object data type {objectData.GetType()}");
                        RamsesPlugin.Log(objectData);
                        break;
                }
            }

            map.Info = new JsonInfo
            {
                SongFilename = "",
                SongName = level.songName,
                BPM = level.beatsPerMinute,
                SongTimeOffset = level.songTimeOffset
            };
            map.Info.DifficultyBeatmapSets = level.beatmapLevelData.difficultyBeatmapSets.Select(
                iDifficultyBeatmapSet =>
                {
                    return new JsonInfoMapSets
                    {
                        DifficultyBeatmaps = iDifficultyBeatmapSet.difficultyBeatmaps.Select(iDifficultyBeatmap =>
                            new JsonInfoMap
                            {
                                Difficulty = iDifficultyBeatmap.difficulty.ToString(),
                                BeatmapFilename = "",
                                DifficultyRank = iDifficultyBeatmap.difficultyRank,
                                NoteJumpMovementSpeed = iDifficultyBeatmap.noteJumpMovementSpeed
                            }).ToArray(),
                        BeatmapCharacteristicName = iDifficultyBeatmapSet.beatmapCharacteristic.serializedName
                    };
                }).ToArray();


            map.MapInfo = new JsonInfoMap
            {
                Difficulty = difficulty.difficulty.ToString(),
                BeatmapFilename = "",
                DifficultyRank = difficulty.difficultyRank,
                NoteJumpMovementSpeed = difficulty.noteJumpMovementSpeed
            };

            return map;
        }
    }
}