using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static class IParameterizedCurveIReadOnlyListExtensions
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
    }
}

