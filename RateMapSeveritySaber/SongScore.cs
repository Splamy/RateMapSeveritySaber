namespace RateMapSeveritySaber
{
	public class SongScore
	{
		public float Average { get; set; }
		public float Max { get; set; }
		public AggregatedHit[] Graph { get; set; }

		public override string ToString() => $"~{Average} ^{Max}";

		public SongScore(float average, float max, AggregatedHit[] graph)
		{
			Average = average;
			Max = max;
			Graph = graph;
		}
	}
}
