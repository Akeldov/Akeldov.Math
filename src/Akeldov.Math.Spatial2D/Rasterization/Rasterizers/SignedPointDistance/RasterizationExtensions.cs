using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class RasterizationExtensions
    {
        public static SpatialRaster<byte> Rasterize(
            this IReadOnlyList<ISignedPointDistanceProvider> signedPointDistanceProviders,
            Func<float, byte> signedDistanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new SignedPointDistanceProviderCollectionGray8BitRasterizer(signedDistanceToGrayLevel);
            var raster = rasterizer.Rasterize(signedPointDistanceProviders, spatialRasterGrid);
            return raster;
        }

        public static SpatialRaster<byte> Rasterize(
            this ISignedPointDistanceProvider signedPointDistanceProvider,
            Func<float, byte> signedDistanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new SignedPointDistanceProviderGray8BitRasterizer(signedDistanceToGrayLevel);
            var raster = rasterizer.Rasterize(signedPointDistanceProvider, spatialRasterGrid);
            return raster;
        }

        public static SpatialRaster<ushort> Rasterize(
            this IReadOnlyList<ISignedPointDistanceProvider> signedPointDistanceProviders,
            Func<float, ushort> signedDistanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new SignedPointDistanceProviderCollectionGray16BitRasterizer(signedDistanceToGrayLevel);
            var raster = rasterizer.Rasterize(signedPointDistanceProviders, spatialRasterGrid);
            return raster;
        }

        public static SpatialRaster<ushort> Rasterize(
            this ISignedPointDistanceProvider signedPointDistanceProvider,
            Func<float, ushort> signedDistanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new SignedPointDistanceProviderGray16BitRasterizer(signedDistanceToGrayLevel);
            var raster = rasterizer.Rasterize(signedPointDistanceProvider, spatialRasterGrid);
            return raster;
        }
    }
}

