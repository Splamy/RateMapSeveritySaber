using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RateMapSeveritySaber;

public sealed class TimeSpanConverter : JsonConverter<TimeSpan>
{
	private const int MaximumTimeSpanFormatLength = 26; // -dddddddd.hh:mm:ss.fffffff

	public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
	{
		Span<byte> output = stackalloc byte[MaximumTimeSpanFormatLength];
		bool result = Utf8Formatter.TryFormat(value, output, out int bytesWritten, 'c');
		Debug.Assert(result);
		writer.WriteStringValue(output[..bytesWritten]);
	}
}
