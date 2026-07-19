using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Precomputes the world-space center of every hex in a map geometry.
    /// </summary>
    public sealed class HexCenterMap : SpatialHexMap<PointXY>
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
            : base(geometry, CreateValues(geometry))
        {
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
