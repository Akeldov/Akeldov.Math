using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Precomputes each map cell's index and the indices of its six neighbors.
    /// </summary>
    public sealed class IndexSeptupletMap : SpatialHexMap<Septuplet<VectorXYInt>>
    {
        /// <summary>
        /// Initializes an adjacency map with unit-radius spatial geometry.
        /// </summary>
        /// <param name="topology">The layout and resolution whose neighborhoods are computed.</param>
        public IndexSeptupletMap(HexMapTopology topology)
            : this(new HexMapGeometry(topology, 1f))
        {
        }

        /// <summary>
        /// Initializes a new instance with the specified spatial geometry.
        /// </summary>
        /// <param name="geometry">The spatial geometry of the map.</param>
        public IndexSeptupletMap(HexMapGeometry geometry)
            : base(geometry, CreateValues(geometry))
        {
        }

        private static Septuplet<VectorXYInt>[] CreateValues(HexMapGeometry geometry)
        {
            HexMapTopology topology = geometry.Topology;
            var values = new Septuplet<VectorXYInt>[topology.Count];

            switch (topology.Layout)
            {
                case Layout.OddR:
                    FillOddRAdjacency(values, topology);
                    break;
                case Layout.EvenR:
                    FillEvenRAdjacency(values, topology);
                    break;
                case Layout.OddQ:
                    FillOddQAdjacency(values, topology);
                    break;
                case Layout.EvenQ:
                    FillEvenQAdjacency(values, topology);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry));
            }

            return values;
        }

        private static void FillOddRAdjacency(
            Septuplet<VectorXYInt>[] values,
            HexMapTopology topology)
        {
            int width = topology.Resolution.X;

            for (int y = 0; y < topology.Resolution.Y; y++)
            {
                int rowStart = y * width;
                VectorXYInt[] offsets = (y & 1) == 0
                    ? HexAdjacencyOffsets.RowUnshiftedVectors
                    : HexAdjacencyOffsets.RowShiftedVectors;

                for (int x = 0; x < width; x++)
                    values[rowStart + x] = CreateAdjacency(x, y, offsets);
            }
        }

        private static void FillEvenRAdjacency(
            Septuplet<VectorXYInt>[] values,
            HexMapTopology topology)
        {
            int width = topology.Resolution.X;

            for (int y = 0; y < topology.Resolution.Y; y++)
            {
                int rowStart = y * width;
                VectorXYInt[] offsets = (y & 1) == 0
                    ? HexAdjacencyOffsets.RowShiftedVectors
                    : HexAdjacencyOffsets.RowUnshiftedVectors;

                for (int x = 0; x < width; x++)
                    values[rowStart + x] = CreateAdjacency(x, y, offsets);
            }
        }

        private static void FillOddQAdjacency(
            Septuplet<VectorXYInt>[] values,
            HexMapTopology topology)
        {
            int width = topology.Resolution.X;

            for (int y = 0; y < topology.Resolution.Y; y++)
            {
                int rowStart = y * width;

                for (int x = 0; x < width; x++)
                {
                    VectorXYInt[] offsets = (x & 1) == 0
                        ? HexAdjacencyOffsets.ColumnUnshiftedVectors
                        : HexAdjacencyOffsets.ColumnShiftedVectors;
                    values[rowStart + x] = CreateAdjacency(x, y, offsets);
                }
            }
        }

        private static void FillEvenQAdjacency(
            Septuplet<VectorXYInt>[] values,
            HexMapTopology topology)
        {
            int width = topology.Resolution.X;

            for (int y = 0; y < topology.Resolution.Y; y++)
            {
                int rowStart = y * width;

                for (int x = 0; x < width; x++)
                {
                    VectorXYInt[] offsets = (x & 1) == 0
                        ? HexAdjacencyOffsets.ColumnShiftedVectors
                        : HexAdjacencyOffsets.ColumnUnshiftedVectors;
                    values[rowStart + x] = CreateAdjacency(x, y, offsets);
                }
            }
        }

        private static Septuplet<VectorXYInt> CreateAdjacency(int x, int y, VectorXYInt[] offsets)
        {
            var main = new VectorXYInt(x, y);

            return new Septuplet<VectorXYInt>(
                main,
                main + offsets[0],
                main + offsets[1],
                main + offsets[2],
                main + offsets[3],
                main + offsets[4],
                main + offsets[5]);
        }
    }
}
