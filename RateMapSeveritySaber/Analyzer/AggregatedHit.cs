using System;

namespace RateMapSeveritySaber.Analyzer;

public readonly record struct AggregatedHit(float HitDifficulty, float ContinuousDifficulty, TimeSpan Time) : IScoredHit
{
	public static AggregatedHit operator +(AggregatedHit a, AggregatedHit b)
	{
		return new AggregatedHit(a.HitDifficulty + b.HitDifficulty, a.ContinuousDifficulty + b.ContinuousDifficulty, a.Time);
	}

	public static AggregatedHit operator -(AggregatedHit a, AggregatedHit b)
	{
		return new AggregatedHit(a.HitDifficulty - b.HitDifficulty, a.ContinuousDifficulty - b.ContinuousDifficulty, a.Time);
	}

	public static AggregatedHit operator *(AggregatedHit a, float s)
	{
		return new AggregatedHit(a.HitDifficulty * s, a.ContinuousDifficulty * s, a.Time);
	}

	public static AggregatedHit operator /(AggregatedHit a, float s)
	{
		return new AggregatedHit(a.HitDifficulty / s, a.ContinuousDifficulty / s, a.Time);
	}
}
