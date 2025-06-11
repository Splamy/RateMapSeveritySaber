using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Buffers;

namespace Ramses.Lights;

public static class Mapping
{
	private static readonly JsonSerializerOptions jIndented = new() { WriteIndented = true };

	public static List<LightToken[]> Tokenize(List<List<LightEvent>> dataset) => dataset
		.AsParallel()
		.WithDegreeOfParallelism(Utils.Threads)
		.Select(m =>
		{
			// Check monotonous

			bool isMonoton = true;
			for (int i = 1; i < m.Count; i++)
			{
				if (m[i].Time < m[i - 1].Time)
				{
					isMonoton = false;
					break;
				}
			}

			if (!isMonoton)
			{
				Console.WriteLine("Re-Sort");
				m.Sort((a, b) => a.Time.CompareTo(b.Time));
			}

			// Preprocess

			using var lightBuilder = new FixedBufferList<LightTokenBuilder>(m.Count);
			byte stateRotLeft = 0; // ET 2
			byte stateRotRight = 0; // ET 3

			for (int i = 0; i < m.Count; i++)
			{
				var ev = m[i];
				if (ev.Value is < 0 or > ushort.MaxValue)
				{
					continue;
				}

				var lightTokenBuilder = new LightTokenBuilder()
				{
					Type = ev.Type,
					Value = checked((ushort)ev.Value),
					StateVal = stateRotLeft,
					// Time
					// IsCombo
					HasFloatValue = ev.HasFloatValue,
				};

				switch (ev.Type)
				{
				case EventType.LIGHT_LEFT_EXTRA2_LIGHT
					or EventType.LIGHT_RIGHT_EXTRA2_LIGHT
					or EventType.ROTATION_EARLY_LANE
					or EventType.ROTATION_LATE_LANE:
					continue;

				case EventType.LEFT_ROT:
					stateRotLeft = (byte)Math.Clamp(ev.Value, 0, 255);
					continue;

				case EventType.RIGHT_ROT:
					stateRotRight = (byte)Math.Clamp(ev.Value, 0, 255);
					continue;

				case EventType.LEFT:
					lightTokenBuilder.StateVal = stateRotLeft;
					break;

				case EventType.RIGHT:
					lightTokenBuilder.StateVal = stateRotRight;
					break;

				default:
					lightTokenBuilder.StateVal = 0;
					break;
				}

				// Check if time is almost the same as previous or next
				lightTokenBuilder.IsCombo =
					(i > 0 && Utils.NearlyEqual(ev.Time, m[i - 1].Time))
					|| (i < m.Count - 1 && Utils.NearlyEqual(ev.Time, m[i + 1].Time));

				float realNextTime = float.MaxValue;
				for (int j = i + 1; j < m.Count; j++)
				{
					if (!Utils.NearlyEqual(m[j].Time, ev.Time))
					{
						realNextTime = m[j].Time;
						break;
					}
				}

				lightTokenBuilder.Time = realNextTime == float.MaxValue
					? LightToken.TimeMax
					: LightToken.CategorizeLightSpeed(realNextTime - ev.Time);

				lightBuilder.Add(lightTokenBuilder);
			}

			if (lightBuilder.Count == 0)
			{
				return [];
			}

			// Tokenize

			var lightTokens = new LightToken[lightBuilder.Count];
			for (int j = 0; j < lightBuilder.Count; j++)
			{
				lightTokens[j] = lightBuilder[j].ToToken();
			}

			return lightTokens;
		})
		.Where(x => x.Length > 0)
		.ToList();

	public static MarkovChain<LightToken> Train(List<LightToken[]> tokens, int order = 2)
	{
		return MarkovBuilder<LightToken>
			.FromPhrasesParallel(tokens
				.Select(x => (ReadOnlyMemory<LightToken>)x), order);
	}

