using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="OrientedRectangleContour"/>.
    /// </summary>
    public static class OrientedRectangleContourIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between an oriented rectangle contour and a ray using exact comparisons.
        /// </summary>
        /// <param name="source">The source oriented rectangle contour.</param>
        /// <param name="ray">The ray to intersect with the source contour.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the forward direction of the ray. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this OrientedRectangleContour source, Ray ray)
        {
            float localHalfWidth = source.Width * 0.5f;
            float localHalfHeight = source.Height * 0.5f;
            (float localOriginX, float localOriginY, bool isOnVerticalEdge, bool isOnHorizontalEdge) = GetLocalOrigin(source, ray.Origin, localHalfWidth, localHalfHeight);
            double originX = localOriginX;
            double originY = localOriginY;
            (double directionX, double directionY) = GetLocalDirection(source, ray);
            double halfWidth = localHalfWidth;
            double halfHeight = localHalfHeight;

            if ((directionX == 0d &&
                    isOnVerticalEdge &&
                    HasContinuousRayInterval(originY, directionY, -halfHeight, halfHeight)) ||
                (directionY == 0d &&
                    isOnHorizontalEdge &&
                    HasContinuousRayInterval(originX, directionX, -halfWidth, halfWidth)))
            {
                return new List<PointXY>();
            }

            double entryCoordinate = double.NegativeInfinity;
            double exitCoordinate = double.PositiveInfinity;

            if (!RestrictToSlab(originX, directionX, -halfWidth, halfWidth, ref entryCoordinate, ref exitCoordinate) ||
                !RestrictToSlab(originY, directionY, -halfHeight, halfHeight, ref entryCoordinate, ref exitCoordinate) ||
                exitCoordinate < 0d)
            {
                return new List<PointXY>();
            }

            var intersections = new List<PointXY>(2);
            if (entryCoordinate >= 0d)
                intersections.Add(GetPoint(ray, entryCoordinate));

            if (exitCoordinate >= 0d && exitCoordinate != entryCoordinate)
                intersections.Add(GetPoint(ray, exitCoordinate));

            return intersections;
        }

        private static (double X, double Y) GetLocalDirection(OrientedRectangleContour source, Ray ray)
        {
            VectorXY direction = ray.Direction;
            double directionX = Dot(direction, source.AxisX);
            double directionY = Dot(direction, source.AxisY);
            if (MatchesNormalizedAngle(ray.Angle, source.Rotation) ||
                MatchesNormalizedAngle(ray.Angle, source.Rotation + MathF.PI) ||
                MatchesNormalizedAngle(ray.Angle, source.Rotation - MathF.PI))
            {
                directionY = 0d;
            }
            else if (MatchesNormalizedAngle(ray.Angle, source.Rotation + MathF.PI * 0.5f) ||
                MatchesNormalizedAngle(ray.Angle, source.Rotation - MathF.PI * 0.5f) ||
                MatchesNormalizedAngle(ray.Angle, source.Rotation + MathF.PI * 1.5f) ||
                MatchesNormalizedAngle(ray.Angle, source.Rotation - MathF.PI * 1.5f))
            {
                directionX = 0d;
            }

            return (directionX, directionY);
        }

        private static (float X, float Y, bool IsOnVerticalEdge, bool IsOnHorizontalEdge) GetLocalOrigin(OrientedRectangleContour source, PointXY point, float halfWidth, float halfHeight)
        {
            if (point.Equals(source.BottomLeft))
                return (-halfWidth, -halfHeight, true, true);

            if (point.Equals(source.BottomRight))
                return (halfWidth, -halfHeight, true, true);

            if (point.Equals(source.TopLeft))
                return (-halfWidth, halfHeight, true, true);

            if (point.Equals(source.TopRight))
                return (halfWidth, halfHeight, true, true);

            VectorXY centeredOrigin = point - source.Center;
            float localX = VectorXY.Dot(centeredOrigin, source.AxisX);
            float localY = VectorXY.Dot(centeredOrigin, source.AxisY);
            bool isOnLeftEdge = IsOnVerticalBoundary(source, point, localY, -halfWidth);
            bool isOnRightEdge = IsOnVerticalBoundary(source, point, localY, halfWidth);
            bool isOnBottomEdge = IsOnHorizontalBoundary(source, point, localX, -halfHeight);
            bool isOnTopEdge = IsOnHorizontalBoundary(source, point, localX, halfHeight);

            if (isOnLeftEdge)
                localX = -halfWidth;
            else if (isOnRightEdge)
                localX = halfWidth;

            if (isOnBottomEdge)
                localY = -halfHeight;
            else if (isOnTopEdge)
                localY = halfHeight;

            return (localX, localY, isOnLeftEdge || isOnRightEdge, isOnBottomEdge || isOnTopEdge);
        }

        private static bool IsOnVerticalBoundary(OrientedRectangleContour source, PointXY point, float localY, float edgeX) =>
            (source.Center + source.AxisX * edgeX + source.AxisY * localY).Equals(point);

        private static bool IsOnHorizontalBoundary(OrientedRectangleContour source, PointXY point, float localX, float edgeY) =>
            (source.Center + source.AxisX * localX + source.AxisY * edgeY).Equals(point);

        private static bool MatchesNormalizedAngle(float angle, float referenceAngle) =>
            angle.NormalizeAngleRad() == referenceAngle.NormalizeAngleRad();

        private static bool RestrictToSlab(
            double origin,
            double direction,
            double minimum,
            double maximum,
            ref double entryCoordinate,
            ref double exitCoordinate)
        {
            if (direction == 0d)
                return origin >= minimum && origin <= maximum;

            double firstCoordinate = (minimum - origin) / direction;
            double secondCoordinate = (maximum - origin) / direction;
            if (firstCoordinate > secondCoordinate)
            {
                double temporary = firstCoordinate;
                firstCoordinate = secondCoordinate;
                secondCoordinate = temporary;
            }

            if (firstCoordinate > entryCoordinate)
                entryCoordinate = firstCoordinate;

            if (secondCoordinate < exitCoordinate)
                exitCoordinate = secondCoordinate;

            return entryCoordinate <= exitCoordinate;
        }

        private static bool HasContinuousRayInterval(
            double origin,
            double direction,
            double minimum,
            double maximum)
        {
            if (direction == 0d)
                return false;

            double firstCoordinate = (minimum - origin) / direction;
            double secondCoordinate = (maximum - origin) / direction;
            if (firstCoordinate > secondCoordinate)
            {
                double temporary = firstCoordinate;
                firstCoordinate = secondCoordinate;
                secondCoordinate = temporary;
            }

            double overlapStart = firstCoordinate > 0d ? firstCoordinate : 0d;
            return secondCoordinate > overlapStart;
        }

        private static PointXY GetPoint(Ray ray, double rayCoordinate)
        {
            VectorXY direction = ray.Direction;
            return new PointXY(
                (float)(ray.Origin.X + rayCoordinate * direction.X),
                (float)(ray.Origin.Y + rayCoordinate * direction.Y));
        }

        private static double Dot(VectorXY left, VectorXY right) =>
            (double)left.X * right.X + (double)left.Y * right.Y;
    }
}
