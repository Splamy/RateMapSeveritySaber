namespace RateMapSeveritySaber
{
	public interface IScoredHit
	{
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
}
