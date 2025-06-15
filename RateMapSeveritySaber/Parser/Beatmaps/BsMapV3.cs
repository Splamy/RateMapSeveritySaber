using RateMapSeveritySaber.Parser.Abstract;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace RateMapSeveritySaber.Parser.Beatmaps;

public class BsMapV3 : IBsMap
{
	IEnumerable<IBsNote> IBsMap.Notes => Notes;
	IEnumerable<IBsChains> IBsMap.Chains => BurstSliders;
	IEnumerable<IBsArcs> IBsMap.Arcs => Sliders;
	IEnumerable<IBsObstacle> IBsMap.Obstacles => Obstacles;
	IEnumerable<IBsBomb> IBsMap.Bombs => Bombs;

	[JsonPropertyName("version")]
	public required string Version { get; set; }

	[JsonPropertyName("bpmEvents")]
	public List<BsBpmV3> BpmEvent { get; set; } = [];

	[JsonPropertyName("rotationEvents")]
	public List<BsRotationEventV3> RotationEvent { get; set; } = [];

	[JsonPropertyName("colorNotes")]
	public List<BsNoteV3> Notes { get; set; } = [];

	[JsonPropertyName("bombNotes")]
	public List<BsBombV3> Bombs { get; set; } = [];

	[JsonPropertyName("obstacles")]
	public List<BsObstacleV3> Obstacles { get; set; } = [];

	[JsonPropertyName("sliders")]
	public List<BsSliderV3> Sliders { get; set; } = [];

	[JsonPropertyName("burstSliders")]
	public List<BsBurstSliderV3> BurstSliders { get; set; } = [];

	// [JsonPropertyName("basicBeatmapEvents")]
	// public List<object> BasicBeatmapEvents { get; set; }
	//
	// [JsonPropertyName("colorBoostBeatmapEvents")]
	// public List<object> ColorBoostBeatmapEvents { get; set; }
	//
	// [JsonPropertyName("lightColorEventBoxGroups")]
	// public List<object> LightColorEventBoxGroups { get; set; }
	//
	// [JsonPropertyName("lightRotationEventBoxGroups")]
	// public List<object> LightRotationEventBoxGroups { get; set; }
	//
	// [JsonPropertyName("basicEventTypesWithKeywords")]
	// public List<object> BasicEventTypesWithKeywords { get; set; }
}

public class BsBpmV3 : ITimedObject
{
	[JsonPropertyName("b")]
	public float Beat { get; set; }

	[JsonPropertyName("m")]
	public float Bpm { get; set; }
}

public class BsRotationEventV3 : ITimedObject
{
	[JsonPropertyName("b")]
	public float Beat { get; set; }

	[JsonPropertyName("e")]
	public ERotation Type { get; set; }

	[JsonPropertyName("r")]
	public float Rotation { get; set; }

	public enum ERotation
	{
		// The objects will change lanes at the same time as this event.
		Early = 0,

		// The objects will remain in its original lane.
		Late = 1,
	}
}

public class BsNoteV3 : IBsNote
{
	[JsonPropertyName("d")]
	public NoteDir Direction { get; set; }

	[JsonPropertyName("b")]
	public float Beat { get; set; }

	[JsonPropertyName("c")]
	public NoteColor Type { get; set; }

	[JsonPropertyName("x")]
	public int X { get; set; }

	[JsonPropertyName("y")]
	public int Y { get; set; }

	[JsonPropertyName("a")]
	public int AngleOffset { get; set; }
}

public class BsBombV3 : IBsBomb
{
	[JsonPropertyName("b")]
	public float Beat { get; set; }

	[JsonPropertyName("x")]
	public int X { get; set; }

	[JsonPropertyName("y")]
	public int Y { get; set; }
}

public class BsObstacleV3 : IBsObstacle
{
	[JsonPropertyName("b")]
	public float Beat { get; set; }

	[JsonPropertyName("x")]
	public int X { get; set; }

	[JsonPropertyName("y")]
	public int Y { get; set; }

	[JsonPropertyName("w")]
	public int Width { get; set; }

	[JsonPropertyName("h")]
	public int Height { get; set; }

	[JsonPropertyName("d")]
	public float BeatDuration { get; set; }

	float IDurationObject.TailBeat => Beat + BeatDuration;
}

public class BsSliderV3 : IBsArcs
{
	[JsonPropertyName("c")]
	public NoteColor Color { get; set; }

	[JsonPropertyName("b")]
	public float Beat { get; set; }

	[JsonPropertyName("x")]
	public int X { get; set; }

	[JsonPropertyName("y")]
	public int Y { get; set; }

	[JsonPropertyName("d")]
	public NoteDir Direction { get; set; }

	[JsonPropertyName("mu")]
	public float HeadControlPointLengthMultiplier { get; set; }

	[JsonPropertyName("tb")]
	public float TailBeat { get; set; }

	[JsonPropertyName("tx")]
	public int TailX { get; set; }

	[JsonPropertyName("ty")]
	public int TailY { get; set; }

	[JsonPropertyName("tc")]
	public NoteDir TailDirection { get; set; }

	[JsonPropertyName("tmu")]
	public float TailControlPointLengthMultiplier { get; set; }

	[JsonPropertyName("m")]
	public ArcMidAnchorMode MidAnchorMode { get; set; }
}

public class BsBurstSliderV3 : IBsChains
{
	[JsonPropertyName("c")]
	public NoteColor Color { get; set; }

	[JsonPropertyName("b")]
	public float Beat { get; set; }

	[JsonPropertyName("x")]
	public int X { get; set; }

	[JsonPropertyName("y")]
	public int Y { get; set; }

	[JsonPropertyName("d")]
	public NoteDir Direction { get; set; }

	[JsonPropertyName("tb")]
	public float TailBeat { get; set; }

	[JsonPropertyName("tx")]
	public int TailX { get; set; }

	[JsonPropertyName("ty")]
	public int TailY { get; set; }

	[JsonPropertyName("sc")]
	public int SliceCount { get; set; }

	[JsonPropertyName("s")]
	public float SquishFactor { get; set; }
}
