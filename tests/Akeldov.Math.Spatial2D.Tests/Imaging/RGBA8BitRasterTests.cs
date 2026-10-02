using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.IO.Compression;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class RGBA8BitRasterTests
{
    [Test]
    public void RGBA8BitRaster_WhenSourceBufferChanges_ReflectsMutation()
    {
        var values = new RGBA8BitColor[6];
        var raster = new SpatialRaster<RGBA8BitColor>(CreateGrid(), values);
        var color = new RGBA8BitColor(1, 2, 3, 4);

        values[5] = color;

        Assert.That(raster[1, 2], Is.EqualTo(color));
        Assert.That(raster.Values[5], Is.EqualTo(color));
    }

    [Test]
    public void RGBA8BitRaster_WhenValueCountDoesNotMatchGrid_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new SpatialRaster<RGBA8BitColor>(CreateGrid(), new RGBA8BitColor[5]));
    }

    [Test]
    public void RGBA8BitRasterIndexer_WhenCoordinatesAreUsed_MapsToRowMajorValue()
    {
        var raster = new SpatialRaster<RGBA8BitColor>(CreateGrid(), new RGBA8BitColor[6]);
        var color = new RGBA8BitColor(1, 2, 3, 4);

        raster[1, 2] = color;

        Assert.That(raster.Values[5], Is.EqualTo(color));
    }

    [Test]
    public void SaveAsPng_WhenRasterIsRGBA8Bit_WritesPng8WithAlpha()
    {
        SpatialRaster<RGBA8BitColor> raster = CreateRasterWithFirstPixel();
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "rgba8.png");

        raster.SaveAsPng(path);

        Assert.That(File.Exists(path), Is.True);
        Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));

        byte[] bytes = File.ReadAllBytes(path);
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(8));
        Assert.That(bytes[25], Is.EqualTo(6));
    }

    [Test]
    public void SaveAsPng_WhenRGBA8BitStreamIsProvided_WritesPng8WithAlpha()
    {
        SpatialRaster<RGBA8BitColor> raster = CreateRasterWithFirstPixel();
        using var stream = new MemoryStream();

        raster.SaveAsPng(stream);

        byte[] bytes = stream.ToArray();
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(8));
        Assert.That(bytes[25], Is.EqualTo(6));
    }

    [Test]
    public void SaveAsBmp_WhenRasterIsRGBA8Bit_WritesBgra8Pixels()
    {
        SpatialRaster<RGBA8BitColor> raster = CreateRasterWithFirstPixel();
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "rgba8.bmp");

        raster.SaveAsBmp(path);

        Assert.That(File.Exists(path), Is.True);
        Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));

        byte[] bytes = File.ReadAllBytes(path);
        int pixelOffset = BitConverter.ToInt32(bytes, 10);
        short bitsPerPixel = BitConverter.ToInt16(bytes, 28);

        Assert.That(bytes[0], Is.EqualTo((byte)'B'));
        Assert.That(bytes[1], Is.EqualTo((byte)'M'));
        Assert.That(bitsPerPixel, Is.EqualTo(32));
        Assert.That(bytes[pixelOffset], Is.EqualTo(0x56));
        Assert.That(bytes[pixelOffset + 1], Is.EqualTo(0x34));
        Assert.That(bytes[pixelOffset + 2], Is.EqualTo(0x12));
        Assert.That(bytes[pixelOffset + 3], Is.EqualTo(0x78));
    }

    [Test]
    public void SaveAsBmp_WhenRGBA8BitStreamIsProvided_WritesBgra8Pixels()
    {
        SpatialRaster<RGBA8BitColor> raster = CreateRasterWithFirstPixel();
        using var stream = new MemoryStream();

        raster.SaveAsBmp(stream);

        byte[] bytes = stream.ToArray();
        int pixelOffset = BitConverter.ToInt32(bytes, 10);

        Assert.That(bytes[0], Is.EqualTo((byte)'B'));
        Assert.That(bytes[1], Is.EqualTo((byte)'M'));
        Assert.That(BitConverter.ToInt16(bytes, 28), Is.EqualTo(32));
        Assert.That(bytes[pixelOffset], Is.EqualTo(0x56));
        Assert.That(bytes[pixelOffset + 1], Is.EqualTo(0x34));
        Assert.That(bytes[pixelOffset + 2], Is.EqualTo(0x12));
        Assert.That(bytes[pixelOffset + 3], Is.EqualTo(0x78));
    }

    [TestCase(CompressionLevel.NoCompression)]
    [TestCase(CompressionLevel.Fastest)]
    [TestCase(CompressionLevel.Optimal)]
    [TestCase(CompressionLevel.SmallestSize)]
    public void LoadFromPng_AfterSavingToStream_PreservesPixelsAndRowOrientation(CompressionLevel compressionLevel)
    {
        RGBA8BitColor[] values =
        {
            new(0x12, 0x34, 0x56, 0x78), RGBA8BitColor.Transparent,
            RGBA8BitColor.White, new(1, 2, 3, 4),
            new(byte.MaxValue, 0, 0, 0), new(0, 1, 128, byte.MaxValue)
        };
        var source = new SpatialRaster<RGBA8BitColor>(CreateGrid(), values);
        using var stream = new MemoryStream();
        stream.WriteByte(0xff);
        source.SaveAsPng(stream, compressionLevel);
        long imageEnd = stream.Position;
        stream.WriteByte(0x42);
        stream.Position = 1;

        Raster<RGBA8BitColor> loaded = RasterImageLoader.LoadRgba8FromPng(stream);

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Resolution, Is.EqualTo(source.Resolution));
            Assert.That(loaded.Values, Is.EqualTo(values));
            Assert.That(loaded.Values, Is.Not.SameAs(values));
            Assert.That(stream.CanRead, Is.True);
            Assert.That(stream.Position, Is.EqualTo(imageEnd));
            Assert.That(stream.ReadByte(), Is.EqualTo(0x42));
        });
    }

    [Test]
    public void LoadFromPng_AfterSavingToFile_PreservesPixelsAndClosesFile()
    {
        var source = new Raster<RGBA8BitColor>(new VectorXYInt(1, 2),
            new[] { new RGBA8BitColor(1, 2, 3, 4), RGBA8BitColor.White });
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "rgba8-roundtrip.png");
        try
        {
            source.SaveAsPng(path);

            Raster<RGBA8BitColor> loaded = RasterImageLoader.LoadRgba8FromPng(path);

            Assert.That(loaded.Resolution, Is.EqualTo(source.Resolution));
            Assert.That(loaded.Values, Is.EqualTo(source.Values));
            using FileStream exclusive = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
            Assert.That(exclusive.Length, Is.GreaterThan(0));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestCase("rgba8-filter-0.png", 3, 3)]
    [TestCase("rgba8-filter-1.png", 3, 3)]
    [TestCase("rgba8-filter-2.png", 3, 3)]
    [TestCase("rgba8-filter-3.png", 3, 3)]
    [TestCase("rgba8-filter-4.png", 3, 3)]
    [TestCase("rgba8-adam7.png", 9, 9)]
    [TestCase("rgba8-adam7-single.png", 1, 1)]
    public void LoadFromPng_WithIndependentFixture_DecodesFiltersInterlacingAndSplitIdat(
        string fileName, int width, int height)
    {
        // Fixtures were encoded with Python's zlib, independently of the library encoder.
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Imaging", "Fixtures", fileName);
        using var stream = new ShortReadStream(File.ReadAllBytes(path));

        Raster<RGBA8BitColor> loaded = RasterImageLoader.LoadRgba8FromPng(stream);

        Assert.That(loaded.Resolution, Is.EqualTo(new VectorXYInt(width, height)));
        for (int pngY = 0; pngY < height; pngY++)
        for (int x = 0; x < width; x++)
        {
            var expected = new RGBA8BitColor(
                unchecked((byte)(0x12 + x * 79 + pngY * 23)),
                unchecked((byte)(0xab + x * 35 - pngY * 45)),
                unchecked((byte)(x * 12 + pngY * 98)),
                unchecked((byte)(255 - x * 100 - pngY * 77)));
            Assert.That(loaded[x, height - 1 - pngY], Is.EqualTo(expected), $"{fileName}: x={x}, PNG row={pngY}");
        }

        Assert.That(stream.CanRead, Is.True);
    }

    [Test]
    public void LoadFromPng_WithInvalidArguments_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RasterImageLoader.LoadRgba8FromPng((Stream)null!));
        Assert.Throws<ArgumentNullException>(() => RasterImageLoader.LoadRgba8FromPng((string)null!));
        using var stream = new MemoryStream();
        stream.Dispose();
        Assert.Throws<ArgumentException>(() => RasterImageLoader.LoadRgba8FromPng(stream));
    }

    [TestCase("rgba16")]
    [TestCase("gray8")]
    public void LoadFromPng_WithUnsupportedColorFormat_Throws(string format)
    {
        using var stream = new MemoryStream();
        if (format == "rgba16")
        {
            new Raster<RGBA16BitColor>(new VectorXYInt(1, 1), new RGBA16BitColor[1]).SaveAsPng(stream);
        }
        else
        {
            new Raster<Gray8BitColor>(new VectorXYInt(1, 1), new Gray8BitColor[1]).SaveAsPng(stream);
        }
        stream.Position = 0;

        Assert.Throws<NotSupportedException>(() => RasterImageLoader.LoadRgba8FromPng(stream));
        Assert.That(stream.CanRead, Is.True);
    }

    [TestCase("truncated")]
    [TestCase("crc")]
    public void LoadFromPng_WithMalformedData_ThrowsAndLeavesStreamOpen(string defect)
    {
        using var encoded = new MemoryStream();
        CreateRasterWithFirstPixel().SaveAsPng(encoded);
        byte[] png = encoded.ToArray();
        if (defect == "crc")
            png[29] ^= 1;
        using var stream = new MemoryStream(defect == "truncated" ? png[..^1] : png);

        Assert.Throws<InvalidDataException>(() => RasterImageLoader.LoadRgba8FromPng(stream));
        Assert.That(stream.CanRead, Is.True);
    }

    private static SpatialRaster<RGBA8BitColor> CreateRasterWithFirstPixel()
    {
        var values = new RGBA8BitColor[6];
        values[0] = new RGBA8BitColor(0x12, 0x34, 0x56, 0x78);

        return new SpatialRaster<RGBA8BitColor>(CreateGrid(), values);
    }

    private static RasterGeometry CreateGrid()
    {
        return new RasterGeometry(new PointXY(0f, 0f), new VectorXY(2f, 3f), new VectorXYInt(2, 3));
    }
}
