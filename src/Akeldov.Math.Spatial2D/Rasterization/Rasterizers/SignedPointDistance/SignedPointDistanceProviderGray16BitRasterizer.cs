using System;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes signed point-distance providers into 16-bit grayscale rasters using signed distance mapping.
    /// </summary>
    public sealed class SignedPointDistanceProviderGray16BitRasterizer : ISpatialRasterizer<ISignedPointDistanceProvider, Gray16BitColor>
    {
        private readonly Func<float, Gray16BitColor> _signedDistanceToGrayLevel;

        /// <summary>
        /// Initializes a new signed point-distance provider rasterizer.
        /// </summary>
        /// <param name="signedDistanceToGrayLevel">The function that maps signed distance to a 16-bit grayscale value. Negative distances are inside the source; positive distances are outside.</param>
        public SignedPointDistanceProviderGray16BitRasterizer(Func<float, Gray16BitColor> signedDistanceToGrayLevel)
        {
            _signedDistanceToGrayLevel = signedDistanceToGrayLevel ?? throw new ArgumentNullException(nameof(signedDistanceToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray16BitColor> Rasterize(ISignedPointDistanceProvider source, RasterGeometry grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sampler = new SignedPointDistanceRasterSampler<Gray16BitColor>(source, _signedDistanceToGrayLevel);
            return SpatialRasterizationCore<Gray16BitColor>.Rasterize(grid, sampler, nameof(grid));
        }
    }
}
