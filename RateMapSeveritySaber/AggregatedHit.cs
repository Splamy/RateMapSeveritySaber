using System;

namespace RateMapSeveritySaber
{
	public readonly struct AggregatedHit : IScoredHit
	{
		public TimeSpan Time { get; }
		public float HitDifficulty { get; }
		public float ContinuousDifficulty { get; }

		public AggregatedHit(float hitDifficulty, float continuousDifficulty, TimeSpan time)
		{
			HitDifficulty = hitDifficulty;
			ContinuousDifficulty = continuousDifficulty;
			Time = time;
		}

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
}
