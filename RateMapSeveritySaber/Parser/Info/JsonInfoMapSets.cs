using System.Diagnostics;
using System.Text.Json.Serialization;

namespace RateMapSeveritySaber.Parser.Info;

#pragma warning disable CS8618

[DebuggerDisplay("{BeatmapCharacteristicName} ({DifficultyBeatmaps.Length} maps)")]
public class JsonInfoMapSets
{
	[JsonPropertyName("_beatmapCharacteristicName")]
	public string BeatmapCharacteristicName { get; set; }
	[JsonPropertyName("_difficultyBeatmaps")]
	public JsonInfoMap[] DifficultyBeatmaps { get; set; }
}
#pragma warning restore CS8618
