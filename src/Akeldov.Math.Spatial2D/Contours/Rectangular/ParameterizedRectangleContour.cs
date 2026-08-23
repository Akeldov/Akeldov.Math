using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents the closed boundary contour of an axis-aligned rectangle.
    /// A contour with one zero-sized dimension traverses the same line segment in both directions,
    /// and a contour with both dimensions equal to zero represents a point.
    /// </summary>
    /// <remarks>
    /// The default value represents the point at the coordinate origin with counterclockwise traversal.
    /// </remarks>
    public readonly struct ParameterizedRectangleContour : IParameterizedContour, IEquatable<ParameterizedRectangleContour>
    {
        private readonly PointXY _min;
        private readonly PointXY _max;
        private readonly float _parameterOriginCoordinate;
        private readonly ContourDirection _contourDirection;

        /// <summary>
        /// Initializes a new axis-aligned rectangular contour from two opposite corners.
        /// The curve coordinate zero point defaults to the middle of the right edge,
        /// and curve coordinates increase counterclockwise by default.
        /// </summary>
        /// <param name="cornerA">The first rectangle corner.</param>
        /// <param name="cornerB">The opposite rectangle corner.</param>
        /// <param name="contourDirection">The direction in which curve coordinates increase along the contour.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any corner coordinate or resulting size component is not finite, or when
        /// <paramref name="contourDirection"/> is unsupported.
        /// </exception>
        public ParameterizedRectangleContour(
            PointXY cornerA,
            PointXY cornerB,
            ContourDirection contourDirection = ContourDirection.Counterclockwise)
        {
            (PointXY min, PointXY max) = CreateBounds(cornerA, cornerB);
            ValidateContourDirection(contourDirection);

            _min = min;
            _max = max;
            _parameterOriginCoordinate = 0f;
            _contourDirection = contourDirection;
        }

        /// <summary>
        /// Initializes a new axis-aligned rectangular contour from two opposite corners and a named parameter origin.
        /// </summary>
        /// <param name="cornerA">The first rectangle corner.</param>
        /// <param name="cornerB">The opposite rectangle corner.</param>
        /// <param name="parameterOrigin">The named boundary point where curve coordinate zero lies.</param>
        /// <param name="contourDirection">The direction in which curve coordinates increase along the contour.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any coordinate or resulting size component is not finite,
        /// when <paramref name="parameterOrigin"/> is unsupported, or when
        /// <paramref name="contourDirection"/> is unsupported.
        /// </exception>
        public ParameterizedRectangleContour(
            PointXY cornerA,
            PointXY cornerB,
            RectangleContourParameterOrigin parameterOrigin,
            ContourDirection contourDirection = ContourDirection.Counterclockwise)
        {
            (PointXY min, PointXY max) = CreateBounds(cornerA, cornerB);
            ValidateContourDirection(contourDirection);

            _min = min;
            _max = max;
            _parameterOriginCoordinate = WrapBoundaryCoordinate(
                GetBoundaryCoordinateUnchecked(parameterOrigin, min, max),
                GetBoundaryLength(min, max));
            _contourDirection = contourDirection;
        }

        /// <summary>
        /// Initializes a new axis-aligned rectangular contour from two opposite corners and a parameter origin coordinate.
        /// </summary>
        /// <param name="cornerA">The first rectangle corner.</param>
        /// <param name="cornerB">The opposite rectangle corner.</param>
        /// <param name="parameterOrigin">
        /// The counterclockwise boundary coordinate where curve coordinate zero lies,
        /// measured from the default right-edge midpoint.
        /// </param>
        /// <param name="contourDirection">The direction in which curve coordinates increase along the contour.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any coordinate or resulting size component is not finite,
        /// when <paramref name="parameterOrigin"/> does not lie within the rectangle boundary traversal length,
        /// or when <paramref name="contourDirection"/> is unsupported.
        /// </exception>
        public ParameterizedRectangleContour(
            PointXY cornerA,
            PointXY cornerB,
            float parameterOrigin,
            ContourDirection contourDirection = ContourDirection.Counterclockwise)
        {
            (PointXY min, PointXY max) = CreateBounds(cornerA, cornerB);
            ValidateContourDirection(contourDirection);
            float length = GetBoundaryLength(min, max);
            ValidateParameterOriginCoordinate(parameterOrigin, length);

            _min = min;
            _max = max;
            _parameterOriginCoordinate = WrapBoundaryCoordinate(parameterOrigin, length);
            _contourDirection = contourDirection;
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
        /// Gets the nonnegative rectangle width.
        /// </summary>
        public float Width => Max.X - Min.X;

        /// <summary>
        /// Gets the nonnegative rectangle height.
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
        /// Gets the boundary point where curve coordinate zero lies.
        /// </summary>
        public PointXY ParameterOrigin => GetBoundaryPointUnchecked(_parameterOriginCoordinate);

        /// <summary>
        /// Gets the direction in which curve coordinates increase along the contour.
        /// </summary>
        public ContourDirection ContourDirection => _contourDirection;

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
        /// Gets the rectangle boundary traversal length.
        /// A degenerate segment is traversed in both directions and therefore has twice the segment length.
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
        public CurveProjection Project(PointXY point)
        {
            ParameterizedCurveProjection projection = ProjectWithParameter(point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        /// <inheritdoc/>
        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            float x = Clamp(point.X, Min.X, Max.X);
            float y = Clamp(point.Y, Min.Y, Max.Y);
            PointXY closestPoint = default;
            float closestCoordinate = 0f;
            float closestDistanceSquared = float.MaxValue;

            AddProjectionCandidate(
                new PointXY(x, Min.Y),
                GetBoundaryCoordinateUnchecked(new PointXY(x, Min.Y)),
                point,
                ref closestPoint,
                ref closestCoordinate,
                ref closestDistanceSquared);

            AddProjectionCandidate(
                new PointXY(Max.X, y),
                GetBoundaryCoordinateUnchecked(new PointXY(Max.X, y)),
                point,
                ref closestPoint,
                ref closestCoordinate,
                ref closestDistanceSquared);

            AddProjectionCandidate(
                new PointXY(x, Max.Y),
                GetBoundaryCoordinateUnchecked(new PointXY(x, Max.Y)),
                point,
                ref closestPoint,
                ref closestCoordinate,
                ref closestDistanceSquared);

            AddProjectionCandidate(
                new PointXY(Min.X, y),
                GetBoundaryCoordinateUnchecked(new PointXY(Min.X, y)),
                point,
                ref closestPoint,
                ref closestCoordinate,
                ref closestDistanceSquared);

            return new ParameterizedCurveProjection(
                closestPoint,
                closestCoordinate,
                MathF.Sqrt(closestDistanceSquared));
        }

        /// <inheritdoc/>
        public PointXY GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            if (curveCoordinate < 0f || curveCoordinate > Length)
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must lie within the rectangle boundary traversal length.");

            return GetBoundaryPointUnchecked(ToBoundaryCoordinate(curveCoordinate));
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
        public float SignedDistance(PointXY point)
        {
            float distance = Distance(point);

            if (Width == 0f || Height == 0f)
                return distance;

            return point.X >= Min.X && point.X <= Max.X && point.Y >= Min.Y && point.Y <= Max.Y ? -distance : distance;
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
        public override bool Equals(object? obj) => obj is ParameterizedRectangleContour other && Equals(other);

        /// <summary>
        /// Indicates whether this contour has the same rectangle, parameter origin, and traversal direction as another contour.
        /// </summary>
        /// <param name="other">The contour to compare with this contour.</param>
        /// <returns><see langword="true"/> if both contours are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ParameterizedRectangleContour other) =>
            Min.Equals(other.Min) &&
            Max.Equals(other.Max) &&
            _parameterOriginCoordinate.Equals(other._parameterOriginCoordinate) &&
            ContourDirection == other.ContourDirection;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Min, Max, _parameterOriginCoordinate, ContourDirection);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "ParameterizedRectangleContour({0}, {1}, parameterOrigin: {2}, contourDirection: {3})",
                Min,
                Max,
                ParameterOrigin,
                ContourDirection);

        /// <summary>
        /// Converts a rectangular contour to its bounded rectangular region.
        /// </summary>
        /// <param name="contour">The rectangular contour to convert.</param>
        public static explicit operator Rectangle(ParameterizedRectangleContour contour)
        {
            return contour.Rectangle;
        }

        /// <summary>
        /// Converts a parameterized rectangular contour to its geometric rectangular contour.
        /// </summary>
        /// <param name="contour">The parameterized rectangular contour to convert.</param>
        public static explicit operator RectangleContour(ParameterizedRectangleContour contour)
        {
            return new RectangleContour(contour.Min, contour.Max);
        }

        /// <summary>
        /// Indicates whether two rectangular contours are equal.
        /// </summary>
        /// <param name="left">The first contour.</param>
        /// <param name="right">The second contour.</param>
        /// <returns><see langword="true"/> if both contours are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ParameterizedRectangleContour left, ParameterizedRectangleContour right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two rectangular contours are different.
        /// </summary>
        /// <param name="left">The first contour.</param>
        /// <param name="right">The second contour.</param>
        /// <returns><see langword="true"/> if the contours are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ParameterizedRectangleContour left, ParameterizedRectangleContour right) => !left.Equals(right);

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

        private float ToBoundaryCoordinate(float curveCoordinate)
        {
            float boundaryCoordinate = ContourDirection == ContourDirection.Counterclockwise
                ? _parameterOriginCoordinate + curveCoordinate
                : _parameterOriginCoordinate - curveCoordinate;

            return WrapBoundaryCoordinate(boundaryCoordinate, Length);
        }

        private float ToCurveCoordinate(float boundaryCoordinate)
        {
            float curveCoordinate = ContourDirection == ContourDirection.Counterclockwise
                ? boundaryCoordinate - _parameterOriginCoordinate
                : _parameterOriginCoordinate - boundaryCoordinate;

            return WrapBoundaryCoordinate(curveCoordinate, Length);
        }

        private PointXY GetBoundaryPointUnchecked(float curveCoordinate)
        {
            float canonicalCoordinate = ToCanonicalBoundaryCoordinate(curveCoordinate);

            if (canonicalCoordinate <= Width)
                return new PointXY(Min.X + canonicalCoordinate, Min.Y);

            canonicalCoordinate -= Width;
            if (canonicalCoordinate <= Height)
                return new PointXY(Max.X, Min.Y + canonicalCoordinate);

            canonicalCoordinate -= Height;
            if (canonicalCoordinate <= Width)
                return new PointXY(Max.X - canonicalCoordinate, Max.Y);

            canonicalCoordinate -= Width;
            return new PointXY(Min.X, Max.Y - canonicalCoordinate);
        }

        private float GetBoundaryCoordinateUnchecked(PointXY boundaryPoint)
        {
            return GetBoundaryCoordinateUnchecked(boundaryPoint, Min, Max);
        }

        private static float GetBoundaryCoordinateUnchecked(PointXY boundaryPoint, PointXY min, PointXY max)
        {
            float canonicalCoordinate = GetCanonicalBoundaryCoordinateUnchecked(boundaryPoint, min, max);
            float defaultOriginCoordinate = GetDefaultParameterOriginCanonicalCoordinate(min, max);

            return WrapBoundaryCoordinate(
                canonicalCoordinate - defaultOriginCoordinate,
                GetBoundaryLength(min, max));
        }

        private static float GetBoundaryCoordinateUnchecked(
            RectangleContourParameterOrigin parameterOrigin,
            PointXY min,
            PointXY max)
        {
            float width = max.X - min.X;
            float height = max.Y - min.Y;

            switch (parameterOrigin)
            {
                case RectangleContourParameterOrigin.RightEdgeMidpoint:
                    return 0f;
                case RectangleContourParameterOrigin.TopRight:
                    return height * 0.5f;
                case RectangleContourParameterOrigin.TopEdgeMidpoint:
                    return height * 0.5f + width * 0.5f;
                case RectangleContourParameterOrigin.TopLeft:
                    return height * 0.5f + width;
                case RectangleContourParameterOrigin.LeftEdgeMidpoint:
                    return width + height;
                case RectangleContourParameterOrigin.BottomLeft:
                    return width + height * 1.5f;
                case RectangleContourParameterOrigin.BottomEdgeMidpoint:
                    return width * 1.5f + height * 1.5f;
                case RectangleContourParameterOrigin.BottomRight:
                    return width * 2f + height * 1.5f;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(parameterOrigin),
                        parameterOrigin,
                        "Rectangle contour parameter origin is not supported.");
            }
        }

        private static float GetCanonicalBoundaryCoordinateUnchecked(PointXY boundaryPoint, PointXY min, PointXY max)
        {
            float width = max.X - min.X;
            float height = max.Y - min.Y;

            if (boundaryPoint.Y == min.Y)
                return boundaryPoint.X - min.X;

            if (boundaryPoint.X == max.X)
                return width + boundaryPoint.Y - min.Y;

            if (boundaryPoint.Y == max.Y)
                return width + height + max.X - boundaryPoint.X;

            return 2f * width + height + max.Y - boundaryPoint.Y;
        }

        private void AddProjectionCandidate(
            PointXY projectedPoint,
            float boundaryCoordinate,
            PointXY sourcePoint,
            ref PointXY closestPoint,
            ref float closestCoordinate,
            ref float closestDistanceSquared)
        {
            float distanceSquared = sourcePoint.SquaredDistanceTo(projectedPoint);
            if (distanceSquared >= closestDistanceSquared)
                return;

            closestPoint = projectedPoint;
            closestCoordinate = ToCurveCoordinate(boundaryCoordinate);
            closestDistanceSquared = distanceSquared;
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

            float width = max.X - min.X;
            float height = max.Y - min.Y;
            if (float.IsNaN(width) || float.IsInfinity(width) ||
                float.IsNaN(height) || float.IsInfinity(height))
            {
                throw new ArgumentOutOfRangeException(nameof(cornerB), cornerB, "Rectangle contour width and height must be finite.");
            }

            return (min, max);
        }

        private static void ValidateParameterOriginCoordinate(float parameterOrigin, float length)
        {
            if (float.IsNaN(parameterOrigin) || float.IsInfinity(parameterOrigin))
                throw new ArgumentOutOfRangeException(nameof(parameterOrigin), "Parameter origin coordinate must be finite.");

            if (parameterOrigin < 0f || parameterOrigin > length)
                throw new ArgumentOutOfRangeException(nameof(parameterOrigin), "Parameter origin coordinate must lie within the rectangle boundary traversal length.");
        }

        private static void ValidateContourDirection(ContourDirection contourDirection)
        {
            if (contourDirection != ContourDirection.Counterclockwise &&
                contourDirection != ContourDirection.Clockwise)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(contourDirection),
                    contourDirection,
                    "Contour direction is not supported.");
            }
        }

        private float ToCanonicalBoundaryCoordinate(float curveCoordinate)
        {
            return WrapBoundaryCoordinate(
                GetDefaultParameterOriginCanonicalCoordinate(Min, Max) + curveCoordinate,
                Length);
        }

        private static float GetDefaultParameterOriginCanonicalCoordinate(PointXY min, PointXY max)
        {
            return (max.X - min.X) + (max.Y - min.Y) * 0.5f;
        }

        private static float GetBoundaryLength(PointXY min, PointXY max)
        {
            return 2f * ((max.X - min.X) + (max.Y - min.Y));
        }

        private static float WrapBoundaryCoordinate(float coordinate, float length)
        {
            if (coordinate < 0f)
                return coordinate + length;

            if (coordinate >= length)
                return coordinate - length;

            return coordinate;
        }
    }
}
