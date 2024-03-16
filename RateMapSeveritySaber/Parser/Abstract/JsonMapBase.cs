using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace RateMapSeveritySaber.Parser.Abstract;

public class JsonMapBase
{
	[JsonPropertyName("_version")]
	internal string? version1 { get; set; }
	[JsonPropertyName("version")]
	internal string? version2 { get; set; }

	public virtual string? Version
	{
		get => version1 ?? version2 ?? string.Empty;
		set { }
	}
}
