namespace RateMapSeveritySaber.Parser;

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

public enum MapCharacteristic
{
	Unknown = 0,
	Standard = 1,
	Degree90 = 2,
	Degree360 = 3,
	OneSaber = 4,
	NoArrows = 5,
	Lawless = 6,
	Lightshow = 7,
}
