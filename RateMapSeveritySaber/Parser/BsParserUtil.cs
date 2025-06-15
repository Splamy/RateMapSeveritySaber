namespace RateMapSeveritySaber.Parser;

public static class BsParserUtil
{
	public static int DifficultyNameToNumber(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"EASY" => 1,
			"NORMAL" => 3,
			"HARD" => 5,
			"EXPERT" => 7,
			"EXPERTPLUS" => 9,
			_ => 0,
		};
	}

	public static string DifficultyNumberToName(int num)
	{
		return num switch
		{
			1 => "Easy",
			3 => "Normal",
			5 => "Hard",
			7 => "Expert",
			9 => "ExpertPlus",
			_ => "Unknown",
		};
	}

	public static MapCharacteristic NameToCharacteristic(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"STANDARD" => MapCharacteristic.Standard,
			"90DEGREE" => MapCharacteristic.Degree90,
			"360DEGREE" => MapCharacteristic.Degree360,
			"ONESABER" => MapCharacteristic.OneSaber,
			"NOARROWS" => MapCharacteristic.NoArrows,
			"LAWLESS" => MapCharacteristic.Lawless,
			"LIGHTSHOW" => MapCharacteristic.Lightshow,
			_ => MapCharacteristic.Unknown,
		};
	}

	public static string CharacteristicToName(MapCharacteristic map)
	{
		return map switch
		{
			MapCharacteristic.Standard => "Standard",
			MapCharacteristic.Degree90 => "90Degree",
			MapCharacteristic.Degree360 => "360Degree",
			MapCharacteristic.OneSaber => "OneSaber",
			MapCharacteristic.NoArrows => "NoArrows",
			MapCharacteristic.Lawless => "Lawless",
			MapCharacteristic.Lightshow => "Lightshow",
			MapCharacteristic.Unknown or _ => "Unknown",
		};
	}
}
