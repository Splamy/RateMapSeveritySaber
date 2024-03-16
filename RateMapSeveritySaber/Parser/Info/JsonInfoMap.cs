using System.Diagnostics;
using System.Text.Json.Serialization;

namespace RateMapSeveritySaber.Parser.Info;

#pragma warning disable CS8618

[DebuggerDisplay("{Difficulty} (file: {BeatmapFilename})")]
public class JsonInfoMap
{
	[JsonPropertyName("_difficulty")]
	public string Difficulty { get; set; }
	[JsonPropertyName("_difficultyRank")]
	public float DifficultyRank { get; set; }
	[JsonPropertyName("_beatmapFilename")]
	public string BeatmapFilename { get; set; }
	[JsonPropertyName("_noteJumpMovementSpeed")]
	public float NoteJumpMovementSpeed { get; set; }
}
#pragma warning restore CS8618
