using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace RateMapSeveritySaber
{
	class Program
	{
		static void Main(string[] args)
		{
			//const string path = @"E:\Games\SteamGames\SteamApps\common\Beat Saber\Beat Saber_Data\CustomLevels";
			//foreach (var mapFolder in Directory.EnumerateDirectories(path).Take(300))
			{
				//var mapPath = Path.Combine(path, mapFolder);
				var mapPath = @"E:\Downloads\5e78";
				var sw = Stopwatch.StartNew();
				var maps = BSMapIO.Read(mapPath);
				//Console.WriteLine("Parsing: {0}ms", sw.ElapsedMilliseconds);

				foreach (var map in maps)
				{
					Console.Write("Level {0}: ", map.MapInfo.DifficultyRank);
					sw.Restart();
					var score = Analyzer.AnalyzeMap(map);
					Console.Write(" Score: {0} in {1}ms", score, sw.ElapsedMilliseconds);
					Console.WriteLine();
				}
			}

			Console.ReadLine();
		}

		public static void DrawImage(float[] red, float[] blue, JsonNote[] redJ, JsonNote[] blueJ, string name)
		{
			using var bitmap = new Bitmap((int)MathF.Ceiling(redJ.Concat(blueJ).Max(x => x.Time)) + 1, (int)MathF.Ceiling(red.Concat(blue).Max()) + 1);
			bitmap.SetPixel(0, 0, Color.Green);
			DrawPixels(bitmap, red, redJ, true);
			DrawPixels(bitmap, blue, blueJ, false);
			bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
			bitmap.Save(name);
		}

		public static void DrawPixels(Bitmap bitmap, float[] notes, JsonNote[] notesJ, bool red)
		{
			float[] timed = new float[bitmap.Width];

			for (int i = 0; i < notes.Length; i++)
			{
				timed[(int)notesJ[i].Time] = notes[i];
			}

			var smoo = new List<float>(timed[..4]);

			for (int i = 5; i < timed.Length; i++)
			{
				smoo.RemoveAt(0);
				smoo.Add(timed[i]);
				var val = (int)smoo.Average();
				//bitmap.SetPixel(i, (int)val, red ? Color.Red : Color.Blue);

				for (int j = 0; j < val; j++)
				{
					if (bitmap.GetPixel(i, j).A == 0)
						bitmap.SetPixel(i, j, red ? Color.Red : Color.Blue);
					else
						bitmap.SetPixel(i, j, Color.Purple);
				}
			}
		}
	}
}
