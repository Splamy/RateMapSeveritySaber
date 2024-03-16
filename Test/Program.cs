using Microsoft.Win32;
using RateMapSeveritySaber.Parser;
using RateMapSeveritySaber.Parser.Abstract;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RateMapSeveritySaber;

public class Program
{
	public static void Main(string[] args)
	{
		var beatsaberPath = (string?)Registry.GetValue(
			@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 620980",
			"InstallLocation", null);
		if (beatsaberPath == null)
		{
			Console.WriteLine("Beatsaber not found :(");
			return;
		}

		string songFolder = Path.Combine(beatsaberPath, "Beat Saber_Data", "CustomLevels");
		string[] songNames = {
			//"1ca11 (Odo - CoolingCloset)",
			//"1d1dc (RISE - RateGyro & FakePope)" /*Paul*/,
			//"1949e (Viyella's Scream - Timbo)",
			//"2789 quo vadis"
			"Laur-FairyinStrasbourg"
		};

		var scores = songNames
			.SelectMany(songName =>
			{
				var mapPath = Path.Combine(songFolder, songName);
				if (!Directory.Exists(mapPath))
				{
					Console.WriteLine($"Map {songName} ({mapPath}) doesn't exist!");
					return Enumerable.Empty<BSMap>();
				}

				Console.WriteLine($"{songName}:");

				return BSMapIO.Read(mapPath);
			})
			.Select(map =>
			{
				Console.Write("Level {0}: ", map.MapInfo.DifficultyRank);
				var sw = Stopwatch.StartNew();
				var score = Analyzer.DebugMap(map);
				if (score is not null)
				{
					score.Name = map.Info.SongName;
				}

				Console.Write(" Score: {0} in {1}ms", score, sw.ElapsedMilliseconds);
				Console.WriteLine();
				return score;
			})
			.Where(score => score is not null);

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
