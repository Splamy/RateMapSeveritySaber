using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Math2D;
using static RateMapSeveritySaber.BSMapExtensions;

namespace RateMapSeveritySaber
{
	public static class Analyzer
	{
		public static readonly Encoding Utf8 = new UTF8Encoding(false, false);

		public static Score AnalyzeMap(BSMap map)
		{
			int len = (int)BSMath.Ceiling(map.Data.Notes.Max(x => (float?)x.Time) ?? 0);

			if (len == 0)
			{
				return new Score(avg: 0, max: 0, graph: Array.Empty<float>());
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
			(
				avg: combined.Average(),
				max: combined.Max(),
				graph: combined
			);
		}

		public static float[] ProcessColor(BSMap map, NoteColor color, int len)
		{
			var json = map.Data.Notes.Where(n => n.Type == color).ToArray();
			var hits = json.Select(x => Hit.FromSingle(map, x)).ToArray();
			var clustered = ClusterNotes(hits);
			var scored = AnalyzeNotes(clustered);
			return ConvertToTimed(scored, len);
		}

		public static ScoredHit[] AnalyzeNotes(IList<Hit> notes)
		{
			if (notes.Count == 0)
				return Array.Empty<ScoredHit>();

			var scores = new ScoredHit[notes.Count];
			scores[0] = new ScoredHit(notes[0], 0);
			for (int i = 1; i < notes.Count; i++)
			{
				scores[i] = new ScoredHit(notes[i], ScoreDistance(notes[i - 1], notes[i]));
			}
			return scores;
		}

		public static float[] ConvertToTimed(IEnumerable<ScoredHit> notes, int len)
		{
			float[] timed = new float[len];
			foreach (ScoredHit note in notes)
			{
				int timeIndex = (int)note.Cluster.BeatTime;
				if (timeIndex < 0 || timeIndex >= len)
					continue;
				timed[timeIndex] = Math.Max(timed[timeIndex], note.Score);
			}
			return timed;
		}

		/// <summary>
		/// Score for note B
		/// </summary>
		public static float ScoreDistance(Hit noteA, Hit noteB)
		{
			Vector2 resetVec = noteB.Start - noteA.End;

			var totalHitDuration = noteB.RealTime - noteA.RealTime;
			if (totalHitDuration <= Hit.Threshold)
				return 0;

			float resetSwingDist = resetVec.Length;

			float swing = noteB.Dir.Length + noteB.HitCoefficient;

			float swingRelAB = 0, swingRelAReset = 0, swingRelBReset = 0;
			if (!noteA.IsDot && !noteB.IsDot)
				swingRelAB = Relation(noteA.Dir, noteB.Dir);

			if (resetVec.LengthSQ >= Epsilon)
			{
				if (!noteA.IsDot)
					swingRelAReset = Relation(noteA.Dir, resetVec);
				if (!noteB.IsDot)
					swingRelBReset = Relation(noteB.Dir, resetVec);
			}

			float scoreParts = resetSwingDist + swing + swingRelAB + swingRelAReset + swingRelBReset;
			float timeScaled = InvExpToLin(Math.Min((float)totalHitDuration.TotalSeconds, 1));
			float score = scoreParts / timeScaled;
			if (float.IsNaN(score) || float.IsInfinity(score))
			{
				Console.WriteLine($"Error: Invalid note score for note at BeatTime {noteB.BeatTime}");
				return 0;
			}

			return score;
		}


		/// <summary>
		/// Will combine multiple notes into a big one.
		/// </summary>
		private static List<Hit> ClusterNotes(ICollection<Hit> notes)
		{
			var clusters = new List<Hit>(notes.Count);
			var clusterBuild = new List<Hit>();

			foreach (Hit note in notes)
			{
				if (clusterBuild.Count > 0 && clusterBuild[0].RealTime + Hit.Threshold < note.RealTime)
				{
					clusters.Add(Hit.Cluster(clusterBuild));
					clusterBuild.Clear();
				}

				clusterBuild.Add(note);
			}

			if (clusterBuild.Count > 0)
				clusters.Add(Hit.Cluster(clusterBuild));

			return clusters;
		}
	}
}