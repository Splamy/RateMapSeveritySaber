using RateMapSeveritySaber.Parser.Abstract;
using System.Collections.Generic;
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

public class BsMapV4 : IBsMap
{
	IEnumerable<IBsNote> IBsMap.Notes => Notes.Select(note => new BsNoteV4Internal(this, note));
	IEnumerable<IBsChains> IBsMap.Chains => Chains.Select(chain => new BsChainV4Internal(this, chain));
	IEnumerable<IBsArcs> IBsMap.Arcs => Arcs.Select(arc => new BsArcV4Internal(this, arc));
	IEnumerable<IBsObstacle> IBsMap.Obstacles => Obstacles.Select(obstacle => new BsObstacleV4Internal(this, obstacle));
	IEnumerable<IBsBomb> IBsMap.Bombs => Bombs.Select(bomb => new BsBombV4Internal(this, bomb));

	[JsonPropertyName("version")]
	public required string Version { get; set; }

	[JsonPropertyName("colorNotes")]
	public List<BsNoteV4> Notes { get; set; } = [];

	[JsonPropertyName("colorNotesData")]
	public List<BsNoteDataV4> NotesData { get; set; } = [];

	[JsonPropertyName("bombNotes")]
	public List<BsBombV4> Bombs { get; set; } = [];

	[JsonPropertyName("bombNotesData")]
	public List<BsBombDataV4> BombsData { get; set; } = [];

	[JsonPropertyName("obstacles")]
	public List<BsObstacleV4> Obstacles { get; set; } = [];

	[JsonPropertyName("obstaclesData")]
	public List<BsObstacleDataV4> ObstaclesData { get; set; } = [];

	[JsonPropertyName("arcs")]
	public List<BsArcV4> Arcs { get; set; } = [];

	[JsonPropertyName("arcsData")]
	public List<BsArcDataV4> ArcsData { get; set; } = [];

	[JsonPropertyName("chains")]
	public List<BsChainV4> Chains { get; set; } = [];

	[JsonPropertyName("chainsData")]
	public List<BsChainDataV4> ChainsData { get; set; } = [];
}

#region Notes

public class BsNoteV4
{
	[JsonPropertyName("b")]
	public float Beat { get; set; }

	[JsonPropertyName("r")]
	public int RotationLane { get; set; }

	[JsonPropertyName("i")]
	public int Index { get; set; }
}

public class BsNoteDataV4
{
	[JsonPropertyName("x")]
	public int X { get; set; }

	[JsonPropertyName("y")]
	public int Y { get; set; }

	[JsonPropertyName("c")]
	public NoteColor Color { get; set; }

	[JsonPropertyName("d")]
	public NoteDir Direction { get; set; }

	[JsonPropertyName("a")]
	public int AngleOffset { get; set; }
}

internal class BsNoteV4Internal(BsMapV4 map, BsNoteV4 note) : IBsNote
{
	public float Beat => note.Beat;
	private BsNoteDataV4 NoteData => map.NotesData[note.Index];
	public int X => NoteData.X;
	public int Y => NoteData.Y;
	public NoteColor Type => NoteData.Color;
	public NoteDir Direction => NoteData.Direction;
	public int AngleOffset => NoteData.AngleOffset;
}

#endregion

#region Bombs

public class BsBombV4
{
	[JsonPropertyName("b")]
	public float Beat { get; set; }

	[JsonPropertyName("r")]
	public int RotationLane { get; set; }

	[JsonPropertyName("i")]
	public int Index { get; set; }
}

public class BsBombDataV4
{
	[JsonPropertyName("x")]
	public int X { get; set; }

	[JsonPropertyName("y")]
	public int Y { get; set; }
}

internal class BsBombV4Internal(BsMapV4 map, BsBombV4 bomb) : IBsBomb
{
	public float Beat => bomb.Beat;
	private BsBombDataV4 BombData => map.BombsData[bomb.Index];
	public int X => BombData.X;
	public int Y => BombData.Y;
}

#endregion

#region Obstacles

public class BsObstacleV4
{
	[JsonPropertyName("b")]
	public float Beat { get; set; }

	[JsonPropertyName("r")]
	public int RotationLane { get; set; }

	[JsonPropertyName("i")]
	public int Index { get; set; }
}

public class BsObstacleDataV4
{
	[JsonPropertyName("d")]
	public float Duration { get; set; }

	[JsonPropertyName("x")]
	public int X { get; set; }

	[JsonPropertyName("y")]
	public int Y { get; set; }

	[JsonPropertyName("w")]
	public int Width { get; set; }

	[JsonPropertyName("h")]
	public int Height { get; set; }
}

internal class BsObstacleV4Internal(BsMapV4 map, BsObstacleV4 obstacle) : IBsObstacle
{
	private BsObstacleDataV4 ObstacleData => map.ObstaclesData[obstacle.Index];
	public float Beat => obstacle.Beat;
	public float TailBeat => obstacle.Beat + ObstacleData.Duration;
	public int X => ObstacleData.X;
	public int Y => ObstacleData.Y;
	public int Width => ObstacleData.Width;
	public int Height => ObstacleData.Height;
}

#endregion

#region Arcs

public class BsArcV4
{
	[JsonPropertyName("hb")]
	public float HeadBeat { get; set; }

	[JsonPropertyName("tb")]
	public float TailBeat { get; set; }

	[JsonPropertyName("hr")]
	public int HeadRotationLane { get; set; }

	[JsonPropertyName("tr")]
	public int TailRotationLane { get; set; }

	[JsonPropertyName("hi")]
	public int HeadNoteIndex { get; set; }

	[JsonPropertyName("ti")]
	public int TailNoteIndex { get; set; }

