using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents the closed boundary contour of an oriented rectangle.
    /// </summary>
    public readonly struct OrientedRectangleContour : IContour, IEquatable<OrientedRectangleContour>
    {
        private readonly PointXY _center;
        private readonly VectorXY _size;
        private readonly float _rotation;
        private readonly VectorXY _axisX;
        private readonly VectorXY _axisY;

        /// <summary>
        /// Initializes a new oriented rectangular contour.
        /// </summary>
        /// <param name="center">The rectangle center.</param>
        /// <param name="size">The rectangle size along its local X and Y axes.</param>
        /// <param name="rotation">The counterclockwise rotation of the local X axis, in radians.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="center"/>, <paramref name="size"/>, or <paramref name="rotation"/>
        /// contains a non-finite value, or when any size component is not positive.
        /// </exception>
        public OrientedRectangleContour(PointXY center, VectorXY size, float rotation)
        {
            (VectorXY axisX, VectorXY axisY) = CreateAxes(center, size, rotation);

            _center = center;
            _size = size;
            _rotation = rotation;
            _axisX = axisX;
            _axisY = axisY;
        }

        /// <summary>
        /// Gets the oriented rectangular region bounded by this contour.
        /// </summary>
        public OrientedRectangle Rectangle => ToRegion();

        /// <summary>
        /// Gets the rectangle center.
        /// </summary>
        public PointXY Center => _center;

        /// <summary>
        /// Gets the rectangle size along its local X and Y axes.
        /// </summary>
        public VectorXY Size => _size;

        /// <summary>
        /// Gets the counterclockwise rotation of the local X axis, in radians.
        /// </summary>
        public float Rotation => _rotation;

        /// <summary>
        /// Gets the rectangle width along its local X axis.
        /// </summary>
        public float Width => Size.X;

        /// <summary>
        /// Gets the rectangle height along its local Y axis.
        /// </summary>
        public float Height => Size.Y;

        /// <summary>
        /// Gets the unit vector of the rectangle local X axis.
        /// </summary>
        public VectorXY AxisX => _axisX;

        /// <summary>
        /// Gets the unit vector of the rectangle local Y axis.
        /// </summary>
        public VectorXY AxisY => _axisY;

        /// <summary>
        /// Gets the bottom-left corner in the rectangle local coordinate system.
        /// </summary>
        public PointXY BottomLeft => Center - AxisX * (Width * 0.5f) - AxisY * (Height * 0.5f);

        /// <summary>
        /// Gets the bottom-right corner in the rectangle local coordinate system.
        /// </summary>
        public PointXY BottomRight => Center + AxisX * (Width * 0.5f) - AxisY * (Height * 0.5f);

        /// <summary>
        /// Gets the top-left corner in the rectangle local coordinate system.
        /// </summary>
        public PointXY TopLeft => Center - AxisX * (Width * 0.5f) + AxisY * (Height * 0.5f);

        /// <summary>
        /// Gets the top-right corner in the rectangle local coordinate system.
        /// </summary>
        public PointXY TopRight => Center + AxisX * (Width * 0.5f) + AxisY * (Height * 0.5f);

        /// <summary>
        /// Gets the rectangle perimeter length.
        /// </summary>
        public float Length => 2f * (Width + Height);

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            return new Segment(BottomLeft, BottomRight).CountRightwardCrossings(origin) +
                new Segment(BottomRight, TopRight).CountRightwardCrossings(origin) +
                new Segment(TopRight, TopLeft).CountRightwardCrossings(origin) +
                new Segment(TopLeft, BottomLeft).CountRightwardCrossings(origin);
        }

        /// <inheritdoc/>
        public bool Encloses(PointXY point)
        {
            VectorXY local = GetCenteredLocalCoordinates(point);

            return local.X >= -Width * 0.5f && local.X <= Width * 0.5f &&
                local.Y >= -Height * 0.5f && local.Y <= Height * 0.5f;
        }

        /// <inheritdoc/>
        public List<PointXY> GetPointIntersections(Ray ray)
        {
            var edges = new[]
            {
                new Segment(BottomLeft, BottomRight),
                new Segment(BottomRight, TopRight),
                new Segment(TopRight, TopLeft),
                new Segment(TopLeft, BottomLeft),
            };
            var intersections = new List<PointXY>();

            for (int i = 0; i < edges.Length; i++)
            {
                List<PointXY> edgeIntersections = edges[i].GetPointIntersections(ray);
                if (edgeIntersections.Count == 0 && edges[i].HasContinuousIntersection(ray))
                    return new List<PointXY>();

                for (int j = 0; j < edgeIntersections.Count; j++)
                    intersections.AddDistinct(edgeIntersections[j], GeometryConstants.GeometryEpsilon);
            }

            return intersections;
        }

        /// <summary>
        /// Returns point intersections between this contour and the specified ray.
        /// </summary>
        /// <param name="ray">The ray to intersect with this contour.</param>
        /// <param name="geometryEpsilon">The geometry comparison tolerance in world coordinate units.</param>
        /// <returns>A new mutable list of intersection points in the forward direction of the ray, owned by the caller.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="geometryEpsilon"/> is negative, NaN, or infinite.</exception>
        private List<PointXY> GetIntersectionsWithTolerance(
            Ray ray,
            float geometryEpsilon)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            var intersections = new List<PointXY>();
            PointXY localOrigin = ToLocalPoint(ray.Origin);
            VectorXY localDirection = ToLocalVector(ray.Direction);
            float minX = -Width * 0.5f;
            float maxX = Width * 0.5f;
            float minY = -Height * 0.5f;
            float maxY = Height * 0.5f;

            AddLocalVerticalEdgeIntersections(intersections, localOrigin, localDirection, minX, minY, maxY, geometryEpsilon);
            AddLocalVerticalEdgeIntersections(intersections, localOrigin, localDirection, maxX, minY, maxY, geometryEpsilon);
            AddLocalHorizontalEdgeIntersections(intersections, localOrigin, localDirection, minY, minX, maxX, geometryEpsilon);
            AddLocalHorizontalEdgeIntersections(intersections, localOrigin, localDirection, maxY, minX, maxX, geometryEpsilon);

            return intersections;
        }

        /// <inheritdoc/>
        public CurveProjection Project(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            PointXY localPoint = ToLocalPoint(point);
            float minX = -Width * 0.5f;
            float maxX = Width * 0.5f;
            float minY = -Height * 0.5f;
            float maxY = Height * 0.5f;
            float x = Clamp(localPoint.X, minX, maxX);
            float y = Clamp(localPoint.Y, minY, maxY);
            PointXY closestLocalPoint = default;
            float closestDistanceSquared = float.MaxValue;

            AddProjectionCandidate(
                new PointXY(x, minY),
                localPoint,
                ref closestLocalPoint,
                ref closestDistanceSquared);

            AddProjectionCandidate(
                new PointXY(maxX, y),
                localPoint,
                ref closestLocalPoint,
                ref closestDistanceSquared);

            AddProjectionCandidate(
                new PointXY(x, maxY),
                localPoint,
                ref closestLocalPoint,
                ref closestDistanceSquared);

            AddProjectionCandidate(
                new PointXY(minX, y),
                localPoint,
                ref closestLocalPoint,
                ref closestDistanceSquared);

            return new CurveProjection(
                ToWorldPoint(closestLocalPoint),
                MathF.Sqrt(closestDistanceSquared));
        }

        /// <inheritdoc/>
        public float Distance(PointXY point)
        {
            VectorXY local = GetCenteredLocalCoordinates(point);

            return GetLocalDistanceToBoundary(local);
        }

        /// <inheritdoc/>
        public float SignedDistance(PointXY point)
        {
            float distance = Distance(point);
            VectorXY local = GetCenteredLocalCoordinates(point);
            return local.X >= -Width * 0.5f && local.X <= Width * 0.5f &&
                local.Y >= -Height * 0.5f && local.Y <= Height * 0.5f ? -distance : distance;
        }

        /// <summary>
        /// Creates an oriented rectangular region bounded by this contour.
        /// </summary>
        /// <returns>The oriented rectangular region bounded by this contour.</returns>
        public OrientedRectangle ToRegion()
        {
            return new OrientedRectangle(Center, Size, Rotation);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is OrientedRectangleContour other && Equals(other);

        /// <summary>
        /// Indicates whether this contour has the same rectangle as another contour.
        /// </summary>
        /// <param name="other">The contour to compare with this contour.</param>
        /// <returns><see langword="true"/> if both contours are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(OrientedRectangleContour other) =>
            Center.Equals(other.Center) &&
            Size.Equals(other.Size) &&
            Rotation.Equals(other.Rotation);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Center, Size, Rotation);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "OrientedRectangleContour(center: {0}, size: {1}, rotation: {2} rad)",
                Center,
                Size,
                Rotation);

        /// <summary>
        /// Converts an oriented rectangular contour to its bounded rectangular region.
        /// </summary>
        /// <param name="contour">The oriented rectangular contour to convert.</param>
        public static explicit operator OrientedRectangle(OrientedRectangleContour contour)
        {
            return contour.Rectangle;
        }

        /// <summary>
        /// Converts an oriented rectangular contour to a parameterized oriented rectangular contour.
        /// </summary>
        /// <param name="contour">The oriented rectangular contour to convert.</param>
        public static explicit operator ParameterizedOrientedRectangleContour(OrientedRectangleContour contour)
        {
            return new ParameterizedOrientedRectangleContour(contour.Center, contour.Size, contour.Rotation);
        }

        /// <summary>
        /// Indicates whether two oriented rectangular contours are equal.
        /// </summary>
        /// <param name="left">The first contour.</param>
        /// <param name="right">The second contour.</param>
        /// <returns><see langword="true"/> if both contours are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(OrientedRectangleContour left, OrientedRectangleContour right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two oriented rectangular contours are different.
        /// </summary>
        /// <param name="left">The first contour.</param>
        /// <param name="right">The second contour.</param>
        /// <returns><see langword="true"/> if the contours are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(OrientedRectangleContour left, OrientedRectangleContour right) => !left.Equals(right);

        private VectorXY GetCenteredLocalCoordinates(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            VectorXY centered = point - Center;
            return new VectorXY(
                VectorXY.Dot(centered, AxisX),
                VectorXY.Dot(centered, AxisY));
        }

        private float GetLocalDistanceToBoundary(VectorXY local)
        {
            float halfWidth = Width * 0.5f;
            float halfHeight = Height * 0.5f;
            float absoluteX = MathF.Abs(local.X);
            float absoluteY = MathF.Abs(local.Y);
            float outsideX = MathF.Max(absoluteX - halfWidth, 0f);
            float outsideY = MathF.Max(absoluteY - halfHeight, 0f);

            if (outsideX > 0f || outsideY > 0f)
                return MathF.Sqrt(outsideX * outsideX + outsideY * outsideY);

            return MathF.Min(halfWidth - absoluteX, halfHeight - absoluteY);
        }

        private PointXY ToLocalPoint(PointXY point)
        {
            VectorXY centered = point - Center;
            return new PointXY(
                VectorXY.Dot(centered, AxisX),
                VectorXY.Dot(centered, AxisY));
        }

        private VectorXY ToLocalVector(VectorXY vector)
        {
            return new VectorXY(
                VectorXY.Dot(vector, AxisX),
                VectorXY.Dot(vector, AxisY));
        }

        private PointXY ToWorldPoint(PointXY localPoint)
        {
            return Center + AxisX * localPoint.X + AxisY * localPoint.Y;
        }

        private void AddLocalVerticalEdgeIntersections(
            List<PointXY> intersections,
            PointXY localOrigin,
            VectorXY localDirection,
            float edgeX,
            float minY,
            float maxY,
            float geometryEpsilon)
        {
            if (localDirection.X.IsAlmostZero(geometryEpsilon))
            {
                if (localOrigin.X.AlmostEquals(edgeX, geometryEpsilon))
                {
                    AddLocalCollinearEdgeIntersections(
                        intersections,
                        localOrigin,
                        localDirection,
                        new PointXY(edgeX, minY),
                        new PointXY(edgeX, maxY),
                        geometryEpsilon);
                }

                return;
            }

            float rayCoordinate = (edgeX - localOrigin.X) / localDirection.X;
            if (rayCoordinate < -geometryEpsilon)
                return;

            float y = localOrigin.Y + rayCoordinate * localDirection.Y;
            if (y < minY - geometryEpsilon || y > maxY + geometryEpsilon)
                return;

            AddLocalDistinct(intersections, new PointXY(edgeX, Clamp(y, minY, maxY)), geometryEpsilon);
        }

        private void AddLocalHorizontalEdgeIntersections(
            List<PointXY> intersections,
            PointXY localOrigin,
            VectorXY localDirection,
            float edgeY,
            float minX,
            float maxX,
            float geometryEpsilon)
        {
            if (localDirection.Y.IsAlmostZero(geometryEpsilon))
            {
                if (localOrigin.Y.AlmostEquals(edgeY, geometryEpsilon))
                {
                    AddLocalCollinearEdgeIntersections(
                        intersections,
                        localOrigin,
                        localDirection,
                        new PointXY(minX, edgeY),
                        new PointXY(maxX, edgeY),
                        geometryEpsilon);
                }

                return;
            }

            float rayCoordinate = (edgeY - localOrigin.Y) / localDirection.Y;
            if (rayCoordinate < -geometryEpsilon)
                return;

            float x = localOrigin.X + rayCoordinate * localDirection.X;
            if (x < minX - geometryEpsilon || x > maxX + geometryEpsilon)
                return;

            AddLocalDistinct(intersections, new PointXY(Clamp(x, minX, maxX), edgeY), geometryEpsilon);
        }

        private void AddLocalCollinearEdgeIntersections(
            List<PointXY> intersections,
            PointXY localOrigin,
            VectorXY localDirection,
            PointXY endpointA,
            PointXY endpointB,
            float geometryEpsilon)
        {
            if (PointIsOnSegment(localOrigin, endpointA, endpointB, geometryEpsilon))
                AddLocalDistinct(intersections, localOrigin, geometryEpsilon);

            AddLocalIfOnRay(intersections, localOrigin, localDirection, endpointA, geometryEpsilon);
            AddLocalIfOnRay(intersections, localOrigin, localDirection, endpointB, geometryEpsilon);
        }

        private void AddLocalIfOnRay(
            List<PointXY> intersections,
            PointXY localOrigin,
            VectorXY localDirection,
            PointXY localPoint,
            float geometryEpsilon)
        {
            VectorXY toPoint = localPoint - localOrigin;

            if (VectorXY.Dot(toPoint, localDirection) < -geometryEpsilon)
                return;

            if (!VectorXY.Cross(toPoint, localDirection).IsAlmostZero(geometryEpsilon))
                return;

            AddLocalDistinct(intersections, localPoint, geometryEpsilon);
        }

        private void AddLocalDistinct(List<PointXY> intersections, PointXY localPoint, float geometryEpsilon)
        {
            intersections.AddDistinct(ToWorldPoint(localPoint), geometryEpsilon);
        }

        private static void AddProjectionCandidate(
            PointXY projectedPoint,
            PointXY sourcePoint,
            ref PointXY closestPoint,
            ref float closestDistanceSquared)
        {
            float distanceSquared = sourcePoint.SquaredDistanceTo(projectedPoint);
            if (distanceSquared >= closestDistanceSquared)
                return;

            closestPoint = projectedPoint;
            closestDistanceSquared = distanceSquared;
        }

        private static bool PointIsOnSegment(PointXY point, PointXY endpointA, PointXY endpointB, float geometryEpsilon)
        {
            VectorXY segment = endpointB - endpointA;
            VectorXY toPoint = point - endpointA;

            if (!VectorXY.Cross(segment, toPoint).IsAlmostZero(geometryEpsilon))
                return false;

            float dot = VectorXY.Dot(toPoint, segment);
            return dot >= -geometryEpsilon && dot <= segment.SquaredLength + geometryEpsilon;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        private static (VectorXY AxisX, VectorXY AxisY) CreateAxes(PointXY center, VectorXY size, float rotation)
        {
            PointXYValidation.ThrowIfNotFinite(
                center,
                nameof(center),
                "Rectangle contour center coordinates must be finite.");

            if (!size.IsFinite || size.X <= 0f || size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Rectangle contour size components must be finite and positive.");

            GeometryConstants.ValidateFiniteAngle(rotation, nameof(rotation));

            float cos = MathF.Cos(rotation);
            float sin = MathF.Sin(rotation);

            return (new VectorXY(cos, sin), new VectorXY(-sin, cos));
        }
    }
}
