using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes signed point-distance provider collections into 16-bit grayscale rasters using minimum signed distance mapping.
    /// </summary>
    public sealed class SignedPointDistanceProviderCollectionGray16BitRasterizer :
        ISpatialRasterizer<IReadOnlyList<ISignedPointDistanceProvider>, Gray16BitColor>
    {
        private readonly Func<float, Gray16BitColor> _signedDistanceToGrayLevel;

        /// <summary>
        /// Initializes a new signed point-distance provider collection rasterizer.
        /// </summary>
        /// <param name="signedDistanceToGrayLevel">
        /// The function that maps the minimum signed distance to a 16-bit grayscale value.
        /// Negative distances are inside at least one source; positive distances are outside all sources.
        /// </param>
        public SignedPointDistanceProviderCollectionGray16BitRasterizer(Func<float, Gray16BitColor> signedDistanceToGrayLevel)
        {
            _signedDistanceToGrayLevel = signedDistanceToGrayLevel ?? throw new ArgumentNullException(nameof(signedDistanceToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray16BitColor> Rasterize(IReadOnlyList<ISignedPointDistanceProvider> source, RasterGeometry grid)
        {
            ValidateSource(source);
            var sampler = new SignedPointDistanceCollectionRasterSampler<Gray16BitColor>(source, _signedDistanceToGrayLevel);
            return SpatialRasterizationCore<Gray16BitColor>.Rasterize(grid, sampler, nameof(grid));
        }

        private static void ValidateSource(IReadOnlyList<ISignedPointDistanceProvider> source)
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
        }

    }
}
