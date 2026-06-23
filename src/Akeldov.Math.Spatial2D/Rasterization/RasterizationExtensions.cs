using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides rasterization extension methods.
    /// </summary>
    public static class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes a source object on the specified raster grid using the specified rasterizer.
        /// </summary>
        /// <typeparam name="TSource">The source object type to rasterize.</typeparam>
        /// <typeparam name="TRaster">The raster artifact type produced by the rasterizer.</typeparam>
        /// <param name="source">The source object to rasterize.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <param name="rasterizer">The rasterization strategy.</param>
        /// <returns>The raster artifact produced from the source object.</returns>
        public static TRaster Rasterize<TSource, TRaster>(
            this TSource source,
            RasterGrid grid,
            IRasterizer<TSource, TRaster> rasterizer)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (rasterizer == null)
                throw new ArgumentNullException(nameof(rasterizer));

            return rasterizer.Rasterize(source, grid);
        }

        /// <summary>
        /// Rasterizes point-source culling selections on the specified raster grid using the specified source position color selector.
        /// </summary>
        /// <typeparam name="TPointSource">The point influence source type.</typeparam>
        /// <param name="source">The point influence sources used to color culling selections.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <param name="culler">The culler used to select sources for each raster cell center.</param>
        /// <param name="sourcePositionToColor">The function that maps a selected source position to a 16-bit RGBA color.</param>
        /// <returns>A 16-bit RGBA raster showing the culling selection map.</returns>
        public static Raster<RGBA16BitColor> RasterizeCullingMap<TPointSource>(
            this IReadOnlyList<TPointSource> source,
            RasterGrid grid,
            IInfluenceSourceCuller<TPointSource> culler,
            Func<PointXY, RGBA16BitColor> sourcePositionToColor)
            where TPointSource : IPointInfluenceSource
        {
            return source.Rasterize(grid, new CullingMapRGBA16BitRasterizer<TPointSource>(culler, sourcePositionToColor));
        }

        /// <summary>
        /// Rasterizes a floating-point point influence field as a 16-bit RGBA heat map.
        /// </summary>
        /// <param name="source">The influence field to rasterize.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit RGBA heat map raster produced from the influence field values.</returns>
        public static Raster<RGBA16BitColor> RasterizeHeatMap(
            this FloatPointInfluenceField source,
            RasterGrid grid)
        {
            return source.Rasterize(grid, new FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer());
        }

        /// <summary>
        /// Rasterizes Poisson disk point samples on the specified raster grid using nearest-sample distance mapping.
        /// </summary>
        /// <param name="source">The Poisson disk point samples to rasterize.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <param name="sampleDistanceToColor">
        /// The function that maps the nearest sample and distance to that sample, in world coordinate units,
        /// to a 16-bit RGBA color.
        /// </param>
        /// <returns>A 16-bit RGBA raster produced from the nearest Poisson disk point sample at each cell center.</returns>
        public static Raster<RGBA16BitColor> Rasterize(
            this IReadOnlyList<PoissonDiskPointSample> source,
            RasterGrid grid,
            Func<PoissonDiskPointSample, float, RGBA16BitColor> sampleDistanceToColor)
        {
            return source.Rasterize(
                grid,
                new PoissonDiskPointSampleCollectionDistanceRGBA16BitRasterizer(sampleDistanceToColor));
        }

        /// <summary>
        /// Rasterizes Poisson disk point samples on the specified raster grid using nearest-sample distance mapping.
        /// </summary>
        /// <param name="source">The Poisson disk point samples to rasterize.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <param name="sampleDistanceToGrayLevel">
        /// The function that maps the nearest sample and distance to that sample, in world coordinate units,
        /// to a 16-bit grayscale value.
        /// </param>
        /// <returns>A 16-bit grayscale raster produced from the nearest Poisson disk point sample at each cell center.</returns>
        public static Raster<ushort> Rasterize(
            this IReadOnlyList<PoissonDiskPointSample> source,
            RasterGrid grid,
            Func<PoissonDiskPointSample, float, ushort> sampleDistanceToGrayLevel)
        {
            return source.Rasterize(
                grid,
                new PoissonDiskPointSampleCollectionDistanceGray16BitRasterizer(sampleDistanceToGrayLevel));
        }
    }
}
