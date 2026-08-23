using Akeldov.Math.Spatial2D.Imaging;
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
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the nearest point-distance provider at each cell center.</returns>
        public static SpatialRaster<Gray8BitColor> Rasterize<T>(
            this IReadOnlyList<T> pointDistanceProviders,
            Func<float, Gray8BitColor> distanceToGrayLevel,
            RasterGeometry rasterGeometry)
            where T : IPointDistanceProvider
        {
            if (typeof(ISignedPointDistanceProvider).IsAssignableFrom(typeof(T)) &&
                pointDistanceProviders is IReadOnlyList<ISignedPointDistanceProvider> signedPointDistanceProviders)
            {
                var signedRasterizer = new SignedPointDistanceProviderCollectionRasterizer<Gray8BitColor>(distanceToGrayLevel);
                return signedRasterizer.Rasterize(signedPointDistanceProviders, rasterGeometry);
            }

            var rasterizer = new PointDistanceProviderCollectionRasterizer<Gray8BitColor>(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProviders, rasterGeometry);
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
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the point-distance provider at each cell center.</returns>
        public static SpatialRaster<Gray8BitColor> Rasterize<T>(
            this T pointDistanceProvider,
            Func<float, Gray8BitColor> distanceToGrayLevel,
            RasterGeometry rasterGeometry)
            where T : IPointDistanceProvider
        {
            if (typeof(ISignedPointDistanceProvider).IsAssignableFrom(typeof(T)) &&
                pointDistanceProvider is ISignedPointDistanceProvider signedPointDistanceProvider)
            {
                var signedRasterizer = new SignedPointDistanceProviderRasterizer<Gray8BitColor>(distanceToGrayLevel);
                return signedRasterizer.Rasterize(signedPointDistanceProvider, rasterGeometry);
            }

            var rasterizer = new PointDistanceProviderRasterizer<Gray8BitColor>(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProvider, rasterGeometry);
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
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the nearest point-distance provider at each cell center.</returns>
        public static SpatialRaster<Gray16BitColor> Rasterize<T>(
            this IReadOnlyList<T> pointDistanceProviders,
            Func<float, Gray16BitColor> distanceToGrayLevel,
            RasterGeometry rasterGeometry)
            where T : IPointDistanceProvider
        {
            if (typeof(ISignedPointDistanceProvider).IsAssignableFrom(typeof(T)) &&
                pointDistanceProviders is IReadOnlyList<ISignedPointDistanceProvider> signedPointDistanceProviders)
            {
                var signedRasterizer = new SignedPointDistanceProviderCollectionRasterizer<Gray16BitColor>(distanceToGrayLevel);
                return signedRasterizer.Rasterize(signedPointDistanceProviders, rasterGeometry);
            }

            var rasterizer = new PointDistanceProviderCollectionRasterizer<Gray16BitColor>(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProviders, rasterGeometry);
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
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the point-distance provider at each cell center.</returns>
        public static SpatialRaster<Gray16BitColor> Rasterize<T>(
            this T pointDistanceProvider,
            Func<float, Gray16BitColor> distanceToGrayLevel,
            RasterGeometry rasterGeometry)
            where T : IPointDistanceProvider
        {
            if (typeof(ISignedPointDistanceProvider).IsAssignableFrom(typeof(T)) &&
                pointDistanceProvider is ISignedPointDistanceProvider signedPointDistanceProvider)
            {
                var signedRasterizer = new SignedPointDistanceProviderRasterizer<Gray16BitColor>(distanceToGrayLevel);
                return signedRasterizer.Rasterize(signedPointDistanceProvider, rasterGeometry);
            }

            var rasterizer = new PointDistanceProviderRasterizer<Gray16BitColor>(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProvider, rasterGeometry);
            return raster;
        }
    }
}
