using System.IO;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Provides PNG export extension methods for rasters.
    /// </summary>
    public static class RasterPngExtensions
    {
        /// <summary>
        /// Saves an 8-bit grayscale raster as an 8-bit grayscale PNG file.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        public static void SaveAsPng(this IRaster<Gray8BitColor> raster, string path)
        {
            PngEncoder.Save(raster, path);
        }

        /// <summary>
        /// Saves an 8-bit grayscale raster as an 8-bit grayscale PNG stream.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        public static void SaveAsPng(this IRaster<Gray8BitColor> raster, Stream stream)
        {
            PngEncoder.Save(raster, stream);
        }

        /// <summary>
        /// Saves a 16-bit grayscale raster as a 16-bit grayscale PNG file.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        public static void SaveAsPng(this IRaster<Gray16BitColor> raster, string path)
        {
            PngEncoder.Save(raster, path);
        }

        /// <summary>
        /// Saves a 16-bit grayscale raster as a 16-bit grayscale PNG stream.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        public static void SaveAsPng(this IRaster<Gray16BitColor> raster, Stream stream)
        {
            PngEncoder.Save(raster, stream);
        }

        /// <summary>
        /// Saves an 8-bit RGBA raster as an 8-bit RGBA PNG file.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        public static void SaveAsPng(this IRaster<RGBA8BitColor> raster, string path)
        {
            PngEncoder.Save(raster, path);
        }

        /// <summary>
        /// Saves an 8-bit RGBA raster as an 8-bit RGBA PNG stream.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        public static void SaveAsPng(this IRaster<RGBA8BitColor> raster, Stream stream)
        {
            PngEncoder.Save(raster, stream);
        }

        /// <summary>
        /// Saves a 16-bit RGBA raster as a 16-bit RGBA PNG file.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        public static void SaveAsPng(this IRaster<RGBA16BitColor> raster, string path)
        {
            PngEncoder.Save(raster, path);
        }

        /// <summary>
        /// Saves a 16-bit RGBA raster as a 16-bit RGBA PNG stream.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        public static void SaveAsPng(this IRaster<RGBA16BitColor> raster, Stream stream)
        {
            PngEncoder.Save(raster, stream);
        }
    }
}
