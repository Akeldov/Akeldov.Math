using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace Akeldov.Math.Hexes.Tests;

internal static class PngSnapshotComparer
{
    private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    public static bool AreEquivalent(byte[] left, byte[] right)
    {
        if (!TryReadContent(left, out byte[] leftHeader, out byte[] leftImageData) ||
            !TryReadContent(right, out byte[] rightHeader, out byte[] rightImageData))
        {
            return false;
        }

        return leftHeader.SequenceEqual(rightHeader) && leftImageData.SequenceEqual(rightImageData);
    }

    private static bool TryReadContent(byte[] png, out byte[] header, out byte[] imageData)
    {
        header = Array.Empty<byte>();
        imageData = Array.Empty<byte>();
        if (png.Length < PngSignature.Length || !png.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature))
            return false;

        using var compressedImageData = new MemoryStream();
        int offset = PngSignature.Length;
        while (offset <= png.Length - 12)
        {
            uint chunkLengthValue = BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(offset, 4));
            if (chunkLengthValue > int.MaxValue)
                return false;

            int chunkLength = (int)chunkLengthValue;
            int dataOffset = offset + 8;
            if (chunkLength > png.Length - dataOffset - 4)
                return false;

            string chunkType = Encoding.ASCII.GetString(png, offset + 4, 4);
            if (chunkType == "IHDR")
                header = png.AsSpan(dataOffset, chunkLength).ToArray();
            else if (chunkType == "IDAT")
                compressedImageData.Write(png, dataOffset, chunkLength);
            else if (chunkType == "IEND")
                break;

            offset = dataOffset + chunkLength + 4;
        }

        if (header.Length != 13 || compressedImageData.Length == 0)
            return false;

        try
        {
            compressedImageData.Position = 0;
            using var zlibStream = new ZLibStream(compressedImageData, CompressionMode.Decompress);
            using var decompressedImageData = new MemoryStream();
            zlibStream.CopyTo(decompressedImageData);
            imageData = decompressedImageData.ToArray();
            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }
}
