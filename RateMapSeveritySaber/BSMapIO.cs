using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
					if (fs is null) continue;
					var file = ReadAll(fs);
					var map = JsonSerializer.Deserialize<JsonMap>(file);
					if (map is null) continue;
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
		[JsonPropertyName("_version")]
		public string Version { get; set; }
		//public float[] _BPMChanges { get; set; }

		//public List<object> _events { get; set; }
		[JsonPropertyName("_notes")]
		public List<JsonNote> Notes { get; set; }
		//public List<object> _obstacles { get; set; }
		//public List<object> _bookmarks { get; set; }
	}

	[DebuggerDisplay("{X} {Y} | {Direction} {Type}")]
	public class JsonNote
	{
		[JsonPropertyName("_time")]
		public float Time { get; set; }
		[JsonPropertyName("_lineIndex")]
		public int X { get; set; }
		[JsonPropertyName("_lineLayer")]
		public int Y { get; set; }
		[JsonPropertyName("_type")]
		public NoteColor Type { get; set; }
		[JsonPropertyName("_cutDirection")]
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

	[DebuggerDisplay("{BeatmapCharacteristicName} ({DifficultyBeatmaps.Length} maps)")]
	public class JsonInfoMapSets
	{
		[JsonPropertyName("_beatmapCharacteristicName")]
		public string BeatmapCharacteristicName { get; set; }
		[JsonPropertyName("_difficultyBeatmaps")]
		public JsonInfoMap[] DifficultyBeatmaps { get; set; }
	}

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

	public enum MapCharacteristic
	{
		Unknown = 0,
		Standard = 1,
		Degree90 = 2,
		Degree360 = 3,
	}
#pragma warning restore CS8618
}
