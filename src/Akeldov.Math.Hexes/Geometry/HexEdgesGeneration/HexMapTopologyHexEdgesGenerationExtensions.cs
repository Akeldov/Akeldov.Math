using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Provides edge segment generation extensions for hex map geometry and topology values.
    /// </summary>
    public static class HexMapTopologyHexEdgesGenerationExtensions
    {
        private static readonly VectorXYInt[] PointyTopUnshiftedEdgeOffsets =
        {
            new VectorXYInt(0, 1),
            new VectorXYInt(-1, 1),
            new VectorXYInt(-1, 0),
            new VectorXYInt(-1, -1),
            new VectorXYInt(0, -1),
            new VectorXYInt(1, 0)
        };

        private static readonly VectorXYInt[] PointyTopShiftedEdgeOffsets =
        {
            new VectorXYInt(1, 1),
            new VectorXYInt(0, 1),
            new VectorXYInt(-1, 0),
            new VectorXYInt(0, -1),
            new VectorXYInt(1, -1),
            new VectorXYInt(1, 0)
        };

        private static readonly VectorXYInt[] FlatTopUnshiftedEdgeOffsets =
        {
            new VectorXYInt(1, 0),
            new VectorXYInt(0, 1),
            new VectorXYInt(-1, 0),
            new VectorXYInt(-1, -1),
            new VectorXYInt(0, -1),
            new VectorXYInt(1, -1)
        };

        private static readonly VectorXYInt[] FlatTopShiftedEdgeOffsets =
        {
            new VectorXYInt(1, 1),
            new VectorXYInt(0, 1),
            new VectorXYInt(-1, 1),
            new VectorXYInt(-1, 0),
            new VectorXYInt(0, -1),
            new VectorXYInt(1, 0)
        };

        /// <summary>
        /// Generates all unique edge segments for every hex in the topology.
        /// </summary>
        /// <param name="topology">The topology to generate edge segments for.</param>
        /// <param name="apothem">The hex apothem. The unit is the coordinate-space unit.</param>
        /// <returns>A new mutable list of segments owned by the caller. Shared hex edges appear only once.</returns>
        public static List<Segment> ToHexEdgeSegments(this HexMapTopology topology, float apothem)
        {
            if (float.IsNaN(apothem) || float.IsInfinity(apothem) || apothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(apothem), apothem, "Hex apothem must be finite and positive.");

            float radius = apothem.ConvertHexApothemToRadius();
            VectorXY origin = VectorXYInt.Zero.GetHexCenter(apothem, radius, topology.Layout);

            return ToHexEdgeSegments(topology, origin, apothem, radius);
        }

        /// <summary>
        /// Generates all unique edge segments for every hex in the geometry.
        /// </summary>
        /// <param name="geometry">The geometry to generate edge segments for.</param>
        /// <returns>A new mutable list of segments owned by the caller. Shared hex edges appear only once.</returns>
        public static List<Segment> ToHexEdgeSegments(this HexMapGeometry geometry)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Apothem) || float.IsInfinity(geometry.Apothem) || geometry.Apothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry apothem must be finite and positive.");

            return ToHexEdgeSegments(
                geometry.Topology,
                geometry.Origin,
                geometry.Apothem,
                geometry.Radius);
        }

        private static List<Segment> ToHexEdgeSegments(
            HexMapTopology topology,
            VectorXY origin,
            float apothem,
            float radius)
        {
            VectorXY[] normalizedVertices = VectorXYExtensions.GetNormalizedHexVertices(topology.Layout);
            var segments = new List<Segment>();

            for (int y = 0; y < topology.Resolution.Y; y++)
            {
                for (int x = 0; x < topology.Resolution.X; x++)
                {
                    var index = new VectorXYInt(x, y);
                    VectorXY center = index.GetHexCenter(apothem, radius, origin, topology.Layout);
                    int flatIndex = GetFlatIndex(index, topology.Resolution.X);

                    for (int edgeIndex = 0; edgeIndex < 6; edgeIndex++)
                    {
                        VectorXYInt adjacentIndex = GetAdjacentIndex(index, edgeIndex, topology.Layout);
                        if (IsInside(adjacentIndex, topology.Resolution.X, topology.Resolution.Y) &&
                            GetFlatIndex(adjacentIndex, topology.Resolution.X) < flatIndex)
                            continue;

                        VectorXY startPoint = center + normalizedVertices[edgeIndex] * radius;
                        VectorXY endPoint = center + normalizedVertices[(edgeIndex + 1) % 6] * radius;

                        segments.Add(new Segment((PointXY)startPoint, (PointXY)endPoint));
                    }
                }
            }

            return segments;
        }

        private static VectorXYInt GetAdjacentIndex(VectorXYInt index, int edgeIndex, Layout layout)
        {
            switch (layout)
            {
                case Layout.OddR:
                    return index + GetPointyTopEdgeOffset(edgeIndex, (index.Y & 1) == 1);
                case Layout.EvenR:
                    return index + GetPointyTopEdgeOffset(edgeIndex, (index.Y & 1) == 0);
                case Layout.OddQ:
                    return index + GetFlatTopEdgeOffset(edgeIndex, (index.X & 1) == 1);
                case Layout.EvenQ:
                    return index + GetFlatTopEdgeOffset(edgeIndex, (index.X & 1) == 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        private static VectorXYInt GetPointyTopEdgeOffset(int edgeIndex, bool rowIsShifted)
        {
            return (rowIsShifted ? PointyTopShiftedEdgeOffsets : PointyTopUnshiftedEdgeOffsets)[edgeIndex];
        }

        private static VectorXYInt GetFlatTopEdgeOffset(int edgeIndex, bool columnIsShifted)
        {
            return (columnIsShifted ? FlatTopShiftedEdgeOffsets : FlatTopUnshiftedEdgeOffsets)[edgeIndex];
        }

        private static bool IsInside(VectorXYInt index, int width, int height) =>
            index.X >= 0 &&
            index.X < width &&
            index.Y >= 0 &&
            index.Y < height;

        private static int GetFlatIndex(VectorXYInt index, int width) => index.Y * width + index.X;
    }
}
