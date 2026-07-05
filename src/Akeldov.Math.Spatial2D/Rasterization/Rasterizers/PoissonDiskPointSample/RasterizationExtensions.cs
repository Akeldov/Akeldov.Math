using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides rasterization extension methods.
    /// </summary>
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes Poisson disk point samples on the specified raster grid using nearest-sample distance mapping.
        /// </summary>
        /// <param name="source">The Poisson disk point samples to rasterize.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <param name="sampleDistanceToColor">
        /// The function that maps the nearest sample and distance to that sample, in world coordinate units,
        /// to a 16-bit RGBA color.
        /// </param>
        /// <returns>A 16-bit RGBA raster produced from the nearest Poisson disk point sample at each cell center.</returns>
        public static SpatialRaster<RGBA16BitColor> Rasterize(
            this IReadOnlyList<PoissonDiskPointSample> source,
            SpatialRasterGrid grid,
            Func<PoissonDiskPointSample, float, RGBA16BitColor> sampleDistanceToColor)
        {
            return source.Rasterize(
                grid,
                new PoissonDiskPointSampleCollectionDistanceRGBA16BitRasterizer(sampleDistanceToColor));
        }

        /// <summary>
        /// Rasterizes Poisson disk point samples on the specified raster grid using nearest-sample distance mapping.
        /// </summary>
        /// <param name="source">The Poisson disk point samples to rasterize.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <param name="sampleDistanceToGrayLevel">
        /// The function that maps the nearest sample and distance to that sample, in world coordinate units,
        /// to a 16-bit grayscale value.
        /// </param>
        /// <returns>A 16-bit grayscale raster produced from the nearest Poisson disk point sample at each cell center.</returns>
        public static SpatialRaster<ushort> Rasterize(
            this IReadOnlyList<PoissonDiskPointSample> source,
            SpatialRasterGrid grid,
            Func<PoissonDiskPointSample, float, ushort> sampleDistanceToGrayLevel)
        {
            return source.Rasterize(
                grid,
                new PoissonDiskPointSampleCollectionDistanceGray16BitRasterizer(sampleDistanceToGrayLevel));
        }
    }
}
