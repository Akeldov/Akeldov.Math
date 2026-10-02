using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.IO;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Loads image files and streams into rasters with explicitly selected pixel types.
    /// </summary>
    /// <remarks>
    /// Loaded rasters use Y increasing from the bottom image row. Image files do not retain spatial bounds.
    /// PNG loading supports all scanline filters and Adam7 interlacing and preserves samples without color-space conversion.
    /// BMP loading supports bottom-up and top-down images with Windows information headers.
    /// </remarks>
    public static class RasterImageLoader
    {
        /// <summary>
        /// Loads a PNG file into a raster with 8-bit RGBA values.
        /// </summary>
        /// <param name="path">The input PNG file path.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// The PNG must have 8-bit RGBA channels.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="NotSupportedException">The PNG format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The PNG data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<RGBA8BitColor> LoadRgba8FromPng(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using FileStream stream = File.OpenRead(path);
            return LoadRgba8FromPng(stream);
        }

        /// <summary>
        /// Loads a PNG from the current position of a readable stream into a raster with 8-bit RGBA values.
        /// </summary>
        /// <param name="stream">The input PNG stream. It need not support seeking and remains open after loading.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// The PNG must have 8-bit RGBA channels.
        /// Reading stops after the PNG IEND chunk.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> is not readable.</exception>
        /// <exception cref="NotSupportedException">The PNG format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The PNG data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<RGBA8BitColor> LoadRgba8FromPng(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException("PNG stream must be readable.", nameof(stream));

            return PngDecoder.LoadRgba8(stream);
        }

        /// <summary>
        /// Loads a PNG file into a raster with 16-bit RGBA values.
        /// </summary>
        /// <param name="path">The input PNG file path.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// The PNG must have 16-bit RGBA channels.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="NotSupportedException">The PNG format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The PNG data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<RGBA16BitColor> LoadRgba16FromPng(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using FileStream stream = File.OpenRead(path);
            return LoadRgba16FromPng(stream);
        }

        /// <summary>
        /// Loads a PNG from the current position of a readable stream into a raster with 16-bit RGBA values.
        /// </summary>
        /// <param name="stream">The input PNG stream. It need not support seeking and remains open after loading.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// The PNG must have 16-bit RGBA channels.
        /// Reading stops after the PNG IEND chunk.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> is not readable.</exception>
        /// <exception cref="NotSupportedException">The PNG format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The PNG data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<RGBA16BitColor> LoadRgba16FromPng(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException("PNG stream must be readable.", nameof(stream));

            return PngDecoder.LoadRgba16(stream);
        }

        /// <summary>
        /// Loads a PNG file into a raster with 8-bit grayscale values.
        /// </summary>
        /// <param name="path">The input PNG file path.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// The PNG must have 8-bit grayscale samples without alpha.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="NotSupportedException">The PNG format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The PNG data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<Gray8BitColor> LoadGray8FromPng(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using FileStream stream = File.OpenRead(path);
            return LoadGray8FromPng(stream);
        }

        /// <summary>
        /// Loads a PNG from the current position of a readable stream into a raster with 8-bit grayscale values.
        /// </summary>
        /// <param name="stream">The input PNG stream. It need not support seeking and remains open after loading.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// The PNG must have 8-bit grayscale samples without alpha.
        /// Reading stops after the PNG IEND chunk.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> is not readable.</exception>
        /// <exception cref="NotSupportedException">The PNG format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The PNG data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<Gray8BitColor> LoadGray8FromPng(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException("PNG stream must be readable.", nameof(stream));

            return PngDecoder.LoadGray8(stream);
        }

        /// <summary>
        /// Loads a PNG file into a raster with 16-bit grayscale values.
        /// </summary>
        /// <param name="path">The input PNG file path.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// The PNG must have 16-bit grayscale samples without alpha.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="NotSupportedException">The PNG format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The PNG data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<Gray16BitColor> LoadGray16FromPng(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using FileStream stream = File.OpenRead(path);
            return LoadGray16FromPng(stream);
        }

        /// <summary>
        /// Loads a PNG from the current position of a readable stream into a raster with 16-bit grayscale values.
        /// </summary>
        /// <param name="stream">The input PNG stream. It need not support seeking and remains open after loading.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// The PNG must have 16-bit grayscale samples without alpha.
        /// Reading stops after the PNG IEND chunk.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> is not readable.</exception>
        /// <exception cref="NotSupportedException">The PNG format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The PNG data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<Gray16BitColor> LoadGray16FromPng(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException("PNG stream must be readable.", nameof(stream));

            return PngDecoder.LoadGray16(stream);
        }

        /// <summary>
        /// Loads a BMP file into a raster with 8-bit RGBA values.
        /// </summary>
        /// <param name="path">The input BMP file path.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// Supports uncompressed indexed 8-bit BMP with any palette and 32-bit BGRA BMP.
        /// Palette entries are opaque; the fourth byte of 32-bit pixels is retained as alpha, matching BMP export.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="NotSupportedException">The BMP format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The BMP data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<RGBA8BitColor> LoadRgba8FromBmp(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using FileStream stream = File.OpenRead(path);
            return LoadRgba8FromBmp(stream);
        }

        /// <summary>
        /// Loads a BMP from the current position of a readable stream into a raster with 8-bit RGBA values.
        /// </summary>
        /// <param name="stream">The input BMP stream. It need not support seeking and remains open after loading.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// Supports uncompressed indexed 8-bit BMP with any palette and 32-bit BGRA BMP.
        /// Palette entries are opaque; the fourth byte of 32-bit pixels is retained as alpha, matching BMP export.
        /// Reading consumes the size declared in the BMP file header.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> is not readable.</exception>
        /// <exception cref="NotSupportedException">The BMP format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The BMP data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<RGBA8BitColor> LoadRgba8FromBmp(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException("BMP stream must be readable.", nameof(stream));

            return BmpDecoder.LoadRgba8(stream);
        }

        /// <summary>
        /// Loads a BMP file into a raster with 8-bit grayscale values.
        /// </summary>
        /// <param name="path">The input BMP file path.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// Requires an uncompressed indexed 8-bit BMP with a grayscale palette.
        /// Palette indices are resolved to their grayscale intensities.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="NotSupportedException">The BMP format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The BMP data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<Gray8BitColor> LoadGray8FromBmp(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using FileStream stream = File.OpenRead(path);
            return LoadGray8FromBmp(stream);
        }

        /// <summary>
        /// Loads a BMP from the current position of a readable stream into a raster with 8-bit grayscale values.
        /// </summary>
        /// <param name="stream">The input BMP stream. It need not support seeking and remains open after loading.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// Requires an uncompressed indexed 8-bit BMP with a grayscale palette.
        /// Palette indices are resolved to their grayscale intensities.
        /// Reading consumes the size declared in the BMP file header.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> is not readable.</exception>
        /// <exception cref="NotSupportedException">The BMP format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The BMP data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<Gray8BitColor> LoadGray8FromBmp(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException("BMP stream must be readable.", nameof(stream));

            return BmpDecoder.LoadGray8(stream);
        }
    }
}
