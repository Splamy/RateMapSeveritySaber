using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Win32;

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

				foreach (var map in maps)
				{
					Console.Write("Level {0}: ", map.MapInfo.DifficultyRank);
					sw.Restart();
					var score = Analyzer.AnalyzeMap(map);
					DrawImage(score, songName, map.MapInfo.Difficulty + ".png");
					Console.Write(" Score: {0} in {1}ms", score, sw.ElapsedMilliseconds);
					Console.WriteLine();
				}
			}
		}

		public static void DrawImage(SongScore songScore, string dir, string name)
		{
			using var bitmap = new Bitmap(songScore.Graph.Length, (int)MathF.Ceiling(songScore.Max) + 1);
			DrawGraph(bitmap, songScore.Graph.Select(h => h.HitDifficulty).ToList(), Color.Orange, 0);
			DrawGraph(bitmap, songScore.Graph.Select(h => h.ContinuousDifficulty).ToList(), Color.Brown, 0);
			bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
			Directory.CreateDirectory(dir);
			bitmap.Save(Path.Combine(dir, name));
		}

		public static void DrawGraph(Bitmap bitmap, List<float> data, Color color, int offset)
		{
			for (int x = 0; x < data.Count; x++)
			{
				float y = 0;
				while (y < data[x])
				{
					bitmap.SetPixel(x, (int)y + offset, color);
					y++;
				}
			}
		}

		public static void DrawDebugPixels(Bitmap bitmap, AggregatedHit[] timed)
		{
			for (int x = 0; x < timed.Length; x++)
			{
				float y = 0;
				while (y < timed[x].HitDifficulty)
				{
					bitmap.SetPixel(x, (int)y, Color.Orange);
					y++;
				}

				while (y < timed[x].HitDifficulty + timed[x].ContinuousDifficulty)
				{
					bitmap.SetPixel(x, (int)y, Color.Brown);
					y++;
				}
			}
		}

		public static void DrawPixels(Bitmap bitmap, float[] timed)
		{
			var smoo = new List<float>(timed[..4]);

			for (int i = 5; i < timed.Length; i++)
			{
				smoo.RemoveAt(0);
				smoo.Add(timed[i]);
				var val = (int)smoo.Average();
				//bitmap.SetPixel(i, (int)val, red ? Color.Red : Color.Blue);

				for (int j = 0; j < val; j++)
				{
					bitmap.SetPixel(i, j, Color.Orange);
				}
			}
		}
	}
}
