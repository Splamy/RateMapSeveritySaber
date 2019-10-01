using DeepSaber;
using System;
using System.Text;
using System.Linq;
using System.Drawing;
using Vector2 = Math2D.FVector2D;
using System.Collections.Generic;

namespace RateMapSeveritySaber
{
	class Analyzer
	{
		public static readonly Encoding Utf8 = new UTF8Encoding(false, false);

		static void Main(string[] args)
		{
			var an = new Analyzer();
			var maps = BSMapIO.Read(@"C:\beatsaber_song");
			foreach (var map in maps)
			{
				Console.Write("Level {0}: ", map.MapInfo._difficultyRank);
				if (map.Data.Notes.Count < 5)
				{
					Console.WriteLine("No enough notes");
					continue;
				}
				an.AnalyzeMap(map);
			}

			Console.ReadLine();
		}

		public void AnalyzeMap(BSMap map)
		{
			var noDots = map.Data.Notes.Where(x => x.Direction != NoteDir.Dot).ToArray();
			var reds = noDots.Where(n => n.Type == NoteColor.Red).ToArray();
			var blues = noDots.Where(n => n.Type == NoteColor.Blue).ToArray();

			var sRed = AnalyzeNotes(map, reds);
			var sBlue = AnalyzeNotes(map, blues);

			DrawImage(sRed, sBlue, reds, blues, $"beautiful_{map.MapInfo._difficultyRank}.png");

			Console.WriteLine("R: {0} B: {1}", sRed.Average(), sBlue.Average());
		}

		public float[] AnalyzeNotes(BSMap map, JsonNote[] notes)
		{
			var scores = new float[notes.Length];
			for (int i = 0; i < notes.Length - 1; i++)
			{
				scores[i] = ScoreDistance(notes[i], notes[i + 1], map);
			}
			return scores;
		}

		public void DrawImage(float[] red, float[] blue, JsonNote[] redJ, JsonNote[] blueJ, string name)
		{
			using var bitmap = new Bitmap((int)Math.Ceiling(redJ.Concat(blueJ).Max(x => x.Time)) + 1, (int)Math.Ceiling(red.Concat(blue).Max()) + 1);
			bitmap.SetPixel(0, 0, Color.Green);
			DrawPixels(bitmap, red, redJ, true);
			DrawPixels(bitmap, blue, blueJ, false);
			bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
			bitmap.Save(name);
		}

		public void DrawPixels(Bitmap bitmap, float[] notes, JsonNote[] notesJ, bool red)
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

		/// Score for note B
		public float ScoreDistance(JsonNote noteA, JsonNote noteB, BSMap map)
		{
			Vector2 noteAStart = new Vector2(noteA.X, noteA.Y) + .5f + DirectionToVelocity(noteA.Direction) * -.5f;
			Vector2 noteAEnd = new Vector2(noteA.X, noteA.Y) + .5f + DirectionToVelocity(noteA.Direction) * .5f;
			Vector2 noteBStart = new Vector2(noteB.X, noteB.Y) + .5f + DirectionToVelocity(noteB.Direction) * -.5f;
			Vector2 noteBEnd = new Vector2(noteB.X, noteB.Y) + .5f + DirectionToVelocity(noteB.Direction) * .5f;

			Vector2 resetVec = noteBStart - noteAEnd;
			Vector2 hitAVec = noteAEnd - noteAStart;
			Vector2 hitBVec = noteBEnd - noteBStart;

			float totalHitDuration = map.BeatTimeToRealTime(noteB.Time - noteA.Time);
			if (totalHitDuration <= 0.001f)
				return 0;

			float scoreParts = 0;

			scoreParts += resetVec.Length;
			scoreParts += (hitAVec.Normalize() * hitBVec.Normalize() + 1) / 2;

			if (resetVec.LengthSQ >= 0.001f)
			{
				scoreParts += (hitAVec.Normalize() * resetVec.Normalize() + 1) / 2;
				scoreParts += (hitBVec.Normalize() * resetVec.Normalize() + 1) / 2;
			}

			float score = scoreParts / totalHitDuration;
			if (float.IsNaN(score) || float.IsInfinity(score))
				Console.WriteLine("Batman");

			return score;
		}

		public Vector2 DirectionToVelocity(NoteDir direction)
		{
			float sqrt2 = MathF.Sqrt(2f) / 2;

			return direction switch
			{
				NoteDir.Up => new Vector2(0f, 1f),
				NoteDir.Down => new Vector2(0f, -1f),
				NoteDir.Left => new Vector2(-1f, 0f),
				NoteDir.Right => new Vector2(1f, 0f),
				NoteDir.UpLeft => new Vector2(sqrt2, -sqrt2),
				NoteDir.UpRight => new Vector2(sqrt2, sqrt2),
				NoteDir.DownLeft => new Vector2(-sqrt2, -sqrt2),
				NoteDir.DownRight => new Vector2(-sqrt2, sqrt2),
				NoteDir.Dot => new Vector2(0f, 0f), // TODO
			};
		}
	}
}