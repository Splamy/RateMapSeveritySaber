using RateMapSeveritySaber.Parser.Abstract;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace RateMapSeveritySaber.Parser.V3;

#pragma warning disable CS8618
public class JsonMap : JsonMapBase
{
	//public float[] _BPMChanges { get; set; }

	//public List<object> _events { get; set; }
	[JsonPropertyName("bpmEvents")]
	public List<JsonBpm> BpmEvent { get; set; }

	[JsonPropertyName("rotationEvents")]
	public List<JsonRotationEvent> RotationEvent { get; set; }

	[JsonPropertyName("colorNotes")]
	public List<JsonNote> Notes { get; set; }

	[JsonPropertyName("bombNotes")]
	public List<JsonBomb> Bombs { get; set; }

	[JsonPropertyName("obstacles")]
	public List<JsonObstacle> Obstacles { get; set; }

	/// <summary>Aka. Arcs</summary>
	[JsonPropertyName("sliders")]
	public List<object> Sliders { get; set; }

	/// <summary>Aka. Sliders</summary>
	[JsonPropertyName("burstSliders")]
	public List<object> BurstSliders { get; set; }

	[JsonPropertyName("basicBeatmapEvents")]
	public List<object> BasicBeatmapEvents { get; set; }

	[JsonPropertyName("colorBoostBeatmapEvents")]
	public List<object> ColorBoostBeatmapEvents { get; set; }

	[JsonPropertyName("lightColorEventBoxGroups")]
	public List<object> LightColorEventBoxGroups { get; set; }

	[JsonPropertyName("lightRotationEventBoxGroups")]
	public List<object> LightRotationEventBoxGroups { get; set; }

	[JsonPropertyName("basicEventTypesWithKeywords")]
	public List<object> BasicEventTypesWithKeywords { get; set; }
}

public class JsonBpm : ITimedObject
{
	[JsonPropertyName("b")]
	public float Beat { get; set; }
	[JsonPropertyName("m")]
	public float Bpm { get; set; }
}

public class JsonRotationEvent : ITimedObject
{
	[JsonPropertyName("b")]
	public float Beat { get; set; }
	[JsonPropertyName("e")]
	public ERotation Type { get; set; }
	[JsonPropertyName("r")]
	public float Rotation { get; set; }

	public enum ERotation
	{
		//__S Early rotation. Rotates future objects, while also rotating objects at the same time.
		Early = 0,
		//__S Late rotation. Rotates future objects, but ignores rotating objects at the same time.
		Late = 1,
	}
}

public class JsonNote : IJsonNote, ILocatedObject
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

public class JsonBomb : ITimedObject, ILocatedObject
{
	[JsonPropertyName("b")]
	public float Beat { get; set; }
	[JsonPropertyName("x")]
	public int X { get; set; }
	[JsonPropertyName("y")]
	public int Y { get; set; }
}

public class JsonObstacle : ITimedObject, ILocatedObject
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
	public int Heigth { get; set; }
	[JsonPropertyName("d")]
	public float BeatDuration { get; set; }
}

public class JsonSlider { }
public class JsonBurstSlider { }



#pragma warning restore CS8618
