//#define SYSTEM_DRAWING_EXTENSION

using System;
#if SYSTEM_DRAWING_EXTENSION
using System.Drawing;
#endif

namespace Math2D
{
	using SkalTyp = System.Single;
	using static Math2D.BSMath;

#pragma warning disable CS0660
#pragma warning disable CS0661
	public readonly struct Vector2
#pragma warning restore CS0660
#pragma warning restore CS0661
	{
		private const SkalTyp HALF_PI = PI / 2;

		public readonly SkalTyp X { get; }
		public readonly SkalTyp Y { get; }

#if SYSTEM_DRAWING_EXTENSION
		public readonly PointF p
		{
			get
			{
				return new PointF((SkalTyp)X, (SkalTyp)Y);
			}
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}
#endif

		public static readonly Vector2 Zero = new Vector2(0);

		public Vector2(SkalTyp val) : this(val, val) { }

		public Vector2(SkalTyp x, SkalTyp y)
		{
			this.X = x;
			this.Y = y;
		}

		public static Vector2 FromRotation(float rot) => new Vector2(Sin(rot), -Cos(rot)); // TODO remove - in normal x/y system

		/// <summary>Dot product between two vectors.</summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>Returns the dot product.</returns>
		public static SkalTyp operator *(Vector2 v1, Vector2 v2) => v1.X * v2.X + v1.Y * v2.Y; // => |

		/// <summary>Cross product between two vectors.</summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>Returns the cross product.</returns>
		public static SkalTyp operator %(Vector2 v1, Vector2 v2) => v1.X * v2.Y - v1.Y * v2.X; // => ^

		/// <summary>Skalar-multiplicaiton with a Vector and a skalar. Memberwise multiplication.</summary>
		/// <param name="v1">Vector to multiply.</param>
		/// <param name="s">Value to multiply with.</param>
		/// <returns>Returns a new Vector.</returns>
		public static Vector2 operator *(Vector2 v1, SkalTyp s) => new Vector2(v1.X * s, v1.Y * s);

		/// <summary>Skalar-division with a Vector and a skalar. Memberwise division.</summary>
		/// <param name="v1">Vector to divide.</param>
		/// <param name="s">Value to divide by.</param>
		/// <returns>Returns a new Vector.</returns>
		public static Vector2 operator /(Vector2 v1, SkalTyp s) => new Vector2(v1.X / s, v1.Y / s);

		/// <summary>Memberwise-subtraction of two Vectors.</summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>Returns a new Vector.</returns>
		public static Vector2 operator -(Vector2 v1, Vector2 v2) => new Vector2(v1.X - v2.X, v1.Y - v2.Y);

		/// <summary>Skalar-subtraction. Subtracts the skalar memberwise.</summary>
		/// <param name="v1">Vector to subtract of.</param>
		/// <param name="s">Value to subtract.</param>
		/// <returns>Returns a new Vector.</returns>
		public static Vector2 operator -(Vector2 v1, SkalTyp s) => new Vector2(v1.X - s, v1.Y - s);

		public static Vector2 operator -(Vector2 v1) => new Vector2(-v1.X, -v1.Y);

		/// <summary>Memberwise-addition of two Vectors.</summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>Returns a new Vector.</returns>
		public static Vector2 operator +(Vector2 v1, Vector2 v2) => new Vector2(v1.X + v2.X, v1.Y + v2.Y);

		/// <summary>Skalar-addition. Adds the skalar memberwise.</summary>
		/// <param name="v1">Vector to add to.</param>
		/// <param name="s">Value to add.</param>
		/// <returns>Returns a new Vector.</returns>
		public static Vector2 operator +(Vector2 v1, SkalTyp s) => new Vector2(v1.X + s, v1.Y + s);

		public static bool operator ==(Vector2 v1, Vector2 v2) => v1.X == v2.X && v1.Y == v2.Y;

		public static bool operator !=(Vector2 v1, Vector2 v2) => v1.X != v2.X || v1.Y != v2.Y;

		/// <summary>Gets the angle between two Vectors in radian, from the first Vector clockwise to the second.</summary>
		/// <param name="v1">Starting Vector.</param>
		/// <param name="v2">Ending Vector.</param>
		/// <returns>The angle in radian. From -pi/2 to 3pi/2 (yeah, sorry).</returns>
		public static SkalTyp GetAngle(Vector2 v1, Vector2 v2) => v2.GetAngle() - v1.GetAngle();

		/// <summary>Gets the absolut angle of the Vector, starting from the global up-vector {0,-1} going clockwise.</summary>
		/// <returns>The angle in radian. From 0 to 2pi.</returns>
		public readonly SkalTyp GetAngle() => Atan2(Y, X) + HALF_PI; // TODO: fix

		public readonly Vector2 Rotate(float angle) => new Vector2(
			X * Cos(angle) + Y * Sin(angle),
			X * Sin(angle) + Y * Cos(angle));

		public readonly SkalTyp Length => Sqrt(LengthSQ);

		public readonly SkalTyp LengthSQ => X * X + Y * Y;

		public readonly SkalTyp Distance(Vector2 v1) => Sqrt(DistanceSQ(v1));

		public readonly SkalTyp DistanceSQ(Vector2 v1)
		{
			var x1 = X - v1.X;
			var y1 = Y - v1.Y;
			return x1 * x1 + y1 * y1;
		}

		public readonly Vector2 Offset(SkalTyp val) => new Vector2(X + val, Y + val);

		public readonly Vector2 Offset(SkalTyp x, SkalTyp y) => new Vector2(X + x, Y + y);

		public readonly Vector2 Offset(Vector2 v1) => new Vector2(X + v1.X, Y + v1.Y);

		public readonly Vector2 Normalized => this / this.Length;

		public readonly override string ToString() => $"({X} {Y})";
	}
}

namespace Math2D
{
#if NETSTANDARD2_1
	using M = System.MathF;
#else
	using M = System.Math;
#endif

	internal class BSMath
	{
		public const float PI = (float)M.PI;
		public static float Sin(float x) => (float)M.Sin(x);
		public static float Cos(float x) => (float)M.Cos(x);
		public static float Atan2(float y, float x) => (float)M.Atan2(y, x);
		public static float Sqrt(float x) => (float)M.Sqrt(x);
		public static float Log(float x) => (float)M.Log(x);
		public static float Log(float x, float y) => (float)M.Log(x, y);
		public static float Ceiling(float x) => (float)M.Ceiling(x);
	}
}
