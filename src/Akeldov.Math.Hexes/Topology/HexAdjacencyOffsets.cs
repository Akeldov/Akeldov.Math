using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    internal static class HexAdjacencyOffsets
    {
        private static readonly sbyte[] RowUnshifted = new sbyte[]
        {
            // Pointy-top neighbor order: E, SE, SW, W, NW, NE.
            1, 0,
            0, 1,
            -1, 1,
            -1, 0,
            -1, -1,
            0, -1
        };

        private static readonly sbyte[] RowShifted = new sbyte[]
        {
            // Pointy-top neighbor order: E, SE, SW, W, NW, NE.
            1, 0,
            1, 1,
            0, 1,
            -1, 0,
            0, -1,
            1, -1
        };

        internal static readonly VectorXYInt[] RowUnshiftedVectors = CreateVectorOffsets(RowUnshifted);
        internal static readonly VectorXYInt[] RowShiftedVectors = CreateVectorOffsets(RowShifted);

        private static readonly sbyte[] ColumnUnshifted = new sbyte[]
        {
            // Flat-top neighbor order: NE, N, NW, SW, S, SE.
            1, -1,
            0, -1,
            -1, -1,
            -1, 0,
            0, 1,
            1, 0
        };

        private static readonly sbyte[] ColumnShifted = new sbyte[]
        {
            // Flat-top neighbor order: NE, N, NW, SW, S, SE.
            1, 0,
            0, -1,
            -1, 0,
            -1, 1,
            0, 1,
            1, 1
        };

        internal static readonly VectorXYInt[] ColumnUnshiftedVectors = CreateVectorOffsets(ColumnUnshifted);
        internal static readonly VectorXYInt[] ColumnShiftedVectors = CreateVectorOffsets(ColumnShifted);

        /// <summary>
        /// Gets library-owned mutable offsets for the row layout.
        /// </summary>
        /// <param name="y">The row coordinate whose parity selects the offset set.</param>
        /// <param name="evenRowsAreShifted"><see langword="true"/> when even rows are the shifted rows.</param>
        /// <remarks>The returned array is shared, owned by the library, and must not be mutated.</remarks>
        internal static sbyte[] GetRowOffsets(int y, bool evenRowsAreShifted)
        {
            bool rowIsShifted = ((y & 1) == 0) == evenRowsAreShifted;
            return rowIsShifted ? RowShifted : RowUnshifted;
        }

        /// <summary>
        /// Gets library-owned mutable vector offsets for the row layout.
        /// </summary>
        /// <param name="axisIsEven"><see langword="true"/> when the row coordinate is even.</param>
        /// <param name="evenRowsAreShifted"><see langword="true"/> when even rows are the shifted rows.</param>
        /// <remarks>The returned array is shared, owned by the library, and must not be mutated.</remarks>
        internal static VectorXYInt[] GetRowVectorOffsets(bool axisIsEven, bool evenRowsAreShifted)
        {
            bool rowIsShifted = axisIsEven == evenRowsAreShifted;
            return rowIsShifted ? RowShiftedVectors : RowUnshiftedVectors;
        }

        /// <summary>
        /// Gets library-owned mutable offsets for the column layout.
        /// </summary>
        /// <param name="x">The column coordinate whose parity selects the offset set.</param>
        /// <param name="evenColumnsAreShifted"><see langword="true"/> when even columns are the shifted columns.</param>
        /// <remarks>The returned array is shared, owned by the library, and must not be mutated.</remarks>
        internal static sbyte[] GetColumnOffsets(int x, bool evenColumnsAreShifted)
        {
            bool columnIsShifted = ((x & 1) == 0) == evenColumnsAreShifted;
            return columnIsShifted ? ColumnShifted : ColumnUnshifted;
        }

        /// <summary>
        /// Gets library-owned mutable offsets for the specified layout and index.
        /// </summary>
        /// <param name="layout">The offset-coordinate layout.</param>
        /// <param name="x">The column coordinate.</param>
        /// <param name="y">The row coordinate.</param>
        /// <remarks>The returned array is shared, owned by the library, and must not be mutated.</remarks>
        internal static sbyte[] GetOffsets(Layout layout, int x, int y)
        {
            switch (layout)
            {
                case Layout.OddR:
                    return GetRowOffsets(y, false);
                case Layout.EvenR:
                    return GetRowOffsets(y, true);
                case Layout.OddQ:
                    return GetColumnOffsets(x, false);
                case Layout.EvenQ:
                    return GetColumnOffsets(x, true);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        private static VectorXYInt[] CreateVectorOffsets(sbyte[] offsets)
        {
            var vectors = new VectorXYInt[offsets.Length / 2];

            for (int i = 0; i < vectors.Length; i++)
                vectors[i] = new VectorXYInt(offsets[i * 2], offsets[i * 2 + 1]);

            return vectors;
        }
    }
}
