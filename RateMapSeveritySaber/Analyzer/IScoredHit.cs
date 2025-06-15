using System;

namespace RateMapSeveritySaber.Analyzer;

public interface IScoredHit
{
	TimeSpan Time { get; }
	float HitDifficulty { get; }
	float ContinuousDifficulty { get; }
}

public static class IScoredHitExtensions
{
	public static float TotalDifficulty(this IScoredHit self)
	{
		return self.HitDifficulty + self.ContinuousDifficulty;
	}
}
