using System;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes parameterized curves by mapping projection distance and curve coordinate to raster values.
    /// </summary>
    /// <typeparam name="TValue">The raster cell value type produced by the projection mapping.</typeparam>
    public sealed class ParameterizedCurveDistanceRasterizer<TValue> : ISpatialRasterizer<IParameterizedCurve, TValue>
    {
        private readonly Func<float, float, TValue> _projectionToValue;

        /// <summary>
        /// Initializes a new parameterized curve distance rasterizer.
        /// </summary>
        /// <param name="projectionToValue">
        /// The function that maps distance to the curve and curve coordinate to a raster value.
        /// The first argument is distance in world coordinate units; the second argument is curve coordinate.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectionToValue"/> is <see langword="null"/>.</exception>
        public ParameterizedCurveDistanceRasterizer(Func<float, float, TValue> projectionToValue)
        {
            _projectionToValue = projectionToValue ?? throw new ArgumentNullException(nameof(projectionToValue));
        }

        /// <summary>
        /// Rasterizes the specified parameterized curve on the specified spatial raster grid.
        /// </summary>
        /// <param name="source">The parameterized curve to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A spatial raster whose row-major value array is new, mutable, and owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="grid"/> is invalid or its cell count exceeds a 32-bit array length.
        /// </exception>
        public SpatialRaster<TValue> Rasterize(IParameterizedCurve source, RasterGeometry grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sampler = new ParameterizedCurveRasterSampler<TValue>(source, _projectionToValue);
            return SpatialRasterizationCore<TValue>.Rasterize(grid, sampler, nameof(grid));
        }
    }
}
