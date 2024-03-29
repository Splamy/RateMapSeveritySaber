using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

namespace RateMapSeveritySaber;

public class Program
{
	public static void Main()
	{
		List<BsMapProvider> maps = [];

		var beatsaberPath = (string?)Registry.GetValue(
			@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 620980",
			"InstallLocation", null);

		foreach (var file in Directory.GetFiles(@"E:\_Projects\RateMapSeveritySaber\Test\TestMaps", "*.zip"))
		{
			maps.Add(new PlainZipMapProvider(new ZipArchive(File.OpenRead(file))));
		}

		if (beatsaberPath == null)
		{
			Console.WriteLine("Beatsaber not found :(");
		}
		else
		{
			string songFolder = Path.Combine(beatsaberPath, "Beat Saber_Data", "CustomLevels");
			string[] songNames = [
				"1ca11 (Odo - CoolingCloset)",
				"1d1dc (RISE - RateGyro & FakePope)" /*Paul*/,
				"1949e (Viyella's Scream - Timbo)",
				"2789 quo vadis"
			];

			foreach (var songName in songNames)
			{
				var mapPath = Path.Combine(songFolder, songName);
				if (!Directory.Exists(mapPath))
				{
					Console.WriteLine($"Map {songName} ({mapPath}) doesn't exist!");
					continue;
				}

				var sw = Stopwatch.StartNew();
				maps.Add(new FolderMapProvider(mapPath));
			}
		}

		var scores = maps.SelectMany(song =>
		{
			Console.WriteLine($"{song}:");

			var sw = Stopwatch.StartNew();
			var maps = BSMapIO.Read(song);
			//Console.WriteLine("Parsing: {0}ms", sw.ElapsedMilliseconds);

			return maps.Select(map =>
			{
				Console.Write("Level {0}: ", map.MapInfo.DifficultyRank);
				sw.Restart();
				var score = Analyzer.DebugMap(map);
				if (score is not null)
				{
					score.Name = map.Info.SongName;
				}

				Console.Write(" Score: {0} in {1}ms", score, sw.ElapsedMilliseconds);
				Console.WriteLine();
				return score;
			}).Where(score => score is not null);
		});

		var json = JsonSerializer.Serialize(scores, new JsonSerializerOptions()
		{
			Converters = {
				new TimeSpanConverter()
			},
			WriteIndented = true
		});
		File.WriteAllText("scores.js", "const scores = " + json);
	}
}
