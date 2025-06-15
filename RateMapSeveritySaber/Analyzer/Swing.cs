using Math2D;
using System;

namespace RateMapSeveritySaber.Analyzer;

public record Swing
{
	public Vector2 StartPosition { get; init; }
	public Vector2 InDirection { get; init; }
	public Vector2 EndPosition { get; init; }
	public Vector2 OutDirection { get; init; }
	public TimeSpan Start { get; init; }
	public TimeSpan End { get; init; }
}

// - Preprocessor: Alles in swings konvertieren
// - Swing = halbe zwit vom vorherigen block + halbe zeit nach dem block
// - Wenn Reset: ⅓ vorheriger block, ⅓ reset, ⅓ nächster block
