using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a finite directed non-uniform rational B-spline (NURBS) curve.
    /// </summary>
    /// <remarks>
    /// <para>Supports positive weights, degree one or higher, and clamped or unclamped knot
    /// vectors. Interior knot multiplicity cannot exceed the degree: the path must be continuous.</para>
    /// <para><see cref="GetPointAt"/> and <see cref="GetPointAtKnot"/> evaluate the rational
    /// spline using de Boor's algorithm. Length, distance, projection, length-coordinate
    /// traversal and crossing queries use a cached polyline with <see cref="SegmentsPerKnotSpan"/>
    /// equal parameter subdivisions of each non-empty knot span. This is an approximation,
    /// not an error bound; increase the subdivision count for sharp bends or extreme weights.</para>
    /// </remarks>
    public sealed class Nurbs : IContourPath
    {
        private readonly PointXY[] _controlPoints;
        private readonly float[] _weights;
        private readonly float[] _knots;
        private readonly PointXY[] _approximation;
        private readonly double[] _coordinates;

        /// <summary>
        /// Initializes a NURBS curve from copied control points, weights and knots.
        /// </summary>
        /// <param name="degree">The degree, at least one and less than the control point count.</param>
        /// <param name="controlPoints">The finite control points in traversal order. The input is copied.</param>
        /// <param name="weights">One finite, strictly positive weight per control point. The input is copied.</param>
        /// <param name="knots">The finite nondecreasing knot vector, containing control point count
        /// plus degree plus one entries. The active domain is [knots[degree], knots[control point count]]
        /// and must have positive width. Interior multiplicities must not exceed the degree;
        /// other multiplicities must not exceed degree plus one. The input is copied.</param>
        /// <param name="segmentsPerKnotSpan">The positive number of approximation segments per non-empty knot span.</param>
        /// <exception cref="ArgumentNullException">An input collection is null.</exception>
        /// <exception cref="ArgumentException">Collection sizes or knot ordering, domain or multiplicities are invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The degree, a point, weight, knot or subdivision count
        /// is invalid, or the approximate length cannot be represented as a finite float.</exception>
        public Nurbs(
            int degree,
            IReadOnlyList<PointXY> controlPoints,
            IReadOnlyList<float> weights,
            IReadOnlyList<float> knots,
            int segmentsPerKnotSpan = 64)
        {
            if (controlPoints is null)
                throw new ArgumentNullException(nameof(controlPoints));
            if (weights is null)
                throw new ArgumentNullException(nameof(weights));
            if (knots is null)
                throw new ArgumentNullException(nameof(knots));
            if (degree < 1 || degree >= controlPoints.Count)
                throw new ArgumentOutOfRangeException(nameof(degree), "Degree must be positive and less than the control point count.");
            if (weights.Count != controlPoints.Count)
                throw new ArgumentException("Each control point must have one weight.", nameof(weights));
            if (knots.Count != (long)controlPoints.Count + degree + 1)
                throw new ArgumentException("Knot count must equal control point count plus degree plus one.", nameof(knots));
            if (segmentsPerKnotSpan <= 0)
                throw new ArgumentOutOfRangeException(nameof(segmentsPerKnotSpan), "Subdivision count must be positive.");

            Degree = degree;
            SegmentsPerKnotSpan = segmentsPerKnotSpan;
            _controlPoints = new PointXY[controlPoints.Count];
            _weights = new float[weights.Count];

            for (int i = 0; i < _controlPoints.Length; i++)
            {
                PointXY point = controlPoints[i];
                PointXYValidation.ThrowIfNotFinite(point, nameof(controlPoints), "Control point coordinates must be finite.");
                float weight = weights[i];
                if (float.IsNaN(weight) || float.IsInfinity(weight) || weight <= 0f)
                    throw new ArgumentOutOfRangeException(nameof(weights), "Weights must be finite and strictly positive.");

                _controlPoints[i] = point;
                _weights[i] = weight;
            }

            _knots = CopyAndValidateKnots(knots, degree, _controlPoints.Length);
            ControlPoints = Array.AsReadOnly(_controlPoints);
            Weights = Array.AsReadOnly(_weights);
            Knots = Array.AsReadOnly(_knots);
            _approximation = CreateApproximation(segmentsPerKnotSpan);
            _coordinates = new double[_approximation.Length];
            for (int i = 1; i < _approximation.Length; i++)
            {
                double dx = (double)_approximation[i].X - _approximation[i - 1].X;
                double dy = (double)_approximation[i].Y - _approximation[i - 1].Y;
                _coordinates[i] = _coordinates[i - 1] + System.Math.Sqrt(dx * dx + dy * dy);
            }

            double length = _coordinates[_coordinates.Length - 1];
            if (length > float.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(controlPoints), "Approximate curve length must fit in a finite float.");

            Length = (float)length;
        }

        /// <summary>Gets the spline degree.</summary>
        public int Degree { get; }

        /// <summary>Gets a read-only view of the copied control point state.</summary>
        public IReadOnlyList<PointXY> ControlPoints { get; }

        /// <summary>Gets a read-only view of the copied weight state.</summary>
        public IReadOnlyList<float> Weights { get; }

        /// <summary>Gets a read-only view of the copied knot vector state.</summary>
        public IReadOnlyList<float> Knots { get; }

        /// <summary>Gets the start of the active knot parameter domain, in knot units.</summary>
        public float KnotStart => _knots[Degree];

        /// <summary>Gets the end of the active knot parameter domain, in knot units.</summary>
        public float KnotEnd => _knots[_controlPoints.Length];

        /// <summary>Gets the approximation subdivision count for each non-empty knot span.</summary>
        public int SegmentsPerKnotSpan { get; }

        /// <summary>Gets the point at the start of the active knot domain.</summary>
        public PointXY StartPoint => _approximation[0];

        /// <summary>Gets the point at the end of the active knot domain.</summary>
        public PointXY EndPoint => _approximation[_approximation.Length - 1];

        /// <inheritdoc/>
        public PointXY EndpointA => StartPoint;

        /// <inheritdoc/>
        public PointXY EndpointB => EndPoint;

        /// <summary>Gets the cached approximate length in world coordinate units.</summary>
        public float Length { get; }

        /// <summary>Evaluates the rational spline at a normalized parameter.</summary>
        /// <param name="t">The finite normalized parameter in [0, 1], mapped linearly to the active knot domain.</param>
        /// <returns>The point on the rational spline, independent of the polyline approximation.</returns>
        public PointXY GetPointAt(float t)
        {
            if (float.IsNaN(t) || float.IsInfinity(t) || t < 0f || t > 1f)
                throw new ArgumentOutOfRangeException(nameof(t), "Parameter must be finite and lie within [0, 1].");

            return Evaluate((1.0 - t) * KnotStart + (double)t * KnotEnd);
        }

        /// <summary>Evaluates the rational spline at a parameter in the original knot units.</summary>
        /// <param name="knot">The finite knot parameter in [<see cref="KnotStart"/>, <see cref="KnotEnd"/>].</param>
        /// <returns>The point on the rational spline, including either endpoint of the active domain.</returns>
        public PointXY GetPointAtKnot(float knot)
        {
            if (float.IsNaN(knot) || float.IsInfinity(knot) || knot < KnotStart || knot > KnotEnd)
                throw new ArgumentOutOfRangeException(nameof(knot), "Parameter must be finite and lie within the active knot domain.");

            return Evaluate(knot);
        }

        /// <summary>Returns a point on the cached polyline at an approximate distance along the curve.</summary>
        /// <param name="curveCoordinate">The finite distance from <see cref="StartPoint"/> in world coordinate units,
        /// in the inclusive range [0, <see cref="Length"/>].</param>
        /// <returns>The point on the approximation, with exact spline endpoints at zero and length.</returns>
        public PointXY GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate) || curveCoordinate < 0f || curveCoordinate > Length)
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite and lie within the curve length.");

            if (curveCoordinate == 0f)
                return StartPoint;
            if (curveCoordinate == Length)
                return EndPoint;

            int low = 0;
            int high = _coordinates.Length - 1;
            while (high - low > 1)
            {
                int middle = low + (high - low) / 2;
                if (_coordinates[middle] <= curveCoordinate)
                    low = middle;
                else
                    high = middle;
            }

            double amount = (curveCoordinate - _coordinates[low]) / (_coordinates[high] - _coordinates[low]);
            PointXY start = _approximation[low];
            PointXY end = _approximation[high];
            return new PointXY(
                (float)((1.0 - amount) * start.X + amount * end.X),
                (float)((1.0 - amount) * start.Y + amount * end.Y));
        }

        /// <summary>Returns the shortest distance to the cached curve approximation.</summary>
        /// <param name="point">The finite point to measure from.</param>
        /// <returns>The approximate unsigned distance in world coordinate units.</returns>
        public float Distance(PointXY point) => ProjectWithParameter(point).Distance;

        /// <summary>Projects a point onto the cached curve approximation.</summary>
        /// <param name="point">The finite point to project.</param>
        /// <returns>The approximate closest point and distance in world coordinate units.</returns>
        public CurveProjection Project(PointXY point)
        {
            ParameterizedCurveProjection projection = ProjectWithParameter(point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        /// <summary>Projects a point onto the cached polyline and reports its approximate length coordinate.</summary>
        /// <param name="point">The finite point to project.</param>
        /// <returns>The closest point on the approximation, its coordinate in [0, <see cref="Length"/>]
        /// and unsigned distance, both in world coordinate units. Ties use the earliest coordinate.</returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(point, nameof(point), "Point coordinates must be finite.");

            PointXY closestPoint = StartPoint;
            double closestSquaredDistance = double.PositiveInfinity;
            double closestCoordinate = 0.0;
            for (int i = 1; i < _approximation.Length; i++)
            {
                PointXY start = _approximation[i - 1];
                PointXY end = _approximation[i];
                double dx = (double)end.X - start.X;
                double dy = (double)end.Y - start.Y;
                double px = (double)point.X - start.X;
                double py = (double)point.Y - start.Y;
                double squaredLength = dx * dx + dy * dy;
                double amount = squaredLength == 0.0 ? 0.0 : (px * dx + py * dy) / squaredLength;
                amount = System.Math.Max(0.0, System.Math.Min(1.0, amount));
                double x = (1.0 - amount) * start.X + amount * end.X;
                double y = (1.0 - amount) * start.Y + amount * end.Y;
                double distanceX = point.X - x;
                double distanceY = point.Y - y;
                double squaredDistance = distanceX * distanceX + distanceY * distanceY;
                if (squaredDistance < closestSquaredDistance)
                {
                    closestSquaredDistance = squaredDistance;
                    closestPoint = new PointXY((float)x, (float)y);
                    closestCoordinate = _coordinates[i - 1] + amount * (_coordinates[i] - _coordinates[i - 1]);
                }
            }

            double distance = System.Math.Sqrt(closestSquaredDistance);
            if (distance > float.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(point), "Projection distance must fit in a finite float.");

            return new ParameterizedCurveProjection(closestPoint, (float)closestCoordinate, (float)distance);
        }

        /// <summary>Counts rightward scanline crossings of the cached polyline using half-open segment endpoints.</summary>
        /// <param name="origin">The finite scanline origin.</param>
        /// <returns>The approximate crossing count for contour fill queries.</returns>
        public int CountRightwardCrossings(PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            int count = 0;
            for (int i = 1; i < _approximation.Length; i++)
            {
                PointXY start = _approximation[i - 1];
                PointXY end = _approximation[i];
                if ((start.Y <= origin.Y && origin.Y < end.Y) || (end.Y <= origin.Y && origin.Y < start.Y))
                {
                    double x = start.X + ((double)origin.Y - start.Y) * ((double)end.X - start.X) / ((double)end.Y - start.Y);
                    if (x > origin.X)
                        count++;
                }
            }

            return count;
        }

        /// <summary>Returns the cached approximation as directed segments, omitting zero-length segments.</summary>
        /// <returns>A new mutable list of segments owned by the caller.</returns>
        public List<ParameterizedSegment> Flatten()
        {
            var segments = new List<ParameterizedSegment>(_approximation.Length - 1);
            for (int i = 1; i < _approximation.Length; i++)
            {
                if (!_approximation[i - 1].Equals(_approximation[i]))
                    segments.Add(new ParameterizedSegment(_approximation[i - 1], _approximation[i]));
            }

            return segments;
        }

        private static float[] CopyAndValidateKnots(IReadOnlyList<float> knots, int degree, int controlPointCount)
        {
            var copy = new float[knots.Count];
            for (int i = 0; i < copy.Length; i++)
            {
                float knot = knots[i];
                if (float.IsNaN(knot) || float.IsInfinity(knot))
                    throw new ArgumentOutOfRangeException(nameof(knots), "Knots must be finite.");
                if (i > 0 && knot < copy[i - 1])
                    throw new ArgumentException("Knots must be nondecreasing.", nameof(knots));

                copy[i] = knot;
            }

            float start = copy[degree];
            float end = copy[controlPointCount];
            if (start >= end)
                throw new ArgumentException("The active knot domain must have positive width.", nameof(knots));

            int multiplicity = 1;
            for (int i = 1; i < copy.Length; i++)
            {
                multiplicity = copy[i] == copy[i - 1] ? multiplicity + 1 : 1;
                bool interior = copy[i] > start && copy[i] < end;
                if (multiplicity > (interior ? degree : degree + 1))
                    throw new ArgumentException("Knot multiplicity exceeds the continuous path limit.", nameof(knots));
            }

            return copy;
        }

        private PointXY[] CreateApproximation(int segmentsPerKnotSpan)
        {
            int spanCount = 0;
            for (int span = Degree; span < _controlPoints.Length; span++)
            {
                if (_knots[span] < _knots[span + 1])
                    spanCount++;
            }

            long pointCount = (long)spanCount * segmentsPerKnotSpan + 1;
            if (pointCount > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(segmentsPerKnotSpan), "The approximation contains too many points.");

            var points = new PointXY[(int)pointCount];
            Span<HomogeneousPoint> work = Degree < 32
                ? stackalloc HomogeneousPoint[Degree + 1]
                : new HomogeneousPoint[Degree + 1];
            int index = 0;
            for (int span = Degree; span < _controlPoints.Length; span++)
            {
                double from = _knots[span];
                double to = _knots[span + 1];
                if (from == to)
                    continue;

                if (index == 0)
                    points[index++] = Evaluate(from, span, work);
                for (int step = 1; step <= segmentsPerKnotSpan; step++)
                {
                    double amount = step / (double)segmentsPerKnotSpan;
                    points[index++] = Evaluate((1.0 - amount) * from + amount * to, span, work);
                }
            }

            return points;
        }

        private PointXY Evaluate(double knot)
        {
            int low = Degree;
            int high = _controlPoints.Length;
            if (knot == KnotEnd)
            {
                low = high - 1;
                while (_knots[low] == KnotEnd)
                    low--;
            }
            else
            {
                while (high - low > 1)
                {
                    int middle = low + (high - low) / 2;
                    if (_knots[middle] <= knot)
                        low = middle;
                    else
                        high = middle;
                }
            }

            Span<HomogeneousPoint> work = Degree < 32
                ? stackalloc HomogeneousPoint[Degree + 1]
                : new HomogeneousPoint[Degree + 1];
            return Evaluate(knot, low, work);
        }

        private PointXY Evaluate(double knot, int span, Span<HomogeneousPoint> work)
        {
            // Apply de Boor in homogeneous coordinates (w*x, w*y, w), then project to XY.
            // Double intermediates keep products of finite float coordinates and weights finite.
            for (int j = 0; j <= Degree; j++)
            {
                int index = span - Degree + j;
                double weight = _weights[index];
                work[j] = new HomogeneousPoint(_controlPoints[index].X * weight, _controlPoints[index].Y * weight, weight);
            }

            for (int level = 1; level <= Degree; level++)
            {
                for (int j = Degree; j >= level; j--)
                {
                    int index = span - Degree + j;
                    double from = _knots[index];
                    double to = _knots[index + Degree - level + 1];
                    double amount = (knot - from) / (to - from);
                    HomogeneousPoint previous = work[j - 1];
                    HomogeneousPoint current = work[j];
                    work[j] = new HomogeneousPoint(
                        (1.0 - amount) * previous.X + amount * current.X,
                        (1.0 - amount) * previous.Y + amount * current.Y,
                        (1.0 - amount) * previous.Weight + amount * current.Weight);
                }
            }

            HomogeneousPoint result = work[Degree];
            return new PointXY((float)(result.X / result.Weight), (float)(result.Y / result.Weight));
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct HomogeneousPoint
        {
            public HomogeneousPoint(double x, double y, double weight)
            {
                X = x;
                Y = y;
                Weight = weight;
            }

            public double X { get; }
            public double Y { get; }
            public double Weight { get; }
        }
    }
}
