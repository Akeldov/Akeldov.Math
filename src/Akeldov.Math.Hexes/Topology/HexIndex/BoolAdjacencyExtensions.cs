using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Selects layout-specific neighbor offsets from the parity of an offset-grid axis.
    /// </summary>
    public static partial class BoolExtensions
    {
        internal static readonly VectorXYInt[] ColumnUnshiftedEdgeOffsets = new VectorXYInt[]
        {
            new VectorXYInt(1, 0),
            new VectorXYInt(0, 1),
            new VectorXYInt(-1, 0),
            new VectorXYInt(-1, -1),
            new VectorXYInt(0, -1),
            new VectorXYInt(1, -1)
        };

        internal static readonly VectorXYInt[] ColumnShiftedEdgeOffsets = new VectorXYInt[]
        {
            new VectorXYInt(1, 1),
            new VectorXYInt(0, 1),
            new VectorXYInt(-1, 1),
            new VectorXYInt(-1, 0),
            new VectorXYInt(0, -1),
            new VectorXYInt(1, 0)
        };

        /// <summary>
        /// Gets relative offsets for the six adjacent hexes.
        /// </summary>
        /// <param name="axisIsEven">Whether the staggered axis coordinate is even: Y for row layouts, X for column layouts.</param>
        /// <param name="layout">The offset-coordinate layout.</param>
        /// <returns>A new, mutable array owned by the caller.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt[] GetRelativeOffsets(this bool axisIsEven, Layout layout)
        {
            return (VectorXYInt[])axisIsEven.GetSharedRelativeOffsets(layout).Clone();
        }

        /// <summary>
        /// Gets library-owned mutable relative offsets for the six adjacent hexes.
        /// </summary>
        /// <param name="axisIsEven">Whether the staggered axis coordinate is even: Y for row layouts, X for column layouts.</param>
        /// <param name="layout">The offset-coordinate layout.</param>
        /// <remarks>The returned array is shared, owned by the library, and must not be mutated.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static VectorXYInt[] GetSharedRelativeOffsets(this bool axisIsEven, Layout layout)
        {
            switch (layout)
            {
                case Layout.OddR:
                    return HexAdjacencyOffsets.GetRowVectorOffsets(axisIsEven, false);
                case Layout.EvenR:
                    return HexAdjacencyOffsets.GetRowVectorOffsets(axisIsEven, true);
                case Layout.OddQ:
                    return axisIsEven ? ColumnUnshiftedEdgeOffsets : ColumnShiftedEdgeOffsets;
                case Layout.EvenQ:
                    return axisIsEven ? ColumnShiftedEdgeOffsets : ColumnUnshiftedEdgeOffsets;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        /// <summary>
        /// Gets the relative offset to the neighbor across the specified edge.
        /// </summary>
        /// <param name="axisIsEven">Whether the staggered axis coordinate is even: Y for row layouts, X for column layouts.</param>
        /// <param name="hexEdge">The edge shared with the neighbor.</param>
        /// <param name="layout">The offset-coordinate layout.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt GetRelativeOffset(this bool axisIsEven, HexEdge hexEdge, Layout layout)
        {
            return axisIsEven.GetSharedRelativeOffsets(layout)[(int)hexEdge];
        }

        /// <summary>
        /// Gets the relative offset to the neighbor across the edge with the specified ordinal.
        /// </summary>
        /// <param name="axisIsEven">Whether the staggered axis coordinate is even: Y for row layouts, X for column layouts.</param>
        /// <param name="hexEdge">The zero-based edge ordinal.</param>
        /// <param name="layout">The offset-coordinate layout.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt GetRelativeOffset(this bool axisIsEven, int hexEdge, Layout layout)
        {
            return axisIsEven.GetSharedRelativeOffsets(layout)[hexEdge];
        }
    }
}
