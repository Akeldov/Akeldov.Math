using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides rasterization extension methods.
    /// </summary>
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes a floating-point point influence field as a 16-bit RGBA heat map.
        /// </summary>
        /// <param name="sources">The influence field to rasterize.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit RGBA heat map raster produced from the influence field values.</returns>
        public static SpatialRaster<RGBA16BitColor> RasterizeHeatMap(
            this FloatPointInfluenceField sources,
            RasterGeometry grid)
        {
            if (sources is null)
                throw new ArgumentNullException(nameof(sources));

            var rasterizer = new FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer();
            return rasterizer.Rasterize(sources, grid);
        }
    }
}
