using Akeldov.Math.Spatial2D;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public static partial class VectorXYIntExtensions
    {
        /// <summary>
        /// Gets the source hex and the two neighboring hexes that meet at the specified vertex.
        /// </summary>
        /// <param name="hexIndex">The source hex index, returned as the center member of the triplet.</param>
        /// <param name="hexVertex">The vertex shared by the three hexes.</param>
        /// <param name="layout">The offset-coordinate layout.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Triplet<VectorXYInt> GetAdjacentTriplet(this VectorXYInt hexIndex, HexVertex hexVertex, Layout layout)
        {
            (HexEdge leftEdge, HexEdge rightEdge) = hexVertex.GetAdjacentEdges(layout);
            VectorXYInt leftIndex = hexIndex.GetAdjacent(leftEdge, layout);
            VectorXYInt rightIndex = hexIndex.GetAdjacent(rightEdge, layout);
            return new Triplet<VectorXYInt>(hexIndex, leftIndex, rightIndex);
        }
    }
}
