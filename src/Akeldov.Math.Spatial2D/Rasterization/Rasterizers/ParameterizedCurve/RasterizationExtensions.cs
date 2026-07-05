using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class RasterizationExtensions
    {
        public static SpatialRaster<byte> Rasterize(
            this IReadOnlyList<IParameterizedCurve> curves,
            Func<float, float, byte> projectionToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new ParameterizedCurveCollectionDistanceGray8BitRasterizer(projectionToGrayLevel);
            var raster = rasterizer.Rasterize(curves, spatialRasterGrid);
            return raster;
        }

        public static SpatialRaster<byte> Rasterize(
            this IParameterizedCurve curve,
            Func<float, float, byte> projectionToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new ParameterizedCurveDistanceGray8BitRasterizer(projectionToGrayLevel);
            var raster = rasterizer.Rasterize(curve, spatialRasterGrid);
            return raster;
        }

        public static SpatialRaster<ushort> Rasterize(
            this IReadOnlyList<IParameterizedCurve> curves,
            Func<float, float, ushort> projectionToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new ParameterizedCurveCollectionDistanceGray16BitRasterizer(projectionToGrayLevel);
            var raster = rasterizer.Rasterize(curves, spatialRasterGrid);
            return raster;
        }

        public static SpatialRaster<ushort> Rasterize(
            this IParameterizedCurve curve,
            Func<float, float, ushort> projectionToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new ParameterizedCurveDistanceGray16BitRasterizer(projectionToGrayLevel);
            var raster = rasterizer.Rasterize(curve, spatialRasterGrid);
            return raster;
        }
    }
}

