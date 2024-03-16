using RateMapSeveritySaber.Parser.Abstract;
using RateMapSeveritySaber.Parser.Info;
using RateMapSeveritySaber.Parser.V2;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

namespace RateMapSeveritySaber.Parser;

// ReSharper disable once ClassNeverInstantiated.Global
public class BSReader
{
	public delegate Stream? FileProvider(string file);

	public static List<BSDifficulty> ReadZip(string file)
	{
		using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
		return ReadZip(fs);
	}

	public static List<BSDifficulty> ReadZip(Stream stream)
	{
		using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
		return ReadZip(zip);
	}

	public static List<BSDifficulty> ReadZip(ZipArchive zip) => Read(ZipProvider(zip));

	public static JsonInfo? ReadZipInfo(string file)
	{
		using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
		return ReadZipInfo(fs);
	}

	public static JsonInfo? ReadZipInfo(Stream stream)
	{
		using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
		return ReadZipInfo(zip);
	}

	public static JsonInfo? ReadZipInfo(ZipArchive zip) => ReadInfo(ZipProvider(zip));

	public static List<BSDifficulty> Read(string folder)
		=> Read(FolderProvider(folder));

	public static List<BSDifficulty> Read(FileProvider fileProvider)
	{
		JsonInfo? info = ReadInfo(fileProvider);
		if (info is null) throw new Exception("Found no info file");

		var maps = new List<BSDifficulty>();

		for (int difficultySetIndex = 0; difficultySetIndex < info.DifficultyBeatmapSets.Length; difficultySetIndex++)
		{
			var set = info.DifficultyBeatmapSets[difficultySetIndex];
			for (int difficultyIndex = 0; difficultyIndex < set.DifficultyBeatmaps.Length; difficultyIndex++)
			{
				var mapj = set.DifficultyBeatmaps[difficultyIndex];
				using var fs = fileProvider(mapj.BeatmapFilename);
				if (fs is null) continue;
				var file = ReadAll(fs);
				var map = JsonSerializer.Deserialize<JsonMap>(file);
				if (map is null) continue;
				maps.Add(new BSDifficulty
				{
					Info = info,
					MapInfo = mapj,
					Data = map,

					Characteristic = BSMapUtil.NameToCharacteristic(set.BeatmapCharacteristicName),
					DifficultySetIndex = difficultySetIndex,
					DifficultyIndex = difficultyIndex,
				});
			}
		}

		return maps;
	}

	public static JsonInfo? ReadInfo(string folder)
		=> ReadInfo(FolderProvider(folder));

	public static JsonInfo? ReadInfo(FileProvider fileProvider)
	{
		using var fs = fileProvider("info.dat") ?? fileProvider("info.json");
		if (fs is null) return null;
		var file = ReadAll(fs);
		return JsonSerializer.Deserialize<JsonInfo>(file);
	}

	public static FileProvider FolderProvider(string folder)
	{
		return file =>
		{
			var path = Path.Combine(folder, file);
			if (File.Exists(path))
				return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			else
				return null;
		};
	}

	public static FileProvider ZipProvider(ZipArchive zip)
	{
		return file =>
		{
			var infoE = zip.Entries.FirstOrDefault(e => e.Name.Equals(file, StringComparison.OrdinalIgnoreCase));
			if (infoE is null)
				return null;
			return infoE.Open();
		};
	}

	private static byte[] ReadAll(Stream stream)
	{
		int len;
		try { len = (int)stream.Length; } catch { len = 8192; };
		using var mem = new MemoryStream(len);
		stream.CopyTo(mem);
		mem.Seek(0, SeekOrigin.Begin);
		return mem.ToArray();
	}
}
