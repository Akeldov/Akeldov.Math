using Akeldov.Math.Spatial2D;

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

        private static VectorXYInt[] CreateVectorOffsets(sbyte[] offsets)
        {
            var vectors = new VectorXYInt[offsets.Length / 2];

            for (int i = 0; i < vectors.Length; i++)
                vectors[i] = new VectorXYInt(offsets[i * 2], offsets[i * 2 + 1]);

            return vectors;
        }
    }
}
