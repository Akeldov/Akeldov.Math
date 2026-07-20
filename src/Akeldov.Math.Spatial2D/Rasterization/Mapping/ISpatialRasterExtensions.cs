using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides raster value mapping extension methods.
    /// </summary>
    public static class ISpatialRasterExtensions
    {
        /// <summary>
        /// Maps each value of the specified spatial raster while preserving its geometry.
        /// </summary>
        /// <typeparam name="TSource">The source raster value type.</typeparam>
        /// <typeparam name="TResult">The result raster value type.</typeparam>
        /// <param name="raster">The source spatial raster.</param>
        /// <param name="selector">The function that maps each source value to a result value.</param>
        /// <returns>A new raster whose value array is new, mutable, and owned by the caller.</returns>
        public static SpatialRaster<TResult> MapValues<TSource, TResult>(
            this ISpatialRaster<TSource> raster,
            Func<TSource, TResult> selector)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            VectorXYInt resolution = raster.Resolution;
            var values = new TResult[checked(resolution.X * resolution.Y)];

            int index = 0;
            for (int y = 0; y < resolution.Y; y++)
            for (int x = 0; x < resolution.X; x++)
                values[index++] = selector(raster[x, y]);

            return new SpatialRaster<TResult>(raster.Geometry, values);
        }
    }
}
