// See https://aka.ms/new-console-template for more information
using MarkovSharp;
using MarkovSharp.Models;
using MarkovSharp.TokenisationStrategies;
using MemoryPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Ramses.Lights;
using RateMapSeveritySaber;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

//var mapsDir = new DirectoryInfo("F:/maps");

// DataCleanup.Run(mapsDir);
// return;

//await MapSelector.RunToMempack();
//Console.WriteLine("Done");
//return;

//await MapSelector.RunCompressMpack();
//Console.WriteLine("Compressed");

//unsafe
//{
//	Console.WriteLine("LightToken1 {0}", sizeof(LightToken));
//}

//return;

var sw = new Stopwatch();

sw.Restart();
var dataset = await MapSelector.ReadMpack();
Console.WriteLine("Read mpack: {0}ms", sw.Elapsed.TotalSeconds);

var tokens = new LightToken[dataset.Length][];

sw.Restart();
Console.WriteLine("Tokenizing");

Parallel.ForEach(dataset.Select((m, i) => (m, i)), mi =>
{
	var (m, i) = mi;

	// convert to light tokens
	// take absolute value of time and convert to invervals between each event

	var lastEvent = m[0];
	var lightTokens = new LightToken[m.Length];
	lightTokens[0] = new LightToken
	{
		Time = LightToken.LightType.Slow,
		Value = lastEvent.Value,
		Type = lastEvent.Type,
		FloatValue = lastEvent.FloatValue.HasValue
	};

	for (int j = 1; j < m.Length; j++)
	{
		var currentEvent = m[j];
		var t = currentEvent.Time - lastEvent.Time;

		lightTokens[j] = new LightToken
		{
			Time = LightToken.CategorizeLightSpeed(t),
			Type = currentEvent.Type,
			Value = currentEvent.Value,
			FloatValue = currentEvent.FloatValue.HasValue
		};

		lastEvent = currentEvent;
	}

	tokens[i] = lightTokens;
});

Console.WriteLine("Tokenized: {0}ms", sw.Elapsed.TotalSeconds);

var model = new LightMarkov(NullLogger<LightMarkov>.Instance, 2);


sw.Restart();
Console.WriteLine("Learning");

model.Learn(tokens);

Console.WriteLine("Learned: {0}ms", sw.Elapsed.TotalSeconds);

var example = model.Walk().Take(10).ToList();

// json serialize and print

Console.WriteLine(JsonSerializer.Serialize(example));

return;

/*
 light event stats:

_time: 659964199
_type: 659964199
_value: 659964134
_customData: 243335783
_floatValue: 35366593
selected: 100
: 1
 
 */

class LightMarkov(ILogger logger, int level = 2) : GenericMarkov<LightToken[], LightToken>(logger, level)
{
	public override LightToken GetPrepadUnigram() => LightToken.ZeroLight;

	public override LightToken GetTerminatorUnigram() => LightToken.ZeroLight;

	public override LightToken[] RebuildPhrase(IEnumerable<LightToken> tokens) => tokens.ToArray();

	public override IEnumerable<LightToken> SplitTokens(LightToken[] phrase) => phrase;
}

public readonly struct LightToken : IComparable<LightToken>, IEquatable<LightToken>
{
	public static readonly LightToken ZeroLight = default;

	public int Value { get; init; }
	public LightType Time { get; init; }
	public byte Type { get; init; }
	public bool FloatValue { get; init; }

	public static LightType CategorizeLightSpeed(float diff) => diff switch
	{
		< 0.1f => LightType.Strobo,
		< 0.5f => LightType.Highlight,
		_ => LightType.Slow,
	};

	public readonly int CompareTo(LightToken other)
	{
		var timeComparison = Time.CompareTo(other.Time);
		if (timeComparison != 0) return timeComparison;
		var valueComparison = Value.CompareTo(other.Value);
		if (valueComparison != 0) return valueComparison;
		var typeComparison = Type.CompareTo(other.Type);
		if (typeComparison != 0) return typeComparison;
		return FloatValue.CompareTo(other.FloatValue);
	}

	public readonly bool Equals(LightToken other)
	{
		return Value == other.Value
			&& Time == other.Time
			&& Type == other.Type
			&& FloatValue == other.FloatValue;
	}

	public enum LightType : byte
	{
		Strobo,
		Highlight,
		Slow
	}

	public override bool Equals(object obj)
	{
		return obj is LightToken other && Equals(other);
	}

	public override int GetHashCode() => HashCode.Combine(Value, Time, Type, FloatValue);

	public static bool operator ==(LightToken left, LightToken right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(LightToken left, LightToken right)
	{
		return !(left == right);
	}
}
