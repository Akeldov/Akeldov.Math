using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Akeldov.Math.Hexes.Geometry.Contours
{
    public static partial class HexMatrixExtensions
    {
        private static readonly (int Q, int R)[] NeighborDirections =
        {
            (1, 0),
            (0, 1),
            (-1, 1),
            (-1, 0),
            (0, -1),
            (1, -1)
        };

        public static Segment[] ToApothemOffsetContour<TPolyhexGeometry>(this TPolyhexGeometry polyhexGeometry)
            where TPolyhexGeometry : IPolyhexGeometry
        {
            return polyhexGeometry.ToApothemOffsetContour(Layout.OddR);
        }

        public static Segment[] ToApothemOffsetContour<TPolyhexGeometry>(
            this TPolyhexGeometry polyhexGeometry,
            Layout layout)
            where TPolyhexGeometry : IPolyhexGeometry
        {
            if (polyhexGeometry is null)
                throw new ArgumentNullException(nameof(polyhexGeometry));

            var segments = new List<Segment>();
            float hexApothem = polyhexGeometry.HexApothem;
            float hexRadius = polyhexGeometry.HexRadius;
            int qsize = polyhexGeometry.QRSResolution.Q;
            int rsize = polyhexGeometry.QRSResolution.R;

            for (int q = 0; q < qsize; q++)
            {
                for (int r = 0; r < rsize; r++)
                {
                    if (!polyhexGeometry[q, r])
                        continue;

                    AddExtendedSegmentsForHex(
                        segments,
                        polyhexGeometry,
                        q,
                        r,
                        hexApothem,
                        hexRadius,
                        layout);
                }
            }

            return segments
                .Distinct()
                .ToList()
                .KeepBoundaryEdges()
                .ToArray();
        }

        private static void AddExtendedSegmentsForHex<TPolyhexGeometry>(
            List<Segment> segments,
            TPolyhexGeometry polyhexGeometry,
            int q,
            int r,
            float hexApothem,
            float hexRadius,
            Layout layout)
            where TPolyhexGeometry : IPolyhexGeometry
        {
            var outsideCenters = new VectorXY?[NeighborDirections.Length];

            for (int i = 0; i < NeighborDirections.Length; i++)
            {
                var direction = NeighborDirections[i];
                int neighborQ = q + direction.Q;
                int neighborR = r + direction.R;

                if (polyhexGeometry.IsOutside(neighborQ, neighborR))
                {
                    outsideCenters[i] = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexCenter(
                        neighborQ,
                        neighborR,
                        hexApothem,
                        hexRadius,
                        layout);
                }
            }

            for (int i = 0; i < outsideCenters.Length; i++)
            {
                VectorXY? current = outsideCenters[i];
                VectorXY? next = outsideCenters[(i + 1) % outsideCenters.Length];

                if (current.HasValue && next.HasValue)
                    segments.Add(CreateSegment(current.Value, next.Value, true, false));
            }
        }

        private static bool IsOutside<TPolyhexGeometry>(
            this TPolyhexGeometry polyhexGeometry,
            int q,
            int r)
            where TPolyhexGeometry : IPolyhexGeometry
        {
            if (q < 0 || q >= polyhexGeometry.QRSResolution.Q)
                return true;

            if (r < 0 || r >= polyhexGeometry.QRSResolution.R)
                return true;

            return !polyhexGeometry[q, r];
        }

        private static List<Segment> KeepBoundaryEdges(this List<Segment> segments)
        {
            if (segments.Count == 0)
                return segments;

            var vertices = new List<GraphVertex>();
            var edgeSegments = new Dictionary<EdgeKey, Segment>();

            for (int i = 0; i < segments.Count; i++)
            {
                int endpointA = GetVertexIndex(vertices, segments[i].EndpointA);
                int endpointB = GetVertexIndex(vertices, segments[i].EndpointB);

                if (endpointA == endpointB)
                    continue;

                vertices[endpointA].Neighbors.Add(endpointB);
                vertices[endpointB].Neighbors.Add(endpointA);
                edgeSegments[new EdgeKey(endpointA, endpointB)] = segments[i];
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                GraphVertex vertex = vertices[i];
                vertex.Neighbors = vertex.Neighbors.Distinct().ToList();
                vertex.Neighbors.Sort((left, right) =>
                    GetAngle(vertex.Position, vertices[left].Position)
                        .CompareTo(GetAngle(vertex.Position, vertices[right].Position)));
            }

            var visited = new HashSet<DirectedEdge>();
            var positiveFaceEdgeCounts = new Dictionary<EdgeKey, int>();

            for (int from = 0; from < vertices.Count; from++)
            {
                List<int> neighbors = vertices[from].Neighbors;
                for (int i = 0; i < neighbors.Count; i++)
                {
                    var start = new DirectedEdge(from, neighbors[i]);
                    if (visited.Contains(start))
                        continue;

                    List<EdgeKey> faceEdges = TraceFace(start, vertices, visited, out float signedArea);
                    if (signedArea > GeometryConstants.GeometryEpsilon)
                    {
                        for (int j = 0; j < faceEdges.Count; j++)
                        {
                            positiveFaceEdgeCounts.TryGetValue(faceEdges[j], out int count);
                            positiveFaceEdgeCounts[faceEdges[j]] = count + 1;
                        }
                    }
                }
            }

            var result = new List<Segment>();
            foreach (KeyValuePair<EdgeKey, int> faceEdgeCount in positiveFaceEdgeCounts)
            {
                if (faceEdgeCount.Value == 1 && edgeSegments.TryGetValue(faceEdgeCount.Key, out Segment segment))
                {
                    result.Add(segment);
                }
            }

            return result.Count == 0 ? segments : result;
        }

        private static int GetVertexIndex(List<GraphVertex> vertices, PointXY position)
        {
            const float endpointEpsilon = GeometryConstants.GeometryEpsilon * 16f;

            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].Position.AlmostEquals(position, endpointEpsilon))
                    return i;
            }

            vertices.Add(new GraphVertex(position));
            return vertices.Count - 1;
        }

        private static float GetAngle(PointXY origin, PointXY point)
        {
            return MathF.Atan2(point.Y - origin.Y, point.X - origin.X);
        }

        private static List<EdgeKey> TraceFace(
            DirectedEdge start,
            List<GraphVertex> vertices,
            HashSet<DirectedEdge> visited,
            out float signedArea)
        {
            var faceVertices = new List<int>();
            var faceEdges = new List<EdgeKey>();
            DirectedEdge current = start;
            int guard = 0;
            int guardLimit = System.Math.Max(1, vertices.Sum(vertex => vertex.Neighbors.Count) + 1);

            while (!visited.Contains(current) && guard < guardLimit)
            {
                visited.Add(current);
                faceVertices.Add(current.From);
                faceEdges.Add(new EdgeKey(current.From, current.To));

                GraphVertex toVertex = vertices[current.To];
                int reverseIndex = toVertex.Neighbors.IndexOf(current.From);
                if (reverseIndex < 0)
                    break;

                int nextIndex = (reverseIndex - 1 + toVertex.Neighbors.Count) % toVertex.Neighbors.Count;
                current = new DirectedEdge(current.To, toVertex.Neighbors[nextIndex]);
                guard++;
            }

            signedArea = GetSignedArea(faceVertices, vertices);
            return faceEdges;
        }

        private static float GetSignedArea(List<int> faceVertices, List<GraphVertex> vertices)
        {
            float area = 0f;

            for (int i = 0; i < faceVertices.Count; i++)
            {
                PointXY current = vertices[faceVertices[i]].Position;
                PointXY next = vertices[faceVertices[(i + 1) % faceVertices.Count]].Position;
                area += current.X * next.Y - next.X * current.Y;
            }

            return 0.5f * area;
        }

        private sealed class GraphVertex
        {
            public GraphVertex(PointXY position)
            {
                Position = position;
                Neighbors = new List<int>();
            }

            public PointXY Position { get; }

            public List<int> Neighbors { get; set; }
        }

        private readonly struct DirectedEdge : IEquatable<DirectedEdge>
        {
            public DirectedEdge(int from, int to)
            {
                From = from;
                To = to;
            }

            public int From { get; }

            public int To { get; }

            public bool Equals(DirectedEdge other) => From == other.From && To == other.To;

            public override bool Equals(object obj) => obj is DirectedEdge other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(From, To);
        }

        private readonly struct EdgeKey : IEquatable<EdgeKey>
        {
            public EdgeKey(int endpointA, int endpointB)
            {
                if (endpointA <= endpointB)
                {
                    EndpointA = endpointA;
                    EndpointB = endpointB;
                }
                else
                {
                    EndpointA = endpointB;
                    EndpointB = endpointA;
                }
            }

            public int EndpointA { get; }

            public int EndpointB { get; }

            public bool Equals(EdgeKey other) => EndpointA == other.EndpointA && EndpointB == other.EndpointB;

            public override bool Equals(object obj) => obj is EdgeKey other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(EndpointA, EndpointB);
        }
    }
}
