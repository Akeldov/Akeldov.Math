using System;
using System.Globalization;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents an infinite line in three-dimensional space.
    /// </summary>
    /// <remarks>
    /// The default value represents the X axis through the global coordinate origin.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Line : ICurve, IEquatable<Line>
    {
        // Store X shifted so default(Line) has a valid direction along the positive X axis.
        private readonly float _directionXMinusOne;
        private readonly float _directionY;
        private readonly float _directionZ;
        private readonly PointXYZ _closestPointToOrigin;

        /// <summary>
        /// Initializes a new line passing through the specified points.
        /// </summary>
        /// <param name="a">The first point defining the line.</param>
        /// <param name="b">The second point defining the line.</param>
        /// <exception cref="ArgumentException">Thrown when the points are equal.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a point coordinate is NaN or infinite.</exception>
        public Line(PointXYZ a, PointXYZ b)
        {
            if (float.IsNaN(a.X) || float.IsInfinity(a.X) ||
                float.IsNaN(a.Y) || float.IsInfinity(a.Y) ||
                float.IsNaN(a.Z) || float.IsInfinity(a.Z))
                throw new ArgumentOutOfRangeException(nameof(a), "Line point coordinates must be finite.");

            if (float.IsNaN(b.X) || float.IsInfinity(b.X) ||
                float.IsNaN(b.Y) || float.IsInfinity(b.Y) ||
                float.IsNaN(b.Z) || float.IsInfinity(b.Z))
                throw new ArgumentOutOfRangeException(nameof(b), "Line point coordinates must be finite.");

            if (a.Equals(b))
                throw new ArgumentException("Line points must be distinct.", nameof(b));

            Initialize(
                a,
                (double)b.X - a.X,
                (double)b.Y - a.Y,
                (double)b.Z - a.Z,
                out _directionXMinusOne,
                out _directionY,
                out _directionZ,
                out _closestPointToOrigin,
                nameof(b));
        }

        /// <summary>
        /// Initializes a new line passing through the specified point in the specified direction.
        /// </summary>
        /// <param name="point">A point on the line.</param>
        /// <param name="direction">A non-zero vector parallel to the line.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="direction"/> is the zero vector.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when a point coordinate or direction component is NaN or infinite.
        /// </exception>
        public Line(PointXYZ point, VectorXYZ direction)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Line point coordinates must be finite.");

            if (!direction.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(direction), "Line direction components must be finite.");

            Initialize(
                point,
                direction.X,
                direction.Y,
                direction.Z,
                out _directionXMinusOne,
                out _directionY,
                out _directionZ,
                out _closestPointToOrigin,
                nameof(direction));
        }

        /// <summary>
        /// Gets the normalized canonical direction vector of this line.
        /// </summary>
        public VectorXYZ Direction => new VectorXYZ(_directionXMinusOne + 1f, _directionY, _directionZ);

        /// <summary>
        /// Gets the closest point on this line to the global coordinate origin.
        /// </summary>
        public PointXYZ ClosestPointToOrigin => _closestPointToOrigin;

        /// <summary>
        /// Returns the shortest distance from the specified point to this line.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this line.</returns>
        public float Distance(PointXYZ point)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");

            PointXYZ projection = GetProjectedPoint(point);
            return GetDistance(point, projection);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Line other && Equals(other);

        /// <summary>
        /// Indicates whether this line has the same canonical origin and direction as another line.
        /// </summary>
        /// <param name="other">The line to compare with this line.</param>
        /// <returns><see langword="true"/> if both lines are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Line other) =>
            Direction.Equals(other.Direction) &&
            ClosestPointToOrigin.Equals(other.ClosestPointToOrigin);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Direction, ClosestPointToOrigin);

        /// <inheritdoc/>
        public CurveProjection Project(PointXYZ point)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");

            PointXYZ projection = GetProjectedPoint(point);
            return new CurveProjection(projection, GetDistance(point, projection));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0} + t*{1})", ClosestPointToOrigin, Direction);
        }

        /// <summary>
        /// Indicates whether two lines are equal.
        /// </summary>
        /// <param name="left">The first line.</param>
        /// <param name="right">The second line.</param>
        /// <returns><see langword="true"/> if the lines are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Line left, Line right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Indicates whether two lines are different.
        /// </summary>
        /// <param name="left">The first line.</param>
        /// <param name="right">The second line.</param>
        /// <returns><see langword="true"/> if the lines are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Line left, Line right)
        {
            return !(left == right);
        }

        private PointXYZ GetProjectedPoint(PointXYZ point)
        {
            VectorXYZ direction = Direction;
            PointXYZ closestPoint = ClosestPointToOrigin;
            double offsetX = (double)point.X - closestPoint.X;
            double offsetY = (double)point.Y - closestPoint.Y;
            double offsetZ = (double)point.Z - closestPoint.Z;
            double coordinate =
                offsetX * direction.X +
                offsetY * direction.Y +
                offsetZ * direction.Z;

            return new PointXYZ(
                (float)(closestPoint.X + coordinate * direction.X),
                (float)(closestPoint.Y + coordinate * direction.Y),
                (float)(closestPoint.Z + coordinate * direction.Z));
        }

        private static float GetDistance(PointXYZ point, PointXYZ projection)
        {
            double dx = (double)point.X - projection.X;
            double dy = (double)point.Y - projection.Y;
            double dz = (double)point.Z - projection.Z;

            return (float)global::System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private static void Initialize(
            PointXYZ point,
            double directionX,
            double directionY,
            double directionZ,
            out float normalizedXMinusOne,
            out float normalizedY,
            out float normalizedZ,
            out PointXYZ closestPointToOrigin,
            string invalidParamName)
        {
            double length = global::System.Math.Sqrt(
                directionX * directionX +
                directionY * directionY +
                directionZ * directionZ);

            if (length == 0d)
                throw new ArgumentException("Line direction must have non-zero length.", invalidParamName);

            float normalizedX = (float)(directionX / length);
            normalizedY = (float)(directionY / length);
            normalizedZ = (float)(directionZ / length);

            if (normalizedX < 0f ||
                (normalizedX == 0f && normalizedY < 0f) ||
                (normalizedX == 0f && normalizedY == 0f && normalizedZ < 0f))
            {
                normalizedX = -normalizedX;
                normalizedY = -normalizedY;
                normalizedZ = -normalizedZ;
            }

            double pointCoordinate =
                (double)point.X * normalizedX +
                (double)point.Y * normalizedY +
                (double)point.Z * normalizedZ;

            normalizedXMinusOne = normalizedX - 1f;
            closestPointToOrigin = new PointXYZ(
                (float)(point.X - pointCoordinate * normalizedX),
                (float)(point.Y - pointCoordinate * normalizedY),
                (float)(point.Z - pointCoordinate * normalizedZ));
        }
    }
}
