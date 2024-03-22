using Microsoft.Extensions.Logging;
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
		//await MapSelector.ChunkMpackFile(5_000);
		//await MapSelector.RunToMempack();

		var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
		var sw = new Stopwatch();

		sw.Restart();
		var dataset = await MapSelector.ReadMpack<List<List<LightEvent>>>();

		Parallel.ForEach(dataset, m =>
		{
			m.RemoveAll(e => e.Type is 10 or 14 or 15 || e.Value is < 0 or > ushort.MaxValue);
		});
		dataset.RemoveAll(m => m.Count == 0);

		Console.WriteLine("Read mpack: {0}s", sw.Elapsed.TotalSeconds);

		await Console.Out.WriteLineAsync(dataset.Sum(x => x.Count).ToString());

		var tokens = new LightToken[dataset.Count][];

		Parallel.ForEach(dataset.Select((m, i) => (m, i)), mi =>
		{
			var (m, i) = mi;

			// convert to light tokens
			// take absolute value of time and convert to invervals between each event

			var lightTokens = new LightToken[m.Count];
			Dictionary<float, int> times = [];
			foreach (var e in m)
			{
				var key = MathF.Round(e.Time, 3);
				ref var occCnt = ref CollectionsMarshal.GetValueRefOrAddDefault(times, key, out _);
				occCnt++;
			}

			for (int j = 0; j < m.Count - 1; j++)
			{
				var currentEvent = m[j];
				var nextEvent = m[j + 1];

				var t = nextEvent.Time - currentEvent.Time;

				lightTokens[j] = new LightToken(
					currentEvent.Type,
					LightToken.CategorizeLightSpeed(t),
					checked((ushort)currentEvent.Value),
					currentEvent.HasFloatValue,
					times[float.Round(currentEvent.Time, 3)] > 1);
			}

			lightTokens[^1] = new LightToken(
				m[^1].Type,
				LightToken.TimeMax,
				checked((ushort)m[^1].Value),
				m[^1].HasFloatValue,
				times[float.Round(m[^1].Time, 3)] > 1);

			tokens[i] = lightTokens;
		});

		Console.WriteLine("Tokenized: {0}s", sw.Elapsed.TotalSeconds);

		//var model = new LightMarkov(NullLogger<LightMarkov>.Instance, 2);


		sw.Restart();
		Console.WriteLine("Learning");

		var markB = MarkovBuilder<LightToken>.FromPhrasesParallel(tokens.Select(x => (ReadOnlyMemory<LightToken>)x), 2);
		//var markB = new MarkovBuilder<LightToken>(2);
		//foreach (var token in tokens)
		//{
		//	markB.AddPhrase(token);
		//}

		Console.WriteLine("Learned: {0}s", sw.Elapsed.TotalSeconds);

		sw.Restart();

		var mark = markB.Build();

		Console.WriteLine("Build Model: {0}s", sw.Elapsed.TotalSeconds);

		//var example = model.Walk().Take(10).ToList();

		// json serialize and print

		//Console.WriteLine(JsonSerializer.Serialize(example));

		return;
	}
}

[StructLayout(LayoutKind.Explicit)]
public readonly struct LightToken(byte type, byte time, ushort value, bool floatValue, bool combo) : IEquatable<LightToken>
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
	public readonly bool FloatValue = floatValue;
	[FieldOffset(5)]
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
}
