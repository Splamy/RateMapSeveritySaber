using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace RateMapSeveritySaber
{
	public class Program
	{
		public static void Main(string[] args)
		{
			string beatsaberPath = (string)Registry.GetValue(
				@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 620980",
				"InstallLocation", null);
			if (beatsaberPath == null)
			{
				Console.WriteLine("Beatsaber not found :(");
				return;
			}

			string songFolder = Path.Combine(beatsaberPath, "Beat Saber_Data", "CustomLevels");
			string[] songNames = { "4502 Reol - Monster by Saut", "e55 Cycle Hit", "2898 Real Or Fake", "2789 quo vadis"};

			foreach (var songName in songNames)
			{
				var mapPath = Path.Combine(songFolder, songName);
				if (!Directory.Exists(mapPath))
				{
					Console.WriteLine($"Map {songName} ({mapPath}) doesn't exist!");
					continue;
				}

				Console.WriteLine($"{songName}:");

				var sw = Stopwatch.StartNew();
				var maps = BSMapIO.Read(mapPath);
				//Console.WriteLine("Parsing: {0}ms", sw.ElapsedMilliseconds);

				var scores = maps.Select(map =>
				{
					Console.Write("Level {0}: ", map.MapInfo.DifficultyRank);
					sw.Restart();
					var score = Analyzer.AnalyzeMap(map);
					JsonConvert.SerializeObject(score.Graph);
					Console.Write(" Score: {0} in {1}ms", score, sw.ElapsedMilliseconds);
					Console.WriteLine();
					return score;
				});
				File.WriteAllText("scores.json", JsonConvert.SerializeObject(scores));
			}
		}
	}
}
