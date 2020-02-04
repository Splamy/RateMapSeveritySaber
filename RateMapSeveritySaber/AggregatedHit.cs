namespace RateMapSeveritySaber
{
	public readonly struct AggregatedHit : IScoredHit
	{
		public float HitDifficulty { get; }
		public float ContinuousDifficulty { get; }

		public AggregatedHit(float hitDifficulty, float continuousDifficulty)
		{
			HitDifficulty = hitDifficulty;
			ContinuousDifficulty = continuousDifficulty;
		}

		public static AggregatedHit operator +(AggregatedHit a, AggregatedHit b)
		{
			return new AggregatedHit(a.HitDifficulty + b.HitDifficulty, a.ContinuousDifficulty + b.ContinuousDifficulty);
		}

		public static AggregatedHit operator -(AggregatedHit a, AggregatedHit b)
		{
			return new AggregatedHit(a.HitDifficulty - b.HitDifficulty, a.ContinuousDifficulty - b.ContinuousDifficulty);
		}

		public static AggregatedHit operator *(AggregatedHit a, float s)
		{
			return new AggregatedHit(a.HitDifficulty * s, a.ContinuousDifficulty * s);
		}

		public static AggregatedHit operator /(AggregatedHit a, float s)
		{
			return new AggregatedHit(a.HitDifficulty / s, a.ContinuousDifficulty / s);
		}
	}
}
