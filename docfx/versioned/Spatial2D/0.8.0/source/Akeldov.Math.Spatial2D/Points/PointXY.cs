using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Represents a two-dimensional point with single-precision floating-point coordinates.
    /// </summary>
    public readonly struct PointXY : IPointDistanceProvider, IHasPosition2D, IEquatable<PointXY>
    {
        /// <summary>
        /// Initializes a new point with the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="x"/> or <paramref name="y"/> is NaN.
        /// </exception>
        public PointXY(float x, float y)
        {
            if (float.IsNaN(x))
                throw new ArgumentOutOfRangeException(nameof(x), "Point coordinates must not be NaN.");

            if (float.IsNaN(y))
                throw new ArgumentOutOfRangeException(nameof(y), "Point coordinates must not be NaN.");

            X = x;
            Y = y;
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
        /// Gets this point as its own position.
        /// </summary>
        public PointXY Position => this;

        /// <summary>
        /// Indicates whether this point has the same coordinates as another point.
        /// </summary>
        /// <param name="other">The point to compare with this point.</param>
        /// <returns><see langword="true"/> if both coordinates are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(PointXY other) => X.Equals(other.X) && Y.Equals(other.Y);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is PointXY other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(X, Y);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0}, {1})", X, Y);

        /// <summary>
        /// Deconstructs this point into its X and Y coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
        }

        /// <summary>
        /// Returns the Euclidean distance from this point to the specified point.
        /// </summary>
        /// <param name="point">The point to measure to.</param>
        /// <returns>The Euclidean distance between this point and <paramref name="point"/>.</returns>
        public float Distance(PointXY point)
        {
            float dx = point.X - X;
            float dy = point.Y - Y;

            return MathF.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Indicates whether two points have equal coordinates.
        /// </summary>
        /// <param name="left">The first point.</param>
        /// <param name="right">The second point.</param>
        /// <returns><see langword="true"/> if both coordinates are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(PointXY left, PointXY right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two points have different coordinates.
        /// </summary>
        /// <param name="left">The first point.</param>
        /// <param name="right">The second point.</param>
        /// <returns><see langword="true"/> if any coordinate differs; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(PointXY left, PointXY right) => !left.Equals(right);

        /// <summary>
        /// Translates a point by a vector.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <param name="vector">The translation vector.</param>
        /// <returns>The translated point.</returns>
        public static PointXY operator +(PointXY point, VectorXY vector) =>
            new PointXY(point.X + vector.X, point.Y + vector.Y);

        /// <summary>
        /// Translates a point by a vector.
        /// </summary>
        /// <param name="vector">The translation vector.</param>
        /// <param name="point">The point to translate.</param>
        /// <returns>The translated point.</returns>
        public static PointXY operator +(VectorXY vector, PointXY point) =>
            point + vector;

        /// <summary>
        /// Translates a point by the negated vector.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <param name="vector">The translation vector to subtract.</param>
        /// <returns>The translated point.</returns>
        public static PointXY operator -(PointXY point, VectorXY vector) =>
            new PointXY(point.X - vector.X, point.Y - vector.Y);

        /// <summary>
        /// Returns the vector from <paramref name="right"/> to <paramref name="left"/>.
        /// </summary>
        /// <param name="left">The target point.</param>
        /// <param name="right">The source point.</param>
        /// <returns>The vector from <paramref name="right"/> to <paramref name="left"/>.</returns>
        public static VectorXY operator -(PointXY left, PointXY right) =>
            new VectorXY(left.X - right.X, left.Y - right.Y);

        /// <summary>
        /// Converts a point to its coordinate vector.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        public static explicit operator VectorXY(PointXY point)
        {
            return new VectorXY(point.X, point.Y);
        }

        /// <summary>
        /// Converts a coordinate vector to a point.
        /// </summary>
        /// <param name="coordinates">The coordinate vector to convert.</param>
        public static explicit operator PointXY(VectorXY coordinates)
        {
            return new PointXY(coordinates.X, coordinates.Y);
        }
    }
}
