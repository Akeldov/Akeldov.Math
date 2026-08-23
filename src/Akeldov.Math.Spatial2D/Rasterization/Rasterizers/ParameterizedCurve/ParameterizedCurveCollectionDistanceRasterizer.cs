using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes parameterized curve collections by mapping the nearest projection to raster values.
    /// </summary>
    /// <typeparam name="TValue">The raster cell value type produced by the projection mapping.</typeparam>
    public sealed class ParameterizedCurveCollectionDistanceRasterizer<TValue> : ISpatialRasterizer<IReadOnlyList<IParameterizedCurve>, TValue>
    {
        private readonly Func<float, float, TValue> _projectionToValue;

        /// <summary>
        /// Initializes a new parameterized curve collection distance rasterizer.
        /// </summary>
        /// <param name="projectionToValue">
        /// The function that maps distance to the nearest curve and its curve coordinate to a raster value.
        /// The first argument is distance in world coordinate units; the second argument is curve coordinate on the nearest curve.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectionToValue"/> is <see langword="null"/>.</exception>
        public ParameterizedCurveCollectionDistanceRasterizer(Func<float, float, TValue> projectionToValue)
        {
            _projectionToValue = projectionToValue ?? throw new ArgumentNullException(nameof(projectionToValue));
        }

        /// <summary>
        /// Rasterizes the specified parameterized curve collection on the specified spatial raster grid.
        /// </summary>
        /// <param name="source">The non-empty parameterized curve collection to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A spatial raster whose row-major value array is new, mutable, and owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="source"/> is empty or contains a <see langword="null"/> curve.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="grid"/> is invalid or its cell count exceeds a 32-bit array length.
        /// </exception>
        public SpatialRaster<TValue> Rasterize(IReadOnlyList<IParameterizedCurve> source, RasterGeometry grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count == 0)
                throw new ArgumentException("Parameterized curve collection must contain at least one curve.", nameof(source));

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] == null)
                    throw new ArgumentException("Parameterized curve collection must not contain null curves.", nameof(source));
            }

            var sampler = new ParameterizedCurveCollectionRasterSampler<TValue>(source, _projectionToValue);
            return SpatialRasterizationCore<TValue>.Rasterize(grid, sampler, nameof(grid));
        }
    }
}
