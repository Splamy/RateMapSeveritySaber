using System;

namespace RateMapSeveritySaber
{
	public class SongScore
	{
		public float Average { get; }
		public float Max { get; }
		public AggregatedHit[] Graph { get; }

		public override string ToString() => $"~{Average} ^{Max}";

		public SongScore(float average, float max, AggregatedHit[] graph)
		{
			Average = average;
			Max = max;
			Graph = graph;
		}
	}

	public class DebugSongScore
	{
		public string? Name { get; set; }
		public string? DifficultyName { get; set; }
		public DebugHitScore[]? DataRed { get; set; }
		public DebugHitScore[]? DataBlue { get; set; }

		public DebugSongScore()
		{

		}
	}

	public class DebugHitScore : IScoredHit
	{
		public float HitDifficulty { get; set; }
		public float ContinuousDifficulty { get; set; }
		public TimeSpan Time { get; set; }

		public void Set(IScoredHit hit)
		{
			HitDifficulty = hit.HitDifficulty;
			ContinuousDifficulty = hit.ContinuousDifficulty;
			Time = hit.Time;
		}
	}
}
