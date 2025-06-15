using System;
using System.Collections.Generic;

namespace RateMapSeveritySaber.Parser.Info;

public interface IBsInfo
{
	string Version { get; }
	string SongName { get; }
	float Bpm { get; }

	string SongFilename { get; }
	string? CoverImageFilename { get; }

	IEnumerable<IBsDifficulty> GetDifficultyBeatmaps();
	IEnumerable<BsFileInfo> RequiredFiles();
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
