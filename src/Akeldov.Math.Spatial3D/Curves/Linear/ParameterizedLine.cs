using System;
using System.Globalization;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents an infinite three-dimensional line with an explicit curve-coordinate origin and direction.
    /// </summary>
    /// <remarks>
    /// The default value represents the X axis through the global coordinate origin, directed along the positive X axis.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ParameterizedLine : IParameterizedCurve, IEquatable<ParameterizedLine>
    {
        private readonly Line _line;
        private readonly PointXYZ _origin;
        private readonly bool _isDirectionReversed;

        /// <summary>
        /// Initializes a new parameterized line from a line using its canonical origin and direction.
        /// </summary>
        /// <param name="line">The geometric line.</param>
        public ParameterizedLine(Line line)
            : this(line, line.ClosestPointToOrigin, line.Direction)
        {
        }

        /// <summary>
        /// Initializes a new parameterized line from a line and a reference point.
        /// </summary>
        /// <param name="line">The geometric line.</param>
        /// <param name="referencePoint">The point whose projection becomes the curve-coordinate origin.</param>
        public ParameterizedLine(Line line, PointXYZ referencePoint)
            : this(line, referencePoint, line.Direction)
        {
        }

        /// <summary>
        /// Initializes a new parameterized line from a line, reference point, and direction.
        /// </summary>
        /// <param name="line">The geometric line.</param>
        /// <param name="referencePoint">The point whose projection becomes the curve-coordinate origin.</param>
        /// <param name="direction">The parameterized direction along <paramref name="line"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="direction"/> has zero length or is not parallel to <paramref name="line"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when a reference point coordinate or direction component is NaN or infinite.
        /// </exception>
        public ParameterizedLine(Line line, PointXYZ referencePoint, VectorXYZ direction)
        {
            if (float.IsNaN(referencePoint.X) || float.IsInfinity(referencePoint.X) ||
                float.IsNaN(referencePoint.Y) || float.IsInfinity(referencePoint.Y) ||
                float.IsNaN(referencePoint.Z) || float.IsInfinity(referencePoint.Z))
                throw new ArgumentOutOfRangeException(
                    nameof(referencePoint),
                    "Parameterized line reference point coordinates must be finite.");

            if (!direction.IsFinite)
                throw new ArgumentOutOfRangeException(
                    nameof(direction),
                    "Parameterized line direction components must be finite.");

            var directionLine = new Line(default(PointXYZ), direction);
            VectorXYZ normalizedDirection = VectorXYZ.Dot(direction, directionLine.Direction) < 0f
                ? directionLine.Direction * -1f
                : directionLine.Direction;
            VectorXYZ directionCross = VectorXYZ.Cross(normalizedDirection, line.Direction);
            if (directionCross.SquaredLength > GeometryConstants.GeometryEpsilon * GeometryConstants.GeometryEpsilon)
                throw new ArgumentException(
                    "Parameterized line direction must be parallel to the line.",
                    nameof(direction));

            _line = line;
            _origin = line.Project(referencePoint).ProjectedPoint;
            _isDirectionReversed = VectorXYZ.Dot(normalizedDirection, line.Direction) < 0f;
        }

        /// <summary>
        /// Initializes a new parameterized line from an origin point and a direction vector.
        /// </summary>
        /// <param name="origin">The curve-coordinate origin.</param>
        /// <param name="direction">The parameterized direction.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="direction"/> has zero length.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when an origin coordinate or direction component is NaN or infinite.
        /// </exception>
        public ParameterizedLine(PointXYZ origin, VectorXYZ direction)
        {
            if (float.IsNaN(origin.X) || float.IsInfinity(origin.X) ||
                float.IsNaN(origin.Y) || float.IsInfinity(origin.Y) ||
                float.IsNaN(origin.Z) || float.IsInfinity(origin.Z))
                throw new ArgumentOutOfRangeException(
                    nameof(origin),
                    "Parameterized line origin coordinates must be finite.");

            if (!direction.IsFinite)
                throw new ArgumentOutOfRangeException(
                    nameof(direction),
                    "Parameterized line direction components must be finite.");

            var line = new Line(origin, direction);

            _line = line;
            _origin = origin;
            _isDirectionReversed = VectorXYZ.Dot(direction, line.Direction) < 0f;
        }

        /// <summary>
        /// Initializes a new parameterized line passing through the specified points and selects the curve-coordinate origin
        /// from the specified reference point mode.
        /// </summary>
        /// <param name="a">The first point defining the line.</param>
        /// <param name="b">The second point defining the line.</param>
        /// <param name="referencePointMode">The mode used to select the reference point.</param>
        /// <exception cref="ArgumentException">Thrown when the points are equal.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a point coordinate is NaN or infinite.</exception>
        public ParameterizedLine(PointXYZ a, PointXYZ b, LineReferencePointMode referencePointMode)
            : this(new Line(a, b), SelectReferencePoint(a, b, referencePointMode))
        {
        }

        /// <summary>
        /// Initializes a new parameterized line passing through the specified points.
        /// </summary>
        /// <param name="a">The first point defining the line.</param>
        /// <param name="b">The second point defining the line.</param>
        /// <param name="referencePoint">The point whose projection becomes the curve-coordinate origin.</param>
        /// <exception cref="ArgumentException">Thrown when the points are equal.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when a line point or reference point coordinate is NaN or infinite.
        /// </exception>
        public ParameterizedLine(PointXYZ a, PointXYZ b, PointXYZ referencePoint)
            : this(new Line(a, b), referencePoint)
        {
        }

        /// <summary>
        /// Gets the geometric line.
        /// </summary>
        public Line Line => _line;

        /// <summary>
        /// Gets the normalized parameterized direction vector.
        /// </summary>
        public VectorXYZ Direction => _isDirectionReversed
            ? _line.Direction * -1f
            : _line.Direction;

        /// <summary>
        /// Gets the point on this line from which curve coordinates are measured.
        /// </summary>
        public PointXYZ Origin => _origin;

        /// <summary>
        /// Gets the closest point on this line to the global coordinate origin.
        /// </summary>
        public PointXYZ ClosestPointToOrigin => _line.ClosestPointToOrigin;

        /// <summary>
        /// Returns the shortest distance from the specified point to this line.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this line.</returns>
        public float Distance(PointXYZ point)
        {
            return _line.Distance(point);
        }

        /// <summary>
        /// Indicates whether this parameterized line has the same geometric line as another parameterized line.
        /// </summary>
        /// <param name="other">The parameterized line to compare with this line.</param>
        /// <returns><see langword="true"/> if both parameterized lines share the same geometry; otherwise, <see langword="false"/>.</returns>
        public bool HasSameGeometry(ParameterizedLine other) => _line.Equals(other._line);

        /// <summary>
        /// Indicates whether this parameterized line has the same geometric line as the specified line.
        /// </summary>
        /// <param name="line">The line to compare with this parameterized line.</param>
        /// <returns><see langword="true"/> if both lines share the same geometry; otherwise, <see langword="false"/>.</returns>
        public bool HasSameGeometry(Line line) => _line.Equals(line);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is ParameterizedLine other && Equals(other);

        /// <summary>
        /// Indicates whether this parameterized line has the same geometry, origin, and direction as another parameterized line.
        /// </summary>
        /// <param name="other">The parameterized line to compare with this line.</param>
        /// <returns><see langword="true"/> if both parameterized lines are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ParameterizedLine other) =>
            _line.Equals(other._line) &&
            _origin.Equals(other._origin) &&
            _isDirectionReversed == other._isDirectionReversed;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(_line, _origin, _isDirectionReversed);

        /// <summary>
        /// Projects the specified point onto this parameterized line.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point and distance to this line.</returns>
        public CurveProjection Project(PointXYZ point)
        {
            return _line.Project(point);
        }

        /// <summary>
        /// Projects the specified point onto this parameterized line and reports its signed curve coordinate.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point, signed curve coordinate, and distance to this line.</returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXYZ point)
        {
            CurveProjection projection = Project(point);
            float curveCoordinate = VectorXYZ.Dot(projection.ProjectedPoint - _origin, Direction);

            return new ParameterizedCurveProjection(projection.ProjectedPoint, curveCoordinate, projection.Distance);
        }

        /// <summary>
        /// Returns the point at the specified signed curve coordinate.
        /// </summary>
        /// <param name="curveCoordinate">The finite signed curve coordinate in world coordinate units.</param>
        /// <returns>The point on this parameterized line.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="curveCoordinate"/> is NaN or infinite.
        /// </exception>
        public PointXYZ GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            return Origin + curveCoordinate * Direction;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0} + t*{1})", Origin, Direction);

        /// <summary>
        /// Indicates whether two parameterized lines are equal.
        /// </summary>
        /// <param name="left">The first parameterized line.</param>
        /// <param name="right">The second parameterized line.</param>
        /// <returns><see langword="true"/> if the parameterized lines are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ParameterizedLine left, ParameterizedLine right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two parameterized lines are different.
        /// </summary>
        /// <param name="left">The first parameterized line.</param>
        /// <param name="right">The second parameterized line.</param>
        /// <returns><see langword="true"/> if the parameterized lines are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ParameterizedLine left, ParameterizedLine right) => !(left == right);

        /// <summary>
        /// Converts a parameterized line to its geometric line.
        /// </summary>
        /// <param name="line">The parameterized line to convert.</param>
        public static explicit operator Line(ParameterizedLine line)
        {
            return line._line;
        }

        private static PointXYZ SelectReferencePoint(PointXYZ a, PointXYZ b, LineReferencePointMode referencePointMode)
        {
            switch (referencePointMode)
            {
                case LineReferencePointMode.PointA:
                    return a;
                case LineReferencePointMode.PointB:
                    return b;
                case LineReferencePointMode.Midpoint:
                    return new PointXYZ(
                        (float)(((double)a.X + b.X) * 0.5d),
                        (float)(((double)a.Y + b.Y) * 0.5d),
                        (float)(((double)a.Z + b.Z) * 0.5d));
                default:
                    return default;
            }
        }
    }
}
