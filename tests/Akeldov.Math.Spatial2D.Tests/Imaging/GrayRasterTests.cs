using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.IO.Compression;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class GrayRasterTests
{
    [Test]
    public void Gray8BitRaster_WhenSourceBufferChanges_ReflectsMutation()
    {
        Gray8BitColor[] values = { new(1), new(2), new(3), new(4) };
        var raster = new SpatialRaster<Gray8BitColor>(CreateGrid(), values);

        values[1] = new Gray8BitColor(9);

        Assert.That(raster[1, 0].Value, Is.EqualTo(9));
        Assert.That(raster.Values[1].Value, Is.EqualTo(9));
    }

    [Test]
    public void Gray8BitRaster_WhenValueCountDoesNotMatchGrid_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new SpatialRaster<Gray8BitColor>(CreateGrid(), new Gray8BitColor[3]));
    }

    [Test]
    public void Gray8BitRasterIndexer_WhenCoordinatesAreUsed_MapsToRowMajorValue()
    {
        var raster = new SpatialRaster<Gray8BitColor>(CreateGrid(), new Gray8BitColor[4]);

        raster[1, 0] = new Gray8BitColor(9);

        Assert.That(raster[1, 0].Value, Is.EqualTo(9));
        Assert.That(raster.Values[1].Value, Is.EqualTo(9));
    }

    [Test]
    public void SaveAsPng_WhenRasterIsGray8Bit_WritesPng8()
    {
        Gray8BitColor[] values = { new(0x12), new(0x56), new(0x34), new(0x78) };
        var raster = new SpatialRaster<Gray8BitColor>(CreateGrid(), values);
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "gray8.png");

        raster.SaveAsPng(path);

        Assert.That(File.Exists(path), Is.True);
        Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));

        byte[] bytes = File.ReadAllBytes(path);
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(8));
        Assert.That(bytes[25], Is.EqualTo(0));
    }

    [Test]
    public void SaveAsPng_WhenGray8BitStreamIsProvided_WritesPng8()
    {
        var raster = new SpatialRaster<Gray8BitColor>(CreateGrid(), new Gray8BitColor[] { new(0x12), new(0x56), new(0x34), new(0x78) });
        using var stream = new MemoryStream();

        raster.SaveAsPng(stream);

        byte[] bytes = stream.ToArray();
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(8));
        Assert.That(bytes[25], Is.EqualTo(0));
    }

    [Test]
    public void SaveAsPng_WhenNonSpatialGray8BitRasterIsProvided_WritesPng8()
    {
        var raster = new Raster<Gray8BitColor>(new VectorXYInt(2, 2), new Gray8BitColor[] { new(0x12), new(0x56), new(0x34), new(0x78) });
        using var stream = new MemoryStream();

        raster.SaveAsPng(stream);

        byte[] bytes = stream.ToArray();
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(8));
        Assert.That(bytes[25], Is.EqualTo(0));
    }

    [Test]
    public void SaveAsBmp_WhenGray8BitStreamIsProvided_WritesBmp8()
    {
        var raster = new SpatialRaster<Gray8BitColor>(CreateGrid(), new Gray8BitColor[] { new(0x12), new(0x56), new(0x34), new(0x78) });
        using var stream = new MemoryStream();

        raster.SaveAsBmp(stream);

        byte[] bytes = stream.ToArray();
        Assert.That(bytes[0], Is.EqualTo((byte)'B'));
        Assert.That(bytes[1], Is.EqualTo((byte)'M'));
        Assert.That(BitConverter.ToInt16(bytes, 28), Is.EqualTo(8));
    }

    [Test]
    public void SaveAsBmp_WhenNonSpatialGray8BitRasterIsProvided_WritesBmp8()
    {
        var raster = new Raster<Gray8BitColor>(new VectorXYInt(2, 2), new Gray8BitColor[] { new(0x12), new(0x56), new(0x34), new(0x78) });
        using var stream = new MemoryStream();

        raster.SaveAsBmp(stream);

        byte[] bytes = stream.ToArray();
        Assert.That(bytes[0], Is.EqualTo((byte)'B'));
        Assert.That(bytes[1], Is.EqualTo((byte)'M'));
        Assert.That(BitConverter.ToInt16(bytes, 28), Is.EqualTo(8));
    }

    [TestCase(CompressionLevel.NoCompression)]
    [TestCase(CompressionLevel.Fastest)]
    [TestCase(CompressionLevel.Optimal)]
    [TestCase(CompressionLevel.SmallestSize)]
    public void LoadFromPng_WhenGray8BitIsSavedToStream_PreservesSamplesAndRowOrientation(
        CompressionLevel compressionLevel)
    {
        Gray8BitColor[] values = { new(0), new(1), new(0x12), new(0xab), new(128), new(255) };
        var source = new Raster<Gray8BitColor>(new VectorXYInt(2, 3), values);
        using var stream = new MemoryStream();
        stream.WriteByte(0xff);
        source.SaveAsPng(stream, compressionLevel);
        long imageEnd = stream.Position;
        stream.WriteByte(0x42);
        stream.Position = 1;

        Raster<Gray8BitColor> loaded = Raster<Gray8BitColor>.LoadFromPng(stream);

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
    public void LoadFromPng_WhenGray8BitIsSavedToFile_PreservesSamplesAndClosesFile()
    {
        var source = new SpatialRaster<Gray8BitColor>(CreateGrid(),
            new Gray8BitColor[] { new(0x12), new(0x56), new(0x9a), new(0xde) });
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "gray8-roundtrip.png");
        try
        {
            source.SaveAsPng(path);

            Raster<Gray8BitColor> loaded = Raster<Gray8BitColor>.LoadFromPng(path);

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

    [TestCase("gray8-filter-0.png", 3, 3)]
    [TestCase("gray8-filter-1.png", 3, 3)]
    [TestCase("gray8-filter-2.png", 3, 3)]
    [TestCase("gray8-filter-3.png", 3, 3)]
    [TestCase("gray8-filter-4.png", 3, 3)]
    [TestCase("gray8-adam7.png", 9, 9)]
    [TestCase("gray8-adam7-single.png", 1, 1)]
    [TestCase("gray8-adam7-column.png", 1, 9)]
    [TestCase("gray8-adam7-row.png", 9, 1)]
    public void LoadFromPng_WithIndependentGray8BitFixture_DecodesFiltersInterlacingAndSplitIdat(
        string fileName, int width, int height)
    {
        // Fixtures were encoded with Python's zlib, independently of the library encoder.
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Imaging", "Fixtures", fileName);
        using var stream = new ShortReadStream(File.ReadAllBytes(path));

        Raster<Gray8BitColor> loaded = Raster<Gray8BitColor>.LoadFromPng(stream);

        Assert.That(loaded.Resolution, Is.EqualTo(new VectorXYInt(width, height)));
        for (int pngY = 0; pngY < height; pngY++)
        for (int x = 0; x < width; x++)
        {
            var expected = new Gray8BitColor(unchecked((byte)(0x12 + x * 79 + pngY * 23)));
            Assert.That(loaded[x, height - 1 - pngY], Is.EqualTo(expected), $"{fileName}: x={x}, PNG row={pngY}");
        }

        Assert.That(stream.CanRead, Is.True);
    }

    [Test]
    public void LoadFromPng_WithInvalidGray8BitArguments_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Raster<Gray8BitColor>.LoadFromPng((Stream)null!));
        Assert.Throws<ArgumentNullException>(() => Raster<Gray8BitColor>.LoadFromPng((string)null!));
        using var stream = new MemoryStream(new byte[1]);
        stream.Dispose();
        Assert.Throws<ArgumentException>(() => Raster<Gray8BitColor>.LoadFromPng(stream));
        Assert.Throws<FileNotFoundException>(() =>
            Raster<Gray8BitColor>.LoadFromPng(Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid() + ".png")));
    }

    [TestCase("gray16")]
    [TestCase("rgba8")]
    [TestCase("rgba16")]
    public void LoadFromPng_WithUnsupportedGray8BitColorFormat_Throws(string format)
    {
        using var stream = new MemoryStream();
        if (format == "gray16")
            new Raster<Gray16BitColor>(new VectorXYInt(1, 1), new Gray16BitColor[1]).SaveAsPng(stream);
        else if (format == "rgba8")
            new Raster<RGBA8BitColor>(new VectorXYInt(1, 1), new RGBA8BitColor[1]).SaveAsPng(stream);
        else
            new Raster<RGBA16BitColor>(new VectorXYInt(1, 1), new RGBA16BitColor[1]).SaveAsPng(stream);
        stream.Position = 0;

        Assert.Throws<NotSupportedException>(() => Raster<Gray8BitColor>.LoadFromPng(stream));
        Assert.That(stream.CanRead, Is.True);
    }

    [TestCase("signature")]
    [TestCase("crc")]
    [TestCase("truncated")]
    public void LoadFromPng_WithMalformedGray8BitData_ThrowsAndLeavesStreamOpen(string defect)
    {
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Imaging", "Fixtures", "gray8-filter-0.png");
        byte[] bytes = File.ReadAllBytes(path);
        if (defect == "signature")
            bytes[0] = 0;
        else if (defect == "crc")
            bytes[29] ^= 1;
        else
            bytes = bytes[..^1];
        using var stream = new MemoryStream(bytes);

        Assert.Throws<InvalidDataException>(() => Raster<Gray8BitColor>.LoadFromPng(stream));
        Assert.That(stream.CanRead, Is.True);
    }

    [Test]
    public void Gray16BitRaster_WhenSourceBufferChanges_ReflectsMutation()
    {
        Gray16BitColor[] values = { new(1), new(2), new(3), new(4) };
        var raster = new SpatialRaster<Gray16BitColor>(CreateGrid(), values);

        values[1] = new Gray16BitColor(9);

        Assert.That(raster[1, 0].Value, Is.EqualTo(9));
        Assert.That(raster.Values[1].Value, Is.EqualTo(9));
    }

    [Test]
    public void Gray16BitRaster_WhenValueCountDoesNotMatchGrid_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new SpatialRaster<Gray16BitColor>(CreateGrid(), new Gray16BitColor[3]));
    }

    [Test]
    public void Gray16BitRasterIndexer_WhenCoordinatesAreUsed_MapsToRowMajorValue()
    {
        var raster = new SpatialRaster<Gray16BitColor>(CreateGrid(), new Gray16BitColor[4]);

        raster[1, 0] = new Gray16BitColor(9);

        Assert.That(raster[1, 0].Value, Is.EqualTo(9));
        Assert.That(raster.Values[1].Value, Is.EqualTo(9));
    }

    [Test]
    public void SaveAsPng_WhenGray16BitStreamIsProvided_WritesPng16()
    {
        var raster = new SpatialRaster<Gray16BitColor>(CreateGrid(), new Gray16BitColor[] { new(0x1234), new(0x5678), new(0x9abc), new(0xdef0) });
        using var stream = new MemoryStream();

        raster.SaveAsPng(stream);

        byte[] bytes = stream.ToArray();
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(16));
        Assert.That(bytes[25], Is.EqualTo(0));
    }

    [TestCase(CompressionLevel.NoCompression)]
    [TestCase(CompressionLevel.Fastest)]
    [TestCase(CompressionLevel.Optimal)]
    [TestCase(CompressionLevel.SmallestSize)]
    public void LoadFromPng_WhenGray16BitIsSavedToStream_PreservesSamplesAndRowOrientation(
        CompressionLevel compressionLevel)
    {
        Gray16BitColor[] values = { new(0), new(1), new(0x1234), new(0xabcd), new(32768), new(65535) };
        var source = new Raster<Gray16BitColor>(new VectorXYInt(2, 3), values);
        using var stream = new MemoryStream();
        stream.WriteByte(0xff);
        source.SaveAsPng(stream, compressionLevel);
        long imageEnd = stream.Position;
        stream.WriteByte(0x42);
        stream.Position = 1;

        Raster<Gray16BitColor> loaded = Raster<Gray16BitColor>.LoadFromPng(stream);

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
    public void LoadFromPng_WhenGray16BitIsSavedToFile_PreservesSamplesAndClosesFile()
    {
        var source = new SpatialRaster<Gray16BitColor>(CreateGrid(),
            new Gray16BitColor[] { new(0x1234), new(0x5678), new(0x9abc), new(0xdef0) });
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "gray16-roundtrip.png");
        try
        {
            source.SaveAsPng(path);

            Raster<Gray16BitColor> loaded = Raster<Gray16BitColor>.LoadFromPng(path);

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

    [TestCase("gray16-filter-0.png", 3, 3)]
    [TestCase("gray16-filter-1.png", 3, 3)]
    [TestCase("gray16-filter-2.png", 3, 3)]
    [TestCase("gray16-filter-3.png", 3, 3)]
    [TestCase("gray16-filter-4.png", 3, 3)]
    [TestCase("gray16-adam7.png", 9, 9)]
    [TestCase("gray16-adam7-single.png", 1, 1)]
    [TestCase("gray16-adam7-column.png", 1, 9)]
    [TestCase("gray16-adam7-row.png", 9, 1)]
    public void LoadFromPng_WithIndependentGray16BitFixture_DecodesFiltersInterlacingAndSplitIdat(
        string fileName, int width, int height)
    {
        // Fixtures were encoded with Python's zlib, independently of the library encoder.
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Imaging", "Fixtures", fileName);
        using var stream = new ShortReadStream(File.ReadAllBytes(path));

        Raster<Gray16BitColor> loaded = Raster<Gray16BitColor>.LoadFromPng(stream);

        Assert.That(loaded.Resolution, Is.EqualTo(new VectorXYInt(width, height)));
        for (int pngY = 0; pngY < height; pngY++)
        for (int x = 0; x < width; x++)
        {
            var expected = new Gray16BitColor(unchecked((ushort)(0x1234 + x * 7919 + pngY * 2347)));
            Assert.That(loaded[x, height - 1 - pngY], Is.EqualTo(expected), $"{fileName}: x={x}, PNG row={pngY}");
        }

        Assert.That(stream.CanRead, Is.True);
    }

    [Test]
    public void LoadFromPng_WithInvalidGray16BitArguments_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Raster<Gray16BitColor>.LoadFromPng((Stream)null!));
        Assert.Throws<ArgumentNullException>(() => Raster<Gray16BitColor>.LoadFromPng((string)null!));
        using var stream = new MemoryStream(new byte[1]);
        stream.Dispose();
        Assert.Throws<ArgumentException>(() => Raster<Gray16BitColor>.LoadFromPng(stream));
        Assert.Throws<FileNotFoundException>(() =>
            Raster<Gray16BitColor>.LoadFromPng(Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid() + ".png")));
    }

    [TestCase("gray8")]
    [TestCase("rgba8")]
    [TestCase("rgba16")]
    public void LoadFromPng_WithUnsupportedGray16BitColorFormat_Throws(string format)
    {
        using var stream = new MemoryStream();
        if (format == "gray8")
            new Raster<Gray8BitColor>(new VectorXYInt(1, 1), new Gray8BitColor[1]).SaveAsPng(stream);
        else if (format == "rgba8")
            new Raster<RGBA8BitColor>(new VectorXYInt(1, 1), new RGBA8BitColor[1]).SaveAsPng(stream);
        else
            new Raster<RGBA16BitColor>(new VectorXYInt(1, 1), new RGBA16BitColor[1]).SaveAsPng(stream);
        stream.Position = 0;

        Assert.Throws<NotSupportedException>(() => Raster<Gray16BitColor>.LoadFromPng(stream));
        Assert.That(stream.CanRead, Is.True);
    }

    [TestCase("signature")]
    [TestCase("crc")]
    [TestCase("truncated")]
    public void LoadFromPng_WithMalformedGray16BitData_ThrowsAndLeavesStreamOpen(string defect)
    {
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Imaging", "Fixtures", "gray16-filter-0.png");
        byte[] bytes = File.ReadAllBytes(path);
        if (defect == "signature")
            bytes[0] = 0;
        else if (defect == "crc")
            bytes[29] ^= 1;
        else
            bytes = bytes[..^1];
        using var stream = new MemoryStream(bytes);

        Assert.Throws<InvalidDataException>(() => Raster<Gray16BitColor>.LoadFromPng(stream));
        Assert.That(stream.CanRead, Is.True);
    }

    private static RasterGeometry CreateGrid()
    {
        return new RasterGeometry(new PointXY(0f, 0f), VectorXY.One, new VectorXYInt(2, 2));
    }
}
