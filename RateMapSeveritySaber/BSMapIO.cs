using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DeepSaber
{
	public class BSMapIO
	{
		public static List<BSMap> Read(string folder)
		{
			var infoFile = Path.Combine(folder, "info.dat");

			JsonInfo info;

			using (var fs = File.OpenRead(infoFile))
			using (var sr = new StreamReader(fs))
			{
				var json = sr.ReadToEnd();
				info = JsonConvert.DeserializeObject<JsonInfo>(json);
			}

			var maps = new List<BSMap>();

			foreach (var set in info._difficultyBeatmapSets)
			{
				foreach (var mapj in set._difficultyBeatmaps)
				{
					var mapFile = Path.Combine(folder, mapj._beatmapFilename);
					using (var fs = File.OpenRead(mapFile))
					using (var sr = new StreamReader(fs))
					{
						var json = sr.ReadToEnd();
						var map = JsonConvert.DeserializeObject<JsonMap>(json);
						maps.Add(new BSMap
						{
							Info = info,
							MapInfo = mapj,
							Data = map,
							Folder = folder,
							MusicFile = Path.Combine(folder, info._songFilename)
						});
					}
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
		public string MusicFile { get; set; }
		public string Folder { get; set; }

		public float RealTimeToBeatTime(float time) => (time / 60) * Info._beatsPerMinute;
		public float BeatTimeToRealTime(float time) => (time / Info._beatsPerMinute) * 60;
	}

	public class JsonMap
	{
		[JsonProperty(PropertyName = "_version")]
		public string Version { get; set; }
		//public float[] _BPMChanges { get; set; }

		//public List<object> _events { get; set; }
		[JsonProperty(PropertyName = "_notes")]
		public List<JsonNote> Notes { get; set; }
		//public List<object> _obstacles { get; set; }
		//public List<object> _bookmarks { get; set; }
	}

	public class JsonNote
	{
		[JsonProperty(PropertyName = "_time")]
		public float Time { get; set; }
		[JsonProperty(PropertyName = "_lineIndex")]
		public int X { get; set; }
		[JsonProperty(PropertyName = "_lineLayer")]
		public int Y { get; set; }
		[JsonProperty(PropertyName = "_type")]
		public NoteColor Type { get; set; }
		[JsonProperty(PropertyName = "_cutDirection")]
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
		public float _difficultyRank { get; set; }
		public string _beatmapFilename { get; set; }
		public float _noteJumpMovementSpeed { get; set; }
	}
}
