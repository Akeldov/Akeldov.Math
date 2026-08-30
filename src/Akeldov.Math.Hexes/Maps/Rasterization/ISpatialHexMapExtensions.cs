using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides extension methods for spatial hex maps.
    /// </summary>
    public static partial class ISpatialHexMapExtensions
    {
        /// <summary>
        /// Creates a spatial raster grid that covers the whole hex map and optional outer margin.
        /// </summary>
        /// <typeparam name="TValue">The type of value stored in the map.</typeparam>
        /// <param name="map">The spatial hex map.</param>
        /// <param name="pixelsPerApothem">The raster resolution density in pixels per hex apothem.</param>
        /// <param name="margin">The non-negative margin added to each side of the map bounding box. The unit is the coordinate-space unit.</param>
        /// <returns>A raster grid covering all hexes in the map and the requested margin.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="pixelsPerApothem"/> is not finite and positive,
        /// when <paramref name="margin"/> is negative or non-finite,
        /// or when the map geometry has a non-finite origin, non-positive apothem, empty dimensions,
        /// or an unsupported layout.
        /// </exception>
        /// <exception cref="OverflowException">Thrown when the raster resolution does not fit <see cref="int"/>.</exception>
        public static RasterGeometry ToRasterGeometry<TValue>(
            this ISpatialHexMap<TValue> map,
            float pixelsPerApothem,
            float margin = 0f)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            return map.Geometry.ToRasterGeometry(pixelsPerApothem, margin);
        }

        /// <summary>
        /// Rasterizes a spatial hex map on a grid that covers the whole map and an optional margin.
        /// </summary>
        /// <typeparam name="TValue">The map value type.</typeparam>
        /// <typeparam name="TColor">The raster value type.</typeparam>
        /// <param name="map">The spatial hex map.</param>
        /// <param name="pixelsPerApothem">The raster resolution density in pixels per hex apothem.</param>
        /// <param name="margin">The non-negative margin added to each side of the map bounding box. The unit is the coordinate-space unit.</param>
        /// <param name="colorSelector">The function that maps each map value to a raster value.</param>
        /// <returns>A new spatial raster sampled in the coordinate space of the map geometry.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="colorSelector"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the map topology differs from its geometry topology.
        /// </exception>
        public static SpatialRaster<TColor> Rasterize<TValue, TColor>(
            this ISpatialHexMap<TValue> map,
            float pixelsPerApothem,
            float margin,
            Func<TValue, TColor> colorSelector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var grid = map.Geometry.ToRasterGeometry(pixelsPerApothem, margin);
            return map.Rasterize(grid, colorSelector);
        }

        /// <summary>
        /// Rasterizes a spatial hex map on a grid that covers the whole map and an optional margin.
        /// </summary>
        /// <typeparam name="TValue">The map value type.</typeparam>
        /// <typeparam name="TColor">The raster value type.</typeparam>
        /// <param name="map">The spatial hex map.</param>
        /// <param name="pixelsPerApothem">The raster resolution density in pixels per hex apothem.</param>
        /// <param name="colorSelector">The function that maps each map value to a raster value.</param>
        /// <returns>A new spatial raster sampled in the coordinate space of the map geometry.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="colorSelector"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialRaster<TColor> Rasterize<TValue, TColor>(
            this ISpatialHexMap<TValue> map,
            float pixelsPerApothem,
            Func<TValue, TColor> colorSelector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var grid = map.Geometry.ToRasterGeometry(pixelsPerApothem);
            return map.Rasterize(grid, colorSelector);
        }

        /// <summary>
        /// Rasterizes a spatial hex map by sampling the center of every cell in the specified raster geometry.
        /// </summary>
        /// <typeparam name="TValue">The map value type.</typeparam>
        /// <typeparam name="TColor">The raster value type.</typeparam>
        /// <param name="map">The spatial hex map.</param>
        /// <param name="rasterGeometry">The world-space raster geometry to sample.</param>
        /// <param name="colorSelector">The function that maps each map value to a raster value.</param>
        /// <returns>
        /// A new spatial raster. Cells whose centers lie outside the map topology retain the default
        /// value of <typeparamref name="TColor"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="colorSelector"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialRaster<TColor> Rasterize<TValue, TColor>(
            this ISpatialHexMap<TValue> map,
            RasterGeometry rasterGeometry,
            Func<TValue, TColor> colorSelector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (colorSelector == null)
                throw new ArgumentNullException(nameof(colorSelector));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            int count = checked(rasterGeometry.Resolution.X * rasterGeometry.Resolution.Y);
            var values = new TColor[count];
            HexMapGeometry geometry = map.Geometry;
            HexMapTopology topology = map.Topology;
            float pixelWidth = rasterGeometry.CellSize.X;
            float pixelHeight = rasterGeometry.CellSize.Y;

            for (int y = 0; y < rasterGeometry.Resolution.Y; y++)
            {
                float pointY = rasterGeometry.Origin.Y + (y + 0.5f) * pixelHeight;

                for (int x = 0; x < rasterGeometry.Resolution.X; x++)
                {
                    var point = new PointXY(
                        rasterGeometry.Origin.X + (x + 0.5f) * pixelWidth,
                        pointY);
                    VectorXYInt hexIndex = point.ToXYIndex(geometry.Radius, geometry.Origin, topology.Layout);

                    if ((uint)hexIndex.X < (uint)topology.Resolution.X &&
                        (uint)hexIndex.Y < (uint)topology.Resolution.Y)
                    {
                        values[y * rasterGeometry.Resolution.X + x] = colorSelector(map[hexIndex]);
                    }
                }
            }

            return new SpatialRaster<TColor>(rasterGeometry, values);
        }
    }
}
