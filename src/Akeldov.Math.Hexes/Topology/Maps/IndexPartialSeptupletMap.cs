using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Precomputes each map cell's index and its in-bounds neighboring indices.
    /// </summary>
    public sealed class IndexPartialSeptupletMap : SpatialHexMap<PartialSeptuplet<VectorXYInt>>
    {
        /// <summary>
        /// Initializes a clipped adjacency map with unit-radius spatial geometry.
        /// </summary>
        /// <param name="topology">The map topology.</param>
        public IndexPartialSeptupletMap(HexMapTopology topology)
            : this(new HexMapGeometry(topology, 1f))
        {
        }

        /// <summary>
        /// Initializes a new instance with the specified spatial geometry.
        /// </summary>
        /// <param name="geometry">The spatial geometry of the map.</param>
        public IndexPartialSeptupletMap(HexMapGeometry geometry)
            : base(geometry, CreateValues(geometry))
        {
        }

        private static PartialSeptuplet<VectorXYInt>[] CreateValues(HexMapGeometry geometry)
        {
            HexMapTopology topology = geometry.Topology;
            var values = new PartialSeptuplet<VectorXYInt>[topology.Count];

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

        /// <summary>
        /// Gets the number of map columns.
        /// </summary>
        public int Width => Topology.Resolution.X;

        /// <summary>
        /// Gets the number of map rows.
        /// </summary>
        public int Height => Topology.Resolution.Y;

        /// <summary>
        /// Gets the total number of map cells.
        /// </summary>
        public int Count => Topology.Count;

        private static void FillOddRAdjacency(
            PartialSeptuplet<VectorXYInt>[] values,
            HexMapTopology topology)
        {
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * width;
                VectorXYInt[] offsets = (y & 1) == 0
                    ? HexAdjacencyOffsets.RowUnshiftedVectors
                    : HexAdjacencyOffsets.RowShiftedVectors;

                for (int x = 0; x < width; x++)
                    values[rowStart + x] = CreateAdjacency(x, y, width, height, offsets);
            }
        }

        private static void FillEvenRAdjacency(
            PartialSeptuplet<VectorXYInt>[] values,
            HexMapTopology topology)
        {
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * width;
                VectorXYInt[] offsets = (y & 1) == 0
                    ? HexAdjacencyOffsets.RowShiftedVectors
                    : HexAdjacencyOffsets.RowUnshiftedVectors;

                for (int x = 0; x < width; x++)
                    values[rowStart + x] = CreateAdjacency(x, y, width, height, offsets);
            }
        }

        private static void FillOddQAdjacency(
            PartialSeptuplet<VectorXYInt>[] values,
            HexMapTopology topology)
        {
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * width;

                for (int x = 0; x < width; x++)
                {
                    VectorXYInt[] offsets = (x & 1) == 0
                        ? HexAdjacencyOffsets.ColumnUnshiftedVectors
                        : HexAdjacencyOffsets.ColumnShiftedVectors;
                    values[rowStart + x] = CreateAdjacency(x, y, width, height, offsets);
                }
            }
        }

        private static void FillEvenQAdjacency(
            PartialSeptuplet<VectorXYInt>[] values,
            HexMapTopology topology)
        {
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * width;

                for (int x = 0; x < width; x++)
                {
                    VectorXYInt[] offsets = (x & 1) == 0
                        ? HexAdjacencyOffsets.ColumnShiftedVectors
                        : HexAdjacencyOffsets.ColumnUnshiftedVectors;
                    values[rowStart + x] = CreateAdjacency(x, y, width, height, offsets);
                }
            }
        }

        private static PartialSeptuplet<VectorXYInt> CreateAdjacency(
            int x,
            int y,
            int width,
            int height,
            VectorXYInt[] offsets)
        {
            var main = new VectorXYInt(x, y);
            var adjacency = new Septuplet<VectorXYInt>(
                main,
                main + offsets[0],
                main + offsets[1],
                main + offsets[2],
                main + offsets[3],
                main + offsets[4],
                main + offsets[5]);

            SeptupletPresenceFlags presence = SeptupletPresenceFlags.Main;

            if (ContainsIndex(adjacency.Adjacent0, width, height))
                presence |= SeptupletPresenceFlags.Adjacent0;

            if (ContainsIndex(adjacency.Adjacent1, width, height))
                presence |= SeptupletPresenceFlags.Adjacent1;

            if (ContainsIndex(adjacency.Adjacent2, width, height))
                presence |= SeptupletPresenceFlags.Adjacent2;

            if (ContainsIndex(adjacency.Adjacent3, width, height))
                presence |= SeptupletPresenceFlags.Adjacent3;

            if (ContainsIndex(adjacency.Adjacent4, width, height))
                presence |= SeptupletPresenceFlags.Adjacent4;

            if (ContainsIndex(adjacency.Adjacent5, width, height))
                presence |= SeptupletPresenceFlags.Adjacent5;

            return new PartialSeptuplet<VectorXYInt>(adjacency, presence);
        }

        private static bool ContainsIndex(VectorXYInt index, int width, int height)
        {
            return (uint)index.X < (uint)width &&
                   (uint)index.Y < (uint)height;
        }
    }
}
