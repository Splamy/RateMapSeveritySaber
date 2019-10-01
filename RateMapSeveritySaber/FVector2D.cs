//#define SYSTEM_DRAWING_EXTENSION

using System;
#if SYSTEM_DRAWING_EXTENSION
using System.Drawing;
#endif

namespace Math2D
{
	using SkalTyp = System.Single;

#pragma warning disable CS0660
#pragma warning disable CS0661
	public readonly struct FVector2D
#pragma warning restore CS0660
#pragma warning restore CS0661
	{
		private const float HALF_PI = (float)(Math.PI / 2);
		private const float TWO_PI = (float)(Math.PI * 2);

		public SkalTyp X { get; }
		public SkalTyp Y { get; }

#if SYSTEM_DRAWING_EXTENSION
		public PointF p
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

		public static readonly FVector2D Zero = new FVector2D(0);

		public FVector2D(SkalTyp val) : this(val, val) { }

		public FVector2D(SkalTyp x, SkalTyp y)
		{
			this.X = x;
			this.Y = y;
		}

		public static FVector2D FromRotation(float rot) => new FVector2D(MathF.Sin(rot), -MathF.Cos(rot)); // TODO remove - in normal x/y system

		/// <summary>Dot product between two vectors.</summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>Returns the dot product.</returns>
		public static SkalTyp operator *(FVector2D v1, FVector2D v2) => v1.X * v2.X + v1.Y * v2.Y; // => |

		/// <summary>Cross product between two vectors.</summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>Returns the cross product.</returns>
		public static SkalTyp operator %(FVector2D v1, FVector2D v2) => v1.X * v2.Y - v1.Y * v2.X; // => ^

		/// <summary>Skalar-multiplicaiton with a Vector and a skalar. Memberwise multiplication.</summary>
		/// <param name="v1">Vector to multiply.</param>
		/// <param name="s">Value to multiply with.</param>
		/// <returns>Returns a new Vector.</returns>
		public static FVector2D operator *(FVector2D v1, SkalTyp s) => new FVector2D(v1.X * s, v1.Y * s);

		/// <summary>Skalar-division with a Vector and a skalar. Memberwise division.</summary>
		/// <param name="v1">Vector to divide.</param>
		/// <param name="s">Value to divide by.</param>
		/// <returns>Returns a new Vector.</returns>
		public static FVector2D operator /(FVector2D v1, SkalTyp s) => new FVector2D(v1.X / s, v1.Y / s);

		/// <summary>Memberwise-subtraction of two Vectors.</summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>Returns a new Vector.</returns>
		public static FVector2D operator -(FVector2D v1, FVector2D v2) => new FVector2D(v1.X - v2.X, v1.Y - v2.Y);

		/// <summary>Skalar-subtraction. Subtracts the skalar memberwise.</summary>
		/// <param name="v1">Vector to subtract of.</param>
		/// <param name="s">Value to subtract.</param>
		/// <returns>Returns a new Vector.</returns>
		public static FVector2D operator -(FVector2D v1, SkalTyp s) => new FVector2D(v1.X - s, v1.Y - s);

		public static FVector2D operator -(FVector2D v1) => new FVector2D(-v1.X, -v1.Y);

		/// <summary>Memberwise-addition of two Vectors.</summary>
		/// <param name="v1">First vector.</param>
		/// <param name="v2">Second vector.</param>
		/// <returns>Returns a new Vector.</returns>
		public static FVector2D operator +(FVector2D v1, FVector2D v2) => new FVector2D(v1.X + v2.X, v1.Y + v2.Y);

		/// <summary>Skalar-addition. Adds the skalar memberwise.</summary>
		/// <param name="v1">Vector to add to.</param>
		/// <param name="s">Value to add.</param>
		/// <returns>Returns a new Vector.</returns>
		public static FVector2D operator +(FVector2D v1, SkalTyp s) => new FVector2D(v1.X + s, v1.Y + s);

		public static bool operator ==(FVector2D v1, FVector2D v2) => v1.X == v2.X && v1.Y == v2.Y;

		public static bool operator !=(FVector2D v1, FVector2D v2) => v1.X != v2.X || v1.Y != v2.Y;

		/// <summary>Gets the angle between two Vectors in radian, from the first Vector clockwise to the second.</summary>
		/// <param name="v1">Starting Vector.</param>
		/// <param name="v2">Ending Vector.</param>
		/// <returns>The angle in radian. From -pi/2 to 3pi/2 (yeah, sorry).</returns>
		public static SkalTyp GetAngle(FVector2D v1, FVector2D v2) => v2.GetAngle() - v1.GetAngle();

		/// <summary>Gets the absolut angle of the Vector, starting from the global up-vector {0,-1} going clockwise.</summary>
		/// <returns>The angle in radian. From 0 to 2pi.</returns>
		public SkalTyp GetAngle() => (SkalTyp)(MathF.Atan2(Y, X) + HALF_PI); // TODO: fix

		public SkalTyp Length => MathF.Sqrt(LengthSQ);

		public SkalTyp LengthSQ => X * X + Y * Y;

		public SkalTyp Distance(FVector2D v1) => MathF.Sqrt(DistanceSQ(v1));

		public SkalTyp DistanceSQ(FVector2D v1)
		{
			SkalTyp x1 = X - v1.X;
			SkalTyp y1 = Y - v1.Y;
			return x1 * x1 + y1 * y1;
		}

		public FVector2D Offset(SkalTyp val) => new FVector2D(X + val, Y + val);

		public FVector2D Offset(SkalTyp x, SkalTyp y) => new FVector2D(X + x, Y + y);

		public FVector2D Offset(FVector2D v1) => new FVector2D(X + v1.X, Y + v1.Y);

		public FVector2D Normalize() => this / this.Length;

		public override string ToString() => $"(X:{X} Y:{Y})";
	}
}
