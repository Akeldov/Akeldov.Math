using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.Buffers.Binary;
using System.IO;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Decodes uncompressed indexed 8-bit and BGRA 32-bit BMP images.
    /// </summary>
    internal static class BmpDecoder
    {
        public static Raster<Gray8BitColor> LoadGray8(Stream stream) => Load(stream, true, ToGray8);

        public static Raster<RGBA8BitColor> LoadRgba8(Stream stream) => Load(stream, false, color => color);

        private static Raster<TValue> Load<TValue>(Stream stream, bool grayscale, Func<RGBA8BitColor, TValue> toValue)
        {
            (byte[] fileHeader, byte[] infoHeader) = ReadHeaders(stream);
            int width = BinaryPrimitives.ReadInt32LittleEndian(infoHeader.AsSpan(4));
            int signedHeight = BinaryPrimitives.ReadInt32LittleEndian(infoHeader.AsSpan(8));
            if (width <= 0 || signedHeight == 0 || signedHeight == int.MinValue)
                throw new InvalidDataException("Invalid BMP dimensions.");

            int height = System.Math.Abs(signedHeight);
            ushort bits = BinaryPrimitives.ReadUInt16LittleEndian(infoHeader.AsSpan(14));
            uint compression = BinaryPrimitives.ReadUInt32LittleEndian(infoHeader.AsSpan(16));
            if (compression != 0 || (bits != 8 && bits != 32) || (grayscale && bits != 8))
                throw new NotSupportedException("Only uncompressed indexed 8-bit or BGRA 32-bit BMP images are supported; grayscale rasters require an indexed 8-bit BMP.");

            int bytesPerPixel = bits / 8;
            long rowStride = ((long)width * bytesPerPixel + 3) & ~3L;
            if (rowStride > int.MaxValue || (long)width * height > int.MaxValue)
                throw new InvalidDataException("BMP dimensions exceed supported raster sizes.");

            uint colorsUsed = BinaryPrimitives.ReadUInt32LittleEndian(infoHeader.AsSpan(32));
            if (bits == 8 && colorsUsed > 256)
                throw new InvalidDataException("Invalid BMP palette size.");

            long paletteCount = bits == 8 && colorsUsed == 0 ? 256 : colorsUsed;
            uint pixelOffset = BinaryPrimitives.ReadUInt32LittleEndian(fileHeader.AsSpan(10));
            uint fileSize = BinaryPrimitives.ReadUInt32LittleEndian(fileHeader.AsSpan(2));
            long metadataSize = 14L + infoHeader.Length + paletteCount * 4;
            long imageSize = rowStride * height;
            uint declaredImageSize = BinaryPrimitives.ReadUInt32LittleEndian(infoHeader.AsSpan(20));
            if (pixelOffset < metadataSize || fileSize < (long)pixelOffset + imageSize ||
                (declaredImageSize != 0 && declaredImageSize < imageSize))
            {
                throw new InvalidDataException("Invalid BMP pixel offset or file/image size.");
            }

            TValue[] palette = bits == 8 ? ReadPalette(stream, (int)paletteCount, toValue) : Array.Empty<TValue>();
            long consumed = 14L + infoHeader.Length + (bits == 8 ? paletteCount * 4 : 0);
            SkipExactly(stream, pixelOffset - consumed);
            TValue[] values = ReadPixels(stream, width, height, signedHeight < 0, (int)rowStride,
                bytesPerPixel, palette, toValue);
            SkipExactly(stream, fileSize - pixelOffset - imageSize);
            return new Raster<TValue>(new VectorXYInt(width, height), values);
        }

        private static (byte[] FileHeader, byte[] InfoHeader) ReadHeaders(Stream stream)
        {
            var fileHeader = new byte[14];
            ReadExactly(stream, fileHeader);
            if (fileHeader[0] != 'B' || fileHeader[1] != 'M' ||
                BinaryPrimitives.ReadUInt32LittleEndian(fileHeader.AsSpan(6)) != 0)
            {
                throw new InvalidDataException("Invalid BMP file header.");
            }

            var sizeBytes = new byte[4];
            ReadExactly(stream, sizeBytes);
            uint headerSize = BinaryPrimitives.ReadUInt32LittleEndian(sizeBytes);
            if (headerSize != 40 && headerSize != 52 && headerSize != 56 && headerSize != 108 && headerSize != 124)
                throw new NotSupportedException("Unsupported BMP information header.");

            var infoHeader = new byte[(int)headerSize];
            sizeBytes.CopyTo(infoHeader, 0);
            ReadExactly(stream, infoHeader, 4);
            if (BinaryPrimitives.ReadUInt16LittleEndian(infoHeader.AsSpan(12)) != 1)
                throw new InvalidDataException("BMP must have exactly one color plane.");

            return (fileHeader, infoHeader);
        }

        private static TValue[] ReadPalette<TValue>(Stream stream, int count, Func<RGBA8BitColor, TValue> toValue)
        {
            var palette = new TValue[count];
            var entry = new byte[4];
            for (int i = 0; i < count; i++)
            {
                ReadExactly(stream, entry);
                // The fourth RGBQUAD byte is reserved, not an alpha channel.
                palette[i] = toValue(new RGBA8BitColor(entry[2], entry[1], entry[0], byte.MaxValue));
            }

            return palette;
        }

        private static TValue[] ReadPixels<TValue>(Stream stream, int width, int height, bool topDown,
            int rowStride, int bytesPerPixel, TValue[] palette, Func<RGBA8BitColor, TValue> toValue)
        {
            var values = new TValue[checked(width * height)];
            var row = new byte[rowStride];
            for (int rowIndex = 0; rowIndex < height; rowIndex++)
            {
                ReadExactly(stream, row);
                int y = topDown ? height - 1 - rowIndex : rowIndex;
                for (int x = 0; x < width; x++)
                {
                    TValue value;
                    if (bytesPerPixel == 1)
                    {
                        int index = row[x];
                        if (index >= palette.Length)
                            throw new InvalidDataException("BMP pixel index is outside the palette.");

                        value = palette[index];
                    }
                    else
                    {
                        int offset = x * 4;
                        value = toValue(new RGBA8BitColor(row[offset + 2], row[offset + 1], row[offset], row[offset + 3]));
                    }

                    values[y * width + x] = value;
                }
            }

            return values;
        }

        private static Gray8BitColor ToGray8(RGBA8BitColor color)
        {
            if (color.R != color.G || color.R != color.B)
                throw new NotSupportedException("Grayscale BMP loading requires a grayscale palette.");

            return new Gray8BitColor(color.R);
        }

        private static void ReadExactly(Stream stream, byte[] data, int offset = 0)
        {
            while (offset < data.Length)
            {
                int read = stream.Read(data, offset, data.Length - offset);
                if (read == 0)
                    throw new InvalidDataException("BMP data is truncated.");

                offset += read;
            }
        }

        private static void SkipExactly(Stream stream, long count)
        {
            if (count == 0)
                return;

            var buffer = new byte[4096];
            while (count > 0)
            {
                int read = stream.Read(buffer, 0, (int)System.Math.Min(count, buffer.Length));
                if (read == 0)
                    throw new InvalidDataException("BMP data is truncated.");

                count -= read;
            }
        }
    }
}
