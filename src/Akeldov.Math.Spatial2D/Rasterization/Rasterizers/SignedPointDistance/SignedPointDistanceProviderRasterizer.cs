using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes signed point-distance providers by mapping signed distance values to raster cell values.
    /// </summary>
    /// <typeparam name="TValue">The raster cell value type produced by the signed-distance mapping function.</typeparam>
    public sealed class SignedPointDistanceProviderRasterizer<TValue> : ISpatialRasterizer<ISignedPointDistanceProvider, TValue>
    {
        private readonly Func<float, TValue> _signedDistanceToValue;

        /// <summary>
        /// Initializes a new signed point-distance provider rasterizer.
        /// </summary>
        /// <param name="signedDistanceToValue">The function that maps signed distance, in world coordinate units, to a raster cell value. Negative distances are inside the source; positive distances are outside.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="signedDistanceToValue"/> is <see langword="null"/>.</exception>
        public SignedPointDistanceProviderRasterizer(Func<float, TValue> signedDistanceToValue)
        {
            _signedDistanceToValue = signedDistanceToValue ?? throw new ArgumentNullException(nameof(signedDistanceToValue));
        }

        /// <summary>
        /// Rasterizes the specified signed point-distance provider on the specified spatial raster grid.
        /// </summary>
        /// <param name="source">The signed point-distance provider to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A spatial raster whose row-major value array is new, mutable, and owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="grid"/> is invalid or its cell count exceeds a 32-bit array length.</exception>
        public SpatialRaster<TValue> Rasterize(ISignedPointDistanceProvider source, RasterGeometry grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sampler = new SignedPointDistanceRasterSampler<TValue>(source, _signedDistanceToValue);
            return SpatialRasterizationCore<TValue>.Rasterize(grid, sampler, nameof(grid));
        }
    }
}
