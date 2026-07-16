using Akeldov.Math.Hexes.Geometry;
using System;

namespace Akeldov.Math.Hexes.Chromatization
{
    /// <summary>
    /// Initializes a new instance of the ChromaticIndexMap type.
    /// </summary>
    public sealed class ChromaticIndexMap : SpatialHexMap<byte>
    {
        /// <summary>
        /// Initializes a new instance of the ChromaticIndexMap type.
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
            : base(geometry, CreateValues(geometry))
        {
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
