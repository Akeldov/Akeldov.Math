using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes point-distance provider collections by mapping the nearest unsigned distance to raster cell values.
    /// </summary>
    /// <typeparam name="TValue">The raster cell value type produced by the distance mapping function.</typeparam>
    public sealed class PointDistanceProviderCollectionRasterizer<TValue> : ISpatialRasterizer<IReadOnlyList<IPointDistanceProvider>, TValue>
    {
        private readonly Func<float, TValue> _distanceToValue;

        /// <summary>
        /// Initializes a new point-distance provider collection rasterizer.
        /// </summary>
        /// <param name="distanceToValue">The function that maps nearest unsigned distance, in world coordinate units, to a raster cell value.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="distanceToValue"/> is <see langword="null"/>.</exception>
        public PointDistanceProviderCollectionRasterizer(Func<float, TValue> distanceToValue)
        {
            _distanceToValue = distanceToValue ?? throw new ArgumentNullException(nameof(distanceToValue));
        }

        /// <summary>
        /// Rasterizes the specified point-distance providers on the specified spatial raster grid.
        /// </summary>
        /// <param name="source">The point-distance providers to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A spatial raster whose row-major value array is new, mutable, and owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is empty or contains a <see langword="null"/> element.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="grid"/> is invalid or its cell count exceeds a 32-bit array length.</exception>
        public SpatialRaster<TValue> Rasterize(IReadOnlyList<IPointDistanceProvider> source, RasterGeometry grid)
        {
            return Rasterize<IPointDistanceProvider>(source, grid);
        }

        /// <summary>
        /// Rasterizes the specified point-distance providers without boxing value-type providers.
        /// </summary>
        /// <typeparam name="T">The point-distance provider type.</typeparam>
        /// <param name="source">The point-distance providers to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A spatial raster whose row-major value array is new, mutable, and owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is empty or contains a <see langword="null"/> element.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="grid"/> is invalid or its cell count exceeds a 32-bit array length.</exception>
        public SpatialRaster<TValue> Rasterize<T>(IReadOnlyList<T> source, RasterGeometry grid)
            where T : IPointDistanceProvider
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count == 0)
                throw new ArgumentException("Point-distance provider collection must contain at least one source.", nameof(source));

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] is null)
                    throw new ArgumentException("Point-distance provider collection must not contain null sources.", nameof(source));
            }

            var sampler = new PointDistanceCollectionRasterSampler<T, TValue>(source, _distanceToValue);
            return SpatialRasterizationCore<TValue>.Rasterize(grid, sampler, nameof(grid));
        }
    }
}
