using MemoryPack;
using System;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Ramses.Lights;
public class MapSelector
{
	private static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		WriteIndented = false
	};

	public static void RunToJson()
	{
		Console.WriteLine("Reading maps...");

		SelectList selectList;
		{
			var sfile = File.OpenRead("F:/Ramses/select.v2.json");
			selectList = JsonSerializer.Deserialize<SelectList>(sfile)!;
		}

		Console.WriteLine("Reading events...");

		var concurrentPool = new ConcurrentBag<MemoryStream>();

		var fileLock = new object();
		int written = 0;

		using var superFile = File.Open("F:/Ramses/select.v2.events.json", FileMode.Create, FileAccess.Write, FileShare.None);
		superFile.Write("[\n"u8);

		Parallel.ForEach(selectList.Maps, x =>
		{
			using var mapFile = File.OpenRead(x.Path);
			var json = JsonSerializer.Deserialize<JsonElement>(mapFile);
			if (!json.TryGetProperty("_events", out var events))
				return;

			if (!concurrentPool.TryTake(out var buffer))
			{
				buffer = new MemoryStream();
			}

			buffer.SetLength(0);

			JsonSerializer.Serialize(buffer, events);

			lock (fileLock)
			{
				superFile.Write(buffer.GetBuffer().AsSpan(0, (int)buffer.Length));

				written++;
				if (written < selectList.Maps.Count)
				{
					superFile.Write(",\n"u8);
				}

				if (written % 100 == 0)
				{
					Console.WriteLine("Written {0}/{1}", written, selectList.Maps.Count);
				}
			}

			concurrentPool.Add(buffer);
		});

		superFile.Write("]"u8);
	}

	public static async Task RunToMempack()
	{
		Console.WriteLine("Reading maps...");

		SelectList selectList;
		{
			var sfile = File.OpenRead("F:/Ramses/select.v2.json");
			selectList = JsonSerializer.Deserialize<SelectList>(sfile)!;
		}

		Console.WriteLine("Reading events...");

		ConcurrentBag<LightEvent[]> lightMaps = [];
		int written = 0;

		Parallel.ForEach(selectList.Maps, new ParallelOptions() { /*MaxDegreeOfParallelism = 1 */}, x =>
		{
			var w = Interlocked.Increment(ref written);
			if (w % 100 == 0)
			{
				Console.WriteLine("Processed {0}/{1}", w, selectList.Maps.Count);
			}

			using var mapFile = File.OpenRead(x.Path);
			var json = JsonSerializer.Deserialize<JsonElement>(mapFile);
			if (!json.TryGetProperty("_events", out var events))
				return;

			try
			{
				var evs = events.Deserialize<LightEvent[]>(JsonSerializerOptions)!;
				lightMaps.Add(evs);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error reading {0}", x.Path);
				return;
			}
		});

		using Stream superFile = File.Open("F:/Ramses/select.v2.events.mpack", FileMode.Create, FileAccess.Write, FileShare.None);

		var lightMapsList = lightMaps.ToArray();
		await MemoryPackSerializer.SerializeAsync(superFile, lightMapsList);
	}

	public static async Task ChunkMpackFile(int mapCount)
	{
		var data = await ReadMpack<ReadOnlyMemory<LightEvent[]>>();

		var chunkCnt = 0;

		while (data.Length > mapCount)
		{
			var writeChunk = data.Length > mapCount ? mapCount : data.Length;
			await WriteChunk(data[..writeChunk], chunkCnt);
			data = data[writeChunk..];
			chunkCnt++;
		}

		static async Task WriteChunk(ReadOnlyMemory<LightEvent[]> list, int chunkId)
		{
			using var chunkFile = File.Open($"F:/Ramses/select.v2.events.chunk{chunkId:000}.mpack", FileMode.Create, FileAccess.Write, FileShare.None);
			await MemoryPackSerializer.SerializeAsync(chunkFile, list);
		}
	}

	public static async Task RunCompressMpack()
	{
		using Stream superFile = File.OpenRead("F:/Ramses/select.v2.events.mpack");

		using var compressedFile = File.Open("F:/Ramses/select.v2.events.mpack.deflate", FileMode.Create, FileAccess.Write, FileShare.None);

		using var comStream = new DeflateStream(compressedFile, CompressionLevel.Optimal);

		await superFile.CopyToAsync(comStream);

		comStream.Flush();

		Console.WriteLine("Done compressing");
	}

	public static async Task<TAll> ReadMpack<TAll>(int? chunk = default)
	{
		if (chunk.HasValue)
		{
			using var file = File.OpenRead($"F:/Ramses/select.v2.events.chunk{chunk.Value:000}.mpack");
			return await MemoryPackSerializer.DeserializeAsync<TAll>(file);
		}
		else
		{
			using var file = File.OpenRead("F:/Ramses/select.v2.events.mpack");
			return await MemoryPackSerializer.DeserializeAsync<TAll>(file);
		}

	}

	public static List<JsonArray> Read()
	{
		var evfile = File.OpenRead("F:/Ramses/select.v2.events.json");
		return JsonSerializer.Deserialize<List<JsonArray>>(evfile)!;
	}
}

[MemoryPackable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct LightEvent()
{
	[JsonPropertyName("_time")]
	public float Time { get; set; } = 0;

	[JsonPropertyName("_value")]
	public int Value { get; set; } = 0;

	[JsonPropertyName("_floatValue")]
	public float FloatValue { get; set; } = float.NaN;

	[JsonPropertyName("_type")]
	public byte Type { get; set; } = 0;

	[JsonIgnore]
	public readonly bool HasFloatValue => !float.IsNaN(FloatValue);
}
