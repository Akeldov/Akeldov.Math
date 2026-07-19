using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Precomputes the world-space center of every hex in a map geometry.
    /// </summary>
    /// <remarks>
    /// Centers are derived from <see cref="Geometry"/> and cannot be replaced through this map.
    /// </remarks>
    public sealed class HexCenterMap : ISpatialHexMap<PointXY>
    {
        private readonly PointXY[] _values;

        /// <summary>
        /// Initializes a new instance with the specified topology and unit hex radius.
        /// </summary>
        /// <param name="topology">The map topology.</param>
        public HexCenterMap(HexMapTopology topology)
            : this(new HexMapGeometry(topology, 1f))
        {
        }

        /// <summary>
        /// Initializes a center map for the specified spatial geometry.
        /// </summary>
        /// <param name="geometry">The topology, origin, and cell size used to compute the centers.</param>
        public HexCenterMap(HexMapGeometry geometry)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            Geometry = geometry;
            _values = CreateValues(geometry);
        }

        /// <summary>
        /// Gets the spatial geometry from which the centers were computed.
        /// </summary>
        public HexMapGeometry Geometry { get; }

        /// <summary>
        /// Gets the layout and resolution of the center map.
        /// </summary>
        public HexMapTopology Topology => Geometry.Topology;

        /// <summary>
        /// Gets the world-space center at the specified hex coordinates.
        /// </summary>
        /// <param name="index">The X/Y coordinates of the hex cell.</param>
        public PointXY this[VectorXYInt index]
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
        /// Gets the world-space center at the specified flat index.
        /// </summary>
        /// <param name="index">The zero-based row-major index.</param>
        public PointXY this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        private static PointXY[] CreateValues(HexMapGeometry geometry)
        {
            var values = new PointXY[geometry.Topology.Count];

            switch (geometry.Topology.Layout)
            {
                case Layout.OddR:
                    FillOddRCenters(values, geometry);
                    break;
                case Layout.EvenR:
                    FillEvenRCenters(values, geometry);
                    break;
                case Layout.OddQ:
                    FillOddQCenters(values, geometry);
                    break;
                case Layout.EvenQ:
                    FillEvenQCenters(values, geometry);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry));
            }

            return values;
        }

        private static void FillOddRCenters(PointXY[] values, HexMapGeometry geometry)
        {
            int width = geometry.Topology.Resolution.X;

            for (int y = 0; y < geometry.Topology.Resolution.Y; y++)
            {
                int rowStart = y * width;
                float xShift = (y & 1) * geometry.Apothem;
                float centerY = geometry.Origin.Y + 1.5f * geometry.Radius * y;

                for (int x = 0; x < width; x++)
                {
                    values[rowStart + x] = new PointXY(
                        geometry.Origin.X + x * 2f * geometry.Apothem + xShift,
                        centerY);
                }
            }
        }

        private static void FillEvenRCenters(PointXY[] values, HexMapGeometry geometry)
        {
            int width = geometry.Topology.Resolution.X;

            for (int y = 0; y < geometry.Topology.Resolution.Y; y++)
            {
                int rowStart = y * width;
                float xShift = -(y & 1) * geometry.Apothem;
                float centerY = geometry.Origin.Y + 1.5f * geometry.Radius * y;

                for (int x = 0; x < width; x++)
                {
                    values[rowStart + x] = new PointXY(
                        geometry.Origin.X + x * 2f * geometry.Apothem + xShift,
                        centerY);
                }
            }
        }

        private static void FillOddQCenters(PointXY[] values, HexMapGeometry geometry)
        {
            int width = geometry.Topology.Resolution.X;

            for (int y = 0; y < geometry.Topology.Resolution.Y; y++)
            {
                int rowStart = y * width;
                float baseY = geometry.Origin.Y + y * 2f * geometry.Apothem;

                for (int x = 0; x < width; x++)
                {
                    values[rowStart + x] = new PointXY(
                        geometry.Origin.X + 1.5f * geometry.Radius * x,
                        baseY + (x & 1) * geometry.Apothem);
                }
            }
        }

        private static void FillEvenQCenters(PointXY[] values, HexMapGeometry geometry)
        {
            int width = geometry.Topology.Resolution.X;

            for (int y = 0; y < geometry.Topology.Resolution.Y; y++)
            {
                int rowStart = y * width;
                float baseY = geometry.Origin.Y + y * 2f * geometry.Apothem;

                for (int x = 0; x < width; x++)
                {
                    values[rowStart + x] = new PointXY(
                        geometry.Origin.X + 1.5f * geometry.Radius * x,
                        baseY - (x & 1) * geometry.Apothem);
                }
            }
        }

    }
}