	[JsonPropertyName("ai")]
	public int Index { get; set; }
}

public class BsArcDataV4
{
	[JsonPropertyName("m")]
	public float HeadControlPointLengthMultiplier { get; set; }

	[JsonPropertyName("tm")]
	public float TailControlPointLengthMultiplier { get; set; }

	[JsonPropertyName("a")]
	public ArcMidAnchorMode MidAnchorMode { get; set; }
}

internal class BsArcV4Internal(BsMapV4 map, BsArcV4 arc) : IBsArcs
{
	private BsArcDataV4 ArcData => map.ArcsData[arc.Index];
	private BsNoteDataV4 HeadNote => map.NotesData[arc.HeadNoteIndex];
	private BsNoteDataV4 TailNote => map.NotesData[arc.TailNoteIndex];

	public float Beat => arc.HeadBeat;
	public int X => HeadNote.X;
	public int Y => HeadNote.Y;
	public NoteColor Color => HeadNote.Color;
	public NoteDir Direction => HeadNote.Direction;
	public float HeadControlPointLengthMultiplier => ArcData.HeadControlPointLengthMultiplier;
	public float TailBeat => arc.TailBeat;
	public int TailX => TailNote.X;
	public int TailY => TailNote.Y;
	public NoteDir TailDirection => TailNote.Direction;
	public float TailControlPointLengthMultiplier => ArcData.TailControlPointLengthMultiplier;
	public ArcMidAnchorMode MidAnchorMode => ArcData.MidAnchorMode;
}

#endregion

#region Chains

public class BsChainV4
{
	[JsonPropertyName("hb")]
	public float Beat { get; set; }

	[JsonPropertyName("tb")]
	public float TailBeat { get; set; }

	[JsonPropertyName("hr")]
	public int HeadRotationLane { get; set; }

	[JsonPropertyName("tr")]
	public int TailRotationLane { get; set; }

	[JsonPropertyName("i")]
	public int HeadNoteIndex { get; set; }

	[JsonPropertyName("ci")]
	public int Index { get; set; }
}

public class BsChainDataV4
{
	[JsonPropertyName("tx")]
	public int TailX { get; set; }

	[JsonPropertyName("ty")]
	public int TailY { get; set; }

	[JsonPropertyName("c")]
	public int SliceCount { get; set; }

	[JsonPropertyName("s")]
	public float SquishFactor { get; set; }
}

internal class BsChainV4Internal(BsMapV4 map, BsChainV4 chain) : IBsChains
{
	private BsChainDataV4 ChainData => map.ChainsData[chain.Index];
	private BsNoteDataV4 NoteData => map.NotesData[chain.HeadNoteIndex];

	public float Beat => chain.Beat;
	public NoteColor Color => NoteData.Color;
	public int X => NoteData.X;
	public int Y => NoteData.Y;
	public NoteDir Direction => NoteData.Direction;
	public float TailBeat => chain.TailBeat;
	public int TailX => ChainData.TailX;
	public int TailY => ChainData.TailY;
	public int SliceCount => ChainData.SliceCount;
	public float SquishFactor => ChainData.SquishFactor;
}

#endregion

/*
 Example of a V4 map structure:

{
  "version": "4.0.0",
  "colorNotes": [
    {
      "b": 10.0, // Beat
      "r": 0, // Rotation Lane
      "i": 0, // Metadata Index
    }
  ],
  "colorNotesData": [
    {
      "x": 1, // Line Index
      "y": 0, // Line Layer
      "c": 0, // Color
      "d": 1, // Cut Direction
      "a": 0, // Angle Offset
    }
  ],
  "bombNotes": [
    {
      "b": 10.0, // Beat
      "r": 0, // Rotation Lane
      "i": 0, // Metadata Index
    }
  ],
  "bombNotesData": [
    {
      "x": 1, // Line Index
      "y": 0, // Line Layer
    }
  ],
  "obstacles": [
    {
      "b": 10.0, // Beat
      "r": 0, // Rotation Lane
      "i": 0, // Metadata Index
    }
  ],
  "obstaclesData": [
    {
      "d": 5.0, // Duration
      "x": 1, // Line Index
      "y": 0, // Line Layer
      "w": 1, // Width
      "h": 5, // Height
    }
  ],
  "arcs": [
    {
      "hb": 10.0, // Head Beat
      "tb": 15.0, // Tail Beat
      "hr": 0, // Head Rotation Lane
      "tr": 0, // Tail Rotation Lane
      "hi": 0, // Head Note Metadata Index
      "ti": 1, // Tail Note Metadata Index
      "ai": 0, // Arc Metadata Index
    },
  ],
  "arcsData": [
    {
      "m": 1.0, // Head Control Point Length Multiplier
      "tm": 1.0, // Tail Control Point Length Multiplier
      "a": 0, // Mid-Anchor Mode
    },
  ],
  "chains": [
    {
      "hb": 10.0, // Head Beat
      "tb": 15.0, // Tail Beat
      "hr": 0, // Head Rotation Lane
      "tr": 0, // Tail Rotation Lane
      "i": 0, // Head Note Metadata Index
      "ci": 0, // Chain Metadata Index
    },
  ],
  "chainsData": [
    {
      "tx": 2, // Tail Line Index
      "ty": 2, // Tail Line Layer
      "c": 3, // Slice Count
      "s": 0.5, // Squish Factor
    },
  ],
  "njsEvents": [
    {
      "b": 1.0, // Beat
      "i": 0, // NJS Event Metadata Index Index
    },
  ],
  "njsEventData": [
    {
      "p": 1, // Extend (Use previous NJS event values)
      "e": 1, // Easing
      "d": 2.0, // Relative NJS difference from base NJS in Info.dat
    },
  ],
}

*/
