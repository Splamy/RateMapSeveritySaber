using RateMapSeveritySaber.Parser;
using RateMapSeveritySaber.Parser.Info;

namespace RateMapSeveritySaber.Tests;

public class FileParserTests
{
	public static TheoryData<string> Maps => [.. Directory.EnumerateFiles("Assets/Beatmap", "*.json")];
	public static TheoryData<string> Infos => [.. Directory.EnumerateFiles("Assets/Info", "*.json")];

	[Theory]
	[MemberData(nameof(Maps))]
	public void ParseBeatmapInAllVersionsMatchesTestData(string file)
	{
		var stream = File.OpenRead(file);
		var map = BsParser.ParseMap(stream);
		var version = Version.Parse(map.Version!);

		Assert.True(map.Notes.ToList() is [{ Beat: 10, X: 1, Y: 0, Type: NoteColor.Red, Direction: NoteDir.Down }]);
		Assert.True(map.Bombs.ToList() is [{ Beat: 10, X: 1, Y: 0 }]);
		Assert.True(map.Obstacles.ToList() is [{ Beat: 10, X: 1, Y: 0, Width: 1, Height: 5 }]);

		if (version < new Version(2, 6, 0))
		{
			return;
		}

		Assert.True(map.Arcs.ToList() is [{ Beat: 10, TailBeat: 15 }]);

		if (version < new Version(3, 0, 0))
		{
			return;
		}

		Assert.True(map.Chains.ToList() is
			[{ Beat: 10, X: 1, Y: 0, TailX: 2, TailY: 2, TailBeat: 15, SliceCount: 3, SquishFactor: 0.5f }]);
	}

	[Theory]
	[MemberData(nameof(Infos))]
	public void ParseInfoInAllVersionsMatchesTestData(string file)
	{
		var stream = File.OpenRead(file);
		var info = BsParser.ParseInfo(stream);
		var version = Version.Parse(info.Version!);

		Assert.True(info is { SongName: "Magic", Bpm: 208 });

		var difficulties = info
			.GetDifficultyBeatmaps()
			.OrderBy(diff => BSMapUtil.NameToCharacteristic(diff.CharacteristicName))
			.ThenBy(diff => BSMapUtil.DifficultyNameToNumber(diff.DifficultyName))
			.ToList();

		Assert.True(difficulties is
		[
			{ CharacteristicName: "Standard", DifficultyName: "Easy", BeatmapFilename: "Easy.dat" },
			{ CharacteristicName: "Standard", DifficultyName: "Normal", BeatmapFilename: "Normal.dat" },
			{ CharacteristicName: "Standard", DifficultyName: "Hard", BeatmapFilename: "Hard.dat" },
			{ CharacteristicName: "Standard", DifficultyName: "Expert", BeatmapFilename: "Expert.dat" },
			{ CharacteristicName: "Standard", DifficultyName: "ExpertPlus", BeatmapFilename: "ExpertPlus.dat" }
		]);

		var requiredFiles = info
			.RequiredFiles()
			.Distinct()
			.OrderBy(f => f.FileName)
			.ToList();

		if (version < new Version(4, 0, 0))
		{
			Assert.True(requiredFiles is
			[
				{ FileName: "cover.png", Type: BsFileType.Image },
				{ FileName: "Easy.dat", Type: BsFileType.Beatmap },
				{ FileName: "Expert.dat", Type: BsFileType.Beatmap },
				{ FileName: "ExpertPlus.dat", Type: BsFileType.Beatmap },
				{ FileName: "Hard.dat", Type: BsFileType.Beatmap },
				{ FileName: "Magic.wav", Type: BsFileType.Audio },
				{ FileName: "Normal.dat", Type: BsFileType.Beatmap },
			]);
		}
		else
		{
			Assert.True(requiredFiles is
			[
				{ FileName: "BPMInfo.dat", Type: BsFileType.AudioMetadata },
				{ FileName: "cover.png", Type: BsFileType.Image },
				{ FileName: "Easy.dat", Type: BsFileType.Beatmap },
				{ FileName: "Expert.dat", Type: BsFileType.Beatmap },
				{ FileName: "ExpertPlus.dat", Type: BsFileType.Beatmap },
				{ FileName: "Hard.dat", Type: BsFileType.Beatmap },
				{ FileName: "Lightshow.dat", Type: BsFileType.Lightmap },
				{ FileName: "LightshowPlus.dat", Type: BsFileType.Lightmap },
				{ FileName: "Magic.wav", Type: BsFileType.Audio },
				{ FileName: "Normal.dat", Type: BsFileType.Beatmap },
				{ FileName: "song.ogg", Type: BsFileType.Audio },
			]);
		}
	}
}
