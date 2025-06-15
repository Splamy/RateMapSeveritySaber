using RateMapSeveritySaber.Parser.Abstract;
using RateMapSeveritySaber.Parser.Beatmaps;
using RateMapSeveritySaber.Parser.Info;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RateMapSeveritySaber.Parser;

public static class BsParser
{
	public static IBsMap ParseMap(Stream stream)
	{
		var jsonElement = JsonSerializer.Deserialize<JsonElement>(stream);
		return ParseMapInternal(jsonElement);
	}

	public static async Task<IBsMap> ParseMapAsync(Stream stream)
	{
		var jsonElement = await JsonSerializer.DeserializeAsync<JsonElement>(stream);
		return ParseMapInternal(jsonElement);
	}

	private static IBsMap ParseMapInternal(JsonElement jsonElement)
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
			return jsonElement.Deserialize<BsMapV2>()!;
		}

		if (version.StartsWith("3."))
		{
			return jsonElement.Deserialize<BsMapV3>()!;
		}

		if (version.StartsWith("4."))
		{
			return jsonElement.Deserialize<BsMapV4>()!;
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
