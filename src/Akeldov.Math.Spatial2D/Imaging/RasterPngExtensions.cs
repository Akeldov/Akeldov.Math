using Akeldov.Math.Spatial2D.Rasterization;
using System.IO;
using System.IO.Compression;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Provides PNG export extension methods for rasters.
    /// </summary>
    public static class RasterPngExtensions
    {
        /// <summary>
        /// Saves an 8-bit grayscale raster as an 8-bit grayscale PNG file using optimal compression.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        public static void SaveAsPng(this IRaster<Gray8BitColor> raster, string path)
        {
            PngEncoder.Save(raster, path, CompressionLevel.Optimal);
        }

        /// <summary>
        /// Saves an 8-bit grayscale raster as an 8-bit grayscale PNG file using the specified compression level.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        /// <param name="compressionLevel">The DEFLATE compression level to use for PNG image data.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="compressionLevel"/> is not a supported <see cref="CompressionLevel"/> value.
        /// </exception>
        public static void SaveAsPng(
            this IRaster<Gray8BitColor> raster,
            string path,
            CompressionLevel compressionLevel)
        {
            if (compressionLevel != CompressionLevel.NoCompression &&
                compressionLevel != CompressionLevel.Fastest &&
                compressionLevel != CompressionLevel.Optimal &&
                (int)compressionLevel != 3)
            {
                throw new System.ArgumentOutOfRangeException(nameof(compressionLevel));
            }

            PngEncoder.Save(raster, path, compressionLevel);
        }

        /// <summary>
        /// Saves an 8-bit grayscale raster as an 8-bit grayscale PNG stream using optimal compression.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        public static void SaveAsPng(this IRaster<Gray8BitColor> raster, Stream stream)
        {
            PngEncoder.Save(raster, stream, CompressionLevel.Optimal);
        }

        /// <summary>
        /// Saves an 8-bit grayscale raster as an 8-bit grayscale PNG stream using the specified compression level.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        /// <param name="compressionLevel">The DEFLATE compression level to use for PNG image data.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="compressionLevel"/> is not a supported <see cref="CompressionLevel"/> value.
        /// </exception>
        public static void SaveAsPng(
            this IRaster<Gray8BitColor> raster,
            Stream stream,
            CompressionLevel compressionLevel)
        {
            if (compressionLevel != CompressionLevel.NoCompression &&
                compressionLevel != CompressionLevel.Fastest &&
                compressionLevel != CompressionLevel.Optimal &&
                (int)compressionLevel != 3)
            {
                throw new System.ArgumentOutOfRangeException(nameof(compressionLevel));
            }

            PngEncoder.Save(raster, stream, compressionLevel);
        }

        /// <summary>
        /// Saves a 16-bit grayscale raster as a 16-bit grayscale PNG file using optimal compression.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        public static void SaveAsPng(this IRaster<Gray16BitColor> raster, string path)
        {
            PngEncoder.Save(raster, path, CompressionLevel.Optimal);
        }

        /// <summary>
        /// Saves a 16-bit grayscale raster as a 16-bit grayscale PNG file using the specified compression level.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        /// <param name="compressionLevel">The DEFLATE compression level to use for PNG image data.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="compressionLevel"/> is not a supported <see cref="CompressionLevel"/> value.
        /// </exception>
        public static void SaveAsPng(
            this IRaster<Gray16BitColor> raster,
            string path,
            CompressionLevel compressionLevel)
        {
            if (compressionLevel != CompressionLevel.NoCompression &&
                compressionLevel != CompressionLevel.Fastest &&
                compressionLevel != CompressionLevel.Optimal &&
                (int)compressionLevel != 3)
            {
                throw new System.ArgumentOutOfRangeException(nameof(compressionLevel));
            }

            PngEncoder.Save(raster, path, compressionLevel);
        }

        /// <summary>
        /// Saves a 16-bit grayscale raster as a 16-bit grayscale PNG stream using optimal compression.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        public static void SaveAsPng(this IRaster<Gray16BitColor> raster, Stream stream)
        {
            PngEncoder.Save(raster, stream, CompressionLevel.Optimal);
        }

        /// <summary>
        /// Saves a 16-bit grayscale raster as a 16-bit grayscale PNG stream using the specified compression level.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        /// <param name="compressionLevel">The DEFLATE compression level to use for PNG image data.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="compressionLevel"/> is not a supported <see cref="CompressionLevel"/> value.
        /// </exception>
        public static void SaveAsPng(
            this IRaster<Gray16BitColor> raster,
            Stream stream,
            CompressionLevel compressionLevel)
        {
            if (compressionLevel != CompressionLevel.NoCompression &&
                compressionLevel != CompressionLevel.Fastest &&
                compressionLevel != CompressionLevel.Optimal &&
                (int)compressionLevel != 3)
            {
                throw new System.ArgumentOutOfRangeException(nameof(compressionLevel));
            }

            PngEncoder.Save(raster, stream, compressionLevel);
        }

        /// <summary>
        /// Saves an 8-bit RGBA raster as an 8-bit RGBA PNG file using optimal compression.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        public static void SaveAsPng(this IRaster<RGBA8BitColor> raster, string path)
        {
            PngEncoder.Save(raster, path, CompressionLevel.Optimal);
        }

        /// <summary>
        /// Saves an 8-bit RGBA raster as an 8-bit RGBA PNG file using the specified compression level.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        /// <param name="compressionLevel">The DEFLATE compression level to use for PNG image data.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="compressionLevel"/> is not a supported <see cref="CompressionLevel"/> value.
        /// </exception>
        public static void SaveAsPng(
            this IRaster<RGBA8BitColor> raster,
            string path,
            CompressionLevel compressionLevel)
        {
            if (compressionLevel != CompressionLevel.NoCompression &&
                compressionLevel != CompressionLevel.Fastest &&
                compressionLevel != CompressionLevel.Optimal &&
                (int)compressionLevel != 3)
            {
                throw new System.ArgumentOutOfRangeException(nameof(compressionLevel));
            }

            PngEncoder.Save(raster, path, compressionLevel);
        }

        /// <summary>
        /// Saves an 8-bit RGBA raster as an 8-bit RGBA PNG stream using optimal compression.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        public static void SaveAsPng(this IRaster<RGBA8BitColor> raster, Stream stream)
        {
            PngEncoder.Save(raster, stream, CompressionLevel.Optimal);
        }

        /// <summary>
        /// Saves an 8-bit RGBA raster as an 8-bit RGBA PNG stream using the specified compression level.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        /// <param name="compressionLevel">The DEFLATE compression level to use for PNG image data.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="compressionLevel"/> is not a supported <see cref="CompressionLevel"/> value.
        /// </exception>
        public static void SaveAsPng(
            this IRaster<RGBA8BitColor> raster,
            Stream stream,
            CompressionLevel compressionLevel)
        {
            if (compressionLevel != CompressionLevel.NoCompression &&
                compressionLevel != CompressionLevel.Fastest &&
                compressionLevel != CompressionLevel.Optimal &&
                (int)compressionLevel != 3)
            {
                throw new System.ArgumentOutOfRangeException(nameof(compressionLevel));
            }

            PngEncoder.Save(raster, stream, compressionLevel);
        }

        /// <summary>
        /// Saves a 16-bit RGBA raster as a 16-bit RGBA PNG file using optimal compression.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        public static void SaveAsPng(this IRaster<RGBA16BitColor> raster, string path)
        {
            PngEncoder.Save(raster, path, CompressionLevel.Optimal);
        }

        /// <summary>
        /// Saves a 16-bit RGBA raster as a 16-bit RGBA PNG file using the specified compression level.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        /// <param name="compressionLevel">The DEFLATE compression level to use for PNG image data.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="compressionLevel"/> is not a supported <see cref="CompressionLevel"/> value.
        /// </exception>
        public static void SaveAsPng(
            this IRaster<RGBA16BitColor> raster,
            string path,
            CompressionLevel compressionLevel)
        {
            if (compressionLevel != CompressionLevel.NoCompression &&
                compressionLevel != CompressionLevel.Fastest &&
                compressionLevel != CompressionLevel.Optimal &&
                (int)compressionLevel != 3)
            {
                throw new System.ArgumentOutOfRangeException(nameof(compressionLevel));
            }

            PngEncoder.Save(raster, path, compressionLevel);
        }

        /// <summary>
        /// Saves a 16-bit RGBA raster as a 16-bit RGBA PNG stream using optimal compression.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        public static void SaveAsPng(this IRaster<RGBA16BitColor> raster, Stream stream)
        {
            PngEncoder.Save(raster, stream, CompressionLevel.Optimal);
        }

        /// <summary>
        /// Saves a 16-bit RGBA raster as a 16-bit RGBA PNG stream using the specified compression level.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        /// <param name="compressionLevel">The DEFLATE compression level to use for PNG image data.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="compressionLevel"/> is not a supported <see cref="CompressionLevel"/> value.
        /// </exception>
        public static void SaveAsPng(
            this IRaster<RGBA16BitColor> raster,
            Stream stream,
            CompressionLevel compressionLevel)
        {
            if (compressionLevel != CompressionLevel.NoCompression &&
                compressionLevel != CompressionLevel.Fastest &&
                compressionLevel != CompressionLevel.Optimal &&
                (int)compressionLevel != 3)
            {
                throw new System.ArgumentOutOfRangeException(nameof(compressionLevel));
            }

            PngEncoder.Save(raster, stream, compressionLevel);
        }
    }
}
