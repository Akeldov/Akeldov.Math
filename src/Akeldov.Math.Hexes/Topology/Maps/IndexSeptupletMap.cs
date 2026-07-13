using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Initializes a new instance of the IndexSeptupletMap type.
    /// </summary>
    public sealed class IndexSeptupletMap : IHexMap<Septuplet<VectorXYInt>>
    {
        private readonly Septuplet<VectorXYInt>[] _values;

        /// <summary>
        /// Initializes a new instance of the IndexSeptupletMap type.
        /// </summary>
        /// <param name="topology">The topology value.</param>
        public IndexSeptupletMap(HexMapTopology topology)
        {
            Topology = topology;
            _values = new Septuplet<VectorXYInt>[topology.Count];

            switch (topology.Layout)
            {
                case Layout.OddR:
                    FillRowLayoutTopology(false);
                    break;
                case Layout.EvenR:
                    FillRowLayoutTopology(true);
                    break;
                case Layout.OddQ:
                    FillColumnLayoutTopology(false);
                    break;
                case Layout.EvenQ:
                    FillColumnLayoutTopology(true);
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
        public Septuplet<VectorXYInt> this[VectorXYInt index]
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
        public Septuplet<VectorXYInt> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Topology.Resolution.X + index.X;

        private void FillRowLayoutTopology(bool evenRowsAreShifted)
        {
            for (int y = 0; y < Topology.Resolution.Y; y++)
            {
                var rowStart = y * Topology.Resolution.X;
                var offsets = HexAdjacencyOffsets.GetRowOffsets(y, evenRowsAreShifted);

                for (int x = 0; x < Topology.Resolution.X; x++)
                {
                    _values[rowStart + x] = CreateAdjacency(x, y, offsets);
                }
            }
        }

        private void FillColumnLayoutTopology(bool evenColumnsAreShifted)
        {
            for (int y = 0; y < Topology.Resolution.Y; y++)
            {
                var rowStart = y * Topology.Resolution.X;

                for (int x = 0; x < Topology.Resolution.X; x++)
                {
                    var offsets = HexAdjacencyOffsets.GetColumnOffsets(x, evenColumnsAreShifted);
                    _values[rowStart + x] = CreateAdjacency(x, y, offsets);
                }
            }
        }

        private static Septuplet<VectorXYInt> CreateAdjacency(
            int x,
            int y,
            sbyte[] offsets)
        {
            return new Septuplet<VectorXYInt>(
                new VectorXYInt(x, y),
                new VectorXYInt(x + offsets[0], y + offsets[1]),
                new VectorXYInt(x + offsets[2], y + offsets[3]),
                new VectorXYInt(x + offsets[4], y + offsets[5]),
                new VectorXYInt(x + offsets[6], y + offsets[7]),
                new VectorXYInt(x + offsets[8], y + offsets[9]),
                new VectorXYInt(x + offsets[10], y + offsets[11]));
        }
    }
}
