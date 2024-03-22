using MarkovSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObjectLayoutInspector;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;

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
		var dataset = await MapSelector.ReadMpack<List<List<LightEvent>>>(0);

		Parallel.ForEach(dataset, m => {
			m.RemoveAll(e => e.Type is 10 or 14 or 15 || e.Value is < 0 or > ushort.MaxValue);
		});
		dataset.RemoveAll(m => m.Count == 0);

		Console.WriteLine("Read mpack: {0}ms", sw.Elapsed.TotalSeconds);

		await Console.Out.WriteLineAsync(dataset.SelectMany(x => x).Count().ToString());

		var tokens = new LightToken[dataset.Count][];

		Parallel.ForEach(dataset.Select((m, i) => (m, i)), mi =>
		{
			var (m, i) = mi;

			// convert to light tokens
			// take absolute value of time and convert to invervals between each event

			var lightTokens = new LightToken[m.Count];
			var times = m
				.GroupBy(e => float.Round(e.Time, 3))
				.ToDictionary(x => x.Key, x => x.Count());

			for (int j = 0; j < m.Count - 1; j++)
			{
				var currentEvent = m[j];
				var nextEvent = m[j + 1];

				var t = nextEvent.Time - currentEvent.Time;

				lightTokens[j] = new LightToken
				{
					Time = LightToken.CategorizeLightSpeed(t),
					Type = currentEvent.Type,
					Value = checked((ushort)currentEvent.Value),
					FloatValue = currentEvent.HasFloatValue,
					Combo = times[float.Round(currentEvent.Time, 3)] > 1,
				};
			}

			lightTokens[^1] = new LightToken
			{
				Time = 5,
				Value = checked((ushort)m[^1].Value),
				Type = m[^1].Type,
				FloatValue = m[^1].HasFloatValue,
				Combo = times[float.Round(m[^1].Time, 3)] > 1,
			};

			tokens[i] = lightTokens;
		});

		Console.WriteLine("Tokenized: {0}ms", sw.Elapsed.TotalSeconds);

		//var model = new LightMarkov(NullLogger<LightMarkov>.Instance, 2);


		sw.Restart();
		Console.WriteLine("Learning");

		var mark = new MarkovChain<LightToken>(2);

		mark.AddParallel(tokens, 1);

		Console.WriteLine("Learned: {0}ms", sw.Elapsed.TotalSeconds);

		//var example = model.Walk().Take(10).ToList();

		// json serialize and print

		//Console.WriteLine(JsonSerializer.Serialize(example));

		return;
	}
}

public readonly record struct LightToken
{
	public static readonly LightToken ZeroLight = default;

	public ushort Value { get; init; }
	public byte Time { get; init; }
	public byte Type { get; init; }
	public bool FloatValue { get; init; }
	public bool Combo { get; init; }

	public static byte CategorizeLightSpeed(float diff) => diff switch
	{
		< 0.050f => 0,
		< 0.100f => 1,
		< 0.200f => 2,
		< 0.500f => 3,
		< 1.000f => 4,
		_ => 5
	};

	public enum LightType : byte
	{
		Strobo,
		Highlight,
		Slow
	}
}
