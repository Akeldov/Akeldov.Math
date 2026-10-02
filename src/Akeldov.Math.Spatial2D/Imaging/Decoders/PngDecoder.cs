using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Decodes PNG images with 8-bit or 16-bit RGBA samples without changing their color space.
    /// </summary>
    internal static class PngDecoder
    {
        public static Raster<RGBA8BitColor> LoadRgba8(Stream stream) =>
            Load(stream, 8, ReadRgba8);

        public static Raster<RGBA16BitColor> LoadRgba16(Stream stream) =>
            Load(stream, 16, ReadRgba16);

        private static Raster<TValue> Load<TValue>(Stream stream, byte bitDepth, Func<byte[], int, TValue> readPixel)
        {
            byte[] signature = new byte[8];
            ReadExactly(stream, signature);
            byte[] expectedSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            if (!signature.AsSpan().SequenceEqual(expectedSignature))
                throw new InvalidDataException("Invalid PNG signature.");

            byte[] header = ReadChunk(stream, out string type);
            if (!string.Equals(type, "IHDR", StringComparison.Ordinal) || header.Length != 13)
                throw new InvalidDataException("PNG must start with a 13-byte IHDR chunk.");

            uint width = ReadUInt32(header, 0);
            uint height = ReadUInt32(header, 4);
            int bytesPerPixel = bitDepth / 2;
            if (width == 0 || height == 0 || width > int.MaxValue / bytesPerPixel ||
                height > int.MaxValue || (ulong)width * height > int.MaxValue)
            {
                throw new InvalidDataException("PNG dimensions exceed supported raster sizes.");
            }

            if (header[8] != bitDepth || header[9] != 6)
                throw new NotSupportedException("PNG must have RGBA channels with the requested bit depth.");

            if (header[10] != 0 || header[11] != 0 || header[12] > 1)
                throw new InvalidDataException("Invalid PNG compression, filter, or interlace method.");

            byte[] imageData = ReadImageData(stream);
            return DecodeImageData(imageData, (int)width, (int)height, header[12] == 1, bytesPerPixel, readPixel);
        }

        private static byte[] ReadImageData(Stream stream)
        {
            using var imageData = new MemoryStream();
            bool hasImageData = false;
            bool imageDataEnded = false;
            bool hasPalette = false;
            while (true)
            {
                byte[] data = ReadChunk(stream, out string type);
                if (string.Equals(type, "IDAT", StringComparison.Ordinal))
                {
                    if (imageDataEnded)
                        throw new InvalidDataException("PNG IDAT chunks must be consecutive.");

                    imageData.Write(data, 0, data.Length);
                    hasImageData = true;
                    continue;
                }

                imageDataEnded = hasImageData;
                if (string.Equals(type, "IEND", StringComparison.Ordinal))
                {
                    if (!hasImageData || data.Length != 0)
                        throw new InvalidDataException("Invalid PNG image trailer or missing image data.");

                    break;
                }

                if (string.Equals(type, "IHDR", StringComparison.Ordinal))
                    throw new InvalidDataException("Duplicate PNG image header.");

                if (string.Equals(type, "PLTE", StringComparison.Ordinal))
                {
                    if (hasPalette || hasImageData || data.Length == 0 || data.Length > 768 || data.Length % 3 != 0)
                        throw new InvalidDataException("Invalid PNG palette.");

                    hasPalette = true;
                }
                else if ((type[0] & 32) == 0)
                {
                    throw new NotSupportedException("Unsupported critical PNG chunk: " + type);
                }
            }

            return imageData.ToArray();
        }

        private static Raster<TValue> DecodeImageData<TValue>(byte[] data, int width, int height, bool interlaced,
            int bytesPerPixel, Func<byte[], int, TValue> readPixel)
        {
            if (data.Length < 6 || (data[0] & 15) != 8 || (data[0] >> 4) > 7 ||
                ((data[0] << 8) + data[1]) % 31 != 0 || (data[1] & 32) != 0)
            {
                throw new InvalidDataException("Invalid PNG zlib header.");
            }

            using var compressed = new MemoryStream(data, 2, data.Length - 6);
            using var deflate = new DeflateStream(compressed, CompressionMode.Decompress);
            var values = new TValue[checked(width * height)];
            uint adlerA = 1;
            uint adlerB = 0;
            if (interlaced)
            {
                // Adam7 pass origins and strides, in transmission order.
                int[] startX = { 0, 4, 0, 2, 0, 1, 0 };
                int[] startY = { 0, 0, 4, 0, 2, 0, 1 };
                int[] stepX = { 8, 8, 4, 4, 2, 2, 1 };
                int[] stepY = { 8, 8, 8, 4, 4, 2, 2 };
                for (int pass = 0; pass < 7; pass++)
                    ReadPass(deflate, values, width, height, startX[pass], startY[pass],
                        stepX[pass], stepY[pass], bytesPerPixel, readPixel, ref adlerA, ref adlerB);
            }
            else
            {
                ReadPass(deflate, values, width, height, 0, 0, 1, 1, bytesPerPixel, readPixel, ref adlerA, ref adlerB);
            }

            if (deflate.ReadByte() != -1)
                throw new InvalidDataException("PNG contains excess decompressed image data.");

            if (((adlerB << 16) | adlerA) != ReadUInt32(data, data.Length - 4))
                throw new InvalidDataException("PNG zlib checksum mismatch.");

            return new Raster<TValue>(new VectorXYInt(width, height), values);
        }

        private static void ReadPass<TValue>(Stream stream, TValue[] values, int width, int height,
            int startX, int startY, int stepX, int stepY, int bytesPerPixel, Func<byte[], int, TValue> readPixel,
            ref uint adlerA, ref uint adlerB)
        {
            if (startX >= width || startY >= height)
                return;

            int passWidth = (width - 1 - startX) / stepX + 1;
            var scanline = new byte[checked(passWidth * bytesPerPixel + 1)];
            var previous = new byte[scanline.Length];
            for (int row = startY; row < height; row += stepY)
            {
                ReadExactly(stream, scanline);
                for (int i = 0; i < scanline.Length; i++)
                {
                    adlerA = (adlerA + scanline[i]) % 65521;
                    adlerB = (adlerB + adlerA) % 65521;
                }

                int filter = scanline[0];
                if (filter > 4)
                    throw new InvalidDataException("Invalid PNG scanline filter.");

                for (int i = 1; i < scanline.Length; i++)
                {
                    int left = i > bytesPerPixel ? scanline[i - bytesPerPixel] : 0;
                    int above = previous[i];
                    int upperLeft = i > bytesPerPixel ? previous[i - bytesPerPixel] : 0;
                    int predictor = filter switch
                    {
                        0 => 0,
                        1 => left,
                        2 => above,
                        3 => (left + above) / 2,
                        _ => Paeth(left, above, upperLeft)
                    };
                    scanline[i] = unchecked((byte)(scanline[i] + predictor));
                }

                int y = height - 1 - row;
                for (int x = 0; x < passWidth; x++)
                {
                    int offset = 1 + x * bytesPerPixel;
                    values[y * width + startX + x * stepX] = readPixel(scanline, offset);
                }

                byte[] swap = previous;
                previous = scanline;
                scanline = swap;
            }
        }

        private static int Paeth(int left, int above, int upperLeft)
        {
            int prediction = left + above - upperLeft;
            int leftDistance = System.Math.Abs(prediction - left);
            int aboveDistance = System.Math.Abs(prediction - above);
            int upperLeftDistance = System.Math.Abs(prediction - upperLeft);
            if (leftDistance <= aboveDistance && leftDistance <= upperLeftDistance)
                return left;

            return aboveDistance <= upperLeftDistance ? above : upperLeft;
        }

        private static byte[] ReadChunk(Stream stream, out string type)
        {
            var chunkHeader = new byte[8];
            ReadExactly(stream, chunkHeader);
            uint length = ReadUInt32(chunkHeader, 0);
            if (length > int.MaxValue)
                throw new InvalidDataException("Invalid PNG chunk length.");

            for (int i = 4; i < 8; i++)
            {
                byte letter = (byte)(chunkHeader[i] & ~32);
                if (letter < 'A' || letter > 'Z')
                    throw new InvalidDataException("Invalid PNG chunk type.");
            }

            if ((chunkHeader[6] & 32) != 0)
                throw new InvalidDataException("Invalid PNG chunk reserved bit.");

            type = Encoding.ASCII.GetString(chunkHeader, 4, 4);
            var data = new byte[(int)length];
            ReadExactly(stream, data);
            var checksum = new byte[4];
            ReadExactly(stream, checksum);
            uint crc = 0xffffffff;
            for (int i = 4; i < 8; i++)
                crc = UpdateCrc32(crc, chunkHeader[i]);

            for (int i = 0; i < data.Length; i++)
                crc = UpdateCrc32(crc, data[i]);

            if ((crc ^ 0xffffffff) != ReadUInt32(checksum, 0))
                throw new InvalidDataException("PNG chunk checksum mismatch.");

            return data;
        }

        private static uint UpdateCrc32(uint crc, byte value)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 0 ? crc >> 1 : (crc >> 1) ^ 0xedb88320;

            return crc;
        }

        private static void ReadExactly(Stream stream, byte[] data)
        {
            int offset = 0;
            while (offset < data.Length)
            {
                int read = stream.Read(data, offset, data.Length - offset);
                if (read == 0)
                    throw new InvalidDataException("PNG data is truncated.");

                offset += read;
            }
        }

        private static RGBA8BitColor ReadRgba8(byte[] data, int offset) =>
            new RGBA8BitColor(data[offset], data[offset + 1], data[offset + 2], data[offset + 3]);

        private static RGBA16BitColor ReadRgba16(byte[] data, int offset) =>
            new RGBA16BitColor(ReadUInt16(data, offset), ReadUInt16(data, offset + 2),
                ReadUInt16(data, offset + 4), ReadUInt16(data, offset + 6));

        private static ushort ReadUInt16(byte[] data, int offset) =>
            (ushort)((data[offset] << 8) | data[offset + 1]);

        private static uint ReadUInt32(byte[] data, int offset) =>
            ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) |
            ((uint)data[offset + 2] << 8) | data[offset + 3];
    }
}
