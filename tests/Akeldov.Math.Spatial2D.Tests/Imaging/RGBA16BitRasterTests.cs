using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class RGBA16BitRasterTests
{
    [Test]
    public void RGBA16BitRaster_WhenSourceBufferChanges_ReflectsMutation()
    {
        var values = new RGBA16BitColor[6];
        var raster = new SpatialRaster<RGBA16BitColor>(CreateGrid(), values);
        var color = new RGBA16BitColor(1, 2, 3, 4);

        values[5] = color;

        Assert.That(raster[1, 2], Is.EqualTo(color));
        Assert.That(raster.Values[5], Is.EqualTo(color));
    }

    [Test]
    public void RGBA16BitRaster_WhenValueCountDoesNotMatchGrid_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new SpatialRaster<RGBA16BitColor>(CreateGrid(), new RGBA16BitColor[5]));
    }

    [Test]
    public void RGBA16BitRasterIndexer_WhenCoordinatesAreUsed_MapsToRowMajorValue()
    {
        var raster = new SpatialRaster<RGBA16BitColor>(CreateGrid(), new RGBA16BitColor[6]);
        var color = new RGBA16BitColor(1, 2, 3, 4);

        raster[1, 2] = color;

        Assert.That(raster.Values[5], Is.EqualTo(color));
    }

    [Test]
    public void SaveAsPng_WhenRasterIsRGBA16Bit_WritesPng16WithAlpha()
    {
        var values = new RGBA16BitColor[6];
        values[0] = new RGBA16BitColor(0x1234, 0x5678, 0x9abc, 0xdef0);
        var raster = new SpatialRaster<RGBA16BitColor>(CreateGrid(), values);
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "rgba16.png");

        raster.SaveAsPng(path);

        Assert.That(File.Exists(path), Is.True);
        Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));

        byte[] bytes = File.ReadAllBytes(path);
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(16));
        Assert.That(bytes[25], Is.EqualTo(6));
    }

    [Test]
    public void SaveAsPng_WhenRGBA16BitStreamIsProvided_WritesPng16WithAlpha()
    {
        var values = new RGBA16BitColor[6];
        values[0] = new RGBA16BitColor(0x1234, 0x5678, 0x9abc, 0xdef0);
        var raster = new SpatialRaster<RGBA16BitColor>(CreateGrid(), values);
        using var stream = new MemoryStream();

        raster.SaveAsPng(stream);

        byte[] bytes = stream.ToArray();
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(16));
        Assert.That(bytes[25], Is.EqualTo(6));
    }

    [TestCase(CompressionLevel.NoCompression)]
    [TestCase(CompressionLevel.Fastest)]
    [TestCase(CompressionLevel.Optimal)]
    [TestCase(CompressionLevel.SmallestSize)]
    public void LoadFromPng_AfterSavingToStream_PreservesAllChannelsAndRowOrientation(CompressionLevel compressionLevel)
    {
        RGBA16BitColor[] values =
        {
            new(0x1234, 0x5678, 0x9abc, 0xdef0), RGBA16BitColor.Transparent,
            RGBA16BitColor.White, new(1, 2, 3, 4),
            new(ushort.MaxValue, 0, 0, 0), new(0, 1, 32768, ushort.MaxValue)
        };
        var source = new SpatialRaster<RGBA16BitColor>(CreateGrid(), values);
        using var stream = new MemoryStream();
        stream.WriteByte(0xff);
        source.SaveAsPng(stream, compressionLevel);
        long imageEnd = stream.Position;
        stream.WriteByte(0x42);
        stream.Position = 1;

        Raster<RGBA16BitColor> loaded = RasterImageLoader.LoadRgba16FromPng(stream);

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
        var source = new Raster<RGBA16BitColor>(new VectorXYInt(1, 2),
            new[] { new RGBA16BitColor(1, 2, 3, 4), RGBA16BitColor.White });
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "rgba16-roundtrip.png");
        try
        {
            source.SaveAsPng(path);

            Raster<RGBA16BitColor> loaded = RasterImageLoader.LoadRgba16FromPng(path);

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

    [TestCase("rgba16-filter-0.png", 3, 3)]
    [TestCase("rgba16-filter-1.png", 3, 3)]
    [TestCase("rgba16-filter-2.png", 3, 3)]
    [TestCase("rgba16-filter-3.png", 3, 3)]
    [TestCase("rgba16-filter-4.png", 3, 3)]
    [TestCase("rgba16-adam7.png", 9, 9)]
    [TestCase("rgba16-adam7-single.png", 1, 1)]
    public void LoadFromPng_WithIndependentFixture_DecodesFiltersInterlacingAndSplitIdat(
        string fileName, int width, int height)
    {
        // Fixtures were encoded with Python's zlib, independently of the library encoder.
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Imaging", "Fixtures", fileName);
        using var stream = new ShortReadStream(File.ReadAllBytes(path));

        Raster<RGBA16BitColor> loaded = RasterImageLoader.LoadRgba16FromPng(stream);

        Assert.That(loaded.Resolution, Is.EqualTo(new VectorXYInt(width, height)));
        for (int pngY = 0; pngY < height; pngY++)
        for (int x = 0; x < width; x++)
        {
            var expected = new RGBA16BitColor(
                unchecked((ushort)(0x1234 + x * 7919 + pngY * 2347)),
                unchecked((ushort)(0xabcd + x * 3571 - pngY * 4567)),
                unchecked((ushort)(x * 1237 + pngY * 9871)),
                unchecked((ushort)(65535 - x * 10003 - pngY * 7777)));
            Assert.That(loaded[x, height - 1 - pngY], Is.EqualTo(expected), $"{fileName}: x={x}, PNG row={pngY}");
        }

        Assert.That(stream.CanRead, Is.True);
    }

    [Test]
    public void LoadFromPng_WithInvalidArguments_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RasterImageLoader.LoadRgba16FromPng((Stream)null!));
        Assert.Throws<ArgumentNullException>(() => RasterImageLoader.LoadRgba16FromPng((string)null!));
        using var unreadable = new MemoryStream(new byte[1]);
        unreadable.Dispose();
        Assert.Throws<ArgumentException>(() => RasterImageLoader.LoadRgba16FromPng(unreadable));
        Assert.Throws<FileNotFoundException>(() =>
            RasterImageLoader.LoadRgba16FromPng(Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid() + ".png")));
    }

    [TestCase(8, 6)]
    [TestCase(16, 0)]
    [TestCase(16, 2)]
    public void LoadFromPng_WithUnsupportedColorFormat_Throws(byte depth, byte colorType)
    {
        using var stream = new MemoryStream(CreatePng(new byte[9], depth: depth, colorType: colorType));

        Assert.Throws<NotSupportedException>(() => RasterImageLoader.LoadRgba16FromPng(stream));
        Assert.That(stream.CanRead, Is.True);
    }

    [TestCase(0)]
    [TestCase(7)]
    [TestCase(20)]
    [TestCase(40)]
    [TestCase(-1)]
    public void LoadFromPng_WithTruncatedData_Throws(int length)
    {
        byte[] png = CreatePng(new byte[9]);
        using var stream = new MemoryStream(png[..(length < 0 ? png.Length - 1 : length)]);

        Assert.Throws<InvalidDataException>(() => RasterImageLoader.LoadRgba16FromPng(stream));
        Assert.That(stream.CanRead, Is.True);
    }

    [TestCase(8)]
    [TestCase(10)]
    public void LoadFromPng_WithWrongDecompressedLength_Throws(int length)
    {
        using var stream = new MemoryStream(CreatePng(new byte[length]));

        Assert.Throws<InvalidDataException>(() => RasterImageLoader.LoadRgba16FromPng(stream));
    }

    [Test]
    public void LoadFromPng_WithInvalidFilter_Throws()
    {
        byte[] scanline = new byte[9];
        scanline[0] = 5;
        using var stream = new MemoryStream(CreatePng(scanline));

        Assert.Throws<InvalidDataException>(() => RasterImageLoader.LoadRgba16FromPng(stream));
    }

    [TestCase("signature")]
    [TestCase("crc")]
    [TestCase("adler")]
    [TestCase("zlib")]
    [TestCase("dimensions")]
    [TestCase("header")]
    [TestCase("missing-idat")]
    [TestCase("nonconsecutive-idat")]
    [TestCase("duplicate-header")]
    public void LoadFromPng_WithMalformedPng_Throws(string defect)
    {
        using var stream = new MemoryStream(CreatePng(new byte[9], defect));

        Assert.Throws<InvalidDataException>(() => RasterImageLoader.LoadRgba16FromPng(stream));
    }

    [Test]
    public void LoadFromPng_WithUnknownCriticalChunk_Throws()
    {
        using var stream = new MemoryStream(CreatePng(new byte[9], "critical"));

        Assert.Throws<NotSupportedException>(() => RasterImageLoader.LoadRgba16FromPng(stream));
    }

    private static byte[] CreatePng(byte[] scanlines, string? defect = null, byte depth = 16, byte colorType = 6)
    {
        using var png = new MemoryStream();
        png.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        byte[] header = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(0), defect == "dimensions" ? uint.MaxValue : 1u);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4), 1);
        header[8] = depth;
        header[9] = colorType;
        header[10] = defect == "header" ? (byte)1 : (byte)0;
        WriteChunk(png, "IHDR", header);
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(scanlines);

        byte[] data = compressed.ToArray();
        if (defect == "adler")
            data[^1] ^= 1;
        if (defect == "zlib")
            data[0] = 0;
        if (defect != "missing-idat")
            WriteChunk(png, "IDAT", data);
        if (defect == "nonconsecutive-idat")
        {
            WriteChunk(png, "tEXt", Encoding.ASCII.GetBytes("Key\0Value"));
            WriteChunk(png, "IDAT", Array.Empty<byte>());
        }
        if (defect == "duplicate-header")
            WriteChunk(png, "IHDR", header);
        if (defect == "critical")
            WriteChunk(png, "ABCD", Array.Empty<byte>());
        WriteChunk(png, "IEND", Array.Empty<byte>());
        byte[] bytes = png.ToArray();
        if (defect == "signature")
            bytes[0] = 0;
        if (defect == "crc")
            bytes[29] ^= 1;
        return bytes;
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        byte[] typeBytes = Encoding.ASCII.GetBytes(type);
        byte[] integer = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(integer, data.Length);
        stream.Write(integer);
        stream.Write(typeBytes);
        stream.Write(data);
        uint crc = uint.MaxValue;
        foreach (byte value in typeBytes.Concat(data))
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 0 ? crc >> 1 : (crc >> 1) ^ 0xedb88320;
        }

        BinaryPrimitives.WriteUInt32BigEndian(integer, crc ^ uint.MaxValue);
        stream.Write(integer);
    }

    private static RasterGeometry CreateGrid()
    {
        return new RasterGeometry(new PointXY(0f, 0f), new VectorXY(2f, 3f), new VectorXYInt(2, 3));
    }
}
