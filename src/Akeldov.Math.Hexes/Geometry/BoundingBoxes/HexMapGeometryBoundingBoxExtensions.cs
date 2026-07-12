using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;
using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Provides bounding-box extension methods for hex map geometry and topology.
    /// </summary>
    public static class HexMapGeometryBoundingBoxExtensions
    {
        /// <summary>
        /// Creates a spatial raster grid that covers the whole hex map.
        /// </summary>
        /// <param name="topology">The hex map topology.</param>
        /// <param name="apothem">The hex apothem. The unit is the coordinate-space unit.</param>
        /// <param name="origin">The center of the zero hex.</param>
        /// <param name="pixelsPerApothem">The raster resolution density in pixels per hex apothem.</param>
        /// <returns>A raster grid covering all hexes in the map.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="pixelsPerApothem"/> is not finite and positive,
        /// when <paramref name="origin"/> contains a non-finite component,
        /// when <paramref name="apothem"/> is not finite and positive,
        /// when <paramref name="topology"/> has empty dimensions, or when its layout is unsupported.
        /// </exception>
        /// <exception cref="OverflowException">Thrown when the raster resolution does not fit <see cref="int"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SpatialRasterGrid ToSpatialRasterGrid(
            this HexMapTopology topology,
            float apothem,
            VectorXY origin,
            float pixelsPerApothem)
        {
            return new HexMapGeometry(topology, origin, apothem).ToSpatialRasterGrid(pixelsPerApothem);
        }

        /// <summary>
        /// Creates a spatial raster grid that covers the whole hex map with an outer margin.
        /// </summary>
        /// <param name="topology">The hex map topology.</param>
        /// <param name="apothem">The hex apothem. The unit is the coordinate-space unit.</param>
        /// <param name="origin">The center of the zero hex.</param>
        /// <param name="pixelsPerApothem">The raster resolution density in pixels per hex apothem.</param>
        /// <param name="margin">The non-negative margin added to each side of the map bounding box. The unit is the coordinate-space unit.</param>
        /// <returns>A raster grid covering all hexes in the map and the requested margin.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="pixelsPerApothem"/> is not finite and positive,
        /// when <paramref name="margin"/> is negative or non-finite,
        /// when <paramref name="origin"/> contains a non-finite component,
        /// when <paramref name="apothem"/> is not finite and positive,
        /// when <paramref name="topology"/> has empty dimensions, or when its layout is unsupported.
        /// </exception>
        /// <exception cref="OverflowException">Thrown when the raster resolution does not fit <see cref="int"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SpatialRasterGrid ToSpatialRasterGrid(
            this HexMapTopology topology,
            float apothem,
            VectorXY origin,
            float pixelsPerApothem,
            float margin)
        {
            return new HexMapGeometry(topology, origin, apothem).ToSpatialRasterGrid(pixelsPerApothem, margin);
        }

        /// <summary>
        /// Creates a spatial raster grid that covers the whole hex map and optional outer margin.
        /// </summary>
        /// <param name="geometry">The hex map geometry.</param>
        /// <param name="pixelsPerApothem">The raster resolution density in pixels per hex apothem.</param>
        /// <param name="margin">The non-negative margin added to each side of the map bounding box. The unit is the coordinate-space unit.</param>
        /// <returns>A raster grid covering all hexes in the map and the requested margin.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="pixelsPerApothem"/> is not finite and positive,
        /// when <paramref name="margin"/> is negative or non-finite,
        /// or when <paramref name="geometry"/> has a non-finite origin, non-positive apothem, empty dimensions,
        /// or an unsupported layout.
        /// </exception>
        /// <exception cref="OverflowException">Thrown when the raster resolution does not fit <see cref="int"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SpatialRasterGrid ToSpatialRasterGrid(this HexMapGeometry geometry, float pixelsPerApothem, float margin = 0f)
        {
            if (float.IsNaN(pixelsPerApothem) || float.IsInfinity(pixelsPerApothem) || pixelsPerApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(pixelsPerApothem));

            if (float.IsNaN(margin) || float.IsInfinity(margin) || margin < 0f)
                throw new ArgumentOutOfRangeException(nameof(margin));

            Rectangle boundingBox = geometry.BoundingBox();
            double pixelsPerWorldUnit = (double)pixelsPerApothem / geometry.Apothem;
            var marginVector = new VectorXY(margin, margin);
            VectorXY rasterSize = boundingBox.Size + marginVector * 2f;
            int rasterWidth = CalculateRasterResolution(rasterSize.X, pixelsPerWorldUnit);
            int rasterHeight = CalculateRasterResolution(rasterSize.Y, pixelsPerWorldUnit);

            return new SpatialRasterGrid(
                boundingBox.Min - marginVector,
                rasterSize,
                new VectorXYInt(rasterWidth, rasterHeight));
        }

        /// <summary>
        /// Returns the axis-aligned bounding box of the whole hex map as a rectangle.
        /// </summary>
        /// <param name="topology">The hex map topology.</param>
        /// <param name="apothem">The hex apothem. The unit is the coordinate-space unit.</param>
        /// <param name="origin">The center of the zero hex.</param>
        /// <returns>The axis-aligned rectangle that contains all hexes in the map.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="origin"/> contains a non-finite component,
        /// when <paramref name="apothem"/> is not finite and positive,
        /// when <paramref name="topology"/> has empty dimensions, or when its layout is unsupported.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle BoundingBox(this HexMapTopology topology, float apothem, VectorXY origin)
        {
            return new HexMapGeometry(topology, origin, apothem).BoundingBox();
        }

        /// <summary>
        /// Returns the axis-aligned bounding box of the whole hex map as a rectangle.
        /// </summary>
        /// <param name="geometry">The hex map geometry.</param>
        /// <returns>The axis-aligned rectangle that contains all hexes in the map.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="geometry"/> has a non-finite origin, non-positive apothem, empty dimensions,
        /// or an unsupported layout.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle BoundingBox(this HexMapGeometry geometry)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Apothem) || float.IsInfinity(geometry.Apothem) || geometry.Apothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry apothem must be finite and positive.");

            if (geometry.Topology.Resolution.X <= 0 || geometry.Topology.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry dimensions must be positive.");

            float radius = geometry.Radius;
            VectorXY size = geometry.Topology.Resolution.BoundingBox(
                geometry.Apothem,
                radius,
                geometry.Topology.Layout);

            float minX;
            float minY;

            switch (geometry.Topology.Layout)
            {
                case Layout.OddR:
                    minX = geometry.Origin.X - geometry.Apothem;
                    minY = geometry.Origin.Y - radius;
                    break;
                case Layout.EvenR:
                    minX = geometry.Origin.X - geometry.Apothem * (geometry.Topology.Resolution.Y == 1 ? 1f : 2f);
                    minY = geometry.Origin.Y - radius;
                    break;
                case Layout.OddQ:
                    minX = geometry.Origin.X - radius;
                    minY = geometry.Origin.Y - geometry.Apothem;
                    break;
                case Layout.EvenQ:
                    minX = geometry.Origin.X - radius;
                    minY = geometry.Origin.Y - geometry.Apothem * (geometry.Topology.Resolution.X == 1 ? 1f : 2f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry layout is not supported.");
            }

            return new Rectangle(
                new PointXY(minX, minY),
                new PointXY(minX + size.X, minY + size.Y));
        }

        private static int CalculateRasterResolution(float worldSize, double pixelsPerWorldUnit)
        {
            double resolution = System.Math.Ceiling((double)worldSize * pixelsPerWorldUnit);
            if (double.IsNaN(resolution) || double.IsInfinity(resolution) || resolution > int.MaxValue)
                throw new OverflowException("Raster resolution must fit in Int32.");

            return resolution < 1d ? 1 : (int)resolution;
        }
    }
}
