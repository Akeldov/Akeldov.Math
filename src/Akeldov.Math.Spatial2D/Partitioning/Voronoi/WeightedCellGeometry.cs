using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;
using static System.Math;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    // Equal-weight competitors have already clipped the rectangle to a convex polygon.
    // Split its edges and the Apollonius circles at intersections. Between consecutive cuts
    // every other constraint has constant sign, so one midpoint determines each boundary piece.
    internal sealed class WeightedCellGeometry
    {
        private const double CutTolerance = 1.5e-14; // About 64 double-precision ulps at 1.

        private readonly List<PointXY> _polygon;
        private readonly List<BisectorCircle> _circles;
        private readonly List<Cut>[] _edgeCuts, _circleCuts;
        private readonly List<IContour> _contours = new List<IContour>();
        private readonly Dictionary<PointXY, Stack<IContourPath>> _outgoing = new Dictionary<PointXY, Stack<IContourPath>>();
        private readonly CellAreaMoments _moments;
        private readonly Dictionary<PointXY, PointXY> _coincident = new Dictionary<PointXY, PointXY>();

        private WeightedCellGeometry(Rectangle bounds, List<PointXY> polygon, List<BisectorCircle> circles, CellAreaMoments moments)
        {
            _polygon = polygon;
            _circles = circles;
            _moments = moments;
            _edgeCuts = new List<Cut>[polygon.Count];
            _circleCuts = new List<Cut>[circles.Count];
            for (int i = 0; i < circles.Count; i++)
            {
                var cuts = new List<Cut>();
                for (int quadrant = 0; quadrant < 4; quadrant++)
                {
                    bool inBounds = circles[i].TryGetCardinal(quadrant, bounds, out double x, out double y);
                    cuts.Add(new Cut(x, y, quadrant, isCardinal: true, outside: !inBounds));
                }
                _circleCuts[i] = cuts;
            }
        }

        internal static VoronoiCell Build(Site site, Rectangle bounds, List<PointXY> polygon, List<BisectorCircle> circles)
        {
            if (polygon.Count < 3)
                return new VoronoiCell(site, null, default);

            var moments = new CellAreaMoments(polygon[0]);
            for (int i = 0; i < polygon.Count; i++)
                moments.AddChord(polygon[i], polygon[(i + 1) % polygon.Count]);
            if (moments.Area <= 0)
                return new VoronoiCell(site, null, default);
            if (circles.Count == 0)
            {
                var polygonRegion = new ContourBasedRegion(new IContour[] { new CompositeContour(polygon) });
                return new VoronoiCell(site, polygonRegion, moments.Centroid);
            }

            var origin = new PointXY(Max(bounds.Min.X, Min(bounds.Max.X, site.Position.X)),
                Max(bounds.Min.Y, Min(bounds.Max.Y, site.Position.Y)));
            var geometry = new WeightedCellGeometry(bounds, polygon, circles, new CellAreaMoments(origin));
            geometry.FindIntersections(bounds);
            geometry.MergeCoincidentCuts(bounds);
            geometry.AddStraightPieces();
            for (int i = 0; i < circles.Count; i++)
                geometry.AddCircularPieces(i, circles[i]);
            geometry.JoinContours();
            if (geometry._moments.Area <= 0)
                return new VoronoiCell(site, null, default);
            return new VoronoiCell(site, new ContourBasedRegion(geometry._contours), geometry._moments.Centroid);
        }

        private void FindIntersections(Rectangle bounds)
        {
            for (int i = 0; i < _polygon.Count; i++)
            {
                var start = _polygon[i];
                var end = _polygon[(i + 1) % _polygon.Count];
                var cuts = new List<Cut> { new Cut(start.X, start.Y), new Cut(end.X, end.Y) };
                _edgeCuts[i] = cuts;
                for (int j = 0; j < _circles.Count; j++)
                {
                    _circles[j].IntersectSegment(start, end, (x, y) =>
                    {
                        cuts.Add(new Cut(x, y));
                        AddCircleCut(j, x, y);
                    });
                }

                // Sort in traversal order using the coordinate that changes most along this edge.
                bool useX = Abs((double)end.X - start.X) >= Abs((double)end.Y - start.Y);
                int direction = useX ? end.X.CompareTo(start.X) : end.Y.CompareTo(start.Y);
                cuts.Sort((a, b) => direction * (useX ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y)));
            }

            for (int i = 0; i < _circles.Count; i++)
            {
                for (int j = i + 1; j < _circles.Count; j++)
                {
                    _circles[i].IntersectCircle(_circles[j], bounds, (x, y) =>
                    {
                        AddCircleCut(i, x, y);
                        AddCircleCut(j, x, y);
                    });
                }
            }
        }

        // At a vertex shared by three or more boundaries, independent pair intersections
        // may round differently near a coordinate axis. Identify duplicate angular events
        // before constructing paths so all incident edges use the same endpoint.
        private void MergeCoincidentCuts(Rectangle bounds)
        {
            double tolerance = CutTolerance * Max(bounds.Width, bounds.Height);
            for (int j = 0; j < _circleCuts.Length; j++)
            {
                var cuts = _circleCuts[j];
                cuts.Sort(Cut.CompareOnCircle);
                for (int i = 0; i < cuts.Count; i++)
                {
                    var a = cuts[i];
                    var b = cuts[(i + 1) % cuts.Count];
                    double dx = b.X - a.X, dy = b.Y - a.Y;
                    if (a.Outside || b.Outside || Abs(dx) > tolerance || Abs(dy) > tolerance
                        || _circles[j].MinorSweep(dx, dy) > CutTolerance)
                        continue;
                    var first = Vertex(a);
                    var second = Vertex(b);
                    if (first == second)
                        continue;
                    var common = new PointXY(SharedCoordinate(first.X, second.X), SharedCoordinate(first.Y, second.Y));
                    if (first != common) _coincident[first] = common;
                    if (second != common) _coincident[second] = common;
                }
            }
        }

        private static float SharedCoordinate(float a, float b)
            => a <= 0 && b >= 0 || b <= 0 && a >= 0 ? 0 : (Abs(a) < Abs(b) ? a : b);

        private PointXY Vertex(Cut cut)
        {
            var point = cut.Point;
            while (_coincident.TryGetValue(point, out var next))
                point = next;
            return point;
        }

        private void AddCircleCut(int index, double x, double y)
        {
            _circleCuts[index].Add(new Cut(x, y, _circles[index].Quadrant(x, y)));
        }

        private bool PassesCircles(double x, double y, int except)
        {
            for (int i = 0; i < _circles.Count; i++)
            {
                if (i == except)
                    continue;
                var c = _circles[i];
                double value = c.Evaluate(x, y);
                if (c.KeepInside ? value > 0 : value < 0)
                    return false;
            }
            return true;
        }

        private bool InPolygon(double x, double y)
        {
            var a = _polygon[_polygon.Count - 1];
            foreach (var b in _polygon)
            {
                if (((double)b.X - a.X) * (y - a.Y) < ((double)b.Y - a.Y) * (x - a.X))
                    return false;
                a = b;
            }
            return true;
        }

        private void AddStraightPieces()
        {
            foreach (var cuts in _edgeCuts)
            {
                for (int j = 1; j < cuts.Count; j++)
                {
                    var a = cuts[j - 1];
                    var b = cuts[j];
                    var start = Vertex(a);
                    var end = Vertex(b);
                    if (start == end || !PassesCircles(0.5 * (a.X + b.X), 0.5 * (a.Y + b.Y), -1))
                        continue;
                    AddBoundaryPath(new ParameterizedSegment(start, end));
                }
            }
        }

        private void AddCircularPieces(int index, BisectorCircle circle)
        {
            var cuts = _circleCuts[index];
            var visible = new List<(PointXY Start, PointXY End, double Sweep)>();
            bool whole = true;
            for (int j = 0; j < cuts.Count; j++)
            {
                var a = cuts[j];
                var b = cuts[(j + 1) % cuts.Count];
                if (a.Outside || b.Outside)
                {
                    whole = false;
                    continue;
                }
                double angle = circle.MinorSweep(b.X - a.X, b.Y - a.Y);
                if (angle == 0 || (angle <= CutTolerance && Vertex(a) == Vertex(b)))
                    continue;

                // Chord midpoint plus sagitta avoids cancellation for nearly straight arcs.
                double bulge = 0.5 * Tan(angle / 4);
                double x = (a.X + b.X) * 0.5 + (b.Y - a.Y) * bulge;
                double y = (a.Y + b.Y) * 0.5 - (b.X - a.X) * bulge;
                if (InPolygon(x, y) && PassesCircles(x, y, index))
                    visible.Add((Vertex(a), Vertex(b), angle));
                else
                    whole = false;
            }

            if (whole)
            {
                var boundary = circle.ToCircle();
                if (boundary.Radius == 0)
                    return;
                if (float.IsInfinity(boundary.Length))
                    throw new ArgumentOutOfRangeException(nameof(circle), "The cell boundary length exceeds the finite float range.");
                _contours.Add(boundary);
                _moments.AddDisk(boundary, circle.KeepInside);
                return;
            }

            foreach (var part in visible)
            {
                var start = circle.KeepInside ? part.Start : part.End;
                var end = circle.KeepInside ? part.End : part.Start;
                if (start == end)
                    continue;
                double sweep = circle.KeepInside ? part.Sweep : -part.Sweep;
                AddBoundaryPath(new ClippedBisectorArc(start, end, (float)sweep));
                _moments.AddCircularSegment(start, end, sweep);
            }
        }

        private void AddBoundaryPath(IContourPath path)
        {
            if (!_outgoing.TryGetValue(path.StartPoint, out var edges))
                _outgoing.Add(path.StartPoint, edges = new Stack<IContourPath>());
            edges.Push(path);
            _moments.AddChord(path.StartPoint, path.EndPoint);
        }

        private void JoinContours()
        {
            foreach (var entry in _outgoing)
            {
                while (entry.Value.Count > 0)
                {
                    var loop = new List<IContourPath>();
                    var at = entry.Key;
                    do
                    {
                        if (!_outgoing.TryGetValue(at, out var edges) || edges.Count == 0)
                            throw new InvalidOperationException($"The clipped Voronoi boundary does not close at {at}.");
                        var edge = edges.Pop();
                        loop.Add(edge);
                        at = edge.EndPoint;
                    }
                    while (at != entry.Key);
                    _contours.Add(new CompositeContour(loop));
                }
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct Cut
        {
            public Cut(double x, double y, int quadrant = 0, bool isCardinal = false, bool outside = false)
            {
                IsCardinal = isCardinal;
                Outside = outside;
                Quadrant = quadrant;
                X = x;
                Y = y;
            }

            private bool IsCardinal { get; }
            public bool Outside { get; }
            private int Quadrant { get; }
            public double X { get; }
            public double Y { get; }
            public PointXY Point => new PointXY((float)X, (float)Y);

            public static int CompareOnCircle(Cut a, Cut b)
            {
                int order = a.Quadrant.CompareTo(b.Quadrant);
                if (order != 0)
                    return order;
                if (a.IsCardinal || b.IsCardinal)
                    return b.IsCardinal.CompareTo(a.IsCardinal);

                // Within a quadrant, projection along its middle tangent increases.
                // Comparing local chords preserves order even for sub-ulp angular spans.
                double dx = a.X - b.X, dy = a.Y - b.Y;
                switch (a.Quadrant)
                {
                    case 0: return Sign(dy - dx);
                    case 1: return Sign(-dx - dy);
                    case 2: return Sign(dx - dy);
                    default: return Sign(dx + dy);
                }
            }
        }
    }
}
