using System;

namespace RateMapSeveritySaber.Analyzer;

internal interface IMetricAnalyzer
{
	IMetricResult Analyze(Swing[] swings);
}

public interface IMetricResult
{
	public TimeEntry[] GetGraph();
	public float Units { get; }
}

public record struct TimeEntry(TimeSpan Time, float Value);
