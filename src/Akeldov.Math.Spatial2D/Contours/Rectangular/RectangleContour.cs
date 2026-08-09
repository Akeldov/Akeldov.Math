using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents the closed boundary contour of an axis-aligned rectangle.
    /// </summary>
    public readonly struct RectangleContour : IContour, IEquatable<RectangleContour>
    {
        private readonly PointXY _min;
        private readonly PointXY _max;

        /// <summary>
        /// Initializes a new axis-aligned rectangular contour from two opposite corners.
        /// </summary>
        /// <param name="cornerA">The first rectangle corner.</param>
        /// <param name="cornerB">The opposite rectangle corner.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any corner coordinate is not finite, or when the rectangle width or height is zero.
        /// </exception>
        public RectangleContour(PointXY cornerA, PointXY cornerB)
        {
            (PointXY min, PointXY max) = CreateBounds(cornerA, cornerB);

            _min = min;
            _max = max;
        }

        /// <summary>
        /// Gets the rectangular region bounded by this contour.
        /// </summary>
        public Rectangle Rectangle => ToRegion();

        /// <summary>
        /// Gets the corner with the minimum X and Y coordinates.
        /// </summary>
        public PointXY Min => _min;

        /// <summary>
        /// Gets the corner with the maximum X and Y coordinates.
        /// </summary>
        public PointXY Max => _max;

        /// <summary>
        /// Gets the rectangle width.
        /// </summary>
        public float Width => Max.X - Min.X;

        /// <summary>
        /// Gets the rectangle height.
        /// </summary>
        public float Height => Max.Y - Min.Y;

        /// <summary>
        /// Gets the rectangle size.
        /// </summary>
        public VectorXY Size => Max - Min;

        /// <summary>
        /// Gets the rectangle center.
        /// </summary>
        public PointXY Center => Min + Size * 0.5f;

        /// <summary>
        /// Gets the bottom-left corner.
        /// Bottom means the smaller Y coordinate in the rectangle coordinate system.
        /// </summary>
        public PointXY BottomLeft => Min;

        /// <summary>
        /// Gets the bottom-right corner.
        /// Bottom means the smaller Y coordinate in the rectangle coordinate system.
        /// </summary>
        public PointXY BottomRight => new PointXY(Max.X, Min.Y);

        /// <summary>
        /// Gets the top-left corner.
        /// Top means the greater Y coordinate in the rectangle coordinate system.
        /// </summary>
        public PointXY TopLeft => new PointXY(Min.X, Max.Y);

        /// <summary>
        /// Gets the top-right corner.
        /// Top means the greater Y coordinate in the rectangle coordinate system.
        /// </summary>
        public PointXY TopRight => Max;

        /// <summary>
        /// Gets the rectangle perimeter length.
        /// </summary>
        public float Length => 2f * (Width + Height);

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            if (origin.Y < Min.Y || origin.Y >= Max.Y)
                return 0;

            int count = 0;
            if (Min.X > origin.X)
                count++;
            if (Max.X > origin.X)
                count++;

            return count;
        }

        /// <inheritdoc/>
        public bool Encloses(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            return point.X >= Min.X && point.X <= Max.X &&
                point.Y >= Min.Y && point.Y <= Max.Y;
        }

        /// <inheritdoc/>
        public List<PointXY> GetPointIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

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
                List<PointXY> edgeIntersections = edges[i].GetPointIntersections(ray, geometryEpsilon);
                if (edgeIntersections.Count == 0 && edges[i].GetRayIntersections(ray, geometryEpsilon).Count != 0)
                    return new List<PointXY>();

                for (int j = 0; j < edgeIntersections.Count; j++)
                    intersections.AddDistinct(edgeIntersections[j], geometryEpsilon);
            }

            return intersections;
        }

        /// <inheritdoc cref="GetPointIntersections(Ray, float)"/>
        public List<PointXY> GetRayIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            var intersections = new List<PointXY>();
            AddVerticalEdgeIntersections(intersections, ray, Min.X, geometryEpsilon);
            AddVerticalEdgeIntersections(intersections, ray, Max.X, geometryEpsilon);
            AddHorizontalEdgeIntersections(intersections, ray, Min.Y, geometryEpsilon);
            AddHorizontalEdgeIntersections(intersections, ray, Max.Y, geometryEpsilon);

            return intersections;
        }

        /// <inheritdoc/>
        public CurveProjection Project(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            float x = Clamp(point.X, Min.X, Max.X);
            float y = Clamp(point.Y, Min.Y, Max.Y);
            PointXY closestPoint = default;
            float closestDistanceSquared = float.MaxValue;

            AddProjectionCandidate(
                new PointXY(x, Min.Y),
                point,
                ref closestPoint,
                ref closestDistanceSquared);

            AddProjectionCandidate(
                new PointXY(Max.X, y),
                point,
                ref closestPoint,
                ref closestDistanceSquared);

            AddProjectionCandidate(
                new PointXY(x, Max.Y),
                point,
                ref closestPoint,
                ref closestDistanceSquared);

            AddProjectionCandidate(
                new PointXY(Min.X, y),
                point,
                ref closestPoint,
                ref closestDistanceSquared);

            return new CurveProjection(
                closestPoint,
                MathF.Sqrt(closestDistanceSquared));
        }

        /// <inheritdoc/>
        public float Distance(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            return GetDistanceToBoundary(point);
        }

        /// <inheritdoc/>
        public float SignedDistance(PointXY point, float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            float distance = Distance(point);
            return point.X >= Min.X - geometryEpsilon &&
                point.X <= Max.X + geometryEpsilon &&
                point.Y >= Min.Y - geometryEpsilon &&
                point.Y <= Max.Y + geometryEpsilon ? -distance : distance;
        }

        /// <summary>
        /// Creates a rectangular region bounded by this contour.
        /// </summary>
        /// <returns>The rectangular region bounded by this contour.</returns>
        public Rectangle ToRegion()
        {
            return new Rectangle(Min, Max);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is RectangleContour other && Equals(other);

        /// <summary>
        /// Indicates whether this contour has the same rectangle as another contour.
        /// </summary>
        /// <param name="other">The contour to compare with this contour.</param>
        /// <returns><see langword="true"/> if both contours are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(RectangleContour other) => Min.Equals(other.Min) && Max.Equals(other.Max);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Min, Max);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "RectangleContour({0}, {1})", Min, Max);

        /// <summary>
        /// Converts a rectangular contour to its bounded rectangular region.
        /// </summary>
        /// <param name="contour">The rectangular contour to convert.</param>
        public static explicit operator Rectangle(RectangleContour contour)
        {
            return contour.Rectangle;
        }

        /// <summary>
        /// Converts a rectangular contour to a parameterized rectangular contour.
        /// </summary>
        /// <param name="contour">The rectangular contour to convert.</param>
        public static explicit operator ParameterizedRectangleContour(RectangleContour contour)
        {
            return new ParameterizedRectangleContour(contour.Min, contour.Max);
        }

        /// <summary>
        /// Indicates whether two rectangular contours are equal.
        /// </summary>
        /// <param name="left">The first contour.</param>
        /// <param name="right">The second contour.</param>
        /// <returns><see langword="true"/> if both contours are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(RectangleContour left, RectangleContour right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two rectangular contours are different.
        /// </summary>
        /// <param name="left">The first contour.</param>
        /// <param name="right">The second contour.</param>
        /// <returns><see langword="true"/> if the contours are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(RectangleContour left, RectangleContour right) => !left.Equals(right);

        private float GetDistanceToBoundary(PointXY point)
        {
            float outsideX = MathF.Max(MathF.Max(Min.X - point.X, point.X - Max.X), 0f);
            float outsideY = MathF.Max(MathF.Max(Min.Y - point.Y, point.Y - Max.Y), 0f);

            if (outsideX > 0f || outsideY > 0f)
                return MathF.Sqrt(outsideX * outsideX + outsideY * outsideY);

            float left = point.X - Min.X;
            float right = Max.X - point.X;
            float bottom = point.Y - Min.Y;
            float top = Max.Y - point.Y;

            return MathF.Min(MathF.Min(left, right), MathF.Min(bottom, top));
        }

        private void AddVerticalEdgeIntersections(
            List<PointXY> intersections,
            Ray ray,
            float edgeX,
            float geometryEpsilon)
        {
            VectorXY direction = ray.Direction;

            if (direction.X.IsAlmostZero(geometryEpsilon))
            {
                if (ray.Origin.X.AlmostEquals(edgeX, geometryEpsilon))
                    AddCollinearEdgeIntersections(intersections, ray, new PointXY(edgeX, Min.Y), new PointXY(edgeX, Max.Y), geometryEpsilon);

                return;
            }

            float rayCoordinate = (edgeX - ray.Origin.X) / direction.X;
            if (rayCoordinate < -geometryEpsilon)
                return;

            float y = ray.Origin.Y + rayCoordinate * direction.Y;
            if (y < Min.Y - geometryEpsilon || y > Max.Y + geometryEpsilon)
                return;

            intersections.AddDistinct(new PointXY(edgeX, Clamp(y, Min.Y, Max.Y)), geometryEpsilon);
        }

        private void AddHorizontalEdgeIntersections(
            List<PointXY> intersections,
            Ray ray,
            float edgeY,
            float geometryEpsilon)
        {
            VectorXY direction = ray.Direction;

            if (direction.Y.IsAlmostZero(geometryEpsilon))
            {
                if (ray.Origin.Y.AlmostEquals(edgeY, geometryEpsilon))
                    AddCollinearEdgeIntersections(intersections, ray, new PointXY(Min.X, edgeY), new PointXY(Max.X, edgeY), geometryEpsilon);

                return;
            }

            float rayCoordinate = (edgeY - ray.Origin.Y) / direction.Y;
            if (rayCoordinate < -geometryEpsilon)
                return;

            float x = ray.Origin.X + rayCoordinate * direction.X;
            if (x < Min.X - geometryEpsilon || x > Max.X + geometryEpsilon)
                return;

            intersections.AddDistinct(new PointXY(Clamp(x, Min.X, Max.X), edgeY), geometryEpsilon);
        }

        private static void AddCollinearEdgeIntersections(
            List<PointXY> intersections,
            Ray ray,
            PointXY endpointA,
            PointXY endpointB,
            float geometryEpsilon)
        {
            if (PointIsOnSegment(ray.Origin, endpointA, endpointB, geometryEpsilon))
                intersections.AddDistinct(ray.Origin, geometryEpsilon);

            AddIfOnRay(intersections, ray, endpointA, geometryEpsilon);
            AddIfOnRay(intersections, ray, endpointB, geometryEpsilon);
        }

        private static void AddIfOnRay(
            List<PointXY> intersections,
            Ray ray,
            PointXY point,
            float geometryEpsilon)
        {
            VectorXY toPoint = point - ray.Origin;
            VectorXY direction = ray.Direction;

            if (VectorXY.Dot(toPoint, direction) < -geometryEpsilon)
                return;

            if (!VectorXY.Cross(toPoint, direction).IsAlmostZero(geometryEpsilon))
                return;

            intersections.AddDistinct(point, geometryEpsilon);
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

        private static (PointXY Min, PointXY Max) CreateBounds(PointXY cornerA, PointXY cornerB)
        {
            PointXYValidation.ThrowIfNotFinite(
                cornerA,
                nameof(cornerA),
                "Rectangle contour corner coordinates must be finite.");

            PointXYValidation.ThrowIfNotFinite(
                cornerB,
                nameof(cornerB),
                "Rectangle contour corner coordinates must be finite.");

            PointXY min = new PointXY(MathF.Min(cornerA.X, cornerB.X), MathF.Min(cornerA.Y, cornerB.Y));
            PointXY max = new PointXY(MathF.Max(cornerA.X, cornerB.X), MathF.Max(cornerA.Y, cornerB.Y));

            if (max.X - min.X <= 0f || max.Y - min.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(cornerB), cornerB, "Rectangle contour width and height must be positive.");

            return (min, max);
        }
    }
}
