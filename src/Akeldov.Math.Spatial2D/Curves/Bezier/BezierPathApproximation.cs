using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Curves
{
    internal static class BezierPathApproximation
    {
        public const int DefaultSegmentCount = 64;

        public static List<ParameterizedSegment> Flatten(
            Func<float, PointXY> pointAt,
            int segmentCount)
        {
            ValidateSegmentCount(segmentCount, nameof(segmentCount));

            var segments = new List<ParameterizedSegment>(segmentCount);
            PointXY previous = pointAt(0f);

            for (int i = 1; i <= segmentCount; i++)
            {
                PointXY current = pointAt(i / (float)segmentCount);
                if (!current.Equals(previous))
                    segments.Add(new ParameterizedSegment(previous, current));

                previous = current;
            }

            return segments;
        }

        public static float GetLength(Func<float, PointXY> pointAt)
        {
            PointXY previous = pointAt(0f);
            float length = 0f;

            for (int i = 1; i <= DefaultSegmentCount; i++)
            {
                PointXY current = pointAt(i / (float)DefaultSegmentCount);
                length += previous.Distance(current);
                previous = current;
            }

            return length;
        }

        public static PointXY GetPoint(
            Func<float, PointXY> pointAt,
            float length,
            float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            if (curveCoordinate < 0f || curveCoordinate > length)
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must lie within the Bezier curve length.");

            if (length == 0f)
                return pointAt(0f);

            PointXY previous = pointAt(0f);
            float previousCoordinate = 0f;

            for (int i = 1; i <= DefaultSegmentCount; i++)
            {
                float currentParameter = i / (float)DefaultSegmentCount;
                PointXY current = pointAt(currentParameter);
                float segmentLength = previous.Distance(current);
                float currentCoordinate = previousCoordinate + segmentLength;

                if (curveCoordinate <= currentCoordinate || i == DefaultSegmentCount)
                {
                    if (segmentLength == 0f)
                        return current;

                    float segmentParameter = (curveCoordinate - previousCoordinate) / segmentLength;
                    float bezierParameter = (i - 1 + segmentParameter) / DefaultSegmentCount;
                    return pointAt(bezierParameter);
                }

                previous = current;
                previousCoordinate = currentCoordinate;
            }

            return pointAt(1f);
        }

        public static ParameterizedCurveProjection ProjectWithParameter(
            Func<float, PointXY> pointAt,
            PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            PointXY previous = pointAt(0f);
            float previousCoordinate = 0f;
            var closestProjection = new ParameterizedCurveProjection(previous, 0f, point.Distance(previous));

            for (int i = 1; i <= DefaultSegmentCount; i++)
            {
                PointXY current = pointAt(i / (float)DefaultSegmentCount);
                var segment = new ParameterizedSegment(previous, current);
                ParameterizedCurveProjection projection = segment.ProjectWithParameter(point);
                var curveProjection = new ParameterizedCurveProjection(
                    projection.ProjectedPoint,
                    previousCoordinate + projection.CurveCoordinate,
                    projection.Distance);

                if (curveProjection.Distance < closestProjection.Distance)
                    closestProjection = curveProjection;

                previousCoordinate += segment.Length;
                previous = current;
            }

            return closestProjection;
        }

        public static List<PointXY> GetRayIntersections(
            Func<float, PointXY> pointAt,
            Ray ray,
            float geometryEpsilon)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            var intersections = new List<PointXY>();
            PointXY previous = pointAt(0f);

            for (int i = 1; i <= DefaultSegmentCount; i++)
            {
                PointXY current = pointAt(i / (float)DefaultSegmentCount);
                var segment = new ParameterizedSegment(previous, current);
                List<PointXY> segmentIntersections = segment.GetRayIntersections(ray, geometryEpsilon);

                for (int intersectionIndex = 0; intersectionIndex < segmentIntersections.Count; intersectionIndex++)
                {
                    intersections.AddDistinct(segmentIntersections[intersectionIndex], geometryEpsilon);
                }

                previous = current;
            }

            intersections.Sort((left, right) =>
                VectorXY.Dot(left - ray.Origin, ray.Direction).CompareTo(
                    VectorXY.Dot(right - ray.Origin, ray.Direction)));

            return intersections;
        }

        private static void ValidateSegmentCount(int segmentCount, string parameterName)
        {
            if (segmentCount <= 0)
                throw new ArgumentOutOfRangeException(parameterName, "Bezier flattening segment count must be positive.");
        }
    }
}
