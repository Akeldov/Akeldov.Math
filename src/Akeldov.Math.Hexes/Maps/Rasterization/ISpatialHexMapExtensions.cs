using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes
{
    public static class ISpatialHexMapExtensions
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

        public static SpatialRaster<TColor> Rasterize<TValue, TColor>(
            this ISpatialHexMap<TValue> map,
            float pixelsPerApothem,
            float margin,
            Func<TValue, TColor> colorSelector)
        {
            var grid = map.Geometry.ToRasterGeometry(pixelsPerApothem, margin);
            return map.Rasterize(grid, colorSelector);
        }

        public static SpatialRaster<TColor> Rasterize<TValue, TColor>(
            this ISpatialHexMap<TValue> map,
            RasterGeometry rasterGeometry,
            Func<TValue, TColor> colorSelector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (colorSelector == null)
                throw new ArgumentNullException(nameof(colorSelector));

            int count = checked(rasterGeometry.Resolution.X * rasterGeometry.Resolution.Y);
            var values = new TColor[count];
            var geometry = new HexMapGeometry(
                map.Topology.Resolution.X,
                map.Topology.Resolution.Y,
                radius: 1f,
                layout: map.Topology.Layout);
            Rectangle bounds = geometry.GetBoundingBox();
            float pixelWidth = bounds.Size.X / rasterGeometry.Resolution.X;
            float pixelHeight = bounds.Size.Y / rasterGeometry.Resolution.Y;

            for (int y = 0; y < rasterGeometry.Resolution.Y; y++)
            {
                float pointY = bounds.Min.Y + (y + 0.5f) * pixelHeight;

                for (int x = 0; x < rasterGeometry.Resolution.X; x++)
                {
                    var point = new PointXY(
                        bounds.Min.X + (x + 0.5f) * pixelWidth,
                        pointY);
                    VectorXYInt hexIndex = point.ToXYIndex(geometry.Radius, geometry.Origin, map.Topology.Layout);

                    if ((uint)hexIndex.X < (uint)map.Topology.Resolution.X &&
                        (uint)hexIndex.Y < (uint)map.Topology.Resolution.Y)
                    {
                        values[y * rasterGeometry.Resolution.X + x] = colorSelector(map[hexIndex]);
                    }
                }
            }

            return new SpatialRaster<TColor>(rasterGeometry, values);
        }
    }
}
