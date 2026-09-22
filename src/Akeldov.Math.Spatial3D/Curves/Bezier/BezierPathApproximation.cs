using System;
using System.Collections.Generic;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    internal static class BezierPathApproximation
    {
        public const int DefaultSegmentCount = 64;

        public static List<ParameterizedSegment> Flatten(
            Func<float, PointXYZ> pointAt,
            int segmentCount)
        {
            if (segmentCount <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(segmentCount),
                    "Bezier flattening segment count must be positive.");

            var segments = new List<ParameterizedSegment>(segmentCount);
            PointXYZ previous = pointAt(0f);

            for (int i = 1; i <= segmentCount; i++)
            {
                PointXYZ current = pointAt(i / (float)segmentCount);
                if (!current.Equals(previous))
                    segments.Add(new ParameterizedSegment(previous, current));

                previous = current;
            }

            return segments;
        }

        public static float GetLength(Func<float, PointXYZ> pointAt)
        {
            PointXYZ previous = pointAt(0f);
            double length = 0d;

            for (int i = 1; i <= DefaultSegmentCount; i++)
            {
                PointXYZ current = pointAt(i / (float)DefaultSegmentCount);
                length += GetDistance(previous, current);
                previous = current;
            }

            return (float)length;
        }

        public static PointXYZ GetPoint(
            Func<float, PointXYZ> pointAt,
            float length,
            float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            if (curveCoordinate < 0f || curveCoordinate > length)
                throw new ArgumentOutOfRangeException(
                    nameof(curveCoordinate),
                    "Curve coordinate must lie within the Bezier curve length.");

            if (curveCoordinate == 0f || length == 0f)
                return pointAt(0f);

            if (curveCoordinate == length)
                return pointAt(1f);

            PointXYZ previous = pointAt(0f);
            double previousCoordinate = 0d;

            for (int i = 1; i <= DefaultSegmentCount; i++)
            {
                PointXYZ current = pointAt(i / (float)DefaultSegmentCount);
                double segmentLength = GetDistance(previous, current);
                double currentCoordinate = previousCoordinate + segmentLength;

                if (curveCoordinate <= currentCoordinate || i == DefaultSegmentCount)
                {
                    if (segmentLength == 0d)
                        return current;

                    double segmentParameter = (curveCoordinate - previousCoordinate) / segmentLength;
                    float bezierParameter = (float)((i - 1 + segmentParameter) / DefaultSegmentCount);
                    return pointAt(bezierParameter);
                }

                previous = current;
                previousCoordinate = currentCoordinate;
            }

            return pointAt(1f);
        }

        public static ParameterizedCurveProjection ProjectWithParameter(
            Func<float, PointXYZ> pointAt,
            PointXYZ point)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");

            PointXYZ previous = pointAt(0f);
            double previousCoordinate = 0d;
            var closestProjection = new ParameterizedCurveProjection(
                previous,
                0f,
                (float)GetDistance(point, previous));

            for (int i = 1; i <= DefaultSegmentCount; i++)
            {
                PointXYZ current = pointAt(i / (float)DefaultSegmentCount);
                var segment = new ParameterizedSegment(previous, current);
                ParameterizedCurveProjection projection = segment.ProjectWithParameter(point);
                var curveProjection = new ParameterizedCurveProjection(
                    projection.ProjectedPoint,
                    (float)(previousCoordinate + projection.CurveCoordinate),
                    projection.Distance);

                if (curveProjection.Distance < closestProjection.Distance)
                    closestProjection = curveProjection;

                previousCoordinate += GetDistance(previous, current);
                previous = current;
            }

            return closestProjection;
        }

        private static double GetDistance(PointXYZ left, PointXYZ right)
        {
            double dx = (double)right.X - left.X;
            double dy = (double)right.Y - left.Y;
            double dz = (double)right.Z - left.Z;

            return global::System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
