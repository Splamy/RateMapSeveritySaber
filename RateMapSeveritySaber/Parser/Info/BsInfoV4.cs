using System.Collections.Generic;
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

public class BsInfoV4 : IBsInfo
{
	[JsonPropertyName("version")]
	public required string Version { get; set; }

	[JsonPropertyName("customData")]
	public JsonObject? CustomData { get; set; }

	[JsonPropertyName("song")]
	public required BsSongInfoV4 Song { get; set; }

	[JsonPropertyName("audio")]
	public required BsAudioInfoV4 Audio { get; set; }

	[JsonPropertyName("songPreviewFilename")]
	public string? SongPreviewFilename { get; set; }

	[JsonPropertyName("coverImageFilename")]
	public string? CoverImageFilename { get; set; }

	[JsonPropertyName("environmentNames")]
	public List<string> EnvironmentNames { get; set; } = [];

	// [JsonPropertyName("colorSchemes")]
	// public List<BsColorSchemeV4> ColorSchemes { get; set; } = [];

	[JsonPropertyName("difficultyBeatmaps")]
	public List<BsDifficultyBeatmapV4> DifficultyBeatmaps { get; set; } = [];

	string IBsInfo.SongName => Song.Title;
	float IBsInfo.Bpm => Audio.Bpm;
	string IBsInfo.SongFilename => Audio.SongFilename;
	string? IBsInfo.CoverImageFilename => CoverImageFilename;

	public IEnumerable<IBsDifficulty> GetDifficultyBeatmaps() => DifficultyBeatmaps;

	public IEnumerable<BsFileInfo> RequiredFiles()
	{
		foreach (var diff in DifficultyBeatmaps)
		{
			yield return new BsFileInfo(diff.BeatmapFilename, BsFileType.Beatmap);
			if (!string.IsNullOrEmpty(diff.LightshowFilename))
			{
				yield return new BsFileInfo(diff.LightshowFilename, BsFileType.Lightmap);
			}
		}

		yield return new BsFileInfo(Audio.SongFilename, BsFileType.Audio);

		if (!string.IsNullOrEmpty(Audio.AudioDataFilename))
		{
			yield return new BsFileInfo(Audio.AudioDataFilename, BsFileType.AudioMetadata);
		}

		if (!string.IsNullOrEmpty(SongPreviewFilename) && SongPreviewFilename != Audio.SongFilename)
		{
			yield return new BsFileInfo(SongPreviewFilename, BsFileType.Audio);
		}

		if (!string.IsNullOrEmpty(CoverImageFilename))
		{
			yield return new BsFileInfo(CoverImageFilename, BsFileType.Image);
		}
	}
}

public class BsSongInfoV4
{
	[JsonPropertyName("title")]
	public required string Title { get; set; }

	[JsonPropertyName("subTitle")]
	public string? SubTitle { get; set; }

	[JsonPropertyName("author")]
	public string? Author { get; set; }
}

public class BsAudioInfoV4
{
	[JsonPropertyName("songFilename")]
	public required string SongFilename { get; set; }

	[JsonPropertyName("songDuration")]
	public float SongDuration { get; set; }

	[JsonPropertyName("audioDataFilename")]
	public string? AudioDataFilename { get; set; }

	[JsonPropertyName("bpm")]
	public float Bpm { get; set; }

	[JsonPropertyName("lufs")]
	public float Lufs { get; set; }

	[JsonPropertyName("previewStartTime")]
	public float PreviewStartTime { get; set; }

	[JsonPropertyName("previewDuration")]
	public float PreviewDuration { get; set; }
}

public class BsDifficultyBeatmapV4 : IBsDifficulty
{
	[JsonPropertyName("characteristic")]
	public required string CharacteristicName { get; set; }

	[JsonPropertyName("difficulty")]
	public required string DifficultyName { get; set; }

	[JsonPropertyName("beatmapAuthors")]
	public BsBeatmapAuthorsV4 BeatmapAuthors { get; set; } = new();

	[JsonPropertyName("environmentNameIdx")]
	public int EnvironmentNameIdx { get; set; }

	[JsonPropertyName("beatmapColorSchemeIdx")]
	public int BeatmapColorSchemeIdx { get; set; }

	[JsonPropertyName("noteJumpMovementSpeed")]
	public float NoteJumpMovementSpeed { get; set; }

	[JsonPropertyName("noteJumpStartBeatOffset")]
	public float NoteJumpStartBeatOffset { get; set; }

	[JsonPropertyName("beatmapDataFilename")]
	public required string BeatmapFilename { get; set; }

	[JsonPropertyName("lightshowDataFilename")]
	public string? LightshowFilename { get; set; }
}

public class BsBeatmapAuthorsV4
{
	[JsonPropertyName("mappers")]
	public List<string> Mappers { get; set; } = [];

	[JsonPropertyName("lighters")]
	public List<string> Lighters { get; set; } = [];
}
