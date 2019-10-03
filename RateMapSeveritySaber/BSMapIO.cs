using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Utf8Json;

namespace RateMapSeveritySaber
{
	public class BSMapIO
	{
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

			foreach (var set in info._difficultyBeatmapSets)
			{
				foreach (var mapj in set._difficultyBeatmaps)
				{
					using var fs = fileProvider(mapj._beatmapFilename);
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

	public class BSMap
	{
		public JsonInfo Info { get; set; }
		public JsonInfoMap MapInfo { get; set; }
		public JsonMap Data { get; set; }
	}

	public class JsonMap
	{
		//[JsonProperty(PropertyName = "_version")]
		[DataMember(Name = "_version")]
		public string Version { get; set; }
		//public float[] _BPMChanges { get; set; }

		//public List<object> _events { get; set; }
		//[JsonProperty(PropertyName = "_notes")]
		[DataMember(Name = "_notes")]
		public List<JsonNote> Notes { get; set; }
		//public List<object> _obstacles { get; set; }
		//public List<object> _bookmarks { get; set; }
	}

	public class JsonNote
	{
		//[JsonProperty(PropertyName = "_time")]
		[DataMember(Name = "_time")]
		public float Time { get; set; }
		//[JsonProperty(PropertyName = "_lineIndex")]
		[DataMember(Name = "_lineIndex")]
		public int X { get; set; }
		//[JsonProperty(PropertyName = "_lineLayer")]
		[DataMember(Name = "_lineLayer")]
		public int Y { get; set; }
		//[JsonProperty(PropertyName = "_type")]
		[DataMember(Name = "_type")]
		public NoteColor Type { get; set; }
		//[JsonProperty(PropertyName = "_cutDirection")]
		[DataMember(Name = "_cutDirection")]
		public NoteDir Direction { get; set; }
	}

	public enum NoteColor
	{
		Red = 0,
		Blue = 1,
	}

	public enum NoteDir // TODO sort
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

	public class JsonInfo
	{
		public string _songName { get; set; }
		public float _beatsPerMinute { get; set; }
		public float _songTimeOffset { get; set; }
		public string _songFilename { get; set; }
		public JsonInfoMapSets[] _difficultyBeatmapSets { get; set; }

	}
	public class JsonInfoMapSets
	{
		public string _beatmapCharacteristicName { get; set; }
		public JsonInfoMap[] _difficultyBeatmaps { get; set; }
	}
	public class JsonInfoMap
	{
		public string _difficulty { get; set; }
		public float _difficultyRank { get; set; }
		public string _beatmapFilename { get; set; }
		public float _noteJumpMovementSpeed { get; set; }
	}
}
