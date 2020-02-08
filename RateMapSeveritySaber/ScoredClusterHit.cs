using System;

namespace RateMapSeveritySaber
{
	public readonly struct ScoredClusterHit : IScoredHit
	{
		public Hit Cluster { get; }
		public float HitDifficulty { get; }
		public float ContinuousDifficulty { get; }
		public TimeSpan Time => Cluster.RealTime;

		public ScoredClusterHit(Hit cluster, float hitDifficulty, float continuousDifficulty)
		{
			Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
			HitDifficulty = hitDifficulty;
			ContinuousDifficulty = continuousDifficulty;
		}
	}
}
