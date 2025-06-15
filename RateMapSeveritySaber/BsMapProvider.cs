using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace RateMapSeveritySaber;

public abstract class BsMapProvider
{
	public const string InfoJson = "info.json";
	public const string InfoDat = "info.dat";

	public abstract IEnumerable<string> Files { get; }
	public abstract Stream? Get(string file);

	protected static bool MatchName(string a, string b)
		=> string.Equals(NormalizeName(a), NormalizeName(b), StringComparison.OrdinalIgnoreCase);

	protected static string NormalizeName(string name)
	{
		if (name.LastIndexOf('/') is >= 0 and var idx)
		{
			name = name[(idx + 1)..];
		}

		return string.Equals(name, InfoJson, StringComparison.OrdinalIgnoreCase) ? InfoDat : name;
	}

	public virtual Stream? GetInfoFile() => Get(InfoDat);
}

public class BsZipMapProvider(ZipArchive zip) : BsMapProvider
{
	public override IEnumerable<string> Files => zip.Entries.Select(e => e.FullName);

	public override Stream? Get(string file) => zip.Entries
		.FirstOrDefault((e) => MatchName(e.Name, file))?.Open();
}

public class BsFolderMapProvider(string folder) : BsMapProvider
{
	public override IEnumerable<string> Files => Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly);

	public override Stream? Get(string file)
	{
		var realName = Files.FirstOrDefault(f => MatchName(f, file));
		if (realName is null)
			return null;
		var path = Path.Combine(folder, realName);
		if (!File.Exists(path))
			return null;

		return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
	}
}
