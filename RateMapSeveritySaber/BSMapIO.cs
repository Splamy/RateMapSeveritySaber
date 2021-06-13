using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using Utf8Json;

namespace RateMapSeveritySaber
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class BSMapIO
	{
		public delegate Stream? FileProvider(string file);

		public static List<BSMap> ReadZip(string file)
		{
			using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
			return ReadZip(fs);
		}

		public static List<BSMap> ReadZip(Stream stream)
		{
			using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
			return ReadZip(zip);
		}

		public static List<BSMap> ReadZip(ZipArchive zip) => Read(ZipProvider(zip));

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

		public static List<BSMap> Read(string folder)
			=> Read(FolderProvider(folder));

		public static List<BSMap> Read(FileProvider fileProvider)
		{
			JsonInfo? info = ReadInfo(fileProvider);
			if (info is null) throw new Exception("Found no info file");

			var maps = new List<BSMap>();

			for (int difficultySetIndex = 0; difficultySetIndex < info.DifficultyBeatmapSets.Length; difficultySetIndex++)
			{
				var set = info.DifficultyBeatmapSets[difficultySetIndex];
				for (int difficultyIndex = 0; difficultyIndex < set.DifficultyBeatmaps.Length; difficultyIndex++)
				{
					var mapj = set.DifficultyBeatmaps[difficultyIndex];
					using var fs = fileProvider(mapj.BeatmapFilename);
					var map = JsonSerializer.Deserialize<JsonMap>(fs);
					maps.Add(new BSMap
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
			return JsonSerializer.Deserialize<JsonInfo>(fs);
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
	}

#pragma warning disable CS8618
	public class BSMap
	{
		public JsonInfo Info { get; set; }
		public JsonInfoMap MapInfo { get; set; }
		public JsonMap Data { get; set; }

		public MapCharacteristic Characteristic { get; set; }
		public int DifficultySetIndex { get; set; }
		public int DifficultyIndex { get; set; }
	}

	public class JsonMap
	{
		[DataMember(Name = "_version")]
		public string Version { get; set; }
		//public float[] _BPMChanges { get; set; }

		//public List<object> _events { get; set; }
		[DataMember(Name = "_notes")]
		public List<JsonNote> Notes { get; set; }
		//public List<object> _obstacles { get; set; }
		//public List<object> _bookmarks { get; set; }
	}

	[DebuggerDisplay("{X} {Y} | {Direction} {Type}")]
	public class JsonNote
	{
		[DataMember(Name = "_time")]
		public float Time { get; set; }
		[DataMember(Name = "_lineIndex")]
		public int X { get; set; }
		[DataMember(Name = "_lineLayer")]
		public int Y { get; set; }
		[DataMember(Name = "_type")]
		public NoteColor Type { get; set; }
		[DataMember(Name = "_cutDirection")]
		public NoteDir Direction { get; set; }
	}

	public enum NoteColor
	{
		Red = 0,
		Blue = 1,
	}

	public enum NoteDir
	{
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3,
		UpLeft = 4,
		UpRight = 5,
		DownLeft = 6,
		DownRight = 7,
		Dot = 8,
	}

	[DebuggerDisplay("{SongName} ({DifficultyBeatmapSets.Length} maps) @{BPM}")]
	public class JsonInfo
	{
		[DataMember(Name = "_songName")]
		public string SongName { get; set; }
		[DataMember(Name = "_beatsPerMinute")]
		public float BPM { get; set; }
		[DataMember(Name = "_songTimeOffset")]
		public float SongTimeOffset { get; set; }
		[DataMember(Name = "_songFilename")]
		public string SongFilename { get; set; }
		[DataMember(Name = "_difficultyBeatmapSets")]
		public JsonInfoMapSets[] DifficultyBeatmapSets { get; set; }

	}

	[DebuggerDisplay("{BeatmapCharacteristicName} ({DifficultyBeatmaps.Length} maps)")]
	public class JsonInfoMapSets
	{
		[DataMember(Name = "_beatmapCharacteristicName")]
		public string BeatmapCharacteristicName { get; set; }
		[DataMember(Name = "_difficultyBeatmaps")]
		public JsonInfoMap[] DifficultyBeatmaps { get; set; }
	}

	[DebuggerDisplay("{Difficulty} (file: {BeatmapFilename})")]
	public class JsonInfoMap
	{
		[DataMember(Name = "_difficulty")]
		public string Difficulty { get; set; }
		[DataMember(Name = "_difficultyRank")]
		public float DifficultyRank { get; set; }
		[DataMember(Name = "_beatmapFilename")]
		public string BeatmapFilename { get; set; }
		[DataMember(Name = "_noteJumpMovementSpeed")]
		public float NoteJumpMovementSpeed { get; set; }
	}

	public enum MapCharacteristic
	{
		Unknown = 0,
		Standard = 1,
		Degree90 = 2,
		Degree360 = 3,
	}
#pragma warning restore CS8618
}
