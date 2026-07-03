using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public static partial class VectorXYIntExtensions
    {
        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="hexIndex">The hexIndex value.</param>
        /// <param name="hexVertex">The hexVertex value.</param>
        /// <param name="layout">The layout value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pair<VectorXYInt> GetAdjacentPair(this VectorXYInt hexIndex, HexVertex hexVertex, Layout layout)
        {
            var (leftEdge, rightEdge) = hexVertex.GetAdjacentEdges(layout);
            var leftIndex = hexIndex.GetAdjacent(leftEdge, layout);
            var rightIndex = hexIndex.GetAdjacent(rightEdge, layout);
            return new Pair<VectorXYInt>(leftIndex, rightIndex);
        }
    }
}
