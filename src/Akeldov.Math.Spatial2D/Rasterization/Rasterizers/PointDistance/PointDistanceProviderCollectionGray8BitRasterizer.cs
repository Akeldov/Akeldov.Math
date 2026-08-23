using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes point-distance provider collections into 8-bit grayscale rasters using nearest unsigned distance mapping.
    /// </summary>
    public sealed class PointDistanceProviderCollectionGray8BitRasterizer :
        ISpatialRasterizer<IReadOnlyList<IPointDistanceProvider>, Gray8BitColor>
    {
        private readonly Func<float, Gray8BitColor> _distanceToGrayLevel;

        /// <summary>
        /// Initializes a new point-distance provider collection rasterizer.
        /// </summary>
        /// <param name="distanceToGrayLevel">The function that maps nearest unsigned distance to an 8-bit grayscale value.</param>
        public PointDistanceProviderCollectionGray8BitRasterizer(Func<float, Gray8BitColor> distanceToGrayLevel)
        {
            _distanceToGrayLevel = distanceToGrayLevel ?? throw new ArgumentNullException(nameof(distanceToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray8BitColor> Rasterize(IReadOnlyList<IPointDistanceProvider> source, RasterGeometry grid)
        {
            return Rasterize<IPointDistanceProvider>(source, grid);
        }

        /// <summary>
        /// Rasterizes point-distance providers into an 8-bit grayscale raster using nearest unsigned distance mapping.
        /// </summary>
        /// <typeparam name="T">The point-distance provider type.</typeparam>
        /// <param name="source">The point-distance providers to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the nearest point-distance provider at each cell center.</returns>
        public SpatialRaster<Gray8BitColor> Rasterize<T>(IReadOnlyList<T> source, RasterGeometry grid)
            where T : IPointDistanceProvider
        {
            ValidateSource(source);
            var sampler = new PointDistanceCollectionRasterSampler<T, Gray8BitColor>(source, _distanceToGrayLevel);
            return SpatialRasterizationCore<Gray8BitColor>.Rasterize(grid, sampler, nameof(grid));
        }

        private static void ValidateSource<T>(IReadOnlyList<T> source)
            where T : IPointDistanceProvider
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count == 0)
                throw new ArgumentException("Point-distance provider collection must contain at least one source.", nameof(source));

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] is null)
                    throw new ArgumentException("Point-distance provider collection must not contain null sources.", nameof(source));
            }
        }

    }
}
