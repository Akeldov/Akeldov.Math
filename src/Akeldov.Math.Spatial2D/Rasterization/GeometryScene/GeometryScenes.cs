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
        /// Creates a geometry scene that samples <see cref="RGBA16BitColor"/> values with a transparent background.
        /// </summary>
        /// <returns>A geometry scene configured with 16-bit RGBA source-over alpha blending as the default layer blend.</returns>
        public static GeometryScene<RGBA16BitColor> CreateRGBA16Bit()
        {
            return CreateRGBA16Bit(default(RGBA16BitColor));
        }

        /// <summary>
        /// Creates a geometry scene that samples <see cref="RGBA16BitColor"/> values with the specified background color.
        /// </summary>
        /// <param name="backgroundColor">The color used before any layer is composited.</param>
        /// <returns>A geometry scene configured with 16-bit RGBA source-over alpha blending as the default layer blend.</returns>
        public static GeometryScene<RGBA16BitColor> CreateRGBA16Bit(RGBA16BitColor backgroundColor)
        {
            return new GeometryScene<RGBA16BitColor>(
                backgroundColor,
                GeometrySceneColor.AlphaOver,
                GeometrySceneColor.WithAlphaCoverage);
        }

        /// <summary>
        /// Rasterizes a 16-bit RGBA geometry scene on the specified grid.
        /// </summary>
        /// <param name="scene">The geometry scene to rasterize.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <returns>A new 16-bit RGBA raster with a new mutable value buffer owned by the caller.</returns>
        public static RGBA16BitRaster Rasterize(
            this GeometryScene<RGBA16BitColor> scene,
            RasterGrid grid)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            return new RGBA16BitRaster(grid, scene.RasterizeValues(grid));
        }
    }
}
