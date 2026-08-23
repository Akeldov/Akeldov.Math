using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents an infinite two-dimensional line with an explicit curve-coordinate origin and direction.
    /// </summary>
    /// <remarks>
    /// The default value represents the horizontal line <c>y = 0</c>, with origin at the coordinate origin
    /// and direction along the positive X axis.
    /// </remarks>
    public readonly struct ParameterizedLine : IParameterizedCurve, IRightwardCrossingProvider, IEquatable<ParameterizedLine>
    {
        private readonly Line _line;
        private readonly PointXY _origin;
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
        public ParameterizedLine(Line line, PointXY referencePoint)
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
        public ParameterizedLine(Line line, PointXY referencePoint, VectorXY direction)
        {
            PointXYValidation.ThrowIfNotFinite(
                referencePoint,
                nameof(referencePoint),
                "Parameterized line reference point coordinates must be finite.");

            VectorXY normalizedDirection = NormalizeDirection(direction);
            if (!VectorXY.Cross(normalizedDirection, line.Direction).IsAlmostZero())
                throw new ArgumentException("Parameterized line direction must be parallel to the line.", nameof(direction));

            _line = line;
            _origin = line.Project(referencePoint).ProjectedPoint;
            _isDirectionReversed = VectorXY.Dot(normalizedDirection, line.Direction) < 0f;
        }

        /// <summary>
        /// Initializes a new parameterized line from an origin point and a direction vector.
        /// </summary>
        /// <param name="origin">The curve-coordinate origin.</param>
        /// <param name="direction">The parameterized direction.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="direction"/> has zero length.</exception>
        public ParameterizedLine(PointXY origin, VectorXY direction)
        {
            PointXYValidation.ThrowIfNotFinite(
                origin,
                nameof(origin),
                "Parameterized line origin coordinates must be finite.");

            VectorXY normalizedDirection = NormalizeDirection(direction);
            var line = new Line(origin, origin + normalizedDirection);

            _line = line;
            _origin = origin;
            _isDirectionReversed = VectorXY.Dot(normalizedDirection, line.Direction) < 0f;
        }

        /// <summary>
        /// Initializes a new parameterized line from an origin point and direction angle.
        /// </summary>
        /// <param name="origin">The curve-coordinate origin.</param>
        /// <param name="angle">The parameterized direction angle in radians.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="angle"/> is NaN or infinite.</exception>
        public ParameterizedLine(PointXY origin, float angle)
            : this(origin, CreateDirectionFromAngle(angle))
        {
        }

        /// <summary>
        /// Initializes a new parameterized line passing through the specified points and selects the curve-coordinate origin
        /// from the specified reference point mode.
        /// </summary>
        /// <param name="a">The first point defining the line.</param>
        /// <param name="b">The second point defining the line.</param>
        /// <param name="referencePointMode">The mode used to select the reference point.</param>
        /// <exception cref="ArgumentException">Thrown when the points are equal.</exception>
        public ParameterizedLine(PointXY a, PointXY b, LineReferencePointMode referencePointMode)
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
        public ParameterizedLine(PointXY a, PointXY b, PointXY referencePoint)
            : this(new Line(a, b), referencePoint)
        {
        }

        /// <summary>
        /// Initializes a new parameterized line from the implicit equation <c>ax + by + c = 0</c>.
        /// </summary>
        /// <param name="a">The X coefficient.</param>
        /// <param name="b">The Y coefficient.</param>
        /// <param name="c">The offset coefficient.</param>
        /// <param name="referencePoint">The point whose projection becomes the curve-coordinate origin.</param>
        /// <exception cref="ArgumentException">Thrown when both linear coefficients are zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an equation coefficient is NaN or infinite.</exception>
        public ParameterizedLine(float a, float b, float c, PointXY referencePoint)
            : this(new Line(a, b, c), referencePoint)
        {
        }

        /// <summary>
        /// Gets the geometric line.
        /// </summary>
        public Line Line => _line;

        /// <summary>
        /// Gets the normalized X coefficient of the implicit equation <c>ax + by + c = 0</c>.
        /// </summary>
        public float EquationA => _line.EquationA;

        /// <summary>
        /// Gets the normalized Y coefficient of the implicit equation <c>ax + by + c = 0</c>.
        /// </summary>
        public float EquationB => _line.EquationB;

        /// <summary>
        /// Gets the normalized offset coefficient of the implicit equation <c>ax + by + c = 0</c>.
        /// </summary>
        public float EquationC => _line.EquationC;

        /// <summary>
        /// Gets the normalized vector perpendicular to this line.
        /// </summary>
        public VectorXY Normal => _line.Normal;

        /// <summary>
        /// Gets the normalized parameterized direction vector.
        /// </summary>
        public VectorXY Direction => _isDirectionReversed
            ? _line.Direction * -1f
            : _line.Direction;

        /// <summary>
        /// Gets the point on this line from which curve coordinates are measured.
        /// </summary>
        public PointXY Origin => _origin;

        /// <summary>
        /// Gets the closest point on this line to the global coordinate origin.
        /// </summary>
        public PointXY ClosestPointToOrigin => _line.ClosestPointToOrigin;

        /// <summary>
        /// Returns the shortest distance from the specified point to this line.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this line.</returns>
        public float Distance(PointXY point)
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
        public int CountRightwardCrossings(PointXY origin) => _line.CountRightwardCrossings(origin);

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
        public CurveProjection Project(PointXY point)
        {
            return _line.Project(point);
        }

        /// <summary>
        /// Projects the specified point onto this parameterized line and reports its signed curve coordinate.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point, signed curve coordinate, and distance to this line.</returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            CurveProjection projection = Project(point);
            float curveCoordinate = VectorXY.Dot(projection.ProjectedPoint - _origin, Direction);

            return new ParameterizedCurveProjection(projection.ProjectedPoint, curveCoordinate, projection.Distance);
        }

        /// <summary>
        /// Returns the point at the specified signed curve coordinate.
        /// </summary>
        /// <param name="curveCoordinate">The finite signed curve coordinate in world coordinate units.</param>
        /// <returns>The point on this parameterized line.</returns>
        public PointXY GetPoint(float curveCoordinate)
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

        private static VectorXY NormalizeDirection(VectorXY direction)
        {
            if (!direction.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(direction), "Parameterized line direction coordinates must be finite.");

            if (direction.SquaredLength <= GeometryConstants.GeometryEpsilonSquared)
                throw new ArgumentException("Parameterized line direction must have non-zero length.", nameof(direction));

            return direction.Normalize();
        }

        private static VectorXY CreateDirectionFromAngle(float angle)
        {
            if (float.IsNaN(angle) || float.IsInfinity(angle))
                throw new ArgumentOutOfRangeException(nameof(angle), "Parameterized line angle must be finite.");

            return new VectorXY(MathF.Cos(angle), MathF.Sin(angle));
        }

        private static PointXY SelectReferencePoint(PointXY a, PointXY b, LineReferencePointMode referencePointMode)
        {
            switch (referencePointMode)
            {
                case LineReferencePointMode.PointA:
                    return a;
                case LineReferencePointMode.PointB:
                    return b;
                case LineReferencePointMode.Midpoint:
                    return a + (b - a) * 0.5f;
                default:
                    return new PointXY(0f, 0f);
            }
        }

    }
}
