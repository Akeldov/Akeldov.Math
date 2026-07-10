using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents a closed two-dimensional contour made from finite paths.
    /// </summary>
    public sealed class CompositeContour : ICompositeContour
    {
        private readonly IFinitePath[] _curves;
        private readonly IReadOnlyList<IFinitePath> _readOnlyCurves;
        private readonly float _length;

        /// <summary>
        /// Initializes a new composite contour from the specified finite paths.
        /// </summary>
        /// <param name="curves">The finite paths that form the contour.</param>
        public CompositeContour(IReadOnlyList<IFinitePath> curves)
        {
            if (curves == null)
                throw new ArgumentNullException(nameof(curves));

            if (curves.Count == 0)
                throw new ArgumentException("A contour must contain at least one curve.", nameof(curves));

            _curves = new IFinitePath[curves.Count];

            for (int i = 0; i < curves.Count; i++)
            {
                _curves[i] = curves[i] ?? throw new ArgumentException("A contour cannot contain null curves.", nameof(curves));
            }

            ValidateCurvesFormClosedChain(_curves, nameof(curves));
            _length = GetLength(_curves, nameof(curves));

            _readOnlyCurves = Array.AsReadOnly(_curves);
        }

        /// <summary>
        /// Initializes a new composite contour from the specified points by connecting consecutive points with segments.
        /// </summary>
        /// <param name="points">The contour vertices in boundary order. The last point may repeat the first point to close the contour explicitly.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when fewer than three contour vertices are provided or adjacent vertices are equal.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a point has a NaN or infinite coordinate.</exception>
        public CompositeContour(IReadOnlyList<PointXY> points)
            : this(CreateCurvesFromPoints(points, nameof(points)))
        {
        }

        /// <summary>
        /// Initializes a new composite contour from the specified points by connecting consecutive points with segments.
        /// </summary>
        /// <param name="points">The contour vertices in boundary order. The last point may repeat the first point to close the contour explicitly.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when fewer than three contour vertices are provided or adjacent vertices are equal.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a point has a NaN or infinite coordinate.</exception>
        public CompositeContour(params PointXY[] points)
            : this((IReadOnlyList<PointXY>)(points ?? throw new ArgumentNullException(nameof(points))))
        {
        }

        /// <summary>
        /// Gets the read-only structural view of the finite paths that form this contour.
        /// </summary>
        public IReadOnlyList<IFinitePath> Curves => _readOnlyCurves;

        /// <summary>
        /// Gets the finite non-negative contour boundary length in world coordinate units.
        /// </summary>
        public float Length => _length;

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            int count = 0;
            for (int i = 0; i < _curves.Length; i++)
                count += _curves[i].CountRightwardCrossings(origin);

            return count;
        }

        /// <inheritdoc/>
        public bool Encloses(PointXY point, float geometryEpsilon = 1E-06F)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            var ray = new Ray(point);
            int intersectionCount = 0;

            for (int i = 0; i < _curves.Length; i++)
            {
                IFinitePath curve = _curves[i];

                if (curve.Distance(point) <= geometryEpsilon)
                    return true;

                List<PointXY> curveIntersections = curve.GetRayIntersections(ray, geometryEpsilon);
                if (curveIntersections == null)
                    continue;

                for (int j = 0; j < curveIntersections.Count; j++)
                {
                    PointXY intersection = curveIntersections[j];
                    if (ShouldCountRayIntersection(curve, point, intersection, geometryEpsilon))
                        intersectionCount++;
                }
            }

            return intersectionCount % 2 == 1;
        }

        /// <inheritdoc/>
        public List<PointXY> GetRayIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            var intersections = new List<PointXY>();

            for (int i = 0; i < _curves.Length; i++)
            {
                List<PointXY> curveIntersections = _curves[i].GetRayIntersections(ray, geometryEpsilon);
                if (curveIntersections == null)
                    continue;

                for (int j = 0; j < curveIntersections.Count; j++)
                {
                    AddDistinct(intersections, curveIntersections[j], geometryEpsilon);
                }
            }

            return intersections;
        }

        /// <inheritdoc/>
        public CurveProjection Project(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            CurveProjection closestProjection = _curves[0].Project(point);

            for (int i = 1; i < _curves.Length; i++)
            {
                CurveProjection projection = _curves[i].Project(point);
                if (projection.Distance < closestProjection.Distance)
                    closestProjection = projection;
            }

            return closestProjection;
        }

        /// <inheritdoc/>
        public float Distance(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            float minDistance = float.MaxValue;

            for (int i = 0; i < _curves.Length; i++)
            {
                float distance = _curves[i].Distance(point);
                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }

        /// <inheritdoc/>
        public float SignedDistance(PointXY point, float geometryEpsilon = 1E-06F)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            float distance = Distance(point);
            return Encloses(point, geometryEpsilon) ? -distance : distance;
        }

        private static IFinitePath[] CreateCurvesFromPoints(IReadOnlyList<PointXY> points, string parameterName)
        {
            if (points == null)
                throw new ArgumentNullException(parameterName);

            if (points.Count < 3)
                throw new ArgumentException("CompositeContour point contour must contain at least three vertices.", parameterName);

            for (int i = 0; i < points.Count; i++)
            {
                PointXYValidation.ThrowIfNotFinite(
                    points[i],
                    parameterName,
                    "CompositeContour point coordinates must be finite.");
            }

            bool isExplicitlyClosed = points[0].Equals(points[points.Count - 1]);
            int vertexCount = isExplicitlyClosed ? points.Count - 1 : points.Count;

            if (vertexCount < 3)
                throw new ArgumentException("CompositeContour point contour must contain at least three distinct vertices.", parameterName);

            var curves = new IFinitePath[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                PointXY startPoint = points[i];
                PointXY endPoint = points[(i + 1) % vertexCount];

                if (startPoint.Equals(endPoint))
                    throw new ArgumentException("CompositeContour adjacent points must be distinct.", parameterName);

                curves[i] = new ParameterizedSegment(startPoint, endPoint);
            }

            return curves;
        }

        private static void ValidateCurvesFormClosedChain(IReadOnlyList<IFinitePath> curves, string parameterName)
        {
            for (int i = 0; i < curves.Count; i++)
            {
                IFinitePath currentCurve = curves[i];
                IFinitePath nextCurve = curves[(i + 1) % curves.Count];

                if (!currentCurve.EndPoint.AlmostEquals(nextCurve.StartPoint))
                    throw new ArgumentException("CompositeContour curves must form a closed continuous chain.", parameterName);
            }
        }

        private static float GetLength(IReadOnlyList<IFinitePath> curves, string parameterName)
        {
            float length = 0f;

            for (int i = 0; i < curves.Count; i++)
            {
                float curveLength = curves[i].Length;
                if (curveLength < 0f || float.IsNaN(curveLength) || float.IsInfinity(curveLength))
                    throw new ArgumentException("CompositeContour curves must expose finite non-negative lengths.", parameterName);

                length += curveLength;
                if (float.IsInfinity(length))
                    throw new ArgumentException("CompositeContour length must be finite.", parameterName);
            }

            return length;
        }

        private static void AddDistinct(List<PointXY> intersections, PointXY point, float geometryEpsilon)
        {
            for (int i = 0; i < intersections.Count; i++)
            {
                if (intersections[i].AlmostEquals(point, geometryEpsilon))
                    return;
            }

            intersections.Add(point);
        }

        private static bool ShouldCountRayIntersection(
            IFinitePath curve,
            PointXY rayOrigin,
            PointXY intersection,
            float geometryEpsilon)
        {
            if (intersection.X <= rayOrigin.X + geometryEpsilon)
                return false;

            if (IsHorizontalSegmentOnScanline(curve, rayOrigin.Y, geometryEpsilon))
                return false;

            if (curve.StartPoint.AlmostEquals(curve.EndPoint, geometryEpsilon))
                return true;

            if (intersection.AlmostEquals(curve.StartPoint, geometryEpsilon))
                return curve.EndPoint.Y > curve.StartPoint.Y + geometryEpsilon;

            if (intersection.AlmostEquals(curve.EndPoint, geometryEpsilon))
                return curve.StartPoint.Y > curve.EndPoint.Y + geometryEpsilon;

            return true;
        }

        private static bool IsHorizontalSegmentOnScanline(
            IFinitePath curve,
            float scanlineY,
            float geometryEpsilon)
        {
            return curve is ParameterizedSegment &&
                curve.StartPoint.Y.AlmostEquals(scanlineY, geometryEpsilon) &&
                curve.EndPoint.Y.AlmostEquals(scanlineY, geometryEpsilon);
        }
    }
}
