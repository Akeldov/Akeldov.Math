using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    // One bit per original 4x4 sample. Union before compositing preserves translucent joins.
    internal sealed class CurveStrokeCoverage
    {
        private readonly RasterGeometry _grid;
        private readonly float _radius;
        private readonly float _margin;

        internal CurveStrokeCoverage(RasterGeometry grid, float radius)
        {
            _grid = grid;
            _radius = radius;
            _margin = 8f * GeometryConstants.GeometryEpsilon * MathF.Max(1f,
                MathF.Max(MathF.Abs(grid.Origin.X) + grid.Size.X, MathF.Abs(grid.Origin.Y) + grid.Size.Y));
            Masks = new ushort[grid.Resolution.X * grid.Resolution.Y];
        }

        internal ushort[] Masks { get; }

        internal void Add(ICurve curve)
        {
            if (curve is ICompositeContour composite)
            {
                foreach (var child in composite.Curves)
                    Add(child);
                return;
            }
            if (curve is ParameterizedSegmentChain chain)
            {
                foreach (var segment in chain.Segments)
                    Add(segment);
                return;
            }
            GetPixelBounds(curve, out int x0, out int y0, out int x1, out int y1);
            const int tileSize = 8;
            for (int y = y0; y < y1; y += tileSize)
                for (int x = x0; x < x1; x += tileSize)
                    AddTile(curve, x, y, System.Math.Min(x + tileSize, x1), System.Math.Min(y + tileSize, y1));
        }

        private void AddTile(ICurve curve, int x0, int y0, int x1, int y1)
        {
            float halfX = (x1 - x0) * _grid.CellSize.X * 0.5f;
            float halfY = (y1 - y0) * _grid.CellSize.Y * 0.5f;
            var center = new PointXY(_grid.Origin.X + (x0 + (x1 - x0) * 0.5f) * _grid.CellSize.X,
                _grid.Origin.Y + (y0 + (y1 - y0) * 0.5f) * _grid.CellSize.Y);
            float reach = MathF.Sqrt(halfX * halfX + halfY * halfY) + _margin;
            float distance = curve.Distance(center);
            // Distance to a fixed set is 1-Lipschitz: the whole tile is outside this stroke.
            if (distance > _radius + reach)
                return;
            bool fullTile = distance + reach < _radius;
            float cellReach = MathF.Sqrt(_grid.CellSize.X * _grid.CellSize.X + _grid.CellSize.Y * _grid.CellSize.Y) * 0.5f + _margin;
            for (int y = y0; y < y1; y++)
            {
                for (int x = x0; x < x1; x++)
                {
                    int index = y * _grid.Resolution.X + x;
                    if (Masks[index] == ushort.MaxValue)
                        continue;
                    if (fullTile)
                        Masks[index] = ushort.MaxValue;
                    else
                        AddCell(curve, x, y, index, cellReach);
                }
            }
        }

        private void AddCell(ICurve curve, int x, int y, int index, float reach)
        {
            var center = new PointXY(_grid.Origin.X + (x + 0.5f) * _grid.CellSize.X,
                _grid.Origin.Y + (y + 0.5f) * _grid.CellSize.Y);
            float distance = curve.Distance(center);
            if (distance > _radius + reach)
                return;
            if (distance + reach < _radius)
            {
                Masks[index] = ushort.MaxValue;
                return;
            }
            ushort mask = Masks[index];
            for (int sy = 0; sy < 4; sy++)
            {
                for (int sx = 0; sx < 4; sx++)
                {
                    int bit = 1 << (sy * 4 + sx);
                    if ((mask & bit) != 0)
                        continue;
                    var point = new PointXY(_grid.Origin.X + (x + (sx + 0.5f) / 4) * _grid.CellSize.X,
                        _grid.Origin.Y + (y + (sy + 0.5f) / 4) * _grid.CellSize.Y);
                    if (curve.Distance(point) <= _radius)
                        mask |= (ushort)bit;
                }
            }
            Masks[index] = mask;
        }

        private void GetPixelBounds(ICurve curve, out int x0, out int y0, out int x1, out int y1)
        {
            x0 = y0 = 0;
            x1 = _grid.Resolution.X;
            y1 = _grid.Resolution.Y;
            PointXY[]? points = GetBoundPoints(curve);
            if (points == null)
                return; // Infinite and custom curves still benefit from tile rejection.
            float minX = points[0].X, maxX = minX, minY = points[0].Y, maxY = minY;
            foreach (var point in points)
            {
                minX = MathF.Min(minX, point.X); maxX = MathF.Max(maxX, point.X);
                minY = MathF.Min(minY, point.Y); maxY = MathF.Max(maxY, point.Y);
            }
            float padding = _radius + _margin;
            x0 = (int)MathF.Max(0, MathF.Min(x1, MathF.Floor((minX - padding - _grid.Origin.X) / _grid.CellSize.X)));
            y0 = (int)MathF.Max(0, MathF.Min(y1, MathF.Floor((minY - padding - _grid.Origin.Y) / _grid.CellSize.Y)));
            x1 = (int)MathF.Max(0, MathF.Min(x1, MathF.Ceiling((maxX + padding - _grid.Origin.X) / _grid.CellSize.X)));
            y1 = (int)MathF.Max(0, MathF.Min(y1, MathF.Ceiling((maxY + padding - _grid.Origin.Y) / _grid.CellSize.Y)));
        }

        private static PointXY[]? GetBoundPoints(ICurve curve)
        {
            switch (curve)
            {
                case Segment s: return new[] { s.EndpointA, s.EndpointB };
                case ParameterizedSegment s: return new[] { s.StartPoint, s.EndPoint };
                case QuadraticBezier b: return new[] { b.StartPoint, b.ControlPoint, b.EndPoint };
                case CubicBezier b: return new[] { b.StartPoint, b.ControlPointA, b.ControlPointB, b.EndPoint };
                case BSpline b: return CopyPoints(b.ControlPoints);
                case Nurbs n: return CopyPoints(n.ControlPoints);
                case Circle c: return CircleBounds(c.Center, c.Radius);
                case ParameterizedCircle c: return CircleBounds(c.Center, c.Radius);
                case Arc a: return CircleBounds(a.Center, a.Radius);
                case ParameterizedArc a: return CircleBounds(a.Center, a.Radius);
                case RectangleContour r: return new[] { r.Min, r.Max };
                case ParameterizedRectangleContour r: return new[] { r.Min, r.Max };
                case OrientedRectangleContour r: return new[] { r.BottomLeft, r.BottomRight, r.TopLeft, r.TopRight };
                case ParameterizedOrientedRectangleContour r: return new[] { r.BottomLeft, r.BottomRight, r.TopLeft, r.TopRight };
                default: return null;
            }
        }

        private static PointXY[] CircleBounds(PointXY center, float radius) => new[]
        {
            new PointXY(center.X - radius, center.Y - radius),
            new PointXY(center.X + radius, center.Y + radius),
        };

        private static PointXY[] CopyPoints(IReadOnlyList<PointXY> points)
        {
            var result = new PointXY[points.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = points[i];
            return result;
        }
    }
}
