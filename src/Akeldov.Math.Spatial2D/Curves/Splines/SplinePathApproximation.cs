using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Curves
{
    // Owns the sampled points shared by spline distance, traversal and contour operations.
    internal sealed class SplinePathApproximation
    {
        private readonly PointXY[] _approximation;
        private readonly double[] _coordinates;

        public SplinePathApproximation(PointXY[] points)
        {
            _approximation = points;
            _coordinates = new double[points.Length];
            for (int i = 1; i < points.Length; i++)
            {
                double dx = (double)points[i].X - points[i - 1].X;
                double dy = (double)points[i].Y - points[i - 1].Y;
                _coordinates[i] = _coordinates[i - 1] + System.Math.Sqrt(dx * dx + dy * dy);
            }

            Length = _coordinates[_coordinates.Length - 1];
        }

        // Keep the full precision here so public constructors can reject an unrepresentable length.
        public double Length { get; }
        public PointXY StartPoint => _approximation[0];
        public PointXY EndPoint => _approximation[_approximation.Length - 1];

        public PointXY GetPoint(float curveCoordinate)
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

            double amount = (curveCoordinate - _coordinates[low]) / (_coordinates[high] - _coordinates[low]);
            PointXY start = _approximation[low];
            PointXY end = _approximation[high];
            return new PointXY(
                (float)((1.0 - amount) * start.X + amount * end.X),
                (float)((1.0 - amount) * start.Y + amount * end.Y));
        }

        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            PointXY closestPoint = StartPoint;
            double closestSquaredDistance = double.PositiveInfinity;
            double closestCoordinate = 0.0;
            for (int i = 1; i < _approximation.Length; i++)
            {
                PointXY start = _approximation[i - 1];
                PointXY end = _approximation[i];
                double dx = (double)end.X - start.X;
                double dy = (double)end.Y - start.Y;
                double px = (double)point.X - start.X;
                double py = (double)point.Y - start.Y;
                double squaredLength = dx * dx + dy * dy;
                double amount = squaredLength == 0.0 ? 0.0 : (px * dx + py * dy) / squaredLength;
                amount = System.Math.Max(0.0, System.Math.Min(1.0, amount));
                double x = (1.0 - amount) * start.X + amount * end.X;
                double y = (1.0 - amount) * start.Y + amount * end.Y;
                double distanceX = point.X - x;
                double distanceY = point.Y - y;
                double squaredDistance = distanceX * distanceX + distanceY * distanceY;
                if (squaredDistance < closestSquaredDistance)
                {
                    closestSquaredDistance = squaredDistance;
                    closestPoint = new PointXY((float)x, (float)y);
                    closestCoordinate = _coordinates[i - 1] + amount * (_coordinates[i] - _coordinates[i - 1]);
                }
            }

            double distance = System.Math.Sqrt(closestSquaredDistance);
            if (distance > float.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(point), "Projection distance must fit in a finite float.");

            return new ParameterizedCurveProjection(closestPoint, (float)closestCoordinate, (float)distance);
        }

        public int CountRightwardCrossings(PointXY origin)
        {
            int count = 0;
            for (int i = 1; i < _approximation.Length; i++)
            {
                PointXY start = _approximation[i - 1];
                PointXY end = _approximation[i];
                if ((start.Y <= origin.Y && origin.Y < end.Y) || (end.Y <= origin.Y && origin.Y < start.Y))
                {
                    double x = start.X + ((double)origin.Y - start.Y) * ((double)end.X - start.X) / ((double)end.Y - start.Y);
                    if (x > origin.X)
                        count++;
                }
            }

            return count;
        }

        public List<ParameterizedSegment> Flatten()
        {
            var segments = new List<ParameterizedSegment>(_approximation.Length - 1);
            for (int i = 1; i < _approximation.Length; i++)
            {
                if (!_approximation[i - 1].Equals(_approximation[i]))
                    segments.Add(new ParameterizedSegment(_approximation[i - 1], _approximation[i]));
            }

            return segments;
        }
    }
}
