using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace RateMapSeveritySaber
{
	public class Program
	{
		public static void Main(string[] args)
		{
			//const string path = @"E:\Games\SteamGames\SteamApps\common\Beat Saber\Beat Saber_Data\CustomLevels";
			//foreach (var mapFolder in Directory.EnumerateDirectories(path).Take(300))
			{
				//var mapPath = Path.Combine(path, mapFolder);
				var mapPath = @"C:\Users\Splamy\Downloads\6a38 (MEGALOVANIA - puds).zip";
				var sw = Stopwatch.StartNew();
				var maps = BSMapIO.ReadZip(mapPath);
				//Console.WriteLine("Parsing: {0}ms", sw.ElapsedMilliseconds);

				foreach (var map in maps)
				{
					Console.Write("Level {0}: ", map.MapInfo.DifficultyRank);
					sw.Restart();
					var score = Analyzer.AnalyzeMap(map);
					DrawImage(score, map.MapInfo.Difficulty + ".png");
					Console.Write(" Score: {0} in {1}ms", score, sw.ElapsedMilliseconds);
					Console.WriteLine();
				}
			}
		}

		public static void DrawImage(Score score, string name)
		{
			using var bitmap = new Bitmap(score.Graph.Length, (int)MathF.Ceiling(score.Graph.Max()) + 1);
			bitmap.SetPixel(0, 0, Color.Green);
			DrawPixels(bitmap, score.Graph);
			bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
			bitmap.Save(name);
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
