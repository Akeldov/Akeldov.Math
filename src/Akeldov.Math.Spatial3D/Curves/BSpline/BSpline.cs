using System;
using System.Collections.Generic;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a finite directed non-uniform polynomial B-spline curve in three-dimensional space.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Supports degree one or higher and clamped or unclamped knot vectors. Interior knot multiplicity
    /// cannot exceed the degree, so the path remains continuous.
    /// </para>
    /// <para>
    /// <see cref="GetPointAt"/> and <see cref="GetPointAtKnot"/> evaluate the polynomial spline using
    /// de Boor's algorithm. Length, distance, projection, and length-coordinate traversal use a cached
    /// polyline with <see cref="SegmentsPerKnotSpan"/> equal parameter subdivisions of each non-empty knot
    /// span. This is an approximation, not an error bound; increase the subdivision count for sharp bends.
    /// </para>
    /// </remarks>
    public sealed class BSpline : IFinitePath
    {
        private readonly PointXYZ[] _controlPoints;
        private readonly float[] _knots;
        private readonly SplinePathApproximation _approximation;

        /// <summary>
        /// Initializes a B-spline curve from copied control points and knots.
        /// </summary>
        /// <param name="degree">The degree, at least one and less than the control point count.</param>
        /// <param name="controlPoints">The finite control points in traversal order. The input is copied.</param>
        /// <param name="knots">
        /// The finite nondecreasing knot vector, containing control point count plus degree plus one entries.
        /// The active domain is [knots[degree], knots[control point count]] and must have positive width.
        /// Interior multiplicities must not exceed the degree; other multiplicities must not exceed degree plus one.
        /// The input is copied.
        /// </param>
        /// <param name="segmentsPerKnotSpan">
        /// The positive number of approximation segments per non-empty knot span.
        /// </param>
        /// <exception cref="ArgumentNullException">An input collection is null.</exception>
        /// <exception cref="ArgumentException">
        /// Collection sizes or knot ordering, domain, or multiplicities are invalid.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The degree, a point, knot, or subdivision count is invalid, or the approximate length cannot be
        /// represented as a finite float.
        /// </exception>
        public BSpline(
            int degree,
            IReadOnlyList<PointXYZ> controlPoints,
            IReadOnlyList<float> knots,
            int segmentsPerKnotSpan = 64)
        {
            if (controlPoints is null)
                throw new ArgumentNullException(nameof(controlPoints));

            if (knots is null)
                throw new ArgumentNullException(nameof(knots));

            if (degree < 1 || degree >= controlPoints.Count)
                throw new ArgumentOutOfRangeException(
                    nameof(degree),
                    "Degree must be positive and less than the control point count.");

            if (knots.Count != (long)controlPoints.Count + degree + 1)
                throw new ArgumentException(
                    "Knot count must equal control point count plus degree plus one.",
                    nameof(knots));

            if (segmentsPerKnotSpan <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(segmentsPerKnotSpan),
                    "Subdivision count must be positive.");

            Degree = degree;
            SegmentsPerKnotSpan = segmentsPerKnotSpan;
            _controlPoints = new PointXYZ[controlPoints.Count];

            for (int i = 0; i < _controlPoints.Length; i++)
            {
                PointXYZ point = controlPoints[i];
                if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                    float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                    float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                    throw new ArgumentOutOfRangeException(
                        nameof(controlPoints),
                        "Control point coordinates must be finite.");

                _controlPoints[i] = point;
            }

            _knots = SplineEvaluation.CopyAndValidateKnots(knots, degree, _controlPoints.Length);
            ControlPoints = Array.AsReadOnly(_controlPoints);
            Knots = Array.AsReadOnly(_knots);
            _approximation = new SplinePathApproximation(
                SplineEvaluation.CreateApproximation(
                    degree,
                    _controlPoints,
                    weights: null,
                    _knots,
                    segmentsPerKnotSpan));
            double length = _approximation.Length;
            if (length > float.MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(controlPoints),
                    "Approximate curve length must fit in a finite float.");

            Length = (float)length;
        }

        /// <summary>
        /// Gets the spline degree.
        /// </summary>
        public int Degree { get; }

        /// <summary>
        /// Gets a read-only structural view of the copied control point state.
        /// </summary>
        public IReadOnlyList<PointXYZ> ControlPoints { get; }

        /// <summary>
        /// Gets a read-only structural view of the copied knot vector state.
        /// </summary>
        public IReadOnlyList<float> Knots { get; }

        /// <summary>
        /// Gets the start of the active knot parameter domain, in knot units.
        /// </summary>
        public float KnotStart => _knots[Degree];

        /// <summary>
        /// Gets the end of the active knot parameter domain, in knot units.
        /// </summary>
        public float KnotEnd => _knots[_controlPoints.Length];

        /// <summary>
        /// Gets the approximation subdivision count for each non-empty knot span.
        /// </summary>
        public int SegmentsPerKnotSpan { get; }

        /// <summary>
        /// Gets the point at the start of the active knot domain.
        /// </summary>
        public PointXYZ StartPoint => _approximation.StartPoint;

        /// <summary>
        /// Gets the point at the end of the active knot domain.
        /// </summary>
        public PointXYZ EndPoint => _approximation.EndPoint;

        /// <inheritdoc/>
        public PointXYZ EndpointA => StartPoint;

        /// <inheritdoc/>
        public PointXYZ EndpointB => EndPoint;

        /// <summary>
        /// Gets the cached approximate length in world coordinate units.
        /// </summary>
        public float Length { get; }

        /// <summary>
        /// Evaluates the polynomial spline at a normalized parameter.
        /// </summary>
        /// <param name="t">
        /// The finite normalized parameter in [0, 1], mapped linearly to the active knot domain.
        /// </param>
        /// <returns>The point on the polynomial spline, independent of the polyline approximation.</returns>
        public PointXYZ GetPointAt(float t)
        {
            if (float.IsNaN(t) || float.IsInfinity(t) || t < 0f || t > 1f)
                throw new ArgumentOutOfRangeException(
                    nameof(t),
                    "Parameter must be finite and lie within [0, 1].");

            return SplineEvaluation.Evaluate(
                Degree,
                _controlPoints,
                weights: null,
                _knots,
                (1.0 - t) * KnotStart + (double)t * KnotEnd);
        }

        /// <summary>
        /// Evaluates the polynomial spline at a parameter in the original knot units.
        /// </summary>
        /// <param name="knot">
        /// The finite knot parameter in [<see cref="KnotStart"/>, <see cref="KnotEnd"/>].
        /// </param>
        /// <returns>The point on the polynomial spline, including either endpoint of the active domain.</returns>
        public PointXYZ GetPointAtKnot(float knot)
        {
            if (float.IsNaN(knot) || float.IsInfinity(knot) || knot < KnotStart || knot > KnotEnd)
                throw new ArgumentOutOfRangeException(
                    nameof(knot),
                    "Parameter must be finite and lie within the active knot domain.");

            return SplineEvaluation.Evaluate(Degree, _controlPoints, weights: null, _knots, knot);
        }

        /// <summary>
        /// Returns a point on the cached polyline at an approximate distance along the curve.
        /// </summary>
        /// <param name="curveCoordinate">
        /// The finite distance from <see cref="StartPoint"/> in world coordinate units, in the inclusive range
        /// [0, <see cref="Length"/>].
        /// </param>
        /// <returns>The point on the approximation, with exact spline endpoints at zero and length.</returns>
        public PointXYZ GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate) ||
                curveCoordinate < 0f || curveCoordinate > Length)
                throw new ArgumentOutOfRangeException(
                    nameof(curveCoordinate),
                    "Curve coordinate must be finite and lie within the curve length.");

            return _approximation.GetPoint(curveCoordinate);
        }

        /// <summary>
        /// Returns the shortest distance to the cached curve approximation.
        /// </summary>
        /// <param name="point">The finite point to measure from.</param>
        /// <returns>The approximate unsigned distance in world coordinate units.</returns>
        public float Distance(PointXYZ point) => ProjectWithParameter(point).Distance;

        /// <summary>
        /// Projects a point onto the cached curve approximation.
        /// </summary>
        /// <param name="point">The finite point to project.</param>
        /// <returns>The approximate closest point and distance in world coordinate units.</returns>
        public CurveProjection Project(PointXYZ point)
        {
            ParameterizedCurveProjection projection = ProjectWithParameter(point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        /// <summary>
        /// Projects a point onto the cached polyline and reports its approximate length coordinate.
        /// </summary>
        /// <param name="point">The finite point to project.</param>
        /// <returns>
        /// The closest point on the approximation, its coordinate in [0, <see cref="Length"/>], and unsigned
        /// distance, both in world coordinate units. Ties use the earliest coordinate.
        /// </returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXYZ point)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");

            return _approximation.ProjectWithParameter(point);
        }

        /// <summary>
        /// Returns the cached approximation as directed segments, omitting zero-length segments.
        /// </summary>
        /// <returns>A new mutable list of segments owned by the caller.</returns>
#pragma warning disable MA0016 // The concrete mutable collection communicates caller ownership.
        public List<ParameterizedSegment> Flatten()
        {
            return _approximation.Flatten();
        }
#pragma warning restore MA0016
    }
}
