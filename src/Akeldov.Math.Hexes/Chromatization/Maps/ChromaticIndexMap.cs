using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Chromatization
{
    /// <summary>
    /// Initializes a new instance of the ChromaticIndexMap type.
    /// </summary>
    public sealed class ChromaticIndexMap : IHexMap<byte>
    {
        private readonly byte[] _values;

        /// <summary>
        /// Initializes a new instance of the ChromaticIndexMap type.
        /// </summary>
        /// <param name="topology">The map topology.</param>
        public ChromaticIndexMap(HexMapTopology topology)
        {
            Topology = topology;
            _values = new byte[topology.Count];

            switch (topology.Layout)
            {
                case Layout.OddR:
                    FillRowLayoutChromaticIndices(false);
                    break;
                case Layout.EvenR:
                    FillRowLayoutChromaticIndices(true);
                    break;
                case Layout.OddQ:
                    FillColumnLayoutChromaticIndices(false);
                    break;
                case Layout.EvenQ:
                    FillColumnLayoutChromaticIndices(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(topology));
            }
        }

        /// <summary>
        /// Gets the map topology.
        /// </summary>
        public HexMapTopology Topology { get; }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public byte this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Topology.Resolution.X ||
                    index.Y < 0 || index.Y >= Topology.Resolution.Y)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                return _values[GetFlatIndex(index)];
            }
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        private void FillRowLayoutChromaticIndices(bool shiftedRowsUseUpperOffset)
        {
            for (int y = 0; y < Topology.Resolution.Y; y++)
            {
                int rowStart = y * Topology.Resolution.X;
                int qOffset = shiftedRowsUseUpperOffset
                    ? (y + (y & 1)) / 2
                    : (y - (y & 1)) / 2;

                for (int x = 0; x < Topology.Resolution.X; x++)
                {
                    _values[rowStart + x] = (byte)PositiveModulo(x - qOffset - y, 3);
                }
            }
        }

        private void FillColumnLayoutChromaticIndices(bool shiftedColumnsUseUpperOffset)
        {
            for (int y = 0; y < Topology.Resolution.Y; y++)
            {
                int rowStart = y * Topology.Resolution.X;

                for (int x = 0; x < Topology.Resolution.X; x++)
                {
                    int rOffset = shiftedColumnsUseUpperOffset
                        ? (x + (x & 1)) / 2
                        : (x - (x & 1)) / 2;

                    _values[rowStart + x] = (byte)PositiveModulo(y - rOffset - x, 3);
                }
            }
        }

        private static int PositiveModulo(int value, int divisor)
        {
            int result = value % divisor;
            return result < 0 ? result + divisor : result;
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Topology.Resolution.X + index.X;
    }
}
