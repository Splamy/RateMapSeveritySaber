using Math2D;
using System;

namespace RateMapSeveritySaber
{
	public static class BSMapExtensions
	{
		public static float RealTimeToBeatTime(this BSMap map, TimeSpan time) => (float)(time.TotalMinutes * map.Info.BPM);
		public static TimeSpan BeatTimeToRealTime(this BSMap map, float beat) => TimeSpan.FromMinutes(beat / map.Info.BPM);

		// https://github.com/Kylemc1413/MappingExtensions#precision-note-placement
		public static Vector2 Position(this JsonNote note)
			=> new Vector2(ExtendedPositionToRealPosition(note.X), ExtendedPositionToRealPosition(note.Y));

		private static float ExtendedPositionToRealPosition(int num)
			=> Math.Abs(num) >= 1000 ? (num - Math.Sign(num) * 1000) / 1000f : num;

		private static readonly float Sqrt2 = BSMath.Sqrt(2f) / 2;

		// https://github.com/Kylemc1413/MappingExtensions#360-degree-note-rotation
		public static Vector2 Rotation(this JsonNote note)
		{
			return note.Direction switch
			{
				NoteDir.Up => new Vector2(0f, 1f),
				NoteDir.Down => new Vector2(0f, -1f),
				NoteDir.Left => new Vector2(-1f, 0f),
				NoteDir.Right => new Vector2(1f, 0f),
				NoteDir.UpLeft => new Vector2(Sqrt2, -Sqrt2),
				NoteDir.UpRight => new Vector2(Sqrt2, Sqrt2),
				NoteDir.DownLeft => new Vector2(-Sqrt2, -Sqrt2),
				NoteDir.DownRight => new Vector2(-Sqrt2, Sqrt2),
				NoteDir.Dot => new Vector2(0f, 0f), // TODO
				var num when (int)num >= 1000 && (int)num <= 1360 => NoteRotationToVector((int)num),
				_ => Vector2.Zero // Weird other stuff
			};
		}

		private static Vector2 NoteRotationToVector(int num)
			=> new Vector2(0, -1).Rotate((num - 1000) / 180f * BSMath.PI);

		public static float Relation(Vector2 a, Vector2 b) => 1 - (a.Normalized * b.Normalized + 1) / 2;

		/// <summary>
		/// scales the values between [0,1] to 1/log₂.
		/// For example:
		/// 1 => 1/1,
		/// 0.5 => 1/2,
		/// 0.25 => 1/3,
		/// 0.125 => 1/4
		/// </summary>
		public static float InvExpToLin(float value) =>
			value >= 1
			? value
			: 1 / (BSMath.Log(1 / value, 2) + 1);

		/// <summary>
		/// scales the values between [0,∞] to log₂.
		/// For example:
		/// 1 => 1,
		/// 2 => 2,
		/// 4 => 3,
		/// 8 => 4,
		/// </summary>
		public static float ExpToLin(float value) =>
			value <= 1
			? value
			: BSMath.Log(value, 2);

		public const float Epsilon = 0.0001f;
	}
}