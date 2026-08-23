using System;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes signed point-distance providers into 8-bit grayscale rasters using signed distance mapping.
    /// </summary>
    public sealed class SignedPointDistanceProviderGray8BitRasterizer : ISpatialRasterizer<ISignedPointDistanceProvider, Gray8BitColor>
    {
        private readonly Func<float, Gray8BitColor> _signedDistanceToGrayLevel;

        /// <summary>
        /// Initializes a new signed point-distance provider rasterizer.
        /// </summary>
        /// <param name="signedDistanceToGrayLevel">The function that maps signed distance to an 8-bit grayscale value. Negative distances are inside the source; positive distances are outside.</param>
        public SignedPointDistanceProviderGray8BitRasterizer(Func<float, Gray8BitColor> signedDistanceToGrayLevel)
        {
            _signedDistanceToGrayLevel = signedDistanceToGrayLevel ?? throw new ArgumentNullException(nameof(signedDistanceToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray8BitColor> Rasterize(ISignedPointDistanceProvider source, RasterGeometry grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sampler = new SignedPointDistanceRasterSampler<Gray8BitColor>(source, _signedDistanceToGrayLevel);
            return SpatialRasterizationCore<Gray8BitColor>.Rasterize(grid, sampler, nameof(grid));
        }
    }
}