	public static void GenerateMap(MarkovChain<LightToken> mark)
	{
		var mapFileFs = File.ReadAllBytes(@"F:\SteamLibrary\steamapps\common\Beat Saber\Beat Saber_Data\CustomWIPLevels\Laur-FairyinStrasbourg\HardStandard.dat");
		var mapJson = JsonSerializer.Deserialize<JsonNode>(mapFileFs, jIndented);

		var bpm = 138f;
		var bps = bpm / 60f;
		var spb = 1f / bps;

		var noteTimes = mapJson["colorNotes"]
			.AsArray()
			.Select(n => n["b"].GetValue<float>() * spb)
			.ToArray();
		var sliderBitsTimes = mapJson["burstSliders"]
			.AsArray()
			.SelectMany(n =>
			{
				var start = n["b"].AsValue().GetValue<float>() * spb;
				var end = n["tb"].AsValue().GetValue<float>() * spb;
				var duration = end - start;
				var bitTime = duration / n["sc"].GetValue<int>();
				return Enumerable.Range(0, n["sc"].GetValue<int>()).Select(i => start + i * bitTime);
			})
			.ToArray();

		var allTimes = noteTimes
			.Concat(sliderBitsTimes)
			.Select(n => float.Round(n, 3))
			.Distinct()
			.OrderBy(t => t)
			.ToArray();

		List<GenElem> genList = [];
		List<LightToken> combo = new();

		byte stateRotLeft = 0; // ET 2
		byte stateRotRight = 0; // ET 3

		for (int i = 0; i < allTimes.Length - 1; i++)
		{
			var time = allTimes[i];

			const int SampleSize = 16;

			combo.Clear();
			if (mark.GetNextStates(genList.Where(x => x.UseChain).Take(^2..).Select(x => x.Token)) is not { } weigths)
			{
				Console.WriteLine("No continue!!!");
				continue;
			}

			combo.AddRange(Enumerable.Range(0, 16).Select(_ => weigths.PickWeigthed()));
			if (combo.Count(x => x.Combo) > SampleSize / 2)
			{
				combo.RemoveAll(x => !x.Combo);
			}
			else
			{
				combo.RemoveAll(x => x.Combo);
			}

			for (int j = 0; j < 16; j++)
			{
				var next = mark.Chain(genList.Where(x => x.UseChain).Take(^2..).Select(x => x.Token)).FirstOrDefault();
				if (combo.Any(x => x.Type == next.Type))
				{
					continue;
				}
				combo.Add(next);
				if (!next.Combo)
				{
					break;
				}
			}

			foreach (var next in combo)
			{
				if (next.Type == EventType.LEFT && next.StateValue != stateRotLeft)
				{
					genList.Add(new(time, new LightToken(EventType.LEFT_ROT, 0, stateRotLeft, 0, next.FloatValue, false), false));
					stateRotLeft = next.StateValue;
				}
				else if (next.Type == EventType.RIGHT && next.StateValue != stateRotRight)
				{
					genList.Add(new(time, new LightToken(EventType.RIGHT_ROT, 0, stateRotRight, 0, next.FloatValue, false), false));
					stateRotRight = next.StateValue;
				}

				var t = LightToken.CategorizeLightSpeed(allTimes[i + 1] - allTimes[i]);
				genList.Add(new(time, new LightToken(next.Type, t, next.Value, 0, next.FloatValue, false), true));
			}
		}

		mapJson["basicBeatmapEvents"] = new JsonArray(genList.Select(t =>
		{
			var j = new JsonObject();
			j["b"] = float.Round(t.Time * bps, 3);
			j["et"] = t.Token.Type;
			j["i"] = t.Token.Value;
			j["f"] = 1f;
			return j;
		}).ToArray());

		var newMap = JsonSerializer.Serialize(mapJson, jIndented);
		File.WriteAllText(@"F:\SteamLibrary\steamapps\common\Beat Saber\Beat Saber_Data\CustomWIPLevels\Laur-FairyinStrasbourg\NormalStandard.dat", newMap);
	}

	internal record GenElem(
		float Time,
		LightToken Token,
		bool UseChain
	);
}
