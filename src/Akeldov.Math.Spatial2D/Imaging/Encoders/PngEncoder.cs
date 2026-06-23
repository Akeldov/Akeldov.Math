using System;
using System.IO;
using System.Text;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Encodes rasters into PNG images.
    /// </summary>
    internal static class PngEncoder
    {
        private static readonly byte[] PngSignature =
        {
            137, 80, 78, 71, 13, 10, 26, 10
        };

        /// <summary>
        /// Saves an 8-bit grayscale raster as an 8-bit grayscale PNG file.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        public static void Save(Gray8BitRaster raster, string path)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            ValidateRasterSize(raster.Width, raster.Height);
            using (var stream = File.Create(path))
                Save(raster, stream);
        }

        /// <summary>
        /// Saves an 8-bit grayscale raster as an 8-bit grayscale PNG stream.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
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
        /// Saves a 16-bit grayscale raster as a 16-bit grayscale PNG file.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        public static void Save(Gray16BitRaster raster, string path)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            ValidateRasterSize(raster.Width, raster.Height);
            using (var stream = File.Create(path))
                Save(raster, stream);
        }

        /// <summary>
        /// Saves a 16-bit grayscale raster as a 16-bit grayscale PNG stream.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        public static void Save(Gray16BitRaster raster, Stream stream)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            ValidateRasterSize(raster.Width, raster.Height);
            WriteGray16(raster, stream);
        }

        /// <summary>
        /// Saves an 8-bit RGBA raster as an 8-bit RGBA PNG file.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        public static void Save(RGBA8BitRaster raster, string path)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            ValidateRasterSize(raster.Width, raster.Height);
            using (var stream = File.Create(path))
                Save(raster, stream);
        }

        /// <summary>
        /// Saves an 8-bit RGBA raster as an 8-bit RGBA PNG stream.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        public static void Save(RGBA8BitRaster raster, Stream stream)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            ValidateRasterSize(raster.Width, raster.Height);
            WriteRgba8(raster, stream);
        }

        /// <summary>
        /// Saves a 16-bit RGBA raster as a 16-bit RGBA PNG file.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="path">The output PNG file path.</param>
        public static void Save(RGBA16BitRaster raster, string path)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            ValidateRasterSize(raster.Width, raster.Height);
            using (var stream = File.Create(path))
                Save(raster, stream);
        }

        /// <summary>
        /// Saves a 16-bit RGBA raster as a 16-bit RGBA PNG stream.
        /// </summary>
        /// <param name="raster">The raster to save.</param>
        /// <param name="stream">The output PNG stream.</param>
        public static void Save(RGBA16BitRaster raster, Stream stream)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            ValidateRasterSize(raster.Width, raster.Height);
            WriteRgba16(raster, stream);
        }

        private static void ValidateRasterSize(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Raster width and height must be positive.");
        }

        private static void WriteGray8(Gray8BitRaster raster, Stream stream)
        {
            byte[] scanlines = CreateGray8Scanlines(raster);

            WriteBytes(stream, PngSignature);
            WriteChunk(stream, "IHDR", CreateHeader(raster.Width, raster.Height, 8, 0));
            WriteChunk(stream, "IDAT", CreateZlibStoredData(scanlines));
            WriteChunk(stream, "IEND", Array.Empty<byte>());
        }

        private static byte[] CreateGray8Scanlines(Gray8BitRaster raster)
        {
            int width = raster.Width;
            int height = raster.Height;
            int stride = checked(width + 1);
            var scanlines = new byte[checked(height * stride)];
            byte[] values = raster.Values;

            for (int row = 0; row < height; row++)
            {
                int y = height - 1 - row;
                int offset = row * stride;
                int valueIndex = y * width;
                scanlines[offset] = 0;

                for (int x = 0; x < width; x++)
                    scanlines[offset + 1 + x] = values[valueIndex++];
            }

            return scanlines;
        }

        private static void WriteGray16(Gray16BitRaster raster, Stream stream)
        {
            byte[] scanlines = CreateGray16Scanlines(raster);

            WriteBytes(stream, PngSignature);
            WriteChunk(stream, "IHDR", CreateHeader(raster.Width, raster.Height, 16, 0));
            WriteChunk(stream, "IDAT", CreateZlibStoredData(scanlines));
            WriteChunk(stream, "IEND", Array.Empty<byte>());
        }

        private static byte[] CreateGray16Scanlines(Gray16BitRaster raster)
        {
            int width = raster.Width;
            int height = raster.Height;
            int rowLength = checked(width * 2);
            int stride = checked(rowLength + 1);
            var scanlines = new byte[checked(height * stride)];
            ushort[] values = raster.Values;

            for (int row = 0; row < height; row++)
            {
                int y = height - 1 - row;
                int offset = row * stride;
                int valueIndex = y * width;
                scanlines[offset] = 0;

                for (int x = 0; x < width; x++)
                {
                    ushort value = values[valueIndex++];
                    int valueOffset = offset + 1 + x * 2;
                    WriteUInt16BigEndian(scanlines, valueOffset, value);
                }
            }

            return scanlines;
        }

        private static void WriteRgba8(RGBA8BitRaster raster, Stream stream)
        {
            byte[] scanlines = CreateRgba8Scanlines(raster);

            WriteBytes(stream, PngSignature);
            WriteChunk(stream, "IHDR", CreateHeader(raster.Width, raster.Height, 8, 6));
            WriteChunk(stream, "IDAT", CreateZlibStoredData(scanlines));
            WriteChunk(stream, "IEND", Array.Empty<byte>());
        }

        private static byte[] CreateRgba8Scanlines(RGBA8BitRaster raster)
        {
            int width = raster.Width;
            int height = raster.Height;
            int rowLength = checked(width * 4);
            int stride = checked(rowLength + 1);
            var scanlines = new byte[checked(height * stride)];
            RGBA8BitColor[] values = raster.Values;

            for (int row = 0; row < height; row++)
            {
                int y = height - 1 - row;
                int offset = row * stride;
                int valueIndex = y * width;
                scanlines[offset] = 0;

                for (int x = 0; x < width; x++)
                {
                    RGBA8BitColor color = values[valueIndex++];
                    int valueOffset = offset + 1 + x * 4;
                    scanlines[valueOffset] = color.Red;
                    scanlines[valueOffset + 1] = color.Green;
                    scanlines[valueOffset + 2] = color.Blue;
                    scanlines[valueOffset + 3] = color.Alpha;
                }
            }

            return scanlines;
        }

        private static void WriteRgba16(RGBA16BitRaster raster, Stream stream)
        {
            byte[] scanlines = CreateRgba16Scanlines(raster);

            WriteBytes(stream, PngSignature);
            WriteChunk(stream, "IHDR", CreateHeader(raster.Width, raster.Height, 16, 6));
            WriteChunk(stream, "IDAT", CreateZlibStoredData(scanlines));
            WriteChunk(stream, "IEND", Array.Empty<byte>());
        }

        private static byte[] CreateRgba16Scanlines(RGBA16BitRaster raster)
        {
            int width = raster.Width;
            int height = raster.Height;
            int rowLength = checked(width * 8);
            int stride = checked(rowLength + 1);
            var scanlines = new byte[checked(height * stride)];
            RGBA16BitColor[] values = raster.Values;

            for (int row = 0; row < height; row++)
            {
                int y = height - 1 - row;
                int offset = row * stride;
                int valueIndex = y * width;
                scanlines[offset] = 0;

                for (int x = 0; x < width; x++)
                {
                    RGBA16BitColor color = values[valueIndex++];
                    int valueOffset = offset + 1 + x * 8;
                    WriteUInt16BigEndian(scanlines, valueOffset, color.Red);
                    WriteUInt16BigEndian(scanlines, valueOffset + 2, color.Green);
                    WriteUInt16BigEndian(scanlines, valueOffset + 4, color.Blue);
                    WriteUInt16BigEndian(scanlines, valueOffset + 6, color.Alpha);
                }
            }

            return scanlines;
        }

        private static byte[] CreateHeader(int width, int height, byte bitDepth, byte colorType)
        {
            var header = new byte[13];
            WriteUInt32(header, 0, (uint)width);
            WriteUInt32(header, 4, (uint)height);
            header[8] = bitDepth;
            header[9] = colorType;
            header[10] = 0;
            header[11] = 0;
            header[12] = 0;
            return header;
        }

        private static byte[] CreateZlibStoredData(byte[] data)
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x78);
                stream.WriteByte(0x01);

                int offset = 0;
                while (offset < data.Length)
                {
                    int length = System.Math.Min(65535, data.Length - offset);
                    bool isFinal = offset + length == data.Length;
                    stream.WriteByte(isFinal ? (byte)1 : (byte)0);
                    WriteUInt16LittleEndian(stream, (ushort)length);
                    WriteUInt16LittleEndian(stream, (ushort)(length ^ 0xffff));
                    stream.Write(data, offset, length);
                    offset += length;
                }

                WriteUInt32(stream, Adler32(data));
                return stream.ToArray();
            }
        }

        private static void WriteChunk(Stream stream, string type, byte[] data)
        {
            byte[] typeBytes = Encoding.ASCII.GetBytes(type);

            WriteUInt32(stream, (uint)data.Length);
            WriteBytes(stream, typeBytes);
            WriteBytes(stream, data);
            WriteUInt32(stream, Crc32(typeBytes, data));
        }

        private static void WriteBytes(Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        private static void WriteUInt16BigEndian(byte[] data, int offset, ushort value)
        {
            data[offset] = (byte)(value >> 8);
            data[offset + 1] = (byte)value;
        }

        private static void WriteUInt16LittleEndian(Stream stream, ushort value)
        {
            stream.WriteByte((byte)(value & 0xff));
            stream.WriteByte((byte)(value >> 8));
        }

        private static void WriteUInt32(Stream stream, uint value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteUInt32(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)(value >> 24);
            data[offset + 1] = (byte)(value >> 16);
            data[offset + 2] = (byte)(value >> 8);
            data[offset + 3] = (byte)value;
        }

        private static uint Adler32(byte[] data)
        {
            const uint mod = 65521;
            uint a = 1;
            uint b = 0;

            for (int i = 0; i < data.Length; i++)
            {
                a = (a + data[i]) % mod;
                b = (b + a) % mod;
            }

            return (b << 16) | a;
        }

        private static uint Crc32(byte[] typeBytes, byte[] data)
        {
            uint crc = 0xffffffff;

            for (int i = 0; i < typeBytes.Length; i++)
                crc = UpdateCrc32(crc, typeBytes[i]);

            for (int i = 0; i < data.Length; i++)
                crc = UpdateCrc32(crc, data[i]);

            return crc ^ 0xffffffff;
        }

        private static uint UpdateCrc32(uint crc, byte value)
        {
            crc ^= value;

            for (int i = 0; i < 8; i++)
            {
                if ((crc & 1) == 0)
                    crc >>= 1;
                else
                    crc = (crc >> 1) ^ 0xedb88320;
            }

            return crc;
        }
    }
}
