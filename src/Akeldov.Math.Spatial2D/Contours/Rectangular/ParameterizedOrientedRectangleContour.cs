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
    public readonly struct ParameterizedOrientedRectangleContour : IParameterizedContour, IEquatable<ParameterizedOrientedRectangleContour>
    {
        private readonly PointXY _center;
        private readonly VectorXY _size;
        private readonly float _rotation;
        private readonly VectorXY _axisX;
        private readonly VectorXY _axisY;
        private readonly float _parameterOriginCoordinate;
        private readonly ContourDirection _contourDirection;

        /// <summary>
        /// Initializes a new oriented rectangular contour.
        /// The curve coordinate zero point defaults to the middle of the right edge,
        /// and curve coordinates increase counterclockwise by default.
        /// </summary>
        /// <param name="center">The rectangle center.</param>
        /// <param name="size">The rectangle size along its local X and Y axes.</param>
        /// <param name="rotation">The counterclockwise rotation of the local X axis, in radians.</param>
        /// <param name="contourDirection">The direction in which curve coordinates increase along the contour.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="center"/>, <paramref name="size"/>, or <paramref name="rotation"/>
        /// contains a non-finite value, when any size component is not positive,
        /// or when <paramref name="contourDirection"/> is unsupported.
        /// </exception>
        public ParameterizedOrientedRectangleContour(
            PointXY center,
            VectorXY size,
            float rotation,
            ContourDirection contourDirection = ContourDirection.Counterclockwise)
        {
            (VectorXY axisX, VectorXY axisY) = CreateAxes(center, size, rotation);
            ValidateContourDirection(contourDirection);

            _center = center;
            _size = size;
            _rotation = rotation;
            _axisX = axisX;
            _axisY = axisY;
            _parameterOriginCoordinate = 0f;
            _contourDirection = contourDirection;
        }

        /// <summary>
        /// Initializes a new oriented rectangular contour with a named parameter origin.
        /// </summary>
        /// <param name="center">The rectangle center.</param>
        /// <param name="size">The rectangle size along its local X and Y axes.</param>
        /// <param name="rotation">The counterclockwise rotation of the local X axis, in radians.</param>
        /// <param name="parameterOrigin">The named boundary point where curve coordinate zero lies.</param>
        /// <param name="contourDirection">The direction in which curve coordinates increase along the contour.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="center"/>, <paramref name="size"/>, or <paramref name="rotation"/>
        /// contains a non-finite value, when any size component is not positive,
        /// when <paramref name="parameterOrigin"/> is unsupported, or when
        /// <paramref name="contourDirection"/> is unsupported.
        /// </exception>
        public ParameterizedOrientedRectangleContour(
            PointXY center,
            VectorXY size,
            float rotation,
            RectangleContourParameterOrigin parameterOrigin,
            ContourDirection contourDirection = ContourDirection.Counterclockwise)
        {
            (VectorXY axisX, VectorXY axisY) = CreateAxes(center, size, rotation);
            ValidateContourDirection(contourDirection);

            _center = center;
            _size = size;
            _rotation = rotation;
            _axisX = axisX;
            _axisY = axisY;
            _parameterOriginCoordinate = GetBoundaryCoordinateUnchecked(parameterOrigin, size);
            _contourDirection = contourDirection;
        }

        /// <summary>
        /// Initializes a new oriented rectangular contour with a parameter origin coordinate.
        /// </summary>
        /// <param name="center">The rectangle center.</param>
        /// <param name="size">The rectangle size along its local X and Y axes.</param>
        /// <param name="rotation">The counterclockwise rotation of the local X axis, in radians.</param>
        /// <param name="parameterOrigin">
        /// The counterclockwise boundary coordinate where curve coordinate zero lies,
        /// measured from the default right-edge midpoint.
        /// </param>
        /// <param name="contourDirection">The direction in which curve coordinates increase along the contour.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="center"/>, <paramref name="size"/>, or <paramref name="rotation"/>
        /// contains a non-finite value, when any size component is not positive,
        /// when <paramref name="parameterOrigin"/> does not lie within the rectangle perimeter length,
        /// or when <paramref name="contourDirection"/> is unsupported.
        /// </exception>
        public ParameterizedOrientedRectangleContour(
            PointXY center,
            VectorXY size,
            float rotation,
            float parameterOrigin,
            ContourDirection contourDirection = ContourDirection.Counterclockwise)
        {
            (VectorXY axisX, VectorXY axisY) = CreateAxes(center, size, rotation);
            ValidateContourDirection(contourDirection);
            float length = GetBoundaryLength(size);
            ValidateParameterOriginCoordinate(parameterOrigin, length);

            _center = center;
            _size = size;
            _rotation = rotation;
            _axisX = axisX;
            _axisY = axisY;
            _parameterOriginCoordinate = WrapBoundaryCoordinate(parameterOrigin, length);
            _contourDirection = contourDirection;
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
        /// Gets the boundary point where curve coordinate zero lies.
        /// </summary>
        public PointXY ParameterOrigin => ToWorldPoint(GetLocalBoundaryPointUnchecked(_parameterOriginCoordinate));

        /// <summary>
        /// Gets the direction in which curve coordinates increase along the contour.
        /// </summary>
        public ContourDirection ContourDirection => _contourDirection;

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
        public List<PointXY> GetPointIntersections(Ray ray) =>
            ((OrientedRectangleContour)this).GetPointIntersections(ray);

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

            PointXY localPoint = ToLocalPoint(point);
            float minX = -Width * 0.5f;
            float maxX = Width * 0.5f;
            float minY = -Height * 0.5f;
            float maxY = Height * 0.5f;
            float x = Clamp(localPoint.X, minX, maxX);
            float y = Clamp(localPoint.Y, minY, maxY);
            PointXY closestLocalPoint = default;
            float closestCoordinate = 0f;
            float closestDistanceSquared = float.MaxValue;

            AddProjectionCandidate(
                new PointXY(x, minY),
                GetLocalBoundaryCoordinateUnchecked(new PointXY(x, minY)),
                localPoint,
                ref closestLocalPoint,
                ref closestCoordinate,
                ref closestDistanceSquared);

            AddProjectionCandidate(
                new PointXY(maxX, y),
                GetLocalBoundaryCoordinateUnchecked(new PointXY(maxX, y)),
                localPoint,
                ref closestLocalPoint,
                ref closestCoordinate,
                ref closestDistanceSquared);

            AddProjectionCandidate(
                new PointXY(x, maxY),
                GetLocalBoundaryCoordinateUnchecked(new PointXY(x, maxY)),
                localPoint,
                ref closestLocalPoint,
                ref closestCoordinate,
                ref closestDistanceSquared);

            AddProjectionCandidate(
                new PointXY(minX, y),
                GetLocalBoundaryCoordinateUnchecked(new PointXY(minX, y)),
                localPoint,
                ref closestLocalPoint,
                ref closestCoordinate,
                ref closestDistanceSquared);

            PointXY closestPoint = ToWorldPoint(closestLocalPoint);

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
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must lie within the rectangle perimeter length.");

            return ToWorldPoint(GetLocalBoundaryPointUnchecked(ToBoundaryCoordinate(curveCoordinate)));
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
        public override bool Equals(object? obj) => obj is ParameterizedOrientedRectangleContour other && Equals(other);

        /// <summary>
        /// Indicates whether this contour has the same rectangle, parameter origin, and traversal direction as another contour.
        /// </summary>
        /// <param name="other">The contour to compare with this contour.</param>
        /// <returns><see langword="true"/> if both contours are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ParameterizedOrientedRectangleContour other) =>
            Center.Equals(other.Center) &&
            Size.Equals(other.Size) &&
            Rotation.Equals(other.Rotation) &&
            _parameterOriginCoordinate.Equals(other._parameterOriginCoordinate) &&
            ContourDirection == other.ContourDirection;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Center, Size, Rotation, _parameterOriginCoordinate, ContourDirection);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "ParameterizedOrientedRectangleContour(center: {0}, size: {1}, rotation: {2} rad, parameterOrigin: {3}, contourDirection: {4})",
                Center,
                Size,
                Rotation,
                ParameterOrigin,
                ContourDirection);

        /// <summary>
        /// Converts an oriented rectangular contour to its bounded rectangular region.
        /// </summary>
        /// <param name="contour">The oriented rectangular contour to convert.</param>
        public static explicit operator OrientedRectangle(ParameterizedOrientedRectangleContour contour)
        {
            return contour.Rectangle;
        }

        /// <summary>
        /// Converts a parameterized oriented rectangular contour to its geometric oriented rectangular contour.
        /// </summary>
        /// <param name="contour">The parameterized oriented rectangular contour to convert.</param>
        public static explicit operator OrientedRectangleContour(ParameterizedOrientedRectangleContour contour)
        {
            return new OrientedRectangleContour(contour.Center, contour.Size, contour.Rotation);
        }

        /// <summary>
        /// Indicates whether two oriented rectangular contours are equal.
        /// </summary>
        /// <param name="left">The first contour.</param>
        /// <param name="right">The second contour.</param>
        /// <returns><see langword="true"/> if both contours are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ParameterizedOrientedRectangleContour left, ParameterizedOrientedRectangleContour right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two oriented rectangular contours are different.
        /// </summary>
        /// <param name="left">The first contour.</param>
        /// <param name="right">The second contour.</param>
        /// <returns><see langword="true"/> if the contours are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ParameterizedOrientedRectangleContour left, ParameterizedOrientedRectangleContour right) => !left.Equals(right);

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

        private PointXY GetLocalBoundaryPointUnchecked(float curveCoordinate)
        {
            float canonicalCoordinate = ToCanonicalBoundaryCoordinate(curveCoordinate);
            float minX = -Width * 0.5f;
            float maxX = Width * 0.5f;
            float minY = -Height * 0.5f;
            float maxY = Height * 0.5f;

            if (canonicalCoordinate <= Width)
                return new PointXY(minX + canonicalCoordinate, minY);

            canonicalCoordinate -= Width;
            if (canonicalCoordinate <= Height)
                return new PointXY(maxX, minY + canonicalCoordinate);

            canonicalCoordinate -= Height;
            if (canonicalCoordinate <= Width)
                return new PointXY(maxX - canonicalCoordinate, maxY);

            canonicalCoordinate -= Width;
            return new PointXY(minX, maxY - canonicalCoordinate);
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

        private float GetLocalBoundaryCoordinateUnchecked(PointXY localBoundaryPoint)
        {
            float canonicalCoordinate = GetCanonicalLocalBoundaryCoordinateUnchecked(localBoundaryPoint, Size);
            float defaultOriginCoordinate = GetDefaultParameterOriginCanonicalCoordinate(Size);

            return WrapBoundaryCoordinate(
                canonicalCoordinate - defaultOriginCoordinate,
                GetBoundaryLength(Size));
        }

        private static float GetBoundaryCoordinateUnchecked(
            RectangleContourParameterOrigin parameterOrigin,
            VectorXY size)
        {
            float width = size.X;
            float height = size.Y;

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

        private static float GetCanonicalLocalBoundaryCoordinateUnchecked(PointXY localBoundaryPoint, VectorXY size)
        {
            float width = size.X;
            float height = size.Y;
            float minX = -width * 0.5f;
            float maxX = width * 0.5f;
            float minY = -height * 0.5f;
            float maxY = height * 0.5f;

            if (localBoundaryPoint.Y == minY)
                return localBoundaryPoint.X - minX;

            if (localBoundaryPoint.X == maxX)
                return width + localBoundaryPoint.Y - minY;

            if (localBoundaryPoint.Y == maxY)
                return width + height + maxX - localBoundaryPoint.X;

            return 2f * width + height + maxY - localBoundaryPoint.Y;
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

        private static void ValidateParameterOriginCoordinate(float parameterOrigin, float length)
        {
            if (float.IsNaN(parameterOrigin) || float.IsInfinity(parameterOrigin))
                throw new ArgumentOutOfRangeException(nameof(parameterOrigin), "Parameter origin coordinate must be finite.");

            if (parameterOrigin < 0f || parameterOrigin > length)
                throw new ArgumentOutOfRangeException(nameof(parameterOrigin), "Parameter origin coordinate must lie within the rectangle perimeter length.");
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
                GetDefaultParameterOriginCanonicalCoordinate(Size) + curveCoordinate,
                Length);
        }

        private static float GetDefaultParameterOriginCanonicalCoordinate(VectorXY size)
        {
            return size.X + size.Y * 0.5f;
        }

        private static float GetBoundaryLength(VectorXY size)
        {
            return 2f * (size.X + size.Y);
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
