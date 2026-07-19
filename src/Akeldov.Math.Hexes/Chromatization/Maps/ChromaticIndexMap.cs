using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Chromatization
{
    /// <summary>
    /// Precomputes a three-color class for every cell in a hex map.
    /// </summary>
    /// <remarks>
    /// Chromatic classes are derived from <see cref="Geometry"/> and cannot be replaced through this map.
    /// </remarks>
    public sealed class ChromaticIndexMap : ISpatialHexMap<byte>
    {
        private readonly byte[] _values;

        /// <summary>
        /// Initializes a chromatic map with unit-radius spatial geometry.
        /// </summary>
        /// <param name="topology">The map topology.</param>
        public ChromaticIndexMap(HexMapTopology topology)
            : this(new HexMapGeometry(topology, 1f))
        {
        }

        /// <summary>
        /// Initializes a new instance with the specified spatial geometry.
        /// </summary>
        /// <param name="geometry">The spatial geometry of the map.</param>
        public ChromaticIndexMap(HexMapGeometry geometry)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            Geometry = geometry;
            _values = CreateValues(geometry);
        }

        /// <summary>
        /// Gets the spatial geometry used to compute the chromatic classes.
        /// </summary>
        public HexMapGeometry Geometry { get; }

        /// <summary>
        /// Gets the layout and resolution of the chromatic map.
        /// </summary>
        public HexMapTopology Topology => Geometry.Topology;

        /// <summary>
        /// Gets the chromatic class at the specified hex coordinates.
        /// </summary>
        /// <param name="index">The X/Y coordinates of the hex cell.</param>
        public byte this[VectorXYInt index]
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
        /// Gets the chromatic class at the specified flat index.
        /// </summary>
        /// <param name="index">The zero-based row-major index.</param>
        public byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        private static byte[] CreateValues(HexMapGeometry geometry)
        {
            HexMapTopology topology = geometry.Topology;
            var values = new byte[topology.Count];

            switch (topology.Layout)
            {
                case Layout.OddR:
                    FillOddRChromaticIndices(values, topology);
                    break;
                case Layout.EvenR:
                    FillEvenRChromaticIndices(values, topology);
                    break;
                case Layout.OddQ:
                    FillOddQChromaticIndices(values, topology);
                    break;
                case Layout.EvenQ:
                    FillEvenQChromaticIndices(values, topology);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry));
            }

            return values;
        }

        private static void FillOddRChromaticIndices(byte[] values, HexMapTopology topology)
        {
            for (int y = 0; y < topology.Resolution.Y; y++)
            {
                int rowStart = y * topology.Resolution.X;
                int qOffset = (y - (y & 1)) / 2;

                for (int x = 0; x < topology.Resolution.X; x++)
                {
                    int chromaticIndex = (x - qOffset - y) % 3;
                    values[rowStart + x] = (byte)(chromaticIndex < 0 ? chromaticIndex + 3 : chromaticIndex);
                }
            }
        }

        private static void FillEvenRChromaticIndices(byte[] values, HexMapTopology topology)
        {
            for (int y = 0; y < topology.Resolution.Y; y++)
            {
                int rowStart = y * topology.Resolution.X;
                int qOffset = (y + (y & 1)) / 2;

                for (int x = 0; x < topology.Resolution.X; x++)
                {
                    int chromaticIndex = (x - qOffset - y) % 3;
                    values[rowStart + x] = (byte)(chromaticIndex < 0 ? chromaticIndex + 3 : chromaticIndex);
                }
            }
        }

        private static void FillOddQChromaticIndices(byte[] values, HexMapTopology topology)
        {
            for (int y = 0; y < topology.Resolution.Y; y++)
            {
                int rowStart = y * topology.Resolution.X;

                for (int x = 0; x < topology.Resolution.X; x++)
                {
                    int rOffset = (x - (x & 1)) / 2;

                    int chromaticIndex = (y - rOffset - x) % 3;
                    values[rowStart + x] = (byte)(chromaticIndex < 0 ? chromaticIndex + 3 : chromaticIndex);
                }
            }
        }

        private static void FillEvenQChromaticIndices(byte[] values, HexMapTopology topology)
        {
            for (int y = 0; y < topology.Resolution.Y; y++)
            {
                int rowStart = y * topology.Resolution.X;

                for (int x = 0; x < topology.Resolution.X; x++)
                {
                    int rOffset = (x + (x & 1)) / 2;

                    int chromaticIndex = (y - rOffset - x) % 3;
                    values[rowStart + x] = (byte)(chromaticIndex < 0 ? chromaticIndex + 3 : chromaticIndex);
                }
            }
        }
    }
}
