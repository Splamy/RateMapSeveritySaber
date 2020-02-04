using System;

namespace RateMapSeveritySaber
{
	public struct ScoredHit
	{
		public Hit Cluster { get; set; }
		public float HitDifficulty { get; set; }
		public float ContinuousDifficulty { get; set; }

		public ScoredHit(Hit cluster, float hitDifficulty, float continuousDifficulty)
		{
			Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
			HitDifficulty = hitDifficulty;
			ContinuousDifficulty = continuousDifficulty;
		}
	}
}