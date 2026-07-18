using Akeldov.Math.Spatial2D.Rasterization;
using System.IO;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Provides BMP export extension methods for rasters.
    /// </summary>
    public static class RasterBmpExtensions
    {
        /// <summary>
        /// Saves an 8-bit grayscale raster as an indexed grayscale BMP file.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output BMP file path.</param>
        public static void SaveAsBmp(this IRaster<Gray8BitColor> raster, string path)
        {
            BmpEncoder.Save(raster, path);
        }

        /// <summary>
        /// Saves an 8-bit grayscale raster as an indexed grayscale BMP stream.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output BMP stream.</param>
        public static void SaveAsBmp(this IRaster<Gray8BitColor> raster, Stream stream)
        {
            BmpEncoder.Save(raster, stream);
        }

        /// <summary>
        /// Saves an 8-bit RGBA raster as a 32-bit BGRA BMP file.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output BMP file path.</param>
        public static void SaveAsBmp(this IRaster<RGBA8BitColor> raster, string path)
        {
            BmpEncoder.Save(raster, path);
        }

        /// <summary>
        /// Saves an 8-bit RGBA raster as a 32-bit BGRA BMP stream.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output BMP stream.</param>
        public static void SaveAsBmp(this IRaster<RGBA8BitColor> raster, Stream stream)
        {
            BmpEncoder.Save(raster, stream);
        }
    }
}
