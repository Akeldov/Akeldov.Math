using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Represents a two-dimensional vector with single-precision floating-point components.
    /// </summary>
    public readonly struct VectorXY : IEquatable<VectorXY>
    {
        /// <summary>
        /// Initializes a new vector with the specified components.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        public VectorXY(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the X component.
        /// </summary>
        public float X { get; }

        /// <summary>
        /// Gets the Y component.
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Gets the Euclidean length of this vector.
        /// </summary>
        public float Length => MathF.Sqrt(X * X + Y * Y);

        /// <summary>
        /// Gets the squared Euclidean length of this vector.
        /// </summary>
        public float SquaredLength => X * X + Y * Y;

        /// <summary>
        /// Gets a value indicating whether both components are neither NaN nor infinite.
        /// </summary>
        public bool IsFinite =>
            !float.IsNaN(X) && !float.IsInfinity(X) &&
            !float.IsNaN(Y) && !float.IsInfinity(Y);

        /// <summary>
        /// Gets the vector with both components equal to zero.
        /// </summary>
        public static VectorXY Zero => new VectorXY(0f, 0f);

        /// <summary>
        /// Gets the vector with both components equal to one.
        /// </summary>
        public static VectorXY One => new VectorXY(1f, 1f);

        /// <summary>
        /// Returns a vector with the same direction and a length of one.
        /// </summary>
        /// <returns>The normalized vector, or <see cref="Zero"/> when this vector has zero length.</returns>
        public VectorXY Normalize()
        {
            float length = Length;
            return length == 0f ? Zero : new VectorXY(X / length, Y / length);
        }

        /// <summary>
        /// Rounds each component to the nearest integer component.
        /// </summary>
        /// <returns>The rounded integer vector.</returns>
        public VectorXYInt RoundToInt()
        {
            return new VectorXYInt((int)MathF.Round(X), (int)MathF.Round(Y));
        }

        /// <summary>
        /// Indicates whether this vector has the same components as another vector.
        /// </summary>
        /// <param name="other">The vector to compare with this vector.</param>
        /// <returns><see langword="true"/> if both components are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(VectorXY other) => X.Equals(other.X) && Y.Equals(other.Y);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is VectorXY other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(X, Y);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0}, {1})", X, Y);

        /// <summary>
        /// Deconstructs this vector into its X and Y components.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        public void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
        }

        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>The dot product.</returns>
        public static float Dot(in VectorXY left, in VectorXY right) => left.X * right.X + left.Y * right.Y;

        /// <summary>
        /// Returns the scalar two-dimensional cross product of two vectors.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>The signed cross product.</returns>
        public static float Cross(in VectorXY left, in VectorXY right) => left.X * right.Y - left.Y * right.X;

        /// <summary>
        /// Returns the signed angle from one vector to another in radians.
        /// </summary>
        /// <param name="from">The vector to rotate from.</param>
        /// <param name="to">The vector to rotate to.</param>
        /// <returns>The signed angle in radians.</returns>
        public static float Angle(VectorXY from, VectorXY to)
        {
            float dot = Dot(from, to);
            float cross = Cross(from, to);

            return MathF.Atan2(cross, dot);
        }

        /// <summary>
        /// Indicates whether two vectors have equal components.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns><see langword="true"/> if both components are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(VectorXY left, VectorXY right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two vectors have different components.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns><see langword="true"/> if any component differs; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(VectorXY left, VectorXY right) => !left.Equals(right);

        /// <summary>
        /// Adds two vectors component by component.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>The component-wise sum.</returns>
        public static VectorXY operator +(VectorXY left, VectorXY right) =>
            new VectorXY(left.X + right.X, left.Y + right.Y);

        /// <summary>
        /// Subtracts one vector from another component by component.
        /// </summary>
        /// <param name="left">The vector to subtract from.</param>
        /// <param name="right">The vector to subtract.</param>
        /// <returns>The component-wise difference.</returns>
        public static VectorXY operator -(VectorXY left, VectorXY right) =>
            new VectorXY(left.X - right.X, left.Y - right.Y);

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXY operator *(VectorXY vector, float scalar) =>
            new VectorXY(vector.X * scalar, vector.Y * scalar);

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <param name="vector">The vector to multiply.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXY operator *(float scalar, VectorXY vector) =>
            new VectorXY(vector.X * scalar, vector.Y * scalar);

        /// <summary>
        /// Multiplies a vector by an integer scalar.
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXY operator *(VectorXY vector, int scalar) =>
            new VectorXY(vector.X * scalar, vector.Y * scalar);

        /// <summary>
        /// Multiplies a vector by an integer scalar.
        /// </summary>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <param name="vector">The vector to multiply.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXY operator *(int scalar, VectorXY vector) =>
            new VectorXY(vector.X * scalar, vector.Y * scalar);

        /// <summary>
        /// Divides a vector by a scalar.
        /// </summary>
        /// <param name="vector">The vector to divide.</param>
        /// <param name="scalar">The scalar divisor.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXY operator /(VectorXY vector, float scalar) =>
            new VectorXY(vector.X / scalar, vector.Y / scalar);

        /// <summary>
        /// Converts an integer vector to a floating-point vector.
        /// </summary>
        /// <param name="v">The integer vector to convert.</param>
        public static implicit operator VectorXY(VectorXYInt v)
        {
            return new VectorXY(v.X, v.Y);
        }

        /// <summary>
        /// Converts a floating-point vector to an integer vector by truncating each component.
        /// </summary>
        /// <param name="v">The floating-point vector to convert.</param>
        public static explicit operator VectorXYInt(VectorXY v)
        {
            return new VectorXYInt((int)v.X, (int)v.Y);
        }
    }
}
