namespace RateMapSeveritySaber.Parser.Abstract;

public interface IJsonNote : ITimedObject
{
	NoteDir Direction { get; set; }
	NoteColor Type { get; set; }
	int X { get; set; }
	int Y { get; set; }
}
