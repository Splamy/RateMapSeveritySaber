using Math2D;
using RateMapSeveritySaber.Parser;
using RateMapSeveritySaber.Parser.Beatmaps;
using System;
using System.Collections.Generic;
using System.Linq;
using static RateMapSeveritySaber.BsMapExtensions;

namespace RateMapSeveritySaber.Analyzer;

public static class RamsesAnalyzer
{
	public static SongScore AnalyzeMap(BsDifficulty map)
	{
		var notes = map.Beatmap.Notes.ToList();
		if (notes.Count == 0 || notes.All(x => x.Beat == 0))
		{
			return SongScore.Empty;
		}

		int len = (int)BSMath.Ceiling(notes.Max(x => x.Beat));
		if (len == 0)
		{
			return SongScore.Empty;
		}

		var scoredRed = ProcessColor(map, notes, NoteColor.Red);
		var scoredBlue = ProcessColor(map, notes, NoteColor.Blue);

		var timedRed = ConvertToFixedTimeBlocks(scoredRed, len);
		var timedBlue = ConvertToFixedTimeBlocks(scoredBlue, len);

		var combined = new AggregatedHit[len];
		for (int i = 0; i < len; i++)
		{
			int cnt = timedRed[i].HitDifficulty > 0 && timedBlue[i].HitDifficulty > 0 ? 2 : 1;
			combined[i] = (timedRed[i] + timedBlue[i]) / cnt;
		}

		var totalDifficulty = combined.Select(h => h.TotalDifficulty()).ToList();

		return new SongScore
		(
			totalDifficulty.Average(),
			totalDifficulty.Max(),
			combined
		);
	}

	public static DebugSongScore? DebugMap(BsDifficulty map)
	{
		var notes = map.Beatmap.Notes.ToList();
		if (notes.Count == 0 || notes.All(x => x.Beat == 0))
		{
			return null;
		}

		int len = (int)BSMath.Ceiling(notes.Max(x => x.Beat));
		if (len == 0)
		{
			return null;
		}

		var timedRed = ProcessColor(map, notes, NoteColor.Red);
		var timedBlue = ProcessColor(map, notes, NoteColor.Blue);

		var debugRed = DebugHits(timedRed);
		var debugBlue = DebugHits(timedBlue);

		return new DebugSongScore
		{
			Name = map.Info.SongName, DifficultyName = map.Difficulty.DifficultyName, DataRed = debugRed, DataBlue = debugBlue,
		};

		DebugHitScore[] DebugHits(IEnumerable<ScoredClusterHit> arr) => arr.Select(x =>
		{
			var d = new DebugHitScore();
			d.Set(x);
			return d;
		}).ToArray();
	}

	public static ScoredClusterHit[] ProcessColor(BsDifficulty map, IReadOnlyList<IBsNote> notes, NoteColor color)
	{
		var json = notes.Where(n => n.Type == color).ToArray();
		var hits = json.Select(x => Hit.FromSingle(map, x)).ToArray();
		var clustered = ClusterNotes(hits);
		var scored = AnalyzeNotes(clustered);
		return scored;
	}

	public static ScoredClusterHit[] AnalyzeNotes(IList<Hit> notes)
	{
		if (notes.Count == 0)
			return [];

		var scores = new ScoredClusterHit[notes.Count];
		scores[0] = new ScoredClusterHit(notes[0], 0, 0);
		int lastGroup = 0;

		for (int i = 1; i < notes.Count; i++)
		{
			float hitDifficulty = ScoreDistance(notes[i - 1], notes[i]);

			float timeToPreviousHit = (float)(notes[i].RealTime - notes[i - 1].RealTime).TotalSeconds;
			float continuousDifficulty = scores[i - 1].ContinuousDifficulty;
			int currentGroup = (int)(notes[i].RealTime.TotalSeconds / Constants.ContinuousGroupSizeSeconds);
			if (currentGroup != lastGroup)
			{
				if (timeToPreviousHit > Constants.ContinuousGroupSizeSeconds)
				{
					continuousDifficulty =
						System.Math.Max(continuousDifficulty - ((currentGroup - lastGroup) * Constants.ContinuousDecay), 0f);
				}
				else
				{
					continuousDifficulty = System.Math.Min(continuousDifficulty + Constants.ContinuousBuildup, 100f);
				}

				lastGroup = currentGroup;
			}

			scores[i] = new ScoredClusterHit(notes[i], hitDifficulty, continuousDifficulty);
		}

		return scores;
	}

	public static AggregatedHit[] ConvertToFixedTimeBlocks(IEnumerable<ScoredClusterHit> notes, int len)
	{
		var timed = new AggregatedHit[len];
		foreach (ScoredClusterHit note in notes)
		{
			int timeIndex = (int)note.Cluster.BeatTime;
			if (timeIndex < 0 || timeIndex >= len)
				continue;
			timed[timeIndex] = new AggregatedHit(System.Math.Max(timed[timeIndex].HitDifficulty, note.HitDifficulty),
				System.Math.Max(timed[timeIndex].ContinuousDifficulty, note.ContinuousDifficulty), note.Cluster.RealTime);
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
		float timeScaled = InvExpToLin(System.Math.Min((float)totalHitDuration.TotalSeconds, 1));
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
