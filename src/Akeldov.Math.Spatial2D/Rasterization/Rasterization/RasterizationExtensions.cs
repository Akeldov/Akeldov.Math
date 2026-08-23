using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides rasterization extension methods.
    /// </summary>
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes values sampled at the center of each cell in the specified spatial raster grid.
        /// </summary>
        /// <typeparam name="TValue">The raster cell value type produced by the sampling function.</typeparam>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <param name="sample">The function that produces a value at a raster cell center.</param>
        /// <returns>A spatial raster whose row-major value array is new, mutable, and owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sample"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="grid"/> is invalid or its cell count exceeds a 32-bit array length.</exception>
        public static SpatialRaster<TValue> Rasterize<TValue>(this RasterGeometry grid, Func<PointXY, TValue> sample)
        {
            if (sample == null)
                throw new ArgumentNullException(nameof(sample));

            return SpatialRasterizationCore<TValue>.Rasterize(grid, new DelegateSpatialRasterSampler<TValue>(sample), nameof(grid));
        }

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

        private readonly struct DelegateSpatialRasterSampler<TValue> : ISpatialRasterSampler<TValue>
        {
            private readonly Func<PointXY, TValue> _sample;

            public DelegateSpatialRasterSampler(Func<PointXY, TValue> sample)
            {
                _sample = sample;
            }

            public TValue Sample(PointXY point) => _sample(point);
        }
    }
}
