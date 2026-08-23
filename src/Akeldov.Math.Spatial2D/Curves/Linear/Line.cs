using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents an infinite two-dimensional line.
    /// </summary>
    /// <remarks>
    /// The default value represents the horizontal line <c>y = 0</c>.
    /// </remarks>
    public readonly struct Line : ICurve, IEquatable<Line>
    {
        private readonly float _equationA;
        // Store B shifted so default(Line) represents y = 0 instead of an invalid zero-coefficient equation.
        private readonly float _equationBMinusOne;
        private readonly float _equationC;

        /// <summary>
        /// Initializes a new line passing through the specified points.
        /// </summary>
        /// <param name="a">The first point defining the line.</param>
        /// <param name="b">The second point defining the line.</param>
        /// <exception cref="ArgumentException">Thrown when the points are equal.</exception>
        public Line(PointXY a, PointXY b)
        {
            PointXYValidation.ThrowIfNotFinite(
                a,
                nameof(a),
                "Line point coordinates must be finite.");

            PointXYValidation.ThrowIfNotFinite(
                b,
                nameof(b),
                "Line point coordinates must be finite.");

            if (a.Equals(b))
                throw new ArgumentException("Line endpoints must be distinct.", nameof(b));

            float equationA = b.Y - a.Y;
            float equationB = a.X - b.X;
            float equationC = -(equationA * a.X + equationB * a.Y);

            Initialize(equationA, equationB, equationC,
                out _equationA,
                out _equationBMinusOne,
                out _equationC);
        }

        /// <summary>
        /// Initializes a new line passing through the specified point and direction angle.
        /// </summary>
        /// <param name="point">The point defining the line.</param>
        /// <param name="angle">The line direction angle in radians.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="angle"/> is NaN or infinite.</exception>
        public Line(PointXY point, float angle)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Line point coordinates must be finite.");

            if (float.IsNaN(angle) || float.IsInfinity(angle))
                throw new ArgumentOutOfRangeException(nameof(angle), "Line angle must be finite.");

            float equationA = MathF.Sin(angle);
            float equationB = -MathF.Cos(angle);
            float equationC = -(equationA * point.X + equationB * point.Y);

            Initialize(equationA, equationB, equationC,
                out _equationA,
                out _equationBMinusOne,
                out _equationC);
        }

        /// <summary>
        /// Initializes a new line from the implicit equation <c>ax + by + c = 0</c>.
        /// </summary>
        /// <param name="a">The X coefficient.</param>
        /// <param name="b">The Y coefficient.</param>
        /// <param name="c">The offset coefficient.</param>
        /// <exception cref="ArgumentException">Thrown when both linear coefficients are zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an equation coefficient is NaN or infinite.</exception>
        public Line(float a, float b, float c)
        {
            if (float.IsNaN(a) || float.IsInfinity(a))
                throw new ArgumentOutOfRangeException(nameof(a), "Line equation coefficients must be finite.");

            if (float.IsNaN(b) || float.IsInfinity(b))
                throw new ArgumentOutOfRangeException(nameof(b), "Line equation coefficients must be finite.");

            if (float.IsNaN(c) || float.IsInfinity(c))
                throw new ArgumentOutOfRangeException(nameof(c), "Line equation coefficients must be finite.");

            Initialize(a, b, c,
                out _equationA,
                out _equationBMinusOne,
                out _equationC);
        }

        /// <summary>
        /// Gets the normalized X coefficient of the implicit equation <c>ax + by + c = 0</c>.
        /// </summary>
        public float EquationA => _equationA;

        /// <summary>
        /// Gets the normalized Y coefficient of the implicit equation <c>ax + by + c = 0</c>.
        /// </summary>
        public float EquationB => _equationBMinusOne + 1f;

        /// <summary>
        /// Gets the normalized offset coefficient of the implicit equation <c>ax + by + c = 0</c>.
        /// </summary>
        public float EquationC => _equationC;

        /// <summary>
        /// Gets the normalized vector perpendicular to this line.
        /// </summary>
        public VectorXY Normal => new VectorXY(EquationA, EquationB);

        /// <summary>
        /// Gets the normalized canonical direction vector of this line.
        /// </summary>
        public VectorXY Direction => new VectorXY(EquationB, -EquationA);

        /// <summary>
        /// Gets the closest point on this line to the global coordinate origin.
        /// </summary>
        public PointXY ClosestPointToOrigin => new PointXY(
            -_equationC * Normal.X,
            -_equationC * Normal.Y);

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            if (EquationA == 0f)
                return 0;

            float x = -(EquationB * origin.Y + EquationC) / EquationA;
            return x > origin.X ? 1 : 0;
        }

        /// <summary>
        /// Returns the shortest distance from the specified point to this line.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this line.</returns>
        public float Distance(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            return MathF.Abs(GetSignedDistance(point));
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Line other && Equals(other);

        /// <summary>
        /// Indicates whether this line has the same normalized implicit equation as another line.
        /// </summary>
        /// <param name="other">The line to compare with this line.</param>
        /// <returns><see langword="true"/> if both lines are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Line other) =>
            EquationA.Equals(other.EquationA) &&
            EquationB.Equals(other.EquationB) &&
            EquationC.Equals(other.EquationC);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(EquationA, EquationB, EquationC);

        /// <inheritdoc/>
        public List<PointXY> GetPointIntersections(Ray ray)
        {
            const float geometryEpsilon = GeometryConstants.GeometryEpsilon;

            VectorXY direction = Direction;
            if (direction.SquaredLength > geometryEpsilon * geometryEpsilon &&
                VectorXY.Cross(ray.Direction, direction).IsAlmostZero(geometryEpsilon) &&
                VectorXY.Cross(ClosestPointToOrigin - ray.Origin, direction).IsAlmostZero(geometryEpsilon))
            {
                return new List<PointXY>();
            }

            return GetIntersectionsWithTolerance(ray, geometryEpsilon);
        }

        /// <summary>
        /// Returns point intersections with the specified ray. If the ray lies on this line,
        /// returns the ray origin as the first point encountered by the ray.
        /// </summary>
        /// <param name="ray">The ray to intersect with this line.</param>
        /// <param name="geometryEpsilon">The geometry comparison tolerance in world coordinate units.</param>
        /// <returns>A new mutable list of intersection points in the forward direction of the ray, owned by the caller.</returns>
        private List<PointXY> GetIntersectionsWithTolerance(
            Ray ray,
            float geometryEpsilon)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            List<PointXY> intersections = new List<PointXY>();

            PointXY p = ray.Origin;
            VectorXY r = ray.Direction;
            PointXY q = ClosestPointToOrigin;
            VectorXY s = Direction;

            float cross = VectorXY.Cross(r, s);

            if (cross.IsAlmostZero(geometryEpsilon))
            {
                if (s.SquaredLength <= geometryEpsilon * geometryEpsilon)
                {
                    if (IsPointOnRay(q, ray, geometryEpsilon))
                        intersections.AddDistinct(q, geometryEpsilon);
                }
                else if (VectorXY.Cross(q - p, s).IsAlmostZero(geometryEpsilon))
                {
                    intersections.AddDistinct(p, geometryEpsilon);
                }

                return intersections;
            }

            VectorXY qMinusP = q - p;
            float t = VectorXY.Cross(qMinusP, s) / cross;

            if (t >= 0)
            {
                PointXY intersection = p + r * t;
                intersections.AddDistinct(intersection, geometryEpsilon);
            }

            return intersections;
        }

        private static bool IsPointOnRay(
            PointXY point,
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            VectorXY toPoint = point - ray.Origin;

            if (VectorXY.Dot(toPoint, ray.Direction) < -geometryEpsilon)
                return false;

            return VectorXY.Cross(toPoint, ray.Direction).IsAlmostZero(geometryEpsilon);
        }

        /// <summary>
        /// Projects the specified point onto this line.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point and distance to this line.</returns>
        public CurveProjection Project(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            float signedDistance = GetSignedDistance(point);
            PointXY projection = point - Normal * signedDistance;

            return new CurveProjection(projection, MathF.Abs(signedDistance));
        }

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0} + t*{1})", ClosestPointToOrigin, Direction);

        /// <summary>
        /// Indicates whether two lines are equal.
        /// </summary>
        /// <param name="left">The first line.</param>
        /// <param name="right">The second line.</param>
        /// <returns><see langword="true"/> if the lines are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Line left, Line right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two lines are different.
        /// </summary>
        /// <param name="left">The first line.</param>
        /// <param name="right">The second line.</param>
        /// <returns><see langword="true"/> if the lines are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Line left, Line right) => !(left == right);

        private float GetSignedDistance(PointXY point)
        {
            return EquationA * point.X + EquationB * point.Y + EquationC;
        }

        private static void Initialize(
            float equationA,
            float equationB,
            float equationC,
            out float normalizedA,
            out float normalizedBMinusOne,
            out float normalizedC)
        {
            float scale = MathF.Sqrt(equationA * equationA + equationB * equationB);
            if (scale == 0f)
                throw new ArgumentException("Line equation must have at least one non-zero linear coefficient.");

            normalizedA = equationA / scale;
            float normalizedB = equationB / scale;
            normalizedC = equationC / scale;

            if (normalizedA < 0f || (normalizedA == 0f && normalizedB < 0f))
            {
                normalizedA = -normalizedA;
                normalizedB = -normalizedB;
                normalizedC = -normalizedC;
            }

            normalizedBMinusOne = normalizedB - 1f;
        }
    }
}
