using RateMapSeveritySaber.Parser.Abstract;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace RateMapSeveritySaber.Parser.Beatmaps;

public class BsMapV2 : IBsMap
{
	IEnumerable<IBsNote> IBsMap.Notes => Notes.Where(x => x.Type is BsNoteV2.EType.Red or BsNoteV2.EType.Blue);
	IEnumerable<IBsChains> IBsMap.Chains => [];
	IEnumerable<IBsArcs> IBsMap.Arcs => Sliders;
	IEnumerable<IBsObstacle> IBsMap.Obstacles => Obstacles;
	IEnumerable<IBsBomb> IBsMap.Bombs => Notes.Where(x => x.Type == BsNoteV2.EType.Bomb);

	[JsonPropertyName("_version")]
	public required string Version { get; set; }

	[JsonPropertyName("_events")]
	public List<BsEventV2> Events { get; set; } = [];

	[JsonPropertyName("_notes")]
	public List<BsNoteV2> Notes { get; set; } = [];

	[JsonPropertyName("_obstacles")]
	public List<BsObstacleV2> Obstacles { get; set; } = [];

	[JsonPropertyName("_sliders")]
	public List<BsSliderV2> Sliders { get; set; } = []; // Added in 2.6.0
}

[DebuggerDisplay("{X} {Y} | {Direction} {Type}")]
public class BsNoteV2 : IBsNote, IBsBomb
{
	[JsonPropertyName("_time")]
	public float Beat { get; set; }

	[JsonPropertyName("_lineIndex")]
	public int X { get; set; }

	[JsonPropertyName("_lineLayer")]
	public int Y { get; set; }

	[JsonPropertyName("_type")]
	public EType Type { get; set; }

	[JsonPropertyName("_cutDirection")]
	public NoteDir Direction { get; set; }

	NoteColor IBsNote.Type => (NoteColor)Type;
	int IBsNote.AngleOffset => 0;

	public enum EType
	{
		Red = 0,
		Blue = 1,
		Bomb = 3,
	}
}

public class BsObstacleV2 : IBsObstacle
{
	[JsonPropertyName("_time")]
	public float Beat { get; set; }

	[JsonPropertyName("_lineIndex")]
	public int X { get; set; }

	[JsonPropertyName("_lineLayer")]
	public int? Y { get; set; } // Added in 2.6.0

	int ILocatedObject.Y
	{
		get => Type switch
		{
			EType.FullHeight => 0,
			EType.Crouch => 2,
			EType.Free => Y ?? 0, // Added in 2.6.0
		};
	}

	[JsonPropertyName("_type")]
	public EType Type { get; set; }

	[JsonPropertyName("_duration")]
	public float BeatDuration { get; set; }

	[JsonPropertyName("_width")]
	public int Width { get; set; }

	[JsonPropertyName("_height")]
	public int? Height { get; set; } // Added in 2.6.0

	int IBsObstacle.Height
	{
		get => Type switch
		{
			EType.FullHeight => 5,
			EType.Crouch => 3,
			EType.Free => Height ?? 0, // Added in 2.6.0
		};
	}

	float IDurationObject.TailBeat => Beat + BeatDuration;

	public enum EType
	{
		FullHeight = 0,
		Crouch = 1,
		Free = 2, // Added in 2.6.0
	}
}

public class BsEventV2 : ITimedObject
{
	[JsonPropertyName("_time")]
	public float Beat { get; set; }

	[JsonPropertyName("_type")]
	public EType Type { get; set; }

	[JsonPropertyName("_value")]
	public EValue Value { get; set; }

	public enum EType
	{
		///<summary>Controls lights in the Back Lasers group.</summary>
		Ev_0 = 0,

		///<summary>Controls lights in the Ring Lights group.</summary>
		Ev_1 = 1,

		///<summary>Controls lights in the Left Rotating Lasers group.</summary>
		Ev_2 = 2,

		///<summary>Controls lights in the Right Rotating Lasers group.</summary>
		Ev_3 = 3,

		///<summary>Controls lights in the Center Lights group.</summary>
		Ev_4 = 4,

		///<summary>Controls boost light colors (secondary colors).</summary>
		Ev_5 = 5,

