namespace Ramses.Lights;

// https://github.com/Loloppe/ChroMapper-AutoMapper/blob/master/Items/Enumerator.cs

public static class EventType
{
	public const byte BACK = 0;
	public const byte RING = 1;
	public const byte LEFT = 2;
	public const byte RIGHT = 3;
	public const byte SIDE = 4;
	/// <summary>Deprecated</summary>
	public const byte BOOST = 5;
	public const byte LIGHT_LEFT_EXTRA_LIGHT = 6;
	public const byte LIGHT_RIGHT_EXTRA_LIGHT = 7;
	public const byte SPIN = 8;
	public const byte ZOOM = 9;
	public const byte LIGHT_LEFT_EXTRA2_LIGHT = 10;
	public const byte LIGHT_RIGHT_EXTRA2_LIGHT = 11;
	public const byte LEFT_ROT = 12;
	public const byte RIGHT_ROT = 13;
	/// <summary>Deprecated</summary>
	public const byte ROTATION_EARLY_LANE = 14;
	/// <summary>Deprecated</summary>
	public const byte ROTATION_LATE_LANE = 15;
	public const byte EXTRA_EVENT1 = 16;
	public const byte EXTRA_EVENT2 = 17;
	/// <summary>Deprecated</summary>
	public const byte BPM = 100;
}

public enum LaserType : byte
{
	// Further away from the center
	LEFT_BOTTOM_VERTICAL = 0,
	RIGHT_BOTTOM_VERTICAL = 1,
	LEFT_TOP_VERTICAL = 2,
	RIGHT_TOP_VERTICAL = 3,
	// Same as those above, but close to the center
	LEFT_BOTTOM_CENTER_VERTICAL = 4,
	RIGHT_BOTTOM_CENTER_VERTICAL = 5,
	LEFT_TOP_CENTER_VERTICAL = 6,
	RIGHT_TOP_CENTER_VERTICAL = 7,
	// Two horizontal layer on the left and two on the right
	LEFT_BOTTOM_HORIZONTAL = 8,
	RIGHT_BOTTOM_HORIZONTAL = 9,
	LEFT_TOP_HORIZONTAL = 10,
	RIGHT_TOP_HORIZONTAL = 11,
	// At the very back, point directly toward player
	TOP_CENTER = 12,
	BOTTOM_CENTER = 13,
	LEFT_CENTER = 14,
	RIGHT_CENTER = 15,
}

public enum EventLightValue : byte
{
	OFF = 0,
	BLUE_ON = 1,
	BLUE_FLASH = 2,
	BLUE_FADE = 3,
	BLUE_TRANSITION = 4,
	RED_ON = 5,
	RED_FLASH = 6,
	RED_FADE = 7,
	RED_TRANSITION = 8,
	WHITE_ON = 9,
	WHITE_FLASH = 10,
	WHITE_FADE = 11,
	WHITE_TRANSITION = 12,
}
