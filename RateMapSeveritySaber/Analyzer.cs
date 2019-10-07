using System;
using System.Linq;
using System.Text;
using Vector2 = Math2D.FVector2D;

namespace RateMapSeveritySaber
{
	public static class Analyzer
	{
		public static readonly Encoding Utf8 = new UTF8Encoding(false, false);

		public static Score AnalyzeMap(BSMap map)
		{
			int len = (int)MathF.Ceiling(map.Data.Notes.Max(x => (float?)x.Time) ?? 0);

			if (len == 0)
			{
				return new Score { Avg = 0, Max = 0, Graph = Array.Empty<float>() };
			}

			var jsonRed = map.Data.Notes.Where(n => n.Type == NoteColor.Red).ToArray();
			var jsonBlue = map.Data.Notes.Where(n => n.Type == NoteColor.Blue).ToArray();

			var scoredRed = AnalyzeNotes(map, jsonRed);
			var scoredBlue = AnalyzeNotes(map, jsonBlue);

			var timedRed = ConvertToTimed(scoredRed, jsonRed, len);
			var timedBlue = ConvertToTimed(scoredBlue, jsonBlue, len);

			var combined = new float[len];
			for (int i = 0; i < len; i++)
			{
				int cnt = timedRed[i] > 0 && timedBlue[i] > 0 ? 2 : 1;
				combined[i] = (timedRed[i] + timedBlue[i]) / cnt;
			}

			return new Score
			{
				Graph = combined,
				Max = combined.Max(),
				Avg = combined.Average(),
			};
		}

		public static float[] AnalyzeNotes(BSMap map, JsonNote[] notes)
		{
			var scores = new float[notes.Length];
			for (int i = 0; i < notes.Length - 1; i++)
			{
				scores[i] = ScoreDistance(notes[i], notes[i + 1], map);
			}
			return scores;
		}

		public static float[] ConvertToTimed(float[] notes, JsonNote[] notesJ, int len)
		{
			float[] timed = new float[len];
			for (int i = 0; i < notes.Length; i++)
			{
				int timeIndex = (int)notesJ[i].Time;
				if (timeIndex < 0 || timeIndex >= len)
					continue;
				timed[timeIndex] = Math.Max(timed[timeIndex], notes[i]);
			}
			return timed;
		}

		/// Score for note B
		public static float ScoreDistance(JsonNote noteA, JsonNote noteB, BSMap map)
		{
			Vector2 noteAStart = noteA.Position() + .5f + noteA.Rotation() * -.5f;
			Vector2 noteAEnd = noteA.Position() + .5f + noteA.Rotation() * .5f;
			Vector2 noteBStart = noteB.Position() + .5f + noteB.Rotation() * -.5f;
			Vector2 noteBEnd = noteB.Position() + .5f + noteB.Rotation() * .5f;

			Vector2 resetVec = noteBStart - noteAEnd;
			Vector2 hitAVec = noteAEnd - noteAStart;
			Vector2 hitBVec = noteBEnd - noteBStart;

			float totalHitDuration = map.BeatTimeToRealTime(noteB.Time - noteA.Time);
			if (totalHitDuration <= 0.001f)
				return 0;

			float swingDist, swingRelAB = 0, swingRelAReset = 0, swinRelBReset = 0;

			swingDist = resetVec.Length;
			if (hitAVec != Vector2.Zero && hitBVec != Vector2.Zero)
				swingRelAB = Relation(hitAVec, hitBVec);

			if (resetVec.LengthSQ >= 0.001f)
			{
				if (hitAVec != Vector2.Zero)
					swingRelAReset = Relation(hitAVec, resetVec);
				if (hitBVec != Vector2.Zero)
					swinRelBReset = Relation(hitBVec, resetVec);
			}

			float scoreParts = swingDist + swingRelAB + swingRelAReset + swinRelBReset;
			float score = scoreParts / totalHitDuration;
			if (float.IsNaN(score) || float.IsInfinity(score))
				return 0; // TODO some kind of warning

			return score;
		}

		private static float Relation(Vector2 a, Vector2 b) => 1 - (a.Normalized * b.Normalized + 1) / 2;
	}

	internal static class BSMapExtensions
	{
		public static float RealTimeToBeatTime(this BSMap map, float time) => (time / 60) * map.Info._beatsPerMinute;
		public static float BeatTimeToRealTime(this BSMap map, float time) => (time / map.Info._beatsPerMinute) * 60;

		// https://github.com/Kylemc1413/MappingExtensions#precision-note-placement
		public static Vector2 Position(this JsonNote note)
			=> new Vector2(NoteValueToPos(note.X), NoteValueToPos(note.Y));

		private static float NoteValueToPos(int num)
			=> Math.Abs(num) >= 1000 ? (num - Math.Sign(num) * 1000) / 1000f : num;

		private static readonly float sqrt2 = MathF.Sqrt(2f) / 2;

		// https://github.com/Kylemc1413/MappingExtensions#360-degree-note-rotation
		public static Vector2 Rotation(this JsonNote note)
		{
			return note.Direction switch
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
				var num when (int)num >= 1000 && (int)num <= 1360 => NoteRotationToVector((int)num),
				_ => Vector2.Zero // Weird other stuff
			};
		}

		private static Vector2 NoteRotationToVector(int num)
			=> new Vector2(0, -1).Rotate((num - 1000) / 180f * MathF.PI);
	}

	public class Score
	{
		public float[] Graph { get; set; }
		public float Avg { get; set; }
		public float Max { get; set; }

		public override string ToString() => $"~{Avg} ^{Max}";
	}
}