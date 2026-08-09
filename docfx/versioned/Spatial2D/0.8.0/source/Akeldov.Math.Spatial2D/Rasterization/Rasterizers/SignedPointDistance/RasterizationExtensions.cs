using Akeldov.Math.Spatial2D.Imaging;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes signed point-distance providers into an 8-bit grayscale raster using minimum signed distance mapping.
        /// </summary>
        /// <param name="signedPointDistanceProviders">The signed point-distance providers to rasterize.</param>
        /// <param name="signedDistanceToGrayLevel">
        /// The function that maps minimum signed distance, in world coordinate units, to an 8-bit grayscale value.
        /// Negative distances are inside at least one source; positive distances are outside all sources.
        /// </param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the minimum signed distance at each cell center.</returns>
        public static SpatialRaster<Gray8BitColor> Rasterize(
            this IReadOnlyList<ISignedPointDistanceProvider> signedPointDistanceProviders,
            Func<float, Gray8BitColor> signedDistanceToGrayLevel,
            RasterGeometry rasterGeometry)
        {
            var rasterizer = new SignedPointDistanceProviderCollectionGray8BitRasterizer(signedDistanceToGrayLevel);
            var raster = rasterizer.Rasterize(signedPointDistanceProviders, rasterGeometry);
            return raster;
        }

        /// <summary>
        /// Rasterizes a signed point-distance provider into an 8-bit grayscale raster using signed distance mapping.
        /// </summary>
        /// <param name="signedPointDistanceProvider">The signed point-distance provider to rasterize.</param>
        /// <param name="signedDistanceToGrayLevel">
        /// The function that maps signed distance, in world coordinate units, to an 8-bit grayscale value.
        /// Negative distances are inside the source; positive distances are outside.
        /// </param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the signed distance at each cell center.</returns>
        public static SpatialRaster<Gray8BitColor> Rasterize(
            this ISignedPointDistanceProvider signedPointDistanceProvider,
            Func<float, Gray8BitColor> signedDistanceToGrayLevel,
            RasterGeometry rasterGeometry)
        {
            var rasterizer = new SignedPointDistanceProviderGray8BitRasterizer(signedDistanceToGrayLevel);
            var raster = rasterizer.Rasterize(signedPointDistanceProvider, rasterGeometry);
            return raster;
        }

        /// <summary>
        /// Rasterizes signed point-distance providers into a 16-bit grayscale raster using minimum signed distance mapping.
        /// </summary>
        /// <param name="signedPointDistanceProviders">The signed point-distance providers to rasterize.</param>
        /// <param name="signedDistanceToGrayLevel">
        /// The function that maps minimum signed distance, in world coordinate units, to a 16-bit grayscale value.
        /// Negative distances are inside at least one source; positive distances are outside all sources.
        /// </param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the minimum signed distance at each cell center.</returns>
        public static SpatialRaster<Gray16BitColor> Rasterize(
            this IReadOnlyList<ISignedPointDistanceProvider> signedPointDistanceProviders,
            Func<float, Gray16BitColor> signedDistanceToGrayLevel,
            RasterGeometry rasterGeometry)
        {
            var rasterizer = new SignedPointDistanceProviderCollectionGray16BitRasterizer(signedDistanceToGrayLevel);
            var raster = rasterizer.Rasterize(signedPointDistanceProviders, rasterGeometry);
            return raster;
        }

        /// <summary>
        /// Rasterizes a signed point-distance provider into a 16-bit grayscale raster using signed distance mapping.
        /// </summary>
        /// <param name="signedPointDistanceProvider">The signed point-distance provider to rasterize.</param>
        /// <param name="signedDistanceToGrayLevel">
        /// The function that maps signed distance, in world coordinate units, to a 16-bit grayscale value.
        /// Negative distances are inside the source; positive distances are outside.
        /// </param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the signed distance at each cell center.</returns>
        public static SpatialRaster<Gray16BitColor> Rasterize(
            this ISignedPointDistanceProvider signedPointDistanceProvider,
            Func<float, Gray16BitColor> signedDistanceToGrayLevel,
            RasterGeometry rasterGeometry)
        {
            var rasterizer = new SignedPointDistanceProviderGray16BitRasterizer(signedDistanceToGrayLevel);
            var raster = rasterizer.Rasterize(signedPointDistanceProvider, rasterGeometry);
            return raster;
        }
    }
}
