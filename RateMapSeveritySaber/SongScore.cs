namespace RateMapSeveritySaber
{
	public class SongScore
	{
		public float Avg { get; set; }
		public float Max { get; set; }
		public float[] Graph { get; set; }

		public override string ToString() => $"~{Avg} ^{Max}";

		public SongScore(float avg, float max, float[] graph)
		{
			Avg = avg;
			Max = max;
			Graph = graph;
		}
	}
}