using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes point-distance providers into an 8-bit grayscale raster using nearest unsigned distance mapping.
        /// </summary>
        /// <typeparam name="T">The point-distance provider type.</typeparam>
        /// <param name="pointDistanceProviders">The point-distance providers to rasterize.</param>
        /// <param name="distanceToGrayLevel">
        /// The function that maps nearest unsigned distance, in world coordinate units, to an 8-bit grayscale value.
        /// </param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the nearest point-distance provider at each cell center.</returns>
        public static SpatialRaster<byte> Rasterize<T>(
            this IReadOnlyList<T> pointDistanceProviders,
            Func<float, byte> distanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
            where T : IPointDistanceProvider
        {
            if (typeof(ISignedPointDistanceProvider).IsAssignableFrom(typeof(T)) &&
                pointDistanceProviders is IReadOnlyList<ISignedPointDistanceProvider> signedPointDistanceProviders)
            {
                var signedRasterizer = new SignedPointDistanceProviderCollectionGray8BitRasterizer(distanceToGrayLevel);
                return signedRasterizer.Rasterize(signedPointDistanceProviders, spatialRasterGrid);
            }

            var rasterizer = new PointDistanceProviderCollectionGray8BitRasterizer(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProviders, spatialRasterGrid);
            return raster;
        }

        /// <summary>
        /// Rasterizes a point-distance provider into an 8-bit grayscale raster using unsigned distance mapping.
        /// </summary>
        /// <typeparam name="T">The point-distance provider type.</typeparam>
        /// <param name="pointDistanceProvider">The point-distance provider to rasterize.</param>
        /// <param name="distanceToGrayLevel">
        /// The function that maps unsigned distance, in world coordinate units, to an 8-bit grayscale value.
        /// </param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the point-distance provider at each cell center.</returns>
        public static SpatialRaster<byte> Rasterize<T>(
            this T pointDistanceProvider,
            Func<float, byte> distanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
            where T : IPointDistanceProvider
        {
            if (typeof(ISignedPointDistanceProvider).IsAssignableFrom(typeof(T)) &&
                pointDistanceProvider is ISignedPointDistanceProvider signedPointDistanceProvider)
            {
                var signedRasterizer = new SignedPointDistanceProviderGray8BitRasterizer(distanceToGrayLevel);
                return signedRasterizer.Rasterize(signedPointDistanceProvider, spatialRasterGrid);
            }

            var rasterizer = new PointDistanceProviderGray8BitRasterizer(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProvider, spatialRasterGrid);
            return raster;
        }

        /// <summary>
        /// Rasterizes point-distance providers into a 16-bit grayscale raster using nearest unsigned distance mapping.
        /// </summary>
        /// <typeparam name="T">The point-distance provider type.</typeparam>
        /// <param name="pointDistanceProviders">The point-distance providers to rasterize.</param>
        /// <param name="distanceToGrayLevel">
        /// The function that maps nearest unsigned distance, in world coordinate units, to a 16-bit grayscale value.
        /// </param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the nearest point-distance provider at each cell center.</returns>
        public static SpatialRaster<ushort> Rasterize<T>(
            this IReadOnlyList<T> pointDistanceProviders,
            Func<float, ushort> distanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
            where T : IPointDistanceProvider
        {
            if (typeof(ISignedPointDistanceProvider).IsAssignableFrom(typeof(T)) &&
                pointDistanceProviders is IReadOnlyList<ISignedPointDistanceProvider> signedPointDistanceProviders)
            {
                var signedRasterizer = new SignedPointDistanceProviderCollectionGray16BitRasterizer(distanceToGrayLevel);
                return signedRasterizer.Rasterize(signedPointDistanceProviders, spatialRasterGrid);
            }

            var rasterizer = new PointDistanceProviderCollectionGray16BitRasterizer(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProviders, spatialRasterGrid);
            return raster;
        }

        /// <summary>
        /// Rasterizes a point-distance provider into a 16-bit grayscale raster using unsigned distance mapping.
        /// </summary>
        /// <typeparam name="T">The point-distance provider type.</typeparam>
        /// <param name="pointDistanceProvider">The point-distance provider to rasterize.</param>
        /// <param name="distanceToGrayLevel">
        /// The function that maps unsigned distance, in world coordinate units, to a 16-bit grayscale value.
        /// </param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the point-distance provider at each cell center.</returns>
        public static SpatialRaster<ushort> Rasterize<T>(
            this T pointDistanceProvider,
            Func<float, ushort> distanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
            where T : IPointDistanceProvider
        {
            if (typeof(ISignedPointDistanceProvider).IsAssignableFrom(typeof(T)) &&
                pointDistanceProvider is ISignedPointDistanceProvider signedPointDistanceProvider)
            {
                var signedRasterizer = new SignedPointDistanceProviderGray16BitRasterizer(distanceToGrayLevel);
                return signedRasterizer.Rasterize(signedPointDistanceProvider, spatialRasterGrid);
            }

            var rasterizer = new PointDistanceProviderGray16BitRasterizer(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProvider, spatialRasterGrid);
            return raster;
        }
    }
}
