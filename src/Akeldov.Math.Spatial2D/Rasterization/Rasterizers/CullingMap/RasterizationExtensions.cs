using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides rasterization extension methods.
    /// </summary>
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes point-source culling selections on the specified raster grid using the specified source position color selector.
        /// </summary>
        /// <typeparam name="TPointSource">The point influence source type.</typeparam>
        /// <param name="sources">The point influence sources used to color culling selections.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <param name="culler">The culler used to select sources for each raster cell center.</param>
        /// <param name="sourcePositionToColor">The function that maps a selected source position to a 16-bit RGBA color.</param>
        /// <returns>A 16-bit RGBA raster showing the culling selection map.</returns>
        public static SpatialRaster<RGBA16BitColor> RasterizeCullingMap<TPointSource>(
            this IReadOnlyList<TPointSource> sources,
            RasterGeometry grid,
            IInfluenceSourceCuller<TPointSource> culler,
            Func<PointXY, RGBA16BitColor> sourcePositionToColor)
            where TPointSource : IPointInfluenceSource
        {
            if (sources is null)
                throw new ArgumentNullException(nameof(sources));

            var rasterizer = new CullingMapRGBA16BitRasterizer<TPointSource>(culler, sourcePositionToColor);
            return rasterizer.Rasterize(sources, grid);
        }
    }
}
