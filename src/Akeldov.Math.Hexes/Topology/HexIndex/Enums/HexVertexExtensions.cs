using Akeldov.Math.Hexes.Vectors.QRS;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static class HexVertexExtensions
    {
        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="hexVertex">The hexVertex value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pair<HexEdge> GetAdjacentEdges(this HexVertex hexVertex)
        {
            int hexVertexIndex = (int)hexVertex;
            if ((uint)hexVertexIndex >= 6u)
                throw new ArgumentOutOfRangeException(nameof(hexVertex), hexVertex, "The vertex must be a defined hex vertex.");

            return GetPointyTopAdjacentEdges(hexVertexIndex);
        }

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="hexVertex">The hexVertex value.</param>
        /// <param name="layout">The layout value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pair<HexEdge> GetAdjacentEdges(this HexVertex hexVertex, Layout layout)
        {
            int hexVertexIndex = (int)hexVertex;
            if ((uint)hexVertexIndex >= 6u)
                throw new ArgumentOutOfRangeException(nameof(hexVertex), hexVertex, "The vertex must be a defined hex vertex.");

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return GetPointyTopAdjacentEdges(hexVertexIndex);
                case Layout.OddQ:
                case Layout.EvenQ:
                    return GetFlatTopAdjacentEdges(hexVertexIndex);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Pair<HexEdge> GetPointyTopAdjacentEdges(int hexVertexIndex)
        {
            var hexEdgeLeftIndex = (hexVertexIndex + 1) % 6;
            var hexEdgeRightIndex = hexVertexIndex;
            return new Pair<HexEdge>((HexEdge)hexEdgeLeftIndex, (HexEdge)hexEdgeRightIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Pair<HexEdge> GetFlatTopAdjacentEdges(int hexVertexIndex)
        {
            var hexEdgeLeftIndex = hexVertexIndex;
            var hexEdgeRightIndex = (hexVertexIndex + 5) % 6;
            return new Pair<HexEdge>((HexEdge)hexEdgeLeftIndex, (HexEdge)hexEdgeRightIndex);
        }
    }
}
