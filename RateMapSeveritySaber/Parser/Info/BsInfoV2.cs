using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace RateMapSeveritySaber.Parser.Info;

[DebuggerDisplay("{SongName} ({DifficultyBeatmapSets.Count} maps) @{Bpm}")]
public class BsInfoV2 : IBsInfo
{
	[JsonPropertyName("_version")]
	public required string Version { get; set; }

	[JsonPropertyName("_customData")]
	public JsonObject? CustomData { get; set; }

	[JsonPropertyName("_songName")]
	public required string SongName { get; set; }

	[JsonPropertyName("_songSubName")]
	public string? SongSubName { get; set; }

	[JsonPropertyName("_songAuthorName")]
	public string? SongAuthorName { get; set; }

	[JsonPropertyName("_songFilename")]
	public required string SongFilename { get; set; }

	[JsonPropertyName("_coverImageFilename")]
	public string? CoverImageFilename { get; set; }

	[JsonPropertyName("_beatsPerMinute")]
	public float Bpm { get; set; }

	[JsonPropertyName("_songTimeOffset")]
	public float SongTimeOffset { get; set; }

	[JsonPropertyName("_previewStartTime")]
	public float PreviewStartTime { get; set; }

	[JsonPropertyName("_previewDuration")]
	public float PreviewDuration { get; set; }

	[JsonPropertyName("_difficultyBeatmapSets")]
	public List<BsDifficultyBeatmapSetV2> DifficultyBeatmapSets { get; set; } = [];

	string? IBsInfo.SongPreviewFilename => null;

	public IEnumerable<IBsDifficulty> GetDifficultyBeatmaps() => DifficultyBeatmapSets
		.SelectMany(set => set.DifficultyBeatmaps
			.Select(diff => new BsDifficultyInternalV2(set, diff)));

	public IEnumerable<BsFileInfo> RequiredFiles()
	{
		foreach (var diffSet in DifficultyBeatmapSets)
		{
			foreach (var diff in diffSet.DifficultyBeatmaps)
			{
				yield return new BsFileInfo(diff.BeatmapFilename, BsFileType.Beatmap);
			}
		}

		yield return new BsFileInfo(SongFilename, BsFileType.Audio);
		if (!string.IsNullOrEmpty(CoverImageFilename))
		{
			yield return new BsFileInfo(CoverImageFilename, BsFileType.Image);
		}
	}
}

[DebuggerDisplay("{BeatmapCharacteristicName} ({DifficultyBeatmaps.Count} maps)")]
public class BsDifficultyBeatmapSetV2
{
	[JsonPropertyName("_beatmapCharacteristicName")]
	public required string BeatmapCharacteristicName { get; set; }

	[JsonPropertyName("_difficultyBeatmaps")]
	public List<BsDifficultyV2> DifficultyBeatmaps { get; set; } = [];
}

[DebuggerDisplay("{Difficulty} (file: {BeatmapFilename})")]
public class BsDifficultyV2
{
	[JsonPropertyName("_difficulty")]
	public required string Difficulty { get; set; }

	[JsonPropertyName("_difficultyRank")]
	public float DifficultyRank { get; set; }

	[JsonPropertyName("_beatmapFilename")]
	public required string BeatmapFilename { get; set; }

	[JsonPropertyName("_noteJumpMovementSpeed")]
	public float NoteJumpMovementSpeed { get; set; }
}

internal class BsDifficultyInternalV2(BsDifficultyBeatmapSetV2 set, BsDifficultyV2 difficulty) : IBsDifficulty
{
	public string CharacteristicName => set.BeatmapCharacteristicName;
	public string DifficultyName => difficulty.Difficulty;
	public string BeatmapFilename => difficulty.BeatmapFilename;
	public string? LightshowFilename => null; // Not supported in this version

	public override string ToString() => $"{CharacteristicName} - {DifficultyName} ({BeatmapFilename})";
}