		///<summary>Controls extra left side lights in some environments.</summary>
		Ev_6 = 6,

		///<summary>Controls extra right side lights in some environments.</summary>
		Ev_7 = 7,

		///<summary>Creates one ring spin in the environment.</summary>
		Ev_8 = 8,

		///<summary>Controls zoom for applicable rings. Is not affected by _value.</summary>
		Ev_9 = 9,

		///<summary>Billie environment - Controls left side lasers</summary>
		Ev_10 = 10,

		///<summary>Billie environment - Controls right side lasers.</summary>
		Ev_11 = 11,

		///<summary>Controls rotation speed for applicable lights in Left Rotating Lasers.</summary>
		Ev_12 = 12,

		///<summary>Controls rotation speed for applicable lights in Right Rotating Lasers.</summary>
		Ev_13 = 13,

		///<summary>360/90 Early rotation. Rotates future objects, while also rotating objects at the same time.</summary>
		Ev_14 = 14,

		///<summary>360/90 Late rotation. Rotates future objects, but ignores rotating objects at the same time.</summary>
		Ev_15 = 15,

		///<summary>Interscope environment - Lowers car hydraulics. Gaga environment - Controls middle left tower height</summary>
		Ev_16 = 16,

		///<summary>Interscope environment - Raises car hydraulics. Gaga environment - Controls middle right tower height</summary>
		Ev_17 = 17,

		///<summary>Gaga environment - Controls outer left tower height</summary>
		Ev_18 = 18,

		///<summary>Gaga environment - Controls outer right tower height</summary>
		Ev_19 = 19,
	}

	public enum EValue
	{
		//<summary>Turns the light group off.</summary>
		Off = 0,

		//<summary>Changes the lights to blue, and turns the lights on.</summary>
		Blue_On = 1,

		//<summary>Changes the lights to blue, and flashes brightly before returning to normal.</summary>
		Blue_Flash = 2,

		//<summary>Changes the lights to blue, and flashes brightly before fading to black.</summary>
		Blue_Fade = 3,

		//<summary>Changes the lights to blue by fading from the current state.</summary>
		Blue_Transition = 4,

		//<summary>Changes the lights to red, and turns the lights on.</summary>
		Red_On = 5,

		//<summary>Changes the lights to red, and flashes brightly before returning to normal.</summary>
		Red_Flash = 6,

		//<summary>Changes the lights to red, and flashes brightly before fading to black.</summary>
		Red_Fade = 7,

		//<summary>Changes the lights to red by fading from the current state.</summary>
		Red_Transition = 8,

		//<summary>Changes the lights to white, and turns the lights on.</summary>
		White_On = 9,

		//<summary>Changes the lights to white, and flashes brightly before returning to normal.</summary>
		White_Flash = 10,

		//<summary>Changes the lights to white, and flashes brightly before fading to black.</summary>
		White_Fade = 11,

		//<summary>Changes the lights to white by fading from the current state.</summary>
		White_Transition = 12,
	}
}

public class BsSliderV2 : IBsArcs
{
	[JsonPropertyName("_colorType")]
	public NoteColor Color { get; set; }

	[JsonPropertyName("_headTime")]
	public float Beat { get; set; }

	[JsonPropertyName("_headLineIndex")]
	public int X { get; set; }

	[JsonPropertyName("_headLineLayer")]
	public int Y { get; set; }

	[JsonPropertyName("_headCutDirection")]
	public NoteDir Direction { get; set; }

	[JsonPropertyName("_headControlPointLengthMultiplier")]
	public float HeadControlPointLengthMultiplier { get; set; }

	[JsonPropertyName("_tailTime")]
	public float TailBeat { get; set; }

	[JsonPropertyName("_tailLineIndex")]
	public int TailX { get; set; }

	[JsonPropertyName("_tailLineLayer")]
	public int TailY { get; set; }

	[JsonPropertyName("_tailCutDirection")]
	public NoteDir TailDirection { get; set; }

	[JsonPropertyName("_tailControlPointLengthMultiplier")]
	public float TailControlPointLengthMultiplier { get; set; }

	[JsonPropertyName("_sliderMidAnchorMode")]
	public ArcMidAnchorMode MidAnchorMode { get; set; }
}
