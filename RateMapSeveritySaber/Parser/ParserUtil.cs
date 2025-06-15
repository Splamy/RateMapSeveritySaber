using System;

namespace RateMapSeveritySaber.Parser;

public static class ParserUtil
{
	public static T ToEnumChecked<T>(int value) where T : struct, Enum
	{
		if (Enum.IsDefined(typeof(T), value))
		{
			return (T)(object)value;
		}
		throw new ArgumentException($"Invalid enum value: {value} for type {typeof(T).Name}");
	}
}
