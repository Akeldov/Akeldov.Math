using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class RasterizationExtensions
    {
        public static SpatialRaster<byte> Rasterize(
            this IReadOnlyList<IPointDistanceProvider> pointDistanceProviders,
            Func<float, byte> distanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new PointDistanceProviderCollectionGray8BitRasterizer(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProviders, spatialRasterGrid);
            return raster;
        }

        public static SpatialRaster<byte> Rasterize(
            this IPointDistanceProvider pointDistanceProvider,
            Func<float, byte> distanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new PointDistanceProviderGray8BitRasterizer(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProvider, spatialRasterGrid);
            return raster;
        }

        public static SpatialRaster<ushort> Rasterize(
            this IReadOnlyList<IPointDistanceProvider> pointDistanceProviders,
            Func<float, ushort> distanceToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new PointDistanceProviderCollectionGray16BitRasterizer(distanceToGrayLevel);
            var raster = rasterizer.Rasterize(pointDistanceProviders, spatialRasterGrid);
            return raster;
        }

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

