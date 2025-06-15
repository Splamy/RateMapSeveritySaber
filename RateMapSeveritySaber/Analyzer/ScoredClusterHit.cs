using System;

namespace RateMapSeveritySaber.Analyzer;

public readonly record struct ScoredClusterHit(Hit Cluster, float HitDifficulty, float ContinuousDifficulty) : IScoredHit
{
	public Hit Cluster { get; } = Cluster;
	public float HitDifficulty { get; } = HitDifficulty;
	public float ContinuousDifficulty { get; } = ContinuousDifficulty;

	public TimeSpan Time => Cluster.RealTime;
}
