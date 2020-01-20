using System;
using System.Collections.Generic;
using System.Linq;
using Math2D;
using static RateMapSeveritySaber.BSMapExtensions;

namespace RateMapSeveritySaber
{
	/// <summary>
	/// Note: A hit can consist of one or more notes.
	/// </summary>
	public class Hit
	{
		public Vector2 Start { get; }
		public Vector2 End { get; }
		public Vector2 Dir => End - Start;
		public float HitCoefficient { get; set; }
		public float BeatTime { get; }
		public TimeSpan RealTime { get; }
		public bool IsDot => Dir.LengthSQ < Epsilon;
		public List<JsonNote> Group { get; }

		public static readonly TimeSpan Treshold = TimeSpan.FromMilliseconds(5);

		public Hit(Vector2 start, Vector2 end, float time, TimeSpan realTime, List<JsonNote> group)
		{
			Start = start;
			End = end;
			BeatTime = time;
			RealTime = realTime;
			Group = group;
		}

		public static Hit FromSingle(BSMap map, JsonNote note)
		{
			return new Hit(
				note.Position() + .5f + note.Rotation() * -.5f,
				note.Position() + .5f + note.Rotation() * .5f,
				note.Time,
				map.BeatTimeToRealTime(note.Time),
				new List<JsonNote> { note }
			);
		}

		public static Hit Cluster(IList<Hit> hits)
		{
			if (hits.Count == 0)
				throw new InvalidOperationException();
			if (hits.Count == 1)
				return hits[0];

			var (start, end) = GetMaxDistNotes(hits);

			var mainHit = end.End - start.Start;

			// Get the average of all block hit direction to determine the direction
			// of the overall 'main hit'.
			var avgDir = hits
				.Where(x => !x.IsDot)
				.Select(x => x.Dir.Normalized)
				.Aggregate(Vector2.Zero, (a, b) => a + b);

			// Checking if the average of all block directions go in the same direction as 
			// our main hit vector.
			// If not just swap the direction.
			var angle = Vector2.GetAngle(mainHit, avgDir);
			if (angle > BSMath.PI / 2 && angle < BSMath.PI / 2 * 3)
			{
				mainHit = -mainHit;
				var tmp = end;
				end = start;
				start = tmp;
			}

			// Calculating the coefficient for the swing difficulty
			// We consider 2 things:
			// - The distance of the block from the main swing in log₂(dist), so [0,∞]
			// - The rotation difference to the main swing in [0,1]

			float coefficient = 0;

			foreach (var hit in hits)
			{
				// Distance
				var dist = DistanceToLine(start.Start, end.End, hit.Start);
				var distCoeff = ExpToLin(dist);
				// Rotation
				var rot = Relation(mainHit, hit.Dir);
				var rotCoeff = rot;

				var sumCoeff = distCoeff + rotCoeff;

				coefficient += sumCoeff;
			}

			return new Hit(
				start.Start,
				end.End,
				start.BeatTime,
				start.RealTime,
				hits.SelectMany(x => x.Group).ToList())
			{
				HitCoefficient = coefficient,
			};
		}

		private static (Hit, Hit) GetMaxDistNotes(IList<Hit> hits)
		{
			(Hit a, Hit b) maxTup = (hits[0], hits[1]);
			float maxDist = maxTup.a.Start.Distance(maxTup.b.End);
			foreach (var noteA in hits)
				foreach (var noteB in hits)
				{
					if (noteA == noteB)
						continue;
					var checkDist = noteA.Start.Distance(noteB.End);
					if (checkDist > maxDist)
					{
						maxTup = (noteA, noteB);
						maxDist = checkDist;
					}
				}
			return maxTup;
		}

		private static float DistanceToLine(Vector2 start, Vector2 end, Vector2 point)
		{
			var dir = end - start;
			var sp = point - start;
			if (dir == Vector2.Zero)
				return sp.Length;
			if (sp == Vector2.Zero)
				return 0;
			var a = Vector2.GetAngle(dir, sp);
			return dir.Length * BSMath.Sin(a);
		}
	}
}