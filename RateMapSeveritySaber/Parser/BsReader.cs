using RateMapSeveritySaber.Parser.Abstract;
using RateMapSeveritySaber.Parser.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Text.Json;

namespace RateMapSeveritySaber.Parser;

public class BsReader
{
	// Zip

	public static BsSong ReadZip(string file)
	{
		using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
		return ReadZip(fs);
	}

	public static BsSong ReadZip(Stream stream)
	{
		using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
		return ReadZip(zip);
	}

	public static BsSong ReadZip(ZipArchive zip) => Read(new PlainZipMapProvider(zip));

	public static IBsInfo? ReadZipInfo(string file)
	{
		using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
		return ReadZipInfo(fs);
	}

	public static IBsInfo? ReadZipInfo(Stream stream)
	{
		using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
		return ReadZipInfo(zip);
	}

	public static IBsInfo? ReadZipInfo(ZipArchive zip) => ReadInfo(new PlainZipMapProvider(zip));

	// Folder

	public static BsSong ReadFolder(string folder)
		=> Read(new FolderMapProvider(folder));

	public static IBsInfo? ReadFolderInfo(string folder)
		=> ReadInfo(new FolderMapProvider(folder));

	// Generic

	public static BsSong Read(BsMapProvider fileProvider)
	{
		var info = ReadInfo(fileProvider);
		if (info is null) throw new Exception("Found no info file");

		List<BsDifficulty> maps = [];

		foreach (var diff in info.GetDifficultyBeatmaps())
		{
			using var fs = fileProvider.Get(diff.BeatmapFilename);
			if (fs is null) continue;
			var map = BsParser.ParseMap(fs);
			maps.Add(new BsDifficulty
			{
				Info = info,
				Beatmap = map,
				Difficulty = diff
			});
		}

		return new BsSong(info, maps);
	}

	public static IBsInfo? ReadInfo(BsMapProvider fileProvider)
	{
		using var fs = fileProvider.GetInfoFile();
		if (fs is null) return null;
		return BsParser.ParseInfo(fs);
	}
}
