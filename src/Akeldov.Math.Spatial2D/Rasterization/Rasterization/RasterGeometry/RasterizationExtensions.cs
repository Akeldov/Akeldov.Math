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
