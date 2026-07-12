using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides extension methods for hex-indexed map values.
    /// </summary>
    public static class IHexMapExtensions
    {
        /// <summary>
        /// Rasterizes the specified map using the provided raster geometry and value-to-color selector.
        /// </summary>
        /// <typeparam name="TValue">The map value type.</typeparam>
        /// <typeparam name="TColor">The raster color value type.</typeparam>
        /// <param name="map">The map to rasterize.</param>
        /// <param name="resolution">The raster resolution</param>
        /// <param name="colorSelector">The function that maps each map value to a raster color.</param>
        /// <returns>A new raster whose value array is new, mutable, and owned by the caller.</returns>
        public static Raster<TColor> Rasterize<TValue, TColor>(
            this IHexMap<TValue> map,
            VectorXYInt resolution,
            Func<TValue, TColor> colorSelector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (colorSelector == null)
                throw new ArgumentNullException(nameof(colorSelector));

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Raster resolution components must be positive.");

            if (map.Topology.Resolution.X <= 0 || map.Topology.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex map resolution components must be positive.");

            int count = checked(resolution.X * resolution.Y);
            var values = new TColor[count];
            var geometry = new HexMapGeometry(
                map.Topology.Resolution.X,
                map.Topology.Resolution.Y,
                radius: 1f,
                layout: map.Topology.Layout);
            Rectangle bounds = geometry.GetBoundingBox();
            float pixelWidth = bounds.Size.X / resolution.X;
            float pixelHeight = bounds.Size.Y / resolution.Y;

            for (int y = 0; y < resolution.Y; y++)
            {
                float pointY = bounds.Min.Y + (y + 0.5f) * pixelHeight;

                for (int x = 0; x < resolution.X; x++)
                {
                    var point = new PointXY(
                        bounds.Min.X + (x + 0.5f) * pixelWidth,
                        pointY);
                    VectorXYInt hexIndex = point.ToXYIndex(geometry.Radius, geometry.Origin, map.Topology.Layout);

                    if ((uint)hexIndex.X < (uint)map.Topology.Resolution.X &&
                        (uint)hexIndex.Y < (uint)map.Topology.Resolution.Y)
                    {
                        values[y * resolution.X + x] = colorSelector(map[hexIndex]);
                    }
                }
            }

            return new Raster<TColor>(resolution, values);
        }
    }
}
