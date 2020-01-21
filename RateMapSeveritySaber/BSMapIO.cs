using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using Utf8Json;

namespace RateMapSeveritySaber
{
	public class BSMapIO
	{
#if !NET46
		public static List<BSMap> ReadZip(string file)
		{
			using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
			return ReadZip(fs);
		}

		public static List<BSMap> ReadZip(Stream stream)
		{
			using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
			return Read(file =>
			{
				var infoE = zip.GetEntry(file);
				return infoE.Open();
			});
		}
#endif

		public static List<BSMap> Read(string folder)
			=> Read(file => File.Open(Path.Combine(folder, file), FileMode.Open, FileAccess.Read, FileShare.Read));

		public static List<BSMap> Read(Func<string, Stream> fileProvider)
		{
			JsonInfo info;

			using (var fs = fileProvider("info.dat"))
			{
				info = JsonSerializer.Deserialize<JsonInfo>(fs);
			}

			var maps = new List<BSMap>();

			foreach (var set in info.DifficultyBeatmapSets)
			{
				foreach (var mapj in set.DifficultyBeatmaps)
				{
					using var fs = fileProvider(mapj.BeatmapFilename);
					var map = JsonSerializer.Deserialize<JsonMap>(fs);
					maps.Add(new BSMap
					{
						Info = info,
						MapInfo = mapj,
						Data = map,
					});
				}
			}

			return maps;
		}
	}

#pragma warning disable CS8618
	public class BSMap
	{
		public JsonInfo Info { get; set; }
		public JsonInfoMap MapInfo { get; set; }
		public JsonMap Data { get; set; }
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

#pragma warning restore CS8618
}
