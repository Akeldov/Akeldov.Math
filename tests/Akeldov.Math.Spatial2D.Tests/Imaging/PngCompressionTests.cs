using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class PngCompressionTests
{
    [TestCase(CompressionLevel.NoCompression)]
    [TestCase(CompressionLevel.Fastest)]
    [TestCase(CompressionLevel.Optimal)]
    [TestCase(CompressionLevel.SmallestSize)]
    public void SaveAsPng_WithCompressionLevel_WritesDecodableImageData(CompressionLevel compressionLevel)
    {
        var raster = new Raster<Gray8BitColor>(
            new VectorXYInt(2, 1),
            new Gray8BitColor[] { new(0x12), new(0x34) });
        using var stream = new MemoryStream();

        raster.SaveAsPng(stream, compressionLevel);

        Assert.That(DecompressImageData(stream.ToArray()), Is.EqualTo(new byte[] { 0, 0x12, 0x34 }));
        Assert.That(stream.CanWrite, Is.True);
    }

    [Test]
    public void SaveAsPng_WithDefaultCompression_ProducesSmallerFileThanNoCompression()
    {
        var raster = new Raster<Gray8BitColor>(
            new VectorXYInt(128, 128),
            new Gray8BitColor[128 * 128]);
        using var defaultCompressionStream = new MemoryStream();
        using var noCompressionStream = new MemoryStream();

        raster.SaveAsPng(defaultCompressionStream);
        raster.SaveAsPng(noCompressionStream, CompressionLevel.NoCompression);

        Assert.That(defaultCompressionStream.Length, Is.LessThan(noCompressionStream.Length));
        Assert.That(
            DecompressImageData(defaultCompressionStream.ToArray()),
            Is.EqualTo(DecompressImageData(noCompressionStream.ToArray())));
    }

    [Test]
    public void SaveAsPng_WithCompression_WhenEachColorFormatIsUsed_PreservesScanlineValues()
    {
        var gray8 = new Raster<Gray8BitColor>(
            new VectorXYInt(1, 1),
            new Gray8BitColor[] { new(0x12) });
        var gray16 = new Raster<Gray16BitColor>(
            new VectorXYInt(1, 1),
            new Gray16BitColor[] { new(0x1234) });
        var rgba8 = new Raster<RGBA8BitColor>(
            new VectorXYInt(1, 1),
            new RGBA8BitColor[] { new(0x12, 0x34, 0x56, 0x78) });
        var rgba16 = new Raster<RGBA16BitColor>(
            new VectorXYInt(1, 1),
            new RGBA16BitColor[] { new(0x1234, 0x5678, 0x9abc, 0xdef0) });
        using var gray8Stream = new MemoryStream();
        using var gray16Stream = new MemoryStream();
        using var rgba8Stream = new MemoryStream();
        using var rgba16Stream = new MemoryStream();

        gray8.SaveAsPng(gray8Stream, CompressionLevel.Fastest);
        gray16.SaveAsPng(gray16Stream, CompressionLevel.Fastest);
        rgba8.SaveAsPng(rgba8Stream, CompressionLevel.Fastest);
        rgba16.SaveAsPng(rgba16Stream, CompressionLevel.Fastest);

        Assert.Multiple(() =>
        {
            Assert.That(DecompressImageData(gray8Stream.ToArray()), Is.EqualTo(new byte[] { 0, 0x12 }));
            Assert.That(DecompressImageData(gray16Stream.ToArray()), Is.EqualTo(new byte[] { 0, 0x12, 0x34 }));
            Assert.That(
                DecompressImageData(rgba8Stream.ToArray()),
                Is.EqualTo(new byte[] { 0, 0x12, 0x34, 0x56, 0x78 }));
            Assert.That(
                DecompressImageData(rgba16Stream.ToArray()),
                Is.EqualTo(new byte[] { 0, 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf0 }));
        });
    }

    [Test]
    public void SaveAsPng_WithCompressionLevelAndPath_WritesDecodableFile()
    {
        var raster = new Raster<Gray8BitColor>(
            new VectorXYInt(1, 1),
            new Gray8BitColor[] { new(0x5a) });
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "compressed-gray8.png");

        raster.SaveAsPng(path, CompressionLevel.Fastest);

        Assert.That(DecompressImageData(File.ReadAllBytes(path)), Is.EqualTo(new byte[] { 0, 0x5a }));
    }

    [Test]
    public void SaveAsPng_WhenCompressionLevelIsInvalid_ThrowsBeforeChangingOutput()
    {
        var raster = new Raster<Gray8BitColor>(
            new VectorXYInt(1, 1),
            new Gray8BitColor[] { new(0x5a) });
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "invalid-compression-level.png");
        byte[] originalContents = { 1, 2, 3, 4 };
        File.WriteAllBytes(path, originalContents);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            raster.SaveAsPng(path, (CompressionLevel)int.MaxValue));

        Assert.That(File.ReadAllBytes(path), Is.EqualTo(originalContents));
    }

    private static byte[] DecompressImageData(byte[] png)
    {
        using var compressedData = new MemoryStream();
        int offset = 8;
        while (offset < png.Length)
        {
            int chunkLength = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(offset, 4));
            string chunkType = Encoding.ASCII.GetString(png, offset + 4, 4);
            if (chunkType == "IDAT")
                compressedData.Write(png, offset + 8, chunkLength);

            offset += chunkLength + 12;
            if (chunkType == "IEND")
                break;
        }

        compressedData.Position = 0;
        using var zlibStream = new ZLibStream(compressedData, CompressionMode.Decompress);
        using var scanlines = new MemoryStream();
        zlibStream.CopyTo(scanlines);
        return scanlines.ToArray();
    }
}
