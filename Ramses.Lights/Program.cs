using Microsoft.Extensions.Logging;
using ObjectLayoutInspector;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ramses.Lights;

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

public class Program
{
	private static async Task Main(string[] args)
	{
		//TypeLayout.PrintLayout<Dictionary<ChainState<LightToken>, int>>();
		//return;
		//await MapSelector.ChunkMpackFile(5_000);
		//await MapSelector.RunToMempack();

		var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
		var sw = new Stopwatch();

		sw.Restart();

		var dataset = await MapSelector.ReadMpack<List<List<LightEvent>>>();

		Console.WriteLine("Read mpack: {0}s", sw.Elapsed.TotalSeconds);
		Console.WriteLine("Total Maps: {0}", dataset.Count);
		Console.WriteLine("Total Events: {0}", dataset.Sum(x => x.Count));

		sw.Restart();

		var tokens = Mapping.Tokenize(dataset);

		Console.WriteLine("Tokenized: {0}s", sw.Elapsed.TotalSeconds);

		sw.Restart();
		Console.WriteLine("Learning");

		var mark = Mapping.Train(tokens);

		Console.WriteLine("Learned: {0}s", sw.Elapsed.TotalSeconds);

		void PrintStats()
		{
			Console.WriteLine("Dic Size: {0}", mark.Items.Count);

			var mostCommonWeigth = mark.Items
				.SelectMany(x => x.Value.Values)
				.MaxBy(x => x.Weight);

			Console.WriteLine("Most Common: {0} {1}", mostCommonWeigth.Key, mostCommonWeigth.Weight);

			var hist = mark.Items.Values
				.GroupBy(x => x.Values.Length, y => 1)
				.Select(x => (x.Key, Count: x.Count()))
				.OrderByDescending(x => x.Count)
				.Take(10)
				.ToList();

			Console.WriteLine("Most Common Hist");
			foreach (var (key, count) in hist)
			{
				Console.WriteLine("Hist: {0} {1}", key, count);
			}

			var uniqueTokens = tokens
				.SelectMany(x => x)
				.ToHashSet();

			Console.WriteLine("Unique Tokens: {0}", uniqueTokens.Count);

			var uniqueTokensWithoutTime = uniqueTokens
				.Select(x => new LightToken(x.Type, 0, x.Value, x.StateValue, x.FloatValue, x.Combo))
				.ToHashSet();

			Console.WriteLine("Unique Tokens Without Time: {0}", uniqueTokensWithoutTime.Count);

			var valueHist = tokens
				.SelectMany(t => t)
				.GroupBy(x => x.Value)
				.Select(x => (x.Key, Count: x.Count()))
				.OrderByDescending(x => x.Count)
				.Take(10)
				.ToList();

			Console.WriteLine("Most Common Tokens");
			foreach (var (key, count) in valueHist)
			{
				Console.WriteLine("Hist: {0} {1}", key, count);
			}
		}

		PrintStats();

		Mapping.GenerateMap(mark);
	}
}

[StructLayout(LayoutKind.Auto)]
public record struct LightTokenBuilder
{
	public byte Type;
	public ushort Value;
	public byte StateVal;
	public byte Time;
	public bool IsCombo;
	public bool HasFloatValue;

	public readonly LightToken ToToken()
	{
		return new LightToken(Type, Time, Value, StateVal, HasFloatValue, IsCombo);
	}
}

[StructLayout(LayoutKind.Explicit)]
public readonly struct LightToken(byte type, byte time, ushort value, byte stateVal, bool floatValue, bool combo)
	: IEquatable<LightToken>, IComparable<LightToken>
{
	public const byte TimeMax = 12;

	[FieldOffset(0)]
	public readonly ulong _data;

	[FieldOffset(0)]
	public readonly ushort Value = value;
	[FieldOffset(2)]
	public readonly byte Time = time;
	[FieldOffset(3)]
	public readonly byte Type = type;
	[FieldOffset(4)]
	public readonly byte StateValue = stateVal;
	[FieldOffset(5)]
	public readonly bool FloatValue = floatValue;
	[FieldOffset(6)]
	public readonly bool Combo = combo;

	public static byte CategorizeLightSpeedV1(float diff) => diff switch
	{
		< 0.050f => 0,
		< 0.100f => 1,
		< 0.200f => 2,
		< 0.500f => 3,
		< 1.000f => 4,
		_ => 5
	};

	public static byte CategorizeLightSpeed(float diff)
	{
		if (diff < 0.001f)
		{
			return 0;
		}

		var log = MathF.Log2(diff * 1000 + 1);

		if (log > TimeMax)
		{
			return TimeMax;
		}

		return (byte)log;
	}

	public static bool operator ==(LightToken left, LightToken right) => left.Equals(right);
	public static bool operator !=(LightToken left, LightToken right) => !left.Equals(right);

	public bool Equals(LightToken other) => _data == other._data;
	public override bool Equals(object? obj) => obj is LightToken other && Equals(other);

	public override int GetHashCode() => _data.GetHashCode();

	public int CompareTo(LightToken other) => _data.CompareTo(other._data);

	public override string ToString() => $"et:{Type} t2:{Time} i:{Value} i+:{StateValue} f:{FloatValue} c:{Combo}";
}
