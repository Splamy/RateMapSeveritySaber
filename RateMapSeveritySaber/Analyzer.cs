using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Math2D;

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

			var timedRed = ProcessColor(map, NoteColor.Red, len);
			var timedBlue = ProcessColor(map, NoteColor.Blue, len);

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

		public static float[] ProcessColor(BSMap map, NoteColor color, int len)
		{
			var json = map.Data.Notes.Where(n => n.Type == color).ToArray();
			var clustered = ClusterNotes(map, json);
			var scored = AnalyzeNotes(clustered);
			return ConvertToTimed(scored, len);
		}

		public static ScoredCluster[] AnalyzeNotes(IList<Cluster> notes)
		{
			var scores = new ScoredCluster[notes.Count];
			for (int i = 0; i < notes.Count - 1; i++)
			{
				scores[i] = new ScoredCluster(notes[i + 1], ScoreDistance(notes[i], notes[i + 1]));
			}
			return scores;
		}

		public static float[] ConvertToTimed(IList<ScoredCluster> notes, int len)
		{
			float[] timed = new float[len];
			for (int i = 0; i < notes.Count; i++)
			{
				int timeIndex = (int)notes[i].Cluster.BeatTime;
				if (timeIndex < 0 || timeIndex >= len)
					continue;
				timed[timeIndex] = Math.Max(timed[timeIndex], notes[i].Score);
			}
			return timed;
		}

		/// <summary>
		/// Score for note B
		/// </summary>
		public static float ScoreDistance(Cluster noteA, Cluster noteB)
		{
			Vector2 resetVec = noteB.Start - noteA.End;

			var totalHitDuration = noteB.RealTime - noteA.RealTime;
			if (totalHitDuration.TotalSeconds <= 0.001f)
				return 0;

			float swingDist, swingRelAB = 0, swingRelAReset = 0, swinRelBReset = 0;

			swingDist = resetVec.Length;
			if (noteA.Hit != Vector2.Zero && noteB.Hit != Vector2.Zero)
				swingRelAB = Relation(noteA.Hit, noteB.Hit);

			if (resetVec.LengthSQ >= 0.001f)
			{
				if (noteA.Hit != Vector2.Zero)
					swingRelAReset = Relation(noteA.Hit, resetVec);
				if (noteB.Hit != Vector2.Zero)
					swinRelBReset = Relation(noteB.Hit, resetVec);
			}

			float scoreParts = swingDist + swingRelAB + swingRelAReset + swinRelBReset;
			float timeScaled = ExpToLin((float)totalHitDuration.TotalSeconds);
			float score = scoreParts / timeScaled;
			if (float.IsNaN(score) || float.IsInfinity(score))
				return 0; // TODO some kind of warning

			return score;
		}

		private static float Relation(Vector2 a, Vector2 b) => 1 - (a.Normalized * b.Normalized + 1) / 2;

		/// <summary>
		/// scales the values between [0,1] linearly to log_2.
		/// For example:
		/// 1 => 1/1,
		/// 0.5 => 1/2,
		/// 0.25 => 1/3,
		/// 0.125 => 1/4
		/// </summary>
		private static float ExpToLin(float value) =>
			value >= 1
			? value
			: 1 / (MathF.Log(1 / value, 2) + 1);

		/// <summary>
		/// Will combine multiple notes into a big one.
		/// </summary>
		private static List<Cluster> ClusterNotes(BSMap map, IList<JsonNote> notes)
		{
			var clusters = new List<Cluster>(notes.Count);
			var clusterBuild = new List<JsonNote>();

			for (int i = 0; i < notes.Count; i++)
			{
				if (clusterBuild.Count > 0 &&
					map.BeatTimeToRealTime(clusterBuild[0].Time) + Cluster.Treshold < map.BeatTimeToRealTime(notes[i].Time))
				{
					clusters.Add(Cluster.FromList(map, clusterBuild));
					clusterBuild.Clear();
				}

				clusterBuild.Add(notes[i]);
			}

			if (clusterBuild.Count > 0)
				clusters.Add(Cluster.FromList(map, clusterBuild));

			return clusters;
		}
	}

	/// <summary>
	/// Note: A cluster can consist of one or more notes.
	/// </summary>
	public class Cluster
	{
		public Vector2 Start { get; }
		public Vector2 End { get; }
		public Vector2 Hit { get; }
		public float HitCofactor { get; set; }
		public float BeatTime { get; }
		public TimeSpan RealTime { get; }

		public static readonly TimeSpan Treshold = TimeSpan.FromMilliseconds(5);

		public Cluster(Vector2 start, Vector2 end, float time, TimeSpan realTime)
		{
			Start = start;
			End = end;
			Hit = end - start;
			BeatTime = time;
			RealTime = realTime;
		}

		public static Cluster FromSingle(BSMap map, JsonNote note)
		{
			return new Cluster(
				note.Position() + .5f + note.Rotation() * -.5f,
				note.Position() + .5f + note.Rotation() * .5f,
				note.Time,
				map.BeatTimeToRealTime(note.Time)
			);
		}

		public static Cluster FromList(BSMap map, IList<JsonNote> notes)
		{
			if (notes.Count == 0)
				throw new InvalidOperationException();
			if (notes.Count == 1)
				return FromSingle(map, notes[0]);



			return null;
		}
	}

	public struct ScoredCluster
	{
		public Cluster Cluster { get; set; }
		public float Score { get; set; }

		public ScoredCluster(Cluster cluster, float score)
		{
			Cluster = cluster;
			Score = score;
		}
	}

	internal static class BSMapExtensions
	{
		public static float RealTimeToBeatTime(this BSMap map, TimeSpan time) => (float)(time.TotalMinutes * map.Info.BPM);
		public static TimeSpan BeatTimeToRealTime(this BSMap map, float beat) => TimeSpan.FromMinutes(beat / map.Info.BPM);

		// https://github.com/Kylemc1413/MappingExtensions#precision-note-placement
		public static Vector2 Position(this JsonNote note)
			=> new Vector2(ExtendedPositionToRealPosition(note.X), ExtendedPositionToRealPosition(note.Y));

		private static float ExtendedPositionToRealPosition(int num)
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