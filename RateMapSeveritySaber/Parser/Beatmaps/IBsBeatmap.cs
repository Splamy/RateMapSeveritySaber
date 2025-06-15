using System.Collections.Generic;

namespace RateMapSeveritySaber.Parser.Beatmaps;

public interface IBsBeatmap
{
	public string Version { get; }

	public IEnumerable<IBsNote> Notes { get; }
	public IEnumerable<IBsChains> Chains { get; }
	public IEnumerable<IBsArcs> Arcs { get; }
	public IEnumerable<IBsObstacle> Obstacles { get; }
	public IEnumerable<IBsBomb> Bombs { get; }
}

public interface ITimedObject
{
	float Beat { get; }
}

public interface ILocatedObject
{
	int X { get; }
	int Y { get; }
}

public interface IDurationObject: ITimedObject
{
	/// <summary>Tail Beat</summary>
	float TailBeat { get; }
}

public interface IBsNote : ITimedObject, ILocatedObject
{
	NoteDir Direction { get; }
	NoteColor Type { get; }
	int AngleOffset { get; }
}

public interface IBsChains : IDurationObject, ILocatedObject
{
	/// <summary>Color</summary>
	NoteColor Color { get; }

	/// <summary>Head Cut Direction</summary>
	NoteDir Direction { get; }

	/// <summary>Tail Line Index</summary>
	int TailX { get; }

	/// <summary>Tail Line Layer</summary>
	int TailY { get; }

	/// <summary>Slice Count</summary>
	int SliceCount { get; }

	/// <summary>Squish Factor</summary>
	float SquishFactor { get; }
}

public interface IBsArcs : IDurationObject, ILocatedObject
{
	/// <summary>Color</summary>
	NoteColor Color { get; }

	/// <summary>Head Cut Direction</summary>
	NoteDir Direction { get; }

	/// <summary>Head Control Point Length Multiplier</summary>
	float HeadControlPointLengthMultiplier { get; }

	/// <summary>Tail Line Index</summary>
	int TailX { get; }

	/// <summary>Tail Line Layer</summary>
	int TailY { get; }

	/// <summary>Tail Cut Direction</summary>
	NoteDir TailDirection { get; }

	/// <summary>Tail Control Point Length Multiplier</summary>
	float TailControlPointLengthMultiplier { get; }

	/// <summary>Mid-Anchor Mode</summary>
	ArcMidAnchorMode MidAnchorMode { get; }
}

public enum ArcMidAnchorMode
{
	Straight = 0,
	Clockwise = 1,
	CounterClockwise = 2,
}

public interface IBsObstacle : IDurationObject, ILocatedObject
{
	int Width { get; }
	int Height { get; }
}

public interface IBsBomb : ITimedObject, ILocatedObject
{
}
