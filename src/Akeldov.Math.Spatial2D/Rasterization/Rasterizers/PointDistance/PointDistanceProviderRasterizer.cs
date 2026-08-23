using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes point-distance providers by mapping unsigned distance values to raster cell values.
    /// </summary>
    /// <typeparam name="TValue">The raster cell value type produced by the distance mapping function.</typeparam>
    public sealed class PointDistanceProviderRasterizer<TValue> : ISpatialRasterizer<IPointDistanceProvider, TValue>
    {
        private readonly Func<float, TValue> _distanceToValue;

        /// <summary>
        /// Initializes a new point-distance provider rasterizer.
        /// </summary>
        /// <param name="distanceToValue">The function that maps unsigned distance, in world coordinate units, to a raster cell value.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="distanceToValue"/> is <see langword="null"/>.</exception>
        public PointDistanceProviderRasterizer(Func<float, TValue> distanceToValue)
        {
            _distanceToValue = distanceToValue ?? throw new ArgumentNullException(nameof(distanceToValue));
        }

        /// <summary>
        /// Rasterizes the specified point-distance provider on the specified spatial raster grid.
        /// </summary>
        /// <param name="source">The point-distance provider to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A spatial raster whose row-major value array is new, mutable, and owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="grid"/> is invalid or its cell count exceeds a 32-bit array length.</exception>
        public SpatialRaster<TValue> Rasterize(IPointDistanceProvider source, RasterGeometry grid)
        {
            return Rasterize<IPointDistanceProvider>(source, grid);
        }

        /// <summary>
        /// Rasterizes the specified point-distance provider without boxing a value-type provider.
        /// </summary>
        /// <typeparam name="T">The point-distance provider type.</typeparam>
        /// <param name="source">The point-distance provider to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A spatial raster whose row-major value array is new, mutable, and owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="grid"/> is invalid or its cell count exceeds a 32-bit array length.</exception>
        public SpatialRaster<TValue> Rasterize<T>(T source, RasterGeometry grid)
            where T : IPointDistanceProvider
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var sampler = new PointDistanceRasterSampler<T, TValue>(source, _distanceToValue);
            return SpatialRasterizationCore<TValue>.Rasterize(grid, sampler, nameof(grid));
        }
    }
}
