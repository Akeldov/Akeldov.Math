using System;
using System.Collections.Generic;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    // Owns the sampled points shared by spline distance, traversal and projection operations.
    internal sealed class SplinePathApproximation
    {
        private readonly PointXYZ[] _approximation;
        private readonly double[] _coordinates;

        public SplinePathApproximation(PointXYZ[] points)
        {
            _approximation = points;
            _coordinates = new double[points.Length];
            for (int i = 1; i < points.Length; i++)
            {
                double dx = (double)points[i].X - points[i - 1].X;
                double dy = (double)points[i].Y - points[i - 1].Y;
                double dz = (double)points[i].Z - points[i - 1].Z;
                _coordinates[i] = _coordinates[i - 1] +
                    global::System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }

            Length = _coordinates[_coordinates.Length - 1];
        }

        // Keep the full precision here so public constructors can reject an unrepresentable length.
        public double Length { get; }

        public PointXYZ StartPoint => _approximation[0];

        public PointXYZ EndPoint => _approximation[_approximation.Length - 1];

        public PointXYZ GetPoint(float curveCoordinate)
        {
            if (curveCoordinate == 0f)
                return StartPoint;

            if (curveCoordinate == (float)Length)
                return EndPoint;

            int low = 0;
            int high = _coordinates.Length - 1;
            while (high - low > 1)
            {
                int middle = low + (high - low) / 2;
                if (_coordinates[middle] <= curveCoordinate)
                    low = middle;
                else
                    high = middle;
            }

            double amount =
                (curveCoordinate - _coordinates[low]) /
                (_coordinates[high] - _coordinates[low]);
            PointXYZ start = _approximation[low];
            PointXYZ end = _approximation[high];
            return new PointXYZ(
                (float)((1.0 - amount) * start.X + amount * end.X),
                (float)((1.0 - amount) * start.Y + amount * end.Y),
                (float)((1.0 - amount) * start.Z + amount * end.Z));
        }

        public ParameterizedCurveProjection ProjectWithParameter(PointXYZ point)
        {
            PointXYZ closestPoint = StartPoint;
            double closestSquaredDistance = double.PositiveInfinity;
            double closestCoordinate = 0d;
            for (int i = 1; i < _approximation.Length; i++)
            {
                PointXYZ start = _approximation[i - 1];
                PointXYZ end = _approximation[i];
                double dx = (double)end.X - start.X;
                double dy = (double)end.Y - start.Y;
                double dz = (double)end.Z - start.Z;
                double px = (double)point.X - start.X;
                double py = (double)point.Y - start.Y;
                double pz = (double)point.Z - start.Z;
                double squaredLength = dx * dx + dy * dy + dz * dz;
                double amount = squaredLength == 0d
                    ? 0d
                    : (px * dx + py * dy + pz * dz) / squaredLength;
                amount = global::System.Math.Max(0d, global::System.Math.Min(1d, amount));
                double x = (1.0 - amount) * start.X + amount * end.X;
                double y = (1.0 - amount) * start.Y + amount * end.Y;
                double z = (1.0 - amount) * start.Z + amount * end.Z;
                double distanceX = point.X - x;
                double distanceY = point.Y - y;
                double distanceZ = point.Z - z;
                double squaredDistance =
                    distanceX * distanceX +
                    distanceY * distanceY +
                    distanceZ * distanceZ;
                if (squaredDistance < closestSquaredDistance)
                {
                    closestSquaredDistance = squaredDistance;
                    closestPoint = new PointXYZ((float)x, (float)y, (float)z);
                    closestCoordinate =
                        _coordinates[i - 1] +
                        amount * (_coordinates[i] - _coordinates[i - 1]);
                }
            }

            double distance = global::System.Math.Sqrt(closestSquaredDistance);
            if (distance > float.MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(point),
                    "Projection distance must fit in a finite float.");

            return new ParameterizedCurveProjection(
                closestPoint,
                (float)closestCoordinate,
                (float)distance);
        }

        public List<ParameterizedSegment> Flatten()
        {
            var segments = new List<ParameterizedSegment>(_approximation.Length - 1);
            for (int i = 1; i < _approximation.Length; i++)
            {
                if (!_approximation[i - 1].Equals(_approximation[i]))
                {
                    segments.Add(new ParameterizedSegment(
                        _approximation[i - 1],
                        _approximation[i]));
                }
            }

            return segments;
        }
    }
}
