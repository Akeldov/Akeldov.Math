using System;
using System.IO;
using System.Text;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Encodes rasters into BMP images.
    /// </summary>
    internal static class BmpEncoder
    {
        private const int FileHeaderSize = 14;
        private const int InfoHeaderSize = 40;
        private const int GrayscalePaletteSize = 256 * 4;
        private const int BiRgbCompression = 0;

        /// <summary>
        /// Saves an 8-bit grayscale raster as an indexed grayscale BMP file.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output BMP file path.</param>
        public static void Save(Gray8BitRaster raster, string path)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            ValidateRasterSize(raster.Width, raster.Height);
            using (var stream = File.Create(path))
                Save(raster, stream);
        }

        /// <summary>
        /// Saves an 8-bit grayscale raster as an indexed grayscale BMP stream.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output BMP stream.</param>
        public static void Save(Gray8BitRaster raster, Stream stream)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            ValidateRasterSize(raster.Width, raster.Height);
            WriteGray8(raster, stream);
        }

        /// <summary>
        /// Saves an 8-bit RGBA raster as a 32-bit BGRA BMP file.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output BMP file path.</param>
        public static void Save(RGBA8BitRaster raster, string path)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            ValidateRasterSize(raster.Width, raster.Height);
            using (var stream = File.Create(path))
                Save(raster, stream);
        }

        /// <summary>
        /// Saves an 8-bit RGBA raster as a 32-bit BGRA BMP stream.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output BMP stream.</param>
        public static void Save(RGBA8BitRaster raster, Stream stream)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            ValidateRasterSize(raster.Width, raster.Height);
            WriteRgba8(raster, stream);
        }

        private static void ValidateRasterSize(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Raster width and height must be positive.");
        }

        private static void WriteGray8(Gray8BitRaster raster, Stream stream)
        {
            int width = raster.Width;
            int height = raster.Height;
            int rowStride = GetAlignedRowStride(width);
            int pixelDataOffset = FileHeaderSize + InfoHeaderSize + GrayscalePaletteSize;
            int imageSize = checked(rowStride * height);
            int fileSize = checked(pixelDataOffset + imageSize);

            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                WriteFileHeader(writer, fileSize, pixelDataOffset);
                WriteInfoHeader(writer, width, height, 8, imageSize, 256);
                WriteGrayscalePalette(writer);
                WriteGray8Pixels(writer, raster.Values, width, height, rowStride);
            }
        }

        private static void WriteRgba8(RGBA8BitRaster raster, Stream stream)
        {
            int width = raster.Width;
            int height = raster.Height;
            int rowStride = GetAlignedRowStride(checked(width * 4));
            int pixelDataOffset = FileHeaderSize + InfoHeaderSize;
            int imageSize = checked(rowStride * height);
            int fileSize = checked(pixelDataOffset + imageSize);

            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                WriteFileHeader(writer, fileSize, pixelDataOffset);
                WriteInfoHeader(writer, width, height, 32, imageSize, 0);
                WriteRgba8Pixels(writer, raster, width, height, rowStride);
            }
        }

        private static void WriteFileHeader(BinaryWriter writer, int fileSize, int pixelDataOffset)
        {
            writer.Write((byte)'B');
            writer.Write((byte)'M');
            writer.Write(fileSize);
            writer.Write((short)0);
            writer.Write((short)0);
            writer.Write(pixelDataOffset);
        }

        private static void WriteInfoHeader(
            BinaryWriter writer,
            int width,
            int height,
            short bitsPerPixel,
            int imageSize,
            int colorsUsed)
        {
            writer.Write(InfoHeaderSize);
            writer.Write(width);
            writer.Write(height);
            writer.Write((short)1);
            writer.Write(bitsPerPixel);
            writer.Write(BiRgbCompression);
            writer.Write(imageSize);
            writer.Write(0);
            writer.Write(0);
            writer.Write(colorsUsed);
            writer.Write(0);
        }

        private static void WriteGrayscalePalette(BinaryWriter writer)
        {
            for (int i = 0; i < 256; i++)
            {
                byte value = (byte)i;
                writer.Write(value);
                writer.Write(value);
                writer.Write(value);
                writer.Write((byte)0);
            }
        }

        private static void WriteGray8Pixels(BinaryWriter writer, byte[] values, int width, int height, int rowStride)
        {
            int padding = rowStride - width;
            for (int y = 0; y < height; y++)
            {
                int valueIndex = y * width;

                for (int x = 0; x < width; x++)
                    writer.Write(values[valueIndex++]);

                WritePadding(writer, padding);
            }
        }

        private static void WriteRgba8Pixels(BinaryWriter writer, RGBA8BitRaster raster, int width, int height, int rowStride)
        {
            int rowBytes = checked(width * 4);
            int padding = rowStride - rowBytes;
            RGBA8BitColor[] values = raster.Values;

            for (int y = 0; y < height; y++)
            {
                int valueIndex = y * width;

                for (int x = 0; x < width; x++)
                {
                    RGBA8BitColor color = values[valueIndex++];
                    writer.Write(color.Blue);
                    writer.Write(color.Green);
                    writer.Write(color.Red);
                    writer.Write(color.Alpha);
                }

                WritePadding(writer, padding);
            }
        }

        private static void WritePadding(BinaryWriter writer, int padding)
        {
            for (int i = 0; i < padding; i++)
                writer.Write((byte)0);
        }

        private static int GetAlignedRowStride(int rowBytes)
        {
            return checked((rowBytes + 3) & ~3);
        }
    }
}
