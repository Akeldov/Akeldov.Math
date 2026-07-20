using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Precomputes each map cell's index and its in-bounds neighboring indices.
    /// </summary>
    /// <remarks>
    /// Neighborhoods depend only on <see cref="HexMapGeometry.Topology"/>. The full
    /// <see cref="Geometry"/> is retained to position the map in world space, allowing it to be
    /// rasterized directly onto full or partial spatial raster grids without supplying a separate
    /// hex-map placement. Presence flags exclude adjacent indices outside the map bounds.
    /// </remarks>
    public sealed class IndexPartialSeptupletMap : ISpatialHexMap<PartialSeptuplet<VectorXYInt>>
    {
        private readonly PartialSeptuplet<VectorXYInt>[] _values;

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
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            Geometry = geometry;
            _values = CreateValues(geometry);
        }

        /// <summary>
        /// Gets the spatial geometry that positions the clipped adjacency map in world space.
        /// </summary>
        public HexMapGeometry Geometry { get; }

        /// <summary>
        /// Gets the layout and resolution of the adjacency map.
        /// </summary>
        public HexMapTopology Topology => Geometry.Topology;

        /// <summary>
        /// Gets the clipped seven-index neighborhood at the specified hex coordinates.
        /// </summary>
        /// <param name="index">The X/Y coordinates of the hex cell.</param>
        public PartialSeptuplet<VectorXYInt> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Topology.Resolution.X ||
                    index.Y < 0 || index.Y >= Topology.Resolution.Y)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                return _values[index.Y * Topology.Resolution.X + index.X];
            }
        }

        /// <summary>
        /// Gets the clipped seven-index neighborhood at the specified flat index.
        /// </summary>
        /// <param name="index">The zero-based row-major index.</param>
        public PartialSeptuplet<VectorXYInt> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
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
