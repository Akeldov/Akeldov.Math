using System;
using System.Globalization;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Akeldov.Math.Spatial3D
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Represents a three-dimensional point with single-precision floating-point coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct PointXYZ : IPointDistanceProvider, IHasPosition3D, IEquatable<PointXYZ>
    {
        /// <summary>
        /// Initializes a new point with the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="x"/>, <paramref name="y"/>, or <paramref name="z"/> is NaN.
        /// </exception>
        public PointXYZ(float x, float y, float z)
        {
            if (float.IsNaN(x))
                throw new ArgumentOutOfRangeException(nameof(x), "Point coordinates must not be NaN.");

            if (float.IsNaN(y))
                throw new ArgumentOutOfRangeException(nameof(y), "Point coordinates must not be NaN.");

            if (float.IsNaN(z))
                throw new ArgumentOutOfRangeException(nameof(z), "Point coordinates must not be NaN.");

            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets the X coordinate.
        /// </summary>
        public float X { get; }

        /// <summary>
        /// Gets the Y coordinate.
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Gets the Z coordinate.
        /// </summary>
        public float Z { get; }

        /// <summary>
        /// Gets this point as its own position.
        /// </summary>
        public PointXYZ Position => this;

        /// <summary>
        /// Indicates whether this point has the same coordinates as another point.
        /// </summary>
        /// <param name="other">The point to compare with this point.</param>
        /// <returns><see langword="true"/> if all coordinates are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(PointXYZ other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is PointXYZ other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2})", X, Y, Z);

        /// <summary>
        /// Deconstructs this point into its X, Y, and Z coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public void Deconstruct(out float x, out float y, out float z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        /// <summary>
        /// Returns the Euclidean distance from this point to the specified point.
        /// </summary>
        /// <param name="point">The point to measure to.</param>
        /// <returns>The Euclidean distance between this point and <paramref name="point"/>.</returns>
        public float Distance(PointXYZ point)
        {
            float dx = point.X - X;
            float dy = point.Y - Y;
            float dz = point.Z - Z;

            return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Indicates whether two points have equal coordinates.
        /// </summary>
        /// <param name="left">The first point.</param>
        /// <param name="right">The second point.</param>
        /// <returns><see langword="true"/> if all coordinates are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(PointXYZ left, PointXYZ right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two points have different coordinates.
        /// </summary>
        /// <param name="left">The first point.</param>
        /// <param name="right">The second point.</param>
        /// <returns><see langword="true"/> if any coordinate differs; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(PointXYZ left, PointXYZ right) => !left.Equals(right);

        /// <summary>
        /// Translates a point by a vector.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <param name="vector">The translation vector.</param>
        /// <returns>The translated point.</returns>
        public static PointXYZ operator +(PointXYZ point, VectorXYZ vector) =>
            new PointXYZ(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);

        /// <summary>
        /// Translates a point by a vector.
        /// </summary>
        /// <param name="vector">The translation vector.</param>
        /// <param name="point">The point to translate.</param>
        /// <returns>The translated point.</returns>
        public static PointXYZ operator +(VectorXYZ vector, PointXYZ point) => point + vector;

        /// <summary>
        /// Translates a point by the negated vector.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <param name="vector">The translation vector to subtract.</param>
        /// <returns>The translated point.</returns>
        public static PointXYZ operator -(PointXYZ point, VectorXYZ vector) =>
            new PointXYZ(point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z);

        /// <summary>
        /// Returns the vector from <paramref name="right"/> to <paramref name="left"/>.
        /// </summary>
        /// <param name="left">The target point.</param>
        /// <param name="right">The source point.</param>
        /// <returns>The vector from <paramref name="right"/> to <paramref name="left"/>.</returns>
        public static VectorXYZ operator -(PointXYZ left, PointXYZ right) =>
            new VectorXYZ(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        /// <summary>
        /// Converts a point to its coordinate vector.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        public static explicit operator VectorXYZ(PointXYZ point) =>
            new VectorXYZ(point.X, point.Y, point.Z);

        /// <summary>
        /// Converts a coordinate vector to a point.
        /// </summary>
        /// <param name="coordinates">The coordinate vector to convert.</param>
        public static explicit operator PointXYZ(VectorXYZ coordinates) =>
            new PointXYZ(coordinates.X, coordinates.Y, coordinates.Z);
    }
}
