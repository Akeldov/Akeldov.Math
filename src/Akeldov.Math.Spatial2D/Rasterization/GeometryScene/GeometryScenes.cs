using System;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides 16-bit RGBA helpers for geometry scenes.
    /// </summary>
    public static class GeometryScenes
    {
        /// <summary>
        /// Rasterizes a 16-bit RGBA geometry scene on the specified grid.
        /// </summary>
        /// <param name="scene">The geometry scene to rasterize.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <returns>A new 16-bit RGBA raster with a new mutable value buffer owned by the caller.</returns>
        public static Raster<RGBA16BitColor> Rasterize3(
            this GeometryScene<RGBA16BitColor> scene,
            RasterGrid grid)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            var asd = scene.Rasterize(grid);

            return new Raster<RGBA16BitColor>(grid, asd.Values);
        }
    }
}
