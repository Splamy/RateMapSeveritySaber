using System;

namespace RateMapSeveritySaber
{
	public record SongScore(float Average, float Max, AggregatedHit[] Graph)
	{
		public static readonly SongScore Empty = new(0, 0, []);

		public override string ToString() => $"~{Average} ^{Max}";
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
