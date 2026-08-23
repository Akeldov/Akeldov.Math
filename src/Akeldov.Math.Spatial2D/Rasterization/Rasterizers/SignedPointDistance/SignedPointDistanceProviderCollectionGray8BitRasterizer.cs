using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes signed point-distance provider collections into 8-bit grayscale rasters using minimum signed distance mapping.
    /// </summary>
    public sealed class SignedPointDistanceProviderCollectionGray8BitRasterizer :
        ISpatialRasterizer<IReadOnlyList<ISignedPointDistanceProvider>, Gray8BitColor>
    {
        private readonly Func<float, Gray8BitColor> _signedDistanceToGrayLevel;

        /// <summary>
        /// Initializes a new signed point-distance provider collection rasterizer.
        /// </summary>
        /// <param name="signedDistanceToGrayLevel">
        /// The function that maps the minimum signed distance to an 8-bit grayscale value.
        /// Negative distances are inside at least one source; positive distances are outside all sources.
        /// </param>
        public SignedPointDistanceProviderCollectionGray8BitRasterizer(Func<float, Gray8BitColor> signedDistanceToGrayLevel)
        {
            _signedDistanceToGrayLevel = signedDistanceToGrayLevel ?? throw new ArgumentNullException(nameof(signedDistanceToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray8BitColor> Rasterize(IReadOnlyList<ISignedPointDistanceProvider> source, RasterGeometry grid)
        {
            ValidateSource(source);
            var sampler = new SignedPointDistanceCollectionRasterSampler<Gray8BitColor>(source, _signedDistanceToGrayLevel);
            return SpatialRasterizationCore<Gray8BitColor>.Rasterize(grid, sampler, nameof(grid));
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
