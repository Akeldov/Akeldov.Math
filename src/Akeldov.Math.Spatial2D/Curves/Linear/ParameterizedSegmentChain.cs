using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents an open finite path made from consecutive directed line segments.
    /// </summary>
    public sealed class ParameterizedSegmentChain : IContourPath, ISignedParameterizedCurve
    {
        private readonly PointXY[] _points;
        private readonly ParameterizedSegment[] _segments;
        private readonly float[] _segmentStartCoordinates;
        private readonly IReadOnlyList<PointXY> _readOnlyPoints;
        private readonly IReadOnlyList<ParameterizedSegment> _readOnlySegments;
        private readonly float _length;

        /// <summary>
        /// Initializes a new parameterized segment chain from the specified points.
        /// </summary>
        /// <param name="points">The points that define consecutive segments in traversal order.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when fewer than two points are provided, adjacent points are equal, or the resulting length is not finite and positive.</exception>
        public ParameterizedSegmentChain(params PointXY[] points)
            : this((IReadOnlyList<PointXY>)(points ?? throw new ArgumentNullException(nameof(points))))
        {
        }

        /// <summary>
        /// Initializes a new parameterized segment chain from the specified points.
        /// </summary>
        /// <param name="points">The points that define consecutive segments in traversal order.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when fewer than two points are provided, adjacent points are equal, or the resulting length is not finite and positive.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a point has a NaN or infinite coordinate.</exception>
        public ParameterizedSegmentChain(IReadOnlyList<PointXY> points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            if (points.Count < 2)
                throw new ArgumentException("ParameterizedSegmentChain must contain at least two points.", nameof(points));

            _points = new PointXY[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                PointXY point = points[i];
                PointXYValidation.ThrowIfNotFinite(
                    point,
                    nameof(points),
                    "ParameterizedSegmentChain point coordinates must be finite.");

                if (i > 0 && point.Equals(_points[i - 1]))
                    throw new ArgumentException("ParameterizedSegmentChain adjacent points must be distinct.", nameof(points));

                _points[i] = point;
            }

            _segments = new ParameterizedSegment[_points.Length - 1];
            _segmentStartCoordinates = new float[_segments.Length];

            float length = 0f;
            for (int i = 0; i < _segments.Length; i++)
            {
                _segmentStartCoordinates[i] = length;

                var segment = new ParameterizedSegment(_points[i], _points[i + 1]);
                float segmentLength = segment.Length;
                if (segmentLength <= 0f || float.IsNaN(segmentLength) || float.IsInfinity(segmentLength))
                    throw new ArgumentException("ParameterizedSegmentChain segments must have finite positive lengths.", nameof(points));

                _segments[i] = segment;
                length += segmentLength;

                if (float.IsInfinity(length))
                    throw new ArgumentException("ParameterizedSegmentChain length must be finite.", nameof(points));
            }

            _length = length;
            _readOnlyPoints = Array.AsReadOnly(_points);
            _readOnlySegments = Array.AsReadOnly(_segments);
        }

        /// <summary>
        /// Gets the read-only structural view of the copied points that define this chain.
        /// </summary>
        public IReadOnlyList<PointXY> Points => _readOnlyPoints;

        /// <summary>
        /// Gets the read-only structural view of the generated directed segments that define this chain.
        /// </summary>
        public IReadOnlyList<ParameterizedSegment> Segments => _readOnlySegments;

        /// <summary>
        /// Gets the point at the start of the traversal direction.
        /// </summary>
        public PointXY StartPoint => _points[0];

        /// <summary>
        /// Gets the point at the end of the traversal direction.
        /// </summary>
        public PointXY EndPoint => _points[_points.Length - 1];

        /// <summary>
        /// Gets the endpoint at the start of the traversal direction.
        /// </summary>
        public PointXY EndpointA => StartPoint;

        /// <summary>
        /// Gets the endpoint at the end of the traversal direction.
        /// </summary>
        public PointXY EndpointB => EndPoint;

        /// <summary>
        /// Gets the finite positive chain length in world coordinate units.
        /// </summary>
        public float Length => _length;

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            int count = 0;
            for (int i = 0; i < _segments.Length; i++)
                count += _segments[i].CountRightwardCrossings(origin);

            return count;
        }

        /// <summary>
        /// Returns the shortest distance from the specified point to this chain.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this chain.</returns>
        public float Distance(PointXY point)
        {
            return Project(point).Distance;
        }

        /// <summary>Returns the distance signed by the local directed side of this chain.</summary>
        /// <param name="point">The finite point to measure from.</param>
        /// <returns>Negative on the left, positive on the right, and zero on the chain, in world units.</returns>
        /// <remarks>
        /// Equal-distance projections select the first segment in traversal order, including at self-intersections.
        /// At a shared vertex the sign uses the sum of the adjacent unit left normals. An exact 180-degree
        /// reversal falls back to the selected segment's side. Collinear points beyond an open endpoint
        /// have positive distance, as do points on the zero side of the averaged normal. The sign describes a local side,
        /// not containment, and can jump at nonzero distance when the nearest segment changes or across endpoint extensions.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The point has a non-finite coordinate.</exception>
        public float SignedDistance(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(point, nameof(point), "Point coordinates must be finite.");
            var projection = FindNearestProjection(point, out int segmentIndex);
            if (projection.Distance == 0f)
                return 0f;

            var start = _points[segmentIndex];
            var end = _points[segmentIndex + 1];
            double cross = ((double)end.X - start.X) * ((double)point.Y - start.Y)
                - ((double)end.Y - start.Y) * ((double)point.X - start.X);

            var direction = end - start;
            float localParameter = VectorXY.Dot(point - start, direction) / direction.SquaredLength;
            int vertexIndex = localParameter <= 0f && segmentIndex > 0
                ? segmentIndex
                : localParameter >= 1f && segmentIndex + 1 < _segments.Length
                    ? segmentIndex + 1
                    : -1;
            if (vertexIndex >= 0)
            {
                var previous = _points[vertexIndex - 1];
                var vertex = _points[vertexIndex];
                var next = _points[vertexIndex + 1];
                double incomingX = (double)vertex.X - previous.X;
                double incomingY = (double)vertex.Y - previous.Y;
                double outgoingX = (double)next.X - vertex.X;
                double outgoingY = (double)next.Y - vertex.Y;
                double turn = incomingX * outgoingY - incomingY * outgoingX;
                double dot = incomingX * outgoingX + incomingY * outgoingY;
                // At an exact reversal the unit normals cancel and have no preferred side.
                if (turn != 0 || dot >= 0)
                {
                    double incomingLength = System.Math.Sqrt(incomingX * incomingX + incomingY * incomingY);
                    double outgoingLength = System.Math.Sqrt(outgoingX * outgoingX + outgoingY * outgoingY);
                    double normalX = -incomingY / incomingLength - outgoingY / outgoingLength;
                    double normalY = incomingX / incomingLength + outgoingX / outgoingLength;
                    cross = normalX * ((double)point.X - vertex.X) + normalY * ((double)point.Y - vertex.Y);
                }
            }
            return cross > 0 ? -projection.Distance : projection.Distance;
        }

        /// <summary>
        /// Projects the specified point onto this chain.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point and distance to this chain.</returns>
        public CurveProjection Project(PointXY point)
        {
            ParameterizedCurveProjection projection = ProjectWithParameter(point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        /// <summary>
        /// Projects the specified point onto this chain and reports the chain length coordinate.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point, chain length coordinate, and distance to this chain.</returns>
        /// <remarks>Equal-distance projections select the first segment in traversal order.</remarks>
        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            return FindNearestProjection(point, out _);
        }

        private ParameterizedCurveProjection FindNearestProjection(PointXY point, out int segmentIndex)
        {
            segmentIndex = 0;
            ParameterizedCurveProjection closestProjection = ProjectOnSegment(0, point);

            for (int i = 1; i < _segments.Length; i++)
            {
                ParameterizedCurveProjection projection = ProjectOnSegment(i, point);
                if (projection.Distance < closestProjection.Distance)
                {
                    closestProjection = projection;
                    segmentIndex = i;
                }
            }

            return closestProjection;
        }

        /// <summary>
        /// Returns the point at the specified chain length coordinate.
        /// </summary>
        /// <param name="curveCoordinate">The finite curve coordinate in world coordinate units from the chain start.</param>
        /// <returns>The point on this chain.</returns>
        public PointXY GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            if (curveCoordinate < 0f || curveCoordinate > Length)
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must lie within the chain length.");

            float remainingCoordinate = curveCoordinate;

            for (int i = 0; i < _segments.Length; i++)
            {
                ParameterizedSegment segment = _segments[i];
                float segmentLength = segment.Length;

                if (remainingCoordinate <= segmentLength || i == _segments.Length - 1)
                    return segment.GetPoint(remainingCoordinate);

                remainingCoordinate -= segmentLength;
            }

            return EndPoint;
        }

        private ParameterizedCurveProjection ProjectOnSegment(int segmentIndex, PointXY point)
        {
            ParameterizedCurveProjection projection = _segments[segmentIndex].ProjectWithParameter(point);
            float chainCoordinate = _segmentStartCoordinates[segmentIndex] + projection.CurveCoordinate;

            return new ParameterizedCurveProjection(
                projection.ProjectedPoint,
                chainCoordinate,
                projection.Distance);
        }
    }
}
