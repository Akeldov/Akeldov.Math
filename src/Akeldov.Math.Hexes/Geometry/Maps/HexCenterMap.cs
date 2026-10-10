using Akeldov.Math.Spatial2D;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Precomputes the world-space center of every hex in a map geometry.
    /// </summary>
    /// <remarks>
    /// Initial centers are derived from <see cref="Geometry"/>.
    /// Inherited indexer setters and shared spatial facades change only the stored points;
    /// the geometry remains unchanged. Callers must keep points consistent with the geometry
    /// when using geometric algorithms.
    /// </remarks>
    public sealed class HexCenterMap : HexMap<PointXY>, ISpatialHexMap<PointXY>
    {
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
            : base(geometry.Topology)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            Geometry = geometry;
            FillValues(BackingValues, geometry);
        }

        /// <summary>
        /// Gets the spatial geometry from which the centers were computed.
        /// </summary>
        public HexMapGeometry Geometry { get; }

        /// <summary>
        /// Creates a spatial facade sharing this center map's geometry and point array.
        /// </summary>
        /// <param name="map">The source center map, or <see langword="null"/>.</param>
        /// <returns>
        /// A new mutable spatial facade sharing the source map's points, or <see langword="null"/>
        /// if <paramref name="map"/> is null. Changes through either map are visible through the other.
        /// </returns>
        /// <remarks>
        /// The conversion takes constant time and allocates only the facade object. Points are not copied.
        /// Changes through the facade do not update the center map's geometry.
        /// </remarks>
        [return: NotNullIfNotNull("map")]
        public static implicit operator SpatialHexMap<PointXY>?(HexCenterMap? map)
            => map is null ? null : new SpatialHexMap<PointXY>(map.Geometry, map.BackingValues);

        private static void FillValues(PointXY[] values, HexMapGeometry geometry)
        {
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
