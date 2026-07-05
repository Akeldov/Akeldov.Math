using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk
{
    internal class Grid
    {
        private readonly VectorXY _fieldSize;
        private readonly float _cellSize;
        private readonly List<GridPoint>[,] _points;

        public Grid(VectorXY fieldSize, float cellSize)
        {
            _fieldSize = fieldSize;
            _cellSize = cellSize;

            int gridWidth = (int)MathF.Ceiling(fieldSize.X / cellSize);
            int gridHeight = (int)MathF.Ceiling(fieldSize.Y / cellSize);

            _points = new List<GridPoint>[gridWidth, gridHeight];
        }

        public bool TryAdd(PointXY point, float minimalDistance)
        {
            if (IsValidPoint(point, minimalDistance))
            {
                Add(point, minimalDistance);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsValidPoint(PointXY point, float minimalDistance)
        {
            if (!Contains(point))
                return false;

            int gx = (int)(point.X / _cellSize);
            int gy = (int)(point.Y / _cellSize);

            // _cellSize is the maximum possible minimal distance, so any point outside this 3x3
            // neighbourhood cannot be closer than the required pairwise distance.
            for (int i = (int)MathF.Max(0, gx - 1); i <= (int)MathF.Min(_points.GetLength(0) - 1, gx + 1); i++)
            {
                for (int j = (int)MathF.Max(0, gy - 1); j <= (int)MathF.Min(_points.GetLength(1) - 1, gy + 1); j++)
                {
                    var list = _points[i, j];
                    if (list != null)
                    {
                        for (int k = 0; k < list.Count; k++)
                        {
                            var otherPoint = list[k];
                            var requiredDistance = MathF.Max(minimalDistance, otherPoint.MinimalDistance);

                            if (point.Distance(otherPoint.Point) < requiredDistance)
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        public bool Contains(PointXY point)
        {
            return point.X >= 0 && point.X < _fieldSize.X && point.Y >= 0 && point.Y < _fieldSize.Y;
        }

        private void Add(PointXY point, float minimalDistance)
        {
            if (!Contains(point))
                throw new ArgumentOutOfRangeException(nameof(point));

            int gx = (int)(point.X / _cellSize);
            int gy = (int)(point.Y / _cellSize);
            if (_points[gx, gy] == null)
                _points[gx, gy] = new List<GridPoint>();
            _points[gx, gy].Add(new GridPoint(point, minimalDistance));
        }
    }
}
