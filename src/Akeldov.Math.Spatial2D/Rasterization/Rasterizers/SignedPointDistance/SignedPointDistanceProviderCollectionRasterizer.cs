using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes signed point-distance provider collections by mapping minimum signed distance values to raster cell values.
    /// </summary>
    /// <typeparam name="TValue">The raster cell value type produced by the signed-distance mapping function.</typeparam>
    public sealed class SignedPointDistanceProviderCollectionRasterizer<TValue> : ISpatialRasterizer<IReadOnlyList<ISignedPointDistanceProvider>, TValue>
    {
        private readonly Func<float, TValue> _signedDistanceToValue;

        /// <summary>
        /// Initializes a new signed point-distance provider collection rasterizer.
        /// </summary>
        /// <param name="signedDistanceToValue">The function that maps minimum signed distance, in world coordinate units, to a raster cell value. Negative distances are inside at least one source; positive distances are outside all sources.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="signedDistanceToValue"/> is <see langword="null"/>.</exception>
        public SignedPointDistanceProviderCollectionRasterizer(Func<float, TValue> signedDistanceToValue)
        {
            _signedDistanceToValue = signedDistanceToValue ?? throw new ArgumentNullException(nameof(signedDistanceToValue));
        }

        /// <summary>
        /// Rasterizes the specified signed point-distance providers on the specified spatial raster grid.
        /// </summary>
        /// <param name="source">The signed point-distance providers to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A spatial raster whose row-major value array is new, mutable, and owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is empty or contains a <see langword="null"/> element.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="grid"/> is invalid or its cell count exceeds a 32-bit array length.</exception>
        public SpatialRaster<TValue> Rasterize(IReadOnlyList<ISignedPointDistanceProvider> source, RasterGeometry grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count == 0)
                throw new ArgumentException("Signed point-distance provider collection must contain at least one source.", nameof(source));

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] == null)
                    throw new ArgumentException("Signed point-distance provider collection must not contain null sources.", nameof(source));
            }

            var sampler = new SignedPointDistanceCollectionRasterSampler<TValue>(source, _signedDistanceToValue);
            return SpatialRasterizationCore<TValue>.Rasterize(grid, sampler, nameof(grid));
        }
    }
}
