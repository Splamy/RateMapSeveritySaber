using RateMapSeveritySaber.Parser.Beatmaps;
using RateMapSeveritySaber.Parser.Info;
using System;
using System.Collections.Generic;

namespace RateMapSeveritySaber.Parser;

public record BsSong(IBsInfo Info, IReadOnlyCollection<BsDifficulty> Difficulties);

public class BsDifficulty
{
	public required IBsInfo Info { get; set; }
	public required IBsDifficulty Difficulty { get; set; }
	public required IBsBeatmap Beatmap { get; set; }

	public MapCharacteristic Characteristic => BsParserUtil.NameToCharacteristic(Difficulty.CharacteristicName);
	public int DifficultyRank => BsParserUtil.DifficultyNameToNumber(Difficulty.DifficultyName);

	public float RealTimeToBeatTime(TimeSpan time) => (float)(time.TotalMinutes * Info.Bpm);
	public TimeSpan BeatTimeToRealTime(float beat) => TimeSpan.FromMinutes(beat / Info.Bpm);
}
