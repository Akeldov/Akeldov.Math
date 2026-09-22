using System;
using System.Collections.Generic;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents an open finite path made from consecutive directed line segments.
    /// </summary>
    public sealed class ParameterizedSegmentChain : IFinitePath
    {
        private readonly PointXYZ[] _points;
        private readonly ParameterizedSegment[] _segments;
        private readonly double[] _segmentStartCoordinates;
        private readonly IReadOnlyList<PointXYZ> _readOnlyPoints;
        private readonly IReadOnlyList<ParameterizedSegment> _readOnlySegments;
        private readonly float _length;

        /// <summary>
        /// Initializes a new parameterized segment chain from the specified points.
        /// </summary>
        /// <param name="points">The points that define consecutive segments in traversal order.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when fewer than two points are provided, adjacent points are equal, or the resulting length is not finite and positive.
        /// </exception>
        public ParameterizedSegmentChain(params PointXYZ[] points)
            : this((IReadOnlyList<PointXYZ>)(points ?? throw new ArgumentNullException(nameof(points))))
        {
        }

        /// <summary>
        /// Initializes a new parameterized segment chain from the specified points.
        /// </summary>
        /// <param name="points">The points that define consecutive segments in traversal order.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when fewer than two points are provided, adjacent points are equal, or the resulting length is not finite and positive.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when a point has a NaN or infinite coordinate.
        /// </exception>
        public ParameterizedSegmentChain(IReadOnlyList<PointXYZ> points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            if (points.Count < 2)
                throw new ArgumentException("ParameterizedSegmentChain must contain at least two points.", nameof(points));

            _points = new PointXYZ[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                PointXYZ point = points[i];
                if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                    float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                    float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                    throw new ArgumentOutOfRangeException(
                        nameof(points),
                        "ParameterizedSegmentChain point coordinates must be finite.");

                if (i > 0 && point.Equals(_points[i - 1]))
                    throw new ArgumentException(
                        "ParameterizedSegmentChain adjacent points must be distinct.",
                        nameof(points));

                _points[i] = point;
            }

            _segments = new ParameterizedSegment[_points.Length - 1];
            _segmentStartCoordinates = new double[_segments.Length];

            double length = 0d;
            for (int i = 0; i < _segments.Length; i++)
            {
                _segmentStartCoordinates[i] = length;

                var segment = new ParameterizedSegment(_points[i], _points[i + 1]);
                float segmentLength = segment.Length;
                if (segmentLength <= 0f || float.IsNaN(segmentLength) || float.IsInfinity(segmentLength))
                    throw new ArgumentException(
                        "ParameterizedSegmentChain segments must have finite positive lengths.",
                        nameof(points));

                _segments[i] = segment;
                length += segmentLength;

                if (length > float.MaxValue)
                    throw new ArgumentException("ParameterizedSegmentChain length must be finite.", nameof(points));
            }

            _length = (float)length;
            _readOnlyPoints = Array.AsReadOnly(_points);
            _readOnlySegments = Array.AsReadOnly(_segments);
        }

        /// <summary>
        /// Gets the read-only structural view of the copied points that define this chain.
        /// </summary>
        public IReadOnlyList<PointXYZ> Points => _readOnlyPoints;

        /// <summary>
        /// Gets the read-only structural view of the generated directed segments that define this chain.
        /// </summary>
        public IReadOnlyList<ParameterizedSegment> Segments => _readOnlySegments;

        /// <summary>
        /// Gets the point at the start of the traversal direction.
        /// </summary>
        public PointXYZ StartPoint => _points[0];

        /// <summary>
        /// Gets the point at the end of the traversal direction.
        /// </summary>
        public PointXYZ EndPoint => _points[_points.Length - 1];

        /// <summary>
        /// Gets the endpoint at the start of the traversal direction.
        /// </summary>
        public PointXYZ EndpointA => StartPoint;

        /// <summary>
        /// Gets the endpoint at the end of the traversal direction.
        /// </summary>
        public PointXYZ EndpointB => EndPoint;

        /// <summary>
        /// Gets the finite positive chain length in world coordinate units.
        /// </summary>
        public float Length => _length;

        /// <summary>
        /// Returns the shortest distance from the specified point to this chain.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this chain.</returns>
        public float Distance(PointXYZ point)
        {
            return ProjectWithParameter(point).Distance;
        }

        /// <summary>
        /// Projects the specified point onto this chain.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point and distance to this chain.</returns>
        public CurveProjection Project(PointXYZ point)
        {
            ParameterizedCurveProjection projection = ProjectWithParameter(point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        /// <summary>
        /// Projects the specified point onto this chain and reports the chain length coordinate.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point, chain length coordinate, and distance to this chain.</returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXYZ point)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");

            ParameterizedCurveProjection closestProjection = ProjectOnSegment(0, point);

            for (int i = 1; i < _segments.Length; i++)
            {
                ParameterizedCurveProjection projection = ProjectOnSegment(i, point);
                if (projection.Distance < closestProjection.Distance)
                    closestProjection = projection;
            }

            return closestProjection;
        }

        /// <summary>
        /// Returns the point at the specified chain length coordinate.
        /// </summary>
        /// <param name="curveCoordinate">
        /// The finite curve coordinate in world coordinate units in the closed range from zero to <see cref="Length"/>.
        /// </param>
        /// <returns>The point on this chain.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="curveCoordinate"/> is NaN, infinite, negative, or greater than <see cref="Length"/>.
        /// </exception>
        public PointXYZ GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            if (curveCoordinate < 0f || curveCoordinate > Length)
                throw new ArgumentOutOfRangeException(
                    nameof(curveCoordinate),
                    "Curve coordinate must lie within the chain length.");

            if (curveCoordinate == Length)
                return EndPoint;

            for (int i = 0; i < _segments.Length; i++)
            {
                ParameterizedSegment segment = _segments[i];
                double localCoordinate = curveCoordinate - _segmentStartCoordinates[i];

                if (localCoordinate <= segment.Length || i == _segments.Length - 1)
                {
                    if (localCoordinate <= 0d)
                        return segment.StartPoint;

                    if (localCoordinate >= segment.Length)
                        return segment.EndPoint;

                    return segment.GetPoint((float)localCoordinate);
                }
            }

            return EndPoint;
        }

        private ParameterizedCurveProjection ProjectOnSegment(int segmentIndex, PointXYZ point)
        {
            ParameterizedCurveProjection projection = _segments[segmentIndex].ProjectWithParameter(point);
            float chainCoordinate = (float)(
                _segmentStartCoordinates[segmentIndex] + projection.CurveCoordinate);

            return new ParameterizedCurveProjection(
                projection.ProjectedPoint,
                chainCoordinate,
                projection.Distance);
        }
    }
}
