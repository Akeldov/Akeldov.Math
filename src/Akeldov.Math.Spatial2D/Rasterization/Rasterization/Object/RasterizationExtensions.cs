using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides rasterization extension methods.
    /// </summary>
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes a source object on the specified spatial raster grid using the specified rasterizer.
        /// </summary>
        /// <typeparam name="TSource">The source object type to rasterize.</typeparam>
        /// <typeparam name="TValue">The raster cell value type produced by the rasterizer.</typeparam>
        /// <param name="source">The source object to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <param name="rasterizer">The rasterization strategy.</param>
        /// <returns>The spatial raster produced from the source object.</returns>
        public static SpatialRaster<TValue> Rasterize<TSource, TValue>(
            this TSource source,
            RasterGeometry grid,
            ISpatialRasterizer<TSource, TValue> rasterizer)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (rasterizer == null)
                throw new ArgumentNullException(nameof(rasterizer));

            return rasterizer.Rasterize(source, grid);
        }
    }
}
