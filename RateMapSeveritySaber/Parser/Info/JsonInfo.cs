using System.Diagnostics;
using System.Text.Json.Serialization;

namespace RateMapSeveritySaber.Parser.Info;

#pragma warning disable CS8618

[DebuggerDisplay("{SongName} ({DifficultyBeatmapSets.Length} maps) @{BPM}")]
public class JsonInfo
{
	[JsonPropertyName("_songName")]
	public string SongName { get; set; }
	[JsonPropertyName("_songAuthorName")]
	public string SongAuthorName { get; set; }
	[JsonPropertyName("_beatsPerMinute")]
	public float BPM { get; set; }
	[JsonPropertyName("_songTimeOffset")]
	public float SongTimeOffset { get; set; }
	[JsonPropertyName("_songFilename")]
	public string SongFilename { get; set; }
	[JsonPropertyName("_difficultyBeatmapSets")]
	public JsonInfoMapSets[] DifficultyBeatmapSets { get; set; }
}
#pragma warning restore CS8618
