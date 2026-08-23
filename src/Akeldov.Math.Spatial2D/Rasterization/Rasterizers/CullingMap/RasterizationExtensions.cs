using System;
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
        /// <param name="sourceIndex">The index that owns and selects the point influence sources.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <param name="sourcePositionToColor">The function that maps a selected source position to a 16-bit RGBA color.</param>
        /// <returns>A 16-bit RGBA raster showing the culling selection map.</returns>
        public static SpatialRaster<RGBA16BitColor> RasterizeCullingMap<TPointSource>(
            this IInfluenceSourceIndex<TPointSource> sourceIndex,
            RasterGeometry grid,
            Func<PointXY, RGBA16BitColor> sourcePositionToColor)
            where TPointSource : IPointInfluenceSource
        {
            if (sourceIndex is null)
                throw new ArgumentNullException(nameof(sourceIndex));

            var rasterizer = new CullingMapRGBA16BitRasterizer<TPointSource>(sourcePositionToColor);
            return rasterizer.Rasterize(sourceIndex, grid);
        }
    }
}
