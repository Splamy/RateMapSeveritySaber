using System;

namespace RateMapSeveritySaber
{
	public struct ScoredHit
	{
		public Hit Cluster { get; set; }
		public float Score { get; set; }

		public ScoredHit(Hit cluster, float score)
		{
			Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
			Score = score;
		}
	}
}