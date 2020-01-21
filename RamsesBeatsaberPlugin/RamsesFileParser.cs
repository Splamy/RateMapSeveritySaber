using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RateMapSeveritySaber;

using LevelScores = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<BeatmapDifficulty, RateMapSeveritySaber.Score>>;

namespace RamsesBeatsaberPlugin
{
    public static class RamsesFileParser
    {
        public static Dictionary<string, LevelScores> scoreCache = new Dictionary<string, LevelScores>();

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

            var score = Analyzer.AnalyzeMap(LevelDataToMap(difficulty.beatmapData));
            levelDifficulties[difficulty.difficulty] = score;
            return score;
        }

        private static BSMap LevelDataToMap(BeatmapData data)
        {
            var map = new BSMap();
            map.Data = new JsonMap();
            return map;
        }
    }
}