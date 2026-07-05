using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes point-distance providers into an 8-bit grayscale raster using nearest unsigned distance mapping.
        /// </summary>
        /// <param name="pointDistanceProviders">The point-distance providers to rasterize.</param>
        /// <param name="distanceToGrayLevel">
        /// The function that maps nearest unsigned distance, in world coordinate units, to an 8-bit grayscale value.
        /// </param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the nearest point-distance provider at each cell center.</returns>
        public static SpatialRaster<byte> Rasterize(
            this IReadOnlyList<IPointDistanceProvider> pointDistanceProviders,
            Func<float, byte> distanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new PointDistanceProviderCollectionGray8BitRasterizer(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProviders, spatialRasterGrid);
            return raster;
        }

        /// <summary>
        /// Rasterizes a point-distance provider into an 8-bit grayscale raster using unsigned distance mapping.
        /// </summary>
        /// <param name="pointDistanceProvider">The point-distance provider to rasterize.</param>
        /// <param name="distanceToGrayLevel">
        /// The function that maps unsigned distance, in world coordinate units, to an 8-bit grayscale value.
        /// </param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the point-distance provider at each cell center.</returns>
        public static SpatialRaster<byte> Rasterize(
            this IPointDistanceProvider pointDistanceProvider,
            Func<float, byte> distanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new PointDistanceProviderGray8BitRasterizer(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProvider, spatialRasterGrid);
            return raster;
        }

        /// <summary>
        /// Rasterizes point-distance providers into a 16-bit grayscale raster using nearest unsigned distance mapping.
        /// </summary>
        /// <param name="pointDistanceProviders">The point-distance providers to rasterize.</param>
        /// <param name="distanceToGrayLevel">
        /// The function that maps nearest unsigned distance, in world coordinate units, to a 16-bit grayscale value.
        /// </param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the nearest point-distance provider at each cell center.</returns>
        public static SpatialRaster<ushort> Rasterize(
            this IReadOnlyList<IPointDistanceProvider> pointDistanceProviders,
            Func<float, ushort> distanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new PointDistanceProviderCollectionGray16BitRasterizer(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProviders, spatialRasterGrid);
            return raster;
        }

        /// <summary>
        /// Rasterizes a point-distance provider into a 16-bit grayscale raster using unsigned distance mapping.
        /// </summary>
        /// <param name="pointDistanceProvider">The point-distance provider to rasterize.</param>
        /// <param name="distanceToGrayLevel">
        /// The function that maps unsigned distance, in world coordinate units, to a 16-bit grayscale value.
        /// </param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the point-distance provider at each cell center.</returns>
        public static SpatialRaster<ushort> Rasterize(
            this IPointDistanceProvider pointDistanceProvider,
            Func<float, ushort> distanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new PointDistanceProviderGray16BitRasterizer(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProvider, spatialRasterGrid);
            return raster;
        }
    }
}
