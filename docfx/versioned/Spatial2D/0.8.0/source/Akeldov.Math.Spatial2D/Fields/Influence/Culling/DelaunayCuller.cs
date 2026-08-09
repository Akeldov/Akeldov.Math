using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Selects the Delaunay triangle containing the sampled point.
    /// </summary>
    /// <remarks>
    /// When the sampled point is outside the triangulated area, this culler returns the nearest
    /// convex hull feature: a single hull vertex or the two endpoints of a hull edge. When the
    /// source points cannot form a non-degenerate hull, it falls back to half-plane culling and
    /// keeps at most two sources.
    /// </remarks>
    /// <typeparam name="TPointSource">The point influence source type.</typeparam>
    public sealed class DelaunayCuller<TPointSource> : IInfluenceSourceCuller<TPointSource>
        where TPointSource : IPointInfluenceSource
    {
        private const float Epsilon = GeometryConstants.GeometryEpsilon;

        private readonly TPointSource[] _sources;
        private readonly PointXY[] _positions;
        private readonly Triangle[] _triangles;
        private readonly TriangleSpatialIndex? _triangleIndex;
        private readonly int[] _convexHull;
        private readonly HalfPlaneCuller<TPointSource> _fallbackCuller;

        /// <summary>
        /// Initializes a new Delaunay influence source culler.
        /// </summary>
        /// <param name="pointSources">The point influence sources used to build the triangulation.</param>
        public DelaunayCuller(IReadOnlyList<TPointSource> pointSources)
        {
            _sources = CopySources(pointSources, out _positions);
            ThrowIfAnyDuplicatePoint(_positions, nameof(pointSources));

            _fallbackCuller = new HalfPlaneCuller<TPointSource>(_sources);

            if (!HasNonCollinearTriple(_positions))
            {
                _convexHull = Array.Empty<int>();
                _triangles = Array.Empty<Triangle>();
                _triangleIndex = null;
                return;
            }

            _convexHull = BuildConvexHull(_positions);
            _triangles = BuildDelaunayTriangulation(_positions);
            _triangleIndex = _triangles.Length == 0
                ? null
                : new TriangleSpatialIndex(_triangles, _positions);
        }

        /// <summary>
        /// Returns influence sources selected for the specified point.
        /// </summary>
        /// <param name="point">The point being sampled.</param>
        /// <returns>
        /// A new mutable list owned by the caller containing the selected triangle sources,
        /// nearest hull feature sources, or fallback sources.
        /// </returns>
        public List<TPointSource> Cull(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            if (_triangleIndex != null &&
                _triangleIndex.TryFindContainingTriangle(point, out Triangle triangle))
            {
                return CreateTriangleSourceList(triangle);
            }

            if (_convexHull.Length >= 3)
                return CullToNearestHullFeature(point);

            return CullCollinearFallback(point);
        }

        private static TPointSource[] CopySources(
            IReadOnlyList<TPointSource> pointSources,
            out PointXY[] positions)
        {
            if (pointSources == null)
                throw new ArgumentNullException(nameof(pointSources));

            if (pointSources.Count < 3)
                throw new ArgumentException("At least three influence points are required.", nameof(pointSources));

            var sources = new TPointSource[pointSources.Count];
            positions = new PointXY[pointSources.Count];

            for (int i = 0; i < pointSources.Count; i++)
            {
                TPointSource source = pointSources[i];
                if (source is null)
                    throw new ArgumentException("Influence source collection cannot contain null elements.", nameof(pointSources));

                if (!PointXYValidation.IsFinite(source.Position))
                    throw new ArgumentException("Influence source positions must be finite.", nameof(pointSources));

                sources[i] = source;
                positions[i] = source.Position;
            }

            return sources;
        }

        private List<TPointSource> CreateTriangleSourceList(Triangle triangle)
        {
            return new List<TPointSource>(3)
            {
                _sources[triangle.A],
                _sources[triangle.B],
                _sources[triangle.C]
            };
        }

        private List<TPointSource> CullToNearestHullFeature(PointXY point)
        {
            HullEdge nearestEdge = FindNearestHullEdge(point);

            if (nearestEdge.ProjectionParameter <= Epsilon)
                return new List<TPointSource>(1) { _sources[nearestEdge.StartIndex] };

            if (nearestEdge.ProjectionParameter >= 1f - Epsilon)
                return new List<TPointSource>(1) { _sources[nearestEdge.EndIndex] };

            return new List<TPointSource>(2)
            {
                _sources[nearestEdge.StartIndex],
                _sources[nearestEdge.EndIndex]
            };
        }

        private HullEdge FindNearestHullEdge(PointXY point)
        {
            HullEdge nearestEdge = MeasureHullEdge(0, point);

            for (int hullIndex = 1; hullIndex < _convexHull.Length; hullIndex++)
            {
                HullEdge edge = MeasureHullEdge(hullIndex, point);
                if (edge.DistanceSquared < nearestEdge.DistanceSquared)
                    nearestEdge = edge;
            }

            return nearestEdge;
        }

        private HullEdge MeasureHullEdge(int hullStartIndex, PointXY point)
        {
            int startIndex = _convexHull[hullStartIndex];
            int endIndex = _convexHull[(hullStartIndex + 1) % _convexHull.Length];
            float distanceSquared = DistanceToSegmentSquared(
                _positions[startIndex],
                _positions[endIndex],
                point,
                out float projectionParameter);

            return new HullEdge(startIndex, endIndex, projectionParameter, distanceSquared);
        }

        private List<TPointSource> CullCollinearFallback(PointXY point)
        {
            List<TPointSource> selectedSources = _fallbackCuller.Cull(point);
            return KeepNearestFallbackSegment(selectedSources, point);
        }

        private static List<TPointSource> KeepNearestFallbackSegment(List<TPointSource> sources, PointXY point)
        {
            if (sources.Count <= 2)
                return sources;

            int bestStartIndex = 0;
            int bestEndIndex = 1;
            float bestDistanceSquared = DistanceToSegmentSquared(sources[0].Position, sources[1].Position, point);

            for (int startIndex = 0; startIndex < sources.Count; startIndex++)
            {
                for (int endIndex = startIndex + 1; endIndex < sources.Count; endIndex++)
                {
                    float distanceSquared = DistanceToSegmentSquared(
                        sources[startIndex].Position,
                        sources[endIndex].Position,
                        point);

                    if (distanceSquared < bestDistanceSquared)
                    {
                        bestStartIndex = startIndex;
                        bestEndIndex = endIndex;
                        bestDistanceSquared = distanceSquared;
                    }
                }
            }

            return new List<TPointSource>(2)
            {
                sources[bestStartIndex],
                sources[bestEndIndex]
            };
        }

        private static Triangle[] BuildDelaunayTriangulation(PointXY[] sourcePositions)
        {
            int sourceCount = sourcePositions.Length;
            PointXY[] triangulationPoints = CreatePointSetWithSuperTriangle(sourcePositions);

            var triangles = new List<Triangle>();
            if (Triangle.TryCreate(
                sourceCount,
                sourceCount + 1,
                sourceCount + 2,
                triangulationPoints,
                out Triangle superTriangle))
            {
                triangles.Add(superTriangle);
            }

            if (triangles.Count == 0)
                return Array.Empty<Triangle>();

            var keptTriangles = new List<Triangle>();
            var cavityEdgeCounts = new Dictionary<Edge, int>();

            for (int pointIndex = 0; pointIndex < sourceCount; pointIndex++)
            {
                InsertPointIntoTriangulation(
                    pointIndex,
                    triangulationPoints,
                    ref triangles,
                    ref keptTriangles,
                    cavityEdgeCounts);
            }

            triangles.RemoveAll(triangle =>
                triangle.A >= sourceCount ||
                triangle.B >= sourceCount ||
                triangle.C >= sourceCount);

            return triangles.ToArray();
        }

        private static PointXY[] CreatePointSetWithSuperTriangle(PointXY[] sourcePositions)
        {
            int sourceCount = sourcePositions.Length;
            var points = new PointXY[sourceCount + 3];
            Array.Copy(sourcePositions, points, sourceCount);

            (PointXY first, PointXY second, PointXY third) = CreateSuperTriangle(sourcePositions);
            points[sourceCount] = first;
            points[sourceCount + 1] = second;
            points[sourceCount + 2] = third;

            return points;
        }

        private static void InsertPointIntoTriangulation(
            int pointIndex,
            PointXY[] points,
            ref List<Triangle> triangles,
            ref List<Triangle> keptTriangles,
            Dictionary<Edge, int> cavityEdgeCounts)
        {
            keptTriangles.Clear();
            cavityEdgeCounts.Clear();

            for (int triangleIndex = 0; triangleIndex < triangles.Count; triangleIndex++)
            {
                Triangle triangle = triangles[triangleIndex];
                if (triangle.ContainsInCircumcircle(points[pointIndex]))
                    AddCavityEdges(cavityEdgeCounts, triangle);
                else
                    keptTriangles.Add(triangle);
            }

            List<Triangle> oldTriangles = triangles;
            triangles = keptTriangles;
            keptTriangles = oldTriangles;

            foreach (KeyValuePair<Edge, int> edgeCount in cavityEdgeCounts)
            {
                if (edgeCount.Value != 1)
                    continue;

                if (Triangle.TryCreate(edgeCount.Key.First, edgeCount.Key.Second, pointIndex, points, out Triangle triangle))
                    triangles.Add(triangle);
            }
        }

        private static void AddCavityEdges(Dictionary<Edge, int> edgeCounts, Triangle triangle)
        {
            AddEdge(edgeCounts, triangle.A, triangle.B);
            AddEdge(edgeCounts, triangle.B, triangle.C);
            AddEdge(edgeCounts, triangle.C, triangle.A);
        }

        private static void AddEdge(Dictionary<Edge, int> edgeCounts, int first, int second)
        {
            var edge = new Edge(first, second);
            if (edgeCounts.TryGetValue(edge, out int count))
                edgeCounts[edge] = count + 1;
            else
                edgeCounts.Add(edge, 1);
        }

        private static int[] BuildConvexHull(PointXY[] points)
        {
            int startIndex = FindLeftmostLowestPointIndex(points);
            var hull = new List<int>();
            int currentIndex = startIndex;

            do
            {
                hull.Add(currentIndex);
                currentIndex = FindNextHullPointIndex(points, currentIndex);
            }
            while (currentIndex != startIndex && hull.Count <= points.Length);

            return hull.ToArray();
        }

        private static int FindLeftmostLowestPointIndex(PointXY[] points)
        {
            int leftmostLowestIndex = 0;

            for (int i = 1; i < points.Length; i++)
            {
                PointXY candidate = points[i];
                PointXY current = points[leftmostLowestIndex];
                if (candidate.X < current.X || (candidate.X == current.X && candidate.Y < current.Y))
                    leftmostLowestIndex = i;
            }

            return leftmostLowestIndex;
        }

        private static int FindNextHullPointIndex(PointXY[] points, int currentIndex)
        {
            int nextIndex = currentIndex == 0 ? 1 : 0;

            for (int candidateIndex = 0; candidateIndex < points.Length; candidateIndex++)
            {
                if (candidateIndex == currentIndex)
                    continue;

                PointXY current = points[currentIndex];
                PointXY next = points[nextIndex];
                PointXY candidate = points[candidateIndex];
                float orientation = Cross(next - current, candidate - current);

                if (orientation < -Epsilon ||
                    (orientation.IsAlmostZero() && SquaredDistance(current, candidate) > SquaredDistance(current, next)))
                {
                    nextIndex = candidateIndex;
                }
            }

            return nextIndex;
        }

        private static (PointXY first, PointXY second, PointXY third) CreateSuperTriangle(PointXY[] points)
        {
            GetPointBounds(points, out float minX, out float minY, out float maxX, out float maxY);

            float largestExtent = MathF.Max(maxX - minX, maxY - minY);
            float centerX = (minX + maxX) * 0.5f;
            float centerY = (minY + maxY) * 0.5f;

            return (
                new PointXY(centerX - 20f * largestExtent, centerY - 10f * largestExtent),
                new PointXY(centerX, centerY + 20f * largestExtent),
                new PointXY(centerX + 20f * largestExtent, centerY - 10f * largestExtent)
            );
        }

        private static void ThrowIfAnyDuplicatePoint(PointXY[] points, string paramName)
        {
            for (int i = 0; i < points.Length; i++)
            {
                for (int j = i + 1; j < points.Length; j++)
                {
                    if (points[i].AlmostEquals(points[j]))
                        throw new ArgumentException("Influence points must have distinct centers.", paramName);
                }
            }
        }

        private static bool HasNonCollinearTriple(PointXY[] points)
        {
            PointXY first = points[0];
            PointXY second = points[1];

            for (int i = 2; i < points.Length; i++)
            {
                if (!Cross(second - first, points[i] - first).IsAlmostZero())
                    return true;
            }

            return false;
        }

        private static bool PointInTriangle(PointXY point, Triangle triangle, PointXY[] points)
        {
            PointXY a = points[triangle.A];
            PointXY b = points[triangle.B];
            PointXY c = points[triangle.C];

            float abSide = Cross(b - a, point - a);
            float bcSide = Cross(c - b, point - b);
            float caSide = Cross(a - c, point - c);

            bool hasNegativeSide = abSide < -Epsilon || bcSide < -Epsilon || caSide < -Epsilon;
            bool hasPositiveSide = abSide > Epsilon || bcSide > Epsilon || caSide > Epsilon;

            return !(hasNegativeSide && hasPositiveSide);
        }

        private static float Cross(VectorXY left, VectorXY right)
        {
            return left.X * right.Y - left.Y * right.X;
        }

        private static float SquaredDistance(PointXY a, PointXY b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        private static float DistanceToSegmentSquared(PointXY start, PointXY end, PointXY point)
        {
            return DistanceToSegmentSquared(start, end, point, out _);
        }

        private static float DistanceToSegmentSquared(
            PointXY start,
            PointXY end,
            PointXY point,
            out float projectionParameter)
        {
            VectorXY segment = end - start;
            float segmentLengthSquared = segment.SquaredLength;
            if (segmentLengthSquared <= GeometryConstants.GeometryEpsilonSquared)
            {
                projectionParameter = 0f;
                return SquaredDistance(start, point);
            }

            projectionParameter = VectorXY.Dot(point - start, segment) / segmentLengthSquared;
            if (projectionParameter < 0f)
                projectionParameter = 0f;
            else if (projectionParameter > 1f)
                projectionParameter = 1f;

            PointXY projection = start + projectionParameter * segment;
            return SquaredDistance(projection, point);
        }

        private static bool TryCreateCircumcircle(
            PointXY a,
            PointXY b,
            PointXY c,
            out PointXY center,
            out float radiusSquared)
        {
            float ax = a.X;
            float ay = a.Y;
            float bx = b.X;
            float by = b.Y;
            float cx = c.X;
            float cy = c.Y;

            float denominator = 2f * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
            if (denominator.IsAlmostZero())
            {
                center = default;
                radiusSquared = 0f;
                return false;
            }

            float aLengthSquared = ax * ax + ay * ay;
            float bLengthSquared = bx * bx + by * by;
            float cLengthSquared = cx * cx + cy * cy;

            float centerX = (aLengthSquared * (by - cy) +
                             bLengthSquared * (cy - ay) +
                             cLengthSquared * (ay - by)) / denominator;

            float centerY = (aLengthSquared * (cx - bx) +
                             bLengthSquared * (ax - cx) +
                             cLengthSquared * (bx - ax)) / denominator;

            center = new PointXY(centerX, centerY);
            radiusSquared = SquaredDistance(center, a);
            return true;
        }

        private static void GetPointBounds(
            PointXY[] points,
            out float minX,
            out float minY,
            out float maxX,
            out float maxY)
        {
            minX = points[0].X;
            minY = points[0].Y;
            maxX = minX;
            maxY = minY;

            for (int i = 1; i < points.Length; i++)
            {
                PointXY point = points[i];
                if (point.X < minX) minX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.X > maxX) maxX = point.X;
                if (point.Y > maxY) maxY = point.Y;
            }
        }

        private readonly struct HullEdge
        {
            public HullEdge(int startIndex, int endIndex, float projectionParameter, float distanceSquared)
            {
                StartIndex = startIndex;
                EndIndex = endIndex;
                ProjectionParameter = projectionParameter;
                DistanceSquared = distanceSquared;
            }

            public int StartIndex { get; }

            public int EndIndex { get; }

            public float ProjectionParameter { get; }

            public float DistanceSquared { get; }
        }

        private readonly struct Triangle
        {
            private readonly PointXY _circumcenter;
            private readonly float _circumradiusSquared;

            private Triangle(int a, int b, int c, PointXY circumcenter, float circumradiusSquared)
            {
                A = a;
                B = b;
                C = c;
                _circumcenter = circumcenter;
                _circumradiusSquared = circumradiusSquared;
            }

            public int A { get; }

            public int B { get; }

            public int C { get; }

            public static bool TryCreate(int a, int b, int c, PointXY[] points, out Triangle triangle)
            {
                if (!TryCreateCircumcircle(points[a], points[b], points[c], out PointXY center, out float radiusSquared))
                {
                    triangle = default;
                    return false;
                }

                triangle = new Triangle(a, b, c, center, radiusSquared);
                return true;
            }

            public bool ContainsInCircumcircle(PointXY point)
            {
                return SquaredDistance(point, _circumcenter) <= _circumradiusSquared + GeometryConstants.GeometryEpsilon;
            }
        }

        private readonly struct Edge : IEquatable<Edge>
        {
            public Edge(int first, int second)
            {
                if (first < second)
                {
                    First = first;
                    Second = second;
                }
                else
                {
                    First = second;
                    Second = first;
                }
            }

            public int First { get; }

            public int Second { get; }

            public bool Equals(Edge other)
            {
                return First == other.First && Second == other.Second;
            }

            public override bool Equals(object? obj)
            {
                return obj is Edge other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(First, Second);
            }
        }

        private sealed class TriangleSpatialIndex
        {
            // Candidate lists preserve triangle array order, matching the old linear scan on shared edges.
            private readonly Triangle[] _triangles;
            private readonly PointXY[] _points;
            private readonly int[][] _cells;
            private readonly int _cellCountX;
            private readonly int _cellCountY;
            private readonly float _minX;
            private readonly float _minY;
            private readonly float _maxX;
            private readonly float _maxY;
            private readonly float _inverseCellWidth;
            private readonly float _inverseCellHeight;

            public TriangleSpatialIndex(Triangle[] triangles, PointXY[] points)
            {
                _triangles = triangles;
                _points = points;

                GetPointBounds(points, out _minX, out _minY, out _maxX, out _maxY);

                int cellCount = GetCellCount(triangles.Length);
                _cellCountX = cellCount;
                _cellCountY = cellCount;
                _inverseCellWidth = _cellCountX / (_maxX - _minX);
                _inverseCellHeight = _cellCountY / (_maxY - _minY);

                _cells = BuildCells(triangles);
            }

            public bool TryFindContainingTriangle(PointXY point, out Triangle triangle)
            {
                if (!TryGetCellIndex(point, out int cellIndex))
                {
                    triangle = default;
                    return false;
                }

                int[] candidateIndexes = _cells[cellIndex];
                for (int i = 0; i < candidateIndexes.Length; i++)
                {
                    Triangle candidate = _triangles[candidateIndexes[i]];
                    if (PointInTriangle(point, candidate, _points))
                    {
                        triangle = candidate;
                        return true;
                    }
                }

                triangle = default;
                return false;
            }

            private int[][] BuildCells(Triangle[] triangles)
            {
                var cellLists = new List<int>[_cellCountX * _cellCountY];

                for (int triangleIndex = 0; triangleIndex < triangles.Length; triangleIndex++)
                    AddTriangleToTouchedCells(cellLists, triangleIndex, triangles[triangleIndex]);

                var cells = new int[cellLists.Length][];
                for (int i = 0; i < cellLists.Length; i++)
                    cells[i] = cellLists[i]?.ToArray() ?? Array.Empty<int>();

                return cells;
            }

            private void AddTriangleToTouchedCells(
                List<int>[] cellLists,
                int triangleIndex,
                Triangle triangle)
            {
                GetTriangleBounds(
                    triangle,
                    out float triangleMinX,
                    out float triangleMinY,
                    out float triangleMaxX,
                    out float triangleMaxY);

                int minCellX = GetCellCoordinateX(triangleMinX);
                int maxCellX = GetCellCoordinateX(triangleMaxX);
                int minCellY = GetCellCoordinateY(triangleMinY);
                int maxCellY = GetCellCoordinateY(triangleMaxY);

                for (int y = minCellY; y <= maxCellY; y++)
                {
                    for (int x = minCellX; x <= maxCellX; x++)
                    {
                        int cellIndex = y * _cellCountX + x;
                        if (cellLists[cellIndex] == null)
                            cellLists[cellIndex] = new List<int>();

                        cellLists[cellIndex].Add(triangleIndex);
                    }
                }
            }

            private bool TryGetCellIndex(PointXY point, out int cellIndex)
            {
                if (point.X < _minX || point.X > _maxX ||
                    point.Y < _minY || point.Y > _maxY)
                {
                    cellIndex = -1;
                    return false;
                }

                int cellX = GetCellCoordinateX(point.X);
                int cellY = GetCellCoordinateY(point.Y);
                cellIndex = cellY * _cellCountX + cellX;
                return true;
            }

            private int GetCellCoordinateX(float x)
            {
                int coordinate = (int)((x - _minX) * _inverseCellWidth);
                return ClampCellCoordinate(coordinate, _cellCountX);
            }

            private int GetCellCoordinateY(float y)
            {
                int coordinate = (int)((y - _minY) * _inverseCellHeight);
                return ClampCellCoordinate(coordinate, _cellCountY);
            }

            private void GetTriangleBounds(
                Triangle triangle,
                out float minX,
                out float minY,
                out float maxX,
                out float maxY)
            {
                PointXY a = _points[triangle.A];
                PointXY b = _points[triangle.B];
                PointXY c = _points[triangle.C];

                minX = MathF.Min(a.X, MathF.Min(b.X, c.X));
                minY = MathF.Min(a.Y, MathF.Min(b.Y, c.Y));
                maxX = MathF.Max(a.X, MathF.Max(b.X, c.X));
                maxY = MathF.Max(a.Y, MathF.Max(b.Y, c.Y));
            }

            private static int ClampCellCoordinate(int coordinate, int cellCount)
            {
                if (coordinate < 0)
                    return 0;

                if (coordinate >= cellCount)
                    return cellCount - 1;

                return coordinate;
            }

            private static int GetCellCount(int triangleCount)
            {
                int cellCount = (int)MathF.Ceiling(MathF.Sqrt(triangleCount));

                if (cellCount < 4)
                    return 4;

                if (cellCount > 64)
                    return 64;

                return cellCount;
            }
        }
    }
}
