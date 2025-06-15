using RateMapSeveritySaber.Parser.Beatmaps;
using RateMapSeveritySaber.Parser.Info;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RateMapSeveritySaber.Parser;

public static class BsParser
{
	public static IBsBeatmap ParseMap(Stream stream)
	{
		var jsonElement = JsonSerializer.Deserialize<JsonElement>(stream);
		return ParseMapInternal(jsonElement);
	}

	public static async Task<IBsBeatmap> ParseMapAsync(Stream stream)
	{
		var jsonElement = await JsonSerializer.DeserializeAsync<JsonElement>(stream);
		return ParseMapInternal(jsonElement);
	}

	private static IBsBeatmap ParseMapInternal(JsonElement jsonElement)
	{
		string? version = null;
		if (jsonElement.TryGetProperty("version", out var version1))
		{
			version = version1.GetString();
		} else if (jsonElement.TryGetProperty("_version", out var version2))
		{
			version = version2.GetString();
		}
		if (version is null)
		{
			throw new Exception("No version found in map");
		}

		if (version.StartsWith("2."))
		{
			return jsonElement.Deserialize<BsBeatmapV2>()!;
		}

		if (version.StartsWith("3."))
		{
			return jsonElement.Deserialize<BsBeatmapV3>()!;
		}

		if (version.StartsWith("4."))
		{
			return jsonElement.Deserialize<BsBeatmapV4>()!;
		}

		throw new NotSupportedException($"Unknown map version: {version}");
	}

	public static IBsInfo ParseInfo(Stream stream)
	{
		var jsonElement = JsonSerializer.Deserialize<JsonElement>(stream);
		return ParseInfoInternal(jsonElement);
	}

	public static async Task<IBsInfo> ParseInfoAsync(Stream stream)
	{
		var jsonElement = await JsonSerializer.DeserializeAsync<JsonElement>(stream);
		return ParseInfoInternal(jsonElement);
	}

	private static IBsInfo ParseInfoInternal(JsonElement jsonElement)
	{
		string? version = null;
		if (jsonElement.TryGetProperty("version", out var version1))
		{
			version = version1.GetString();
		} else if (jsonElement.TryGetProperty("_version", out var version2))
		{
			version = version2.GetString();
		}
		if (version is null)
		{
			throw new Exception("No version found in info");
		}

		if (version.StartsWith("2."))
		{
			return jsonElement.Deserialize<BsInfoV2>()!;
		}

		if (version.StartsWith("4."))
		{
			return jsonElement.Deserialize<BsInfoV4>()!;
		}

		throw new NotSupportedException($"Unknown info version: {version}");
	}

}
