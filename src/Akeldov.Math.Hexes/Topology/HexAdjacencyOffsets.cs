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

        private static readonly VectorXYInt[] RowUnshiftedVectors = CreateVectorOffsets(RowUnshifted);
        private static readonly VectorXYInt[] RowShiftedVectors = CreateVectorOffsets(RowShifted);

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

        internal static sbyte[] GetRowOffsets(int y, bool evenRowsAreShifted)
        {
            bool rowIsShifted = ((y & 1) == 0) == evenRowsAreShifted;
            return rowIsShifted ? RowShifted : RowUnshifted;
        }

        internal static VectorXYInt[] GetRowVectorOffsets(bool axisIsEven, bool evenRowsAreShifted)
        {
            bool rowIsShifted = axisIsEven == evenRowsAreShifted;
            return rowIsShifted ? RowShiftedVectors : RowUnshiftedVectors;
        }

        internal static sbyte[] GetColumnOffsets(int x, bool evenColumnsAreShifted)
        {
            bool columnIsShifted = ((x & 1) == 0) == evenColumnsAreShifted;
            return columnIsShifted ? ColumnShifted : ColumnUnshifted;
        }

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
