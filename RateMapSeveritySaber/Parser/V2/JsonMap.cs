using RateMapSeveritySaber.Parser.Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace RateMapSeveritySaber.Parser.V2;

#pragma warning disable CS8618
public class JsonMap
{
	[JsonPropertyName("_version")]
	public string Version { get; set; }

	[JsonPropertyName("_events")]
	public List<JsonEvent> Events { get; set; }

	[JsonPropertyName("_notes")]
	public List<JsonNote> Notes { get; set; }

	[JsonPropertyName("_obstacles")]
	public List<JsonObstacle> Obstacles { get; set; }
}

[DebuggerDisplay("{X} {Y} | {Direction} {Type}")]
public class JsonNote : IJsonNote, ILocatedObject
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

	public enum EType
	{
		Red = 0,
		Blue = 1,
	}
}

public class JsonObstacle : ITimedObject, ILocatedObject
{
	[JsonPropertyName("_time")]
	public float Beat { get; set; }
	[JsonPropertyName("_lineIndex")]
	public int X { get; set; }
	public int Y
	{
		get => Type switch
		{
			EType.FullHeight => 0,
			EType.Crouch => 1,
		};
		set => Type = value switch
		{
			0 => EType.FullHeight,
			1 => EType.Crouch,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	[JsonPropertyName("_type")]
	public EType Type { get; set; }
	[JsonPropertyName("_duration")]
	public float BeatDuration { get; set; }
	[JsonPropertyName("_width")]
	public int Width { get; set; }

	public enum EType
	{
		FullHeight,
		Crouch,
	}


}

public class JsonEvent : ITimedObject
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
#pragma warning restore CS8618
