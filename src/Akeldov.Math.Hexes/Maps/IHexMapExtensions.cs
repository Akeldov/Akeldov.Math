using Akeldov.Math.Spatial2D.Imaging;
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
        /// <param name="rasterGrid">The raster geometry. Its resolution must match the map dimensions.</param>
        /// <param name="colorSelector">The function that maps each map value to a raster color.</param>
        /// <returns>A new raster whose value array is new, mutable, and owned by the caller.</returns>
        public static SpatialRaster<TColor> Rasterize<TValue, TColor>(
            this IHexMap<TValue> map,
            SpatialRasterGrid rasterGrid,
            Func<TValue, TColor> colorSelector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (colorSelector == null)
                throw new ArgumentNullException(nameof(colorSelector));

            if (rasterGrid.Resolution.X != map.Width ||
                rasterGrid.Resolution.Y != map.Height)
                throw new ArgumentException("Raster grid resolution must match map dimensions.", nameof(rasterGrid));

            int count = checked(map.Width * map.Height);
            var values = new TColor[count];

            for (int i = 0; i < values.Length; i++)
                values[i] = colorSelector(map[i]);

            return new SpatialRaster<TColor>(rasterGrid, values);
        }
    }
}
