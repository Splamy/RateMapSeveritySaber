using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace RateMapSeveritySaber.Parser.Info;

public interface IBsInfo
{
	string Version { get; }
	string SongName { get; }
	float Bpm { get; }
	JsonObject? CustomData { get; }

	string SongFilename { get; }
	string? SongPreviewFilename { get; }
	string? AudioDataFilename { get; }
	string? CoverImageFilename { get; }

	IEnumerable<IBsDifficulty> GetDifficultyBeatmaps();
}

public record struct BsFileInfo(string FileName, BsFileType Type);

public interface IBsDifficulty
{
	string CharacteristicName { get; }
	string DifficultyName { get; }

	string BeatmapFilename { get; }
	string? LightshowFilename { get; }
}

[Flags]
public enum BsFileType
{
	Unknown = 0,
	Info,
	Beatmap,
	Audio,
	Image,
	Lightmap,
	AudioMetadata,
}
