using System;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes point-distance providers into 8-bit grayscale rasters using unsigned distance mapping.
    /// </summary>
    public sealed class PointDistanceProviderGray8BitRasterizer : ISpatialRasterizer<IPointDistanceProvider, Gray8BitColor>
    {
        private readonly Func<float, Gray8BitColor> _distanceToGrayLevel;

        /// <summary>
        /// Initializes a new point-distance provider rasterizer.
        /// </summary>
        /// <param name="distanceToGrayLevel">The function that maps unsigned distance to an 8-bit grayscale value.</param>
        public PointDistanceProviderGray8BitRasterizer(Func<float, Gray8BitColor> distanceToGrayLevel)
        {
            _distanceToGrayLevel = distanceToGrayLevel ?? throw new ArgumentNullException(nameof(distanceToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray8BitColor> Rasterize(IPointDistanceProvider source, RasterGeometry grid)
        {
            return Rasterize<IPointDistanceProvider>(source, grid);
        }

        /// <summary>
        /// Rasterizes a point-distance provider into an 8-bit grayscale raster using unsigned distance mapping.
        /// </summary>
        /// <typeparam name="T">The point-distance provider type.</typeparam>
        /// <param name="source">The point-distance provider to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the point-distance provider at each cell center.</returns>
        public SpatialRaster<Gray8BitColor> Rasterize<T>(T source, RasterGeometry grid)
            where T : IPointDistanceProvider
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var sampler = new PointDistanceRasterSampler<T, Gray8BitColor>(source, _distanceToGrayLevel);
            return SpatialRasterizationCore<Gray8BitColor>.Rasterize(grid, sampler, nameof(grid));
        }
    }
}
