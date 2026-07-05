using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes parameterized curves into an 8-bit grayscale raster using nearest-curve projection mapping.
        /// </summary>
        /// <param name="curves">The parameterized curves to rasterize.</param>
        /// <param name="projectionToGrayLevel">
        /// The function that maps distance to the nearest curve and curve coordinate on that curve to an 8-bit grayscale value.
        /// The first argument is distance in world coordinate units; the second argument is curve coordinate on the nearest curve.
        /// </param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the nearest parameterized curve projection at each cell center.</returns>
        public static SpatialRaster<byte> Rasterize(
            this IReadOnlyList<IParameterizedCurve> curves,
            Func<float, float, byte> projectionToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new ParameterizedCurveCollectionDistanceGray8BitRasterizer(projectionToGrayLevel);
            var raster = rasterizer.Rasterize(curves, spatialRasterGrid);
            return raster;
        }

        /// <summary>
        /// Rasterizes a parameterized curve into an 8-bit grayscale raster using projection-to-curve mapping.
        /// </summary>
        /// <param name="curve">The parameterized curve to rasterize.</param>
        /// <param name="projectionToGrayLevel">
        /// The function that maps distance to the curve and curve coordinate to an 8-bit grayscale value.
        /// The first argument is distance in world coordinate units; the second argument is curve coordinate.
        /// </param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the parameterized curve projection at each cell center.</returns>
        public static SpatialRaster<byte> Rasterize(
            this IParameterizedCurve curve,
            Func<float, float, byte> projectionToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new ParameterizedCurveDistanceGray8BitRasterizer(projectionToGrayLevel);
            var raster = rasterizer.Rasterize(curve, spatialRasterGrid);
            return raster;
        }

        /// <summary>
        /// Rasterizes parameterized curves into a 16-bit grayscale raster using nearest-curve projection mapping.
        /// </summary>
        /// <param name="curves">The parameterized curves to rasterize.</param>
        /// <param name="projectionToGrayLevel">
        /// The function that maps distance to the nearest curve and curve coordinate on that curve to a 16-bit grayscale value.
        /// The first argument is distance in world coordinate units; the second argument is curve coordinate on the nearest curve.
        /// </param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the nearest parameterized curve projection at each cell center.</returns>
        public static SpatialRaster<ushort> Rasterize(
            this IReadOnlyList<IParameterizedCurve> curves,
            Func<float, float, ushort> projectionToGrayLevel,
            SpatialRasterGrid spatialRasterGrid)
        {
            var rasterizer = new ParameterizedCurveCollectionDistanceGray16BitRasterizer(projectionToGrayLevel);
            var raster = rasterizer.Rasterize(curves, spatialRasterGrid);
            return raster;
        }

        /// <summary>
        /// Rasterizes a parameterized curve into a 16-bit grayscale raster using projection-to-curve mapping.
        /// </summary>
        /// <param name="curve">The parameterized curve to rasterize.</param>
        /// <param name="projectionToGrayLevel">
        /// The function that maps distance to the curve and curve coordinate to a 16-bit grayscale value.
        /// The first argument is distance in world coordinate units; the second argument is curve coordinate.
        /// </param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the parameterized curve projection at each cell center.</returns>
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
