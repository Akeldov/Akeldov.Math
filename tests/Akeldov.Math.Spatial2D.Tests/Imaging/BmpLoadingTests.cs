using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.Buffers.Binary;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class BmpLoadingTests
{
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void LoadFromBmp_AfterGray8Export_PreservesSamplesAndHandlesPadding(int width)
    {
        var values = Enumerable.Range(0, width * 3).Select(i => new Gray8BitColor((byte)(i * 17))).ToArray();
        var source = new Raster<Gray8BitColor>(new VectorXYInt(width, 3), values);
        using var stream = new MemoryStream();
        stream.WriteByte(0xff);
        source.SaveAsBmp(stream);
        long imageEnd = stream.Position;
        stream.WriteByte(0x42);
        stream.Position = 1;

        Raster<Gray8BitColor> loaded = RasterImageLoader.LoadGray8FromBmp(stream);

        Assert.That(loaded.Resolution, Is.EqualTo(source.Resolution));
        Assert.That(loaded.Values, Is.EqualTo(values));
        Assert.That(loaded.Values, Is.Not.SameAs(values));
        Assert.That(stream.CanRead, Is.True);
        Assert.That(stream.Position, Is.EqualTo(imageEnd));
        Assert.That(stream.ReadByte(), Is.EqualTo(0x42));
    }

    [Test]
    public void LoadFromBmp_AfterRgba8Export_PreservesChannelsAlphaAndRows()
    {
        RGBA8BitColor[] values =
        {
            new(0x12, 0x34, 0x56, 0x78), RGBA8BitColor.Transparent,
            RGBA8BitColor.White, new(1, 2, 3, 4),
            new(byte.MaxValue, 0, 0, 0), new(0, 1, 128, byte.MaxValue)
        };
        var source = new Raster<RGBA8BitColor>(new VectorXYInt(2, 3), values);
        using var stream = new MemoryStream();
        stream.WriteByte(0xff);
        source.SaveAsBmp(stream);
        long imageEnd = stream.Position;
        stream.WriteByte(0x42);
        stream.Position = 1;

        Raster<RGBA8BitColor> loaded = RasterImageLoader.LoadRgba8FromBmp(stream);

        Assert.That(loaded.Resolution, Is.EqualTo(source.Resolution));
        Assert.That(loaded.Values, Is.EqualTo(values));
        Assert.That(loaded.Values, Is.Not.SameAs(values));
        Assert.That(stream.CanRead, Is.True);
        Assert.That(stream.Position, Is.EqualTo(imageEnd));
        Assert.That(stream.ReadByte(), Is.EqualTo(0x42));
    }

    [Test]
    public void LoadFromBmp_AfterExportToFile_PreservesGray8AndRgba8AndClosesFiles()
    {
        var gray = new Raster<Gray8BitColor>(new VectorXYInt(2, 2),
            new Gray8BitColor[] { new(0), new(1), new(128), new(255) });
        var rgba = new Raster<RGBA8BitColor>(new VectorXYInt(2, 2),
            new[] { RGBA8BitColor.Red, RGBA8BitColor.Transparent, RGBA8BitColor.Blue, new RGBA8BitColor(1, 2, 3, 4) });
        AssertFileRoundTrip(gray, gray.SaveAsBmp, RasterImageLoader.LoadGray8FromBmp);
        AssertFileRoundTrip(rgba, rgba.SaveAsBmp, RasterImageLoader.LoadRgba8FromBmp);
    }

    [TestCase("gray8-bottom-up.bmp", false)]
    [TestCase("gray8-top-down.bmp", true)]
    [TestCase("gray8-v5.bmp", false)]
    public void LoadFromBmp_WithIndependentGrayFixture_ResolvesPaletteAndRows(string fileName, bool topDown)
    {
        using var stream = new ShortReadStream(ReadFixture(fileName));

        Raster<Gray8BitColor> loaded = RasterImageLoader.LoadGray8FromBmp(stream);

        Assert.That(loaded.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        byte[] expected = fileName == "gray8-v5.bmp"
            ? new byte[] { 17, 54, 91, 128, 165, 202 }
            : topDown ? new byte[] { 128, 17, 54, 17, 54, 91 } : new byte[] { 17, 54, 91, 128, 17, 54 };
        Assert.That(loaded.Values.Select(color => color.Value), Is.EqualTo(expected));
        Assert.That(stream.CanRead, Is.True);
    }

    [TestCase("gray8-bottom-up.bmp")]
    [TestCase("indexed8-color.bmp")]
    public void LoadFromBmp_WithIndexedFixture_ReturnsOpaquePaletteColors(string fileName)
    {
        using var stream = new ShortReadStream(ReadFixture(fileName));

        Raster<RGBA8BitColor> loaded = RasterImageLoader.LoadRgba8FromBmp(stream);

        Assert.That(loaded.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        for (int i = 0; i < 6; i++)
        {
            int index = i % 4;
            byte r = (byte)(17 + index * 37);
            byte g = fileName == "indexed8-color.bmp" ? (byte)(9 + index * 61) : r;
            byte b = fileName == "indexed8-color.bmp" ? (byte)(201 - index * 43) : r;
            Assert.That(loaded.Values[i], Is.EqualTo(new RGBA8BitColor(r, g, b, 255)), $"{fileName}: pixel {i}");
        }
    }

    [Test]
    public void LoadFromBmp_WithTopDownBgraFixture_PreservesAlphaAndNormalizesRows()
    {
        using var stream = new ShortReadStream(ReadFixture("rgba8-top-down.bmp"));

        Raster<RGBA8BitColor> loaded = RasterImageLoader.LoadRgba8FromBmp(stream);

        RGBA8BitColor[] expected =
        {
            new(1, 2, 3, 4), new(128, 255, 0, 127),
            new(0x12, 0x34, 0x56, 0), new(0x56, 0x78, 0x9a, 255)
        };
        Assert.That(loaded.Resolution, Is.EqualTo(new VectorXYInt(2, 2)));
        Assert.That(loaded.Values, Is.EqualTo(expected));
    }

    [TestCase("indexed8-color.bmp")]
    [TestCase("rgba8-top-down.bmp")]
    public void LoadFromBmp_WithUnsupportedGrayscaleFormat_Throws(string fileName)
    {
        using var stream = new MemoryStream(ReadFixture(fileName));

        Assert.Throws<NotSupportedException>(() => RasterImageLoader.LoadGray8FromBmp(stream));
        Assert.That(stream.CanRead, Is.True);
    }

    [Test]
    public void LoadFromBmp_WithInvalidArguments_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RasterImageLoader.LoadGray8FromBmp((Stream)null!));
        Assert.Throws<ArgumentNullException>(() => RasterImageLoader.LoadRgba8FromBmp((string)null!));
        Assert.Throws<ArgumentNullException>(() => RasterImageLoader.LoadGray8FromBmp((string)null!));
        Assert.Throws<ArgumentNullException>(() => RasterImageLoader.LoadRgba8FromBmp((Stream)null!));
        using var unreadable = new MemoryStream();
        unreadable.Dispose();
        Assert.Throws<ArgumentException>(() => RasterImageLoader.LoadRgba8FromBmp(unreadable));
        Assert.Throws<ArgumentException>(() => RasterImageLoader.LoadGray8FromBmp(unreadable));
        Assert.Throws<FileNotFoundException>(() =>
            RasterImageLoader.LoadGray8FromBmp(Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid() + ".bmp")));
    }

    [TestCase("signature")]
    [TestCase("reserved")]
    [TestCase("width")]
    [TestCase("height")]
    [TestCase("oversized")]
    [TestCase("planes")]
    [TestCase("palette")]
    [TestCase("pixel-index")]
    [TestCase("pixel-offset")]
    [TestCase("file-size")]
    [TestCase("image-size")]
    public void LoadFromBmp_WithMalformedData_Throws(string defect)
    {
        byte[] bmp = ReadFixture("gray8-bottom-up.bmp");
        switch (defect)
        {
            case "signature": bmp[0] = 0; break;
            case "reserved": bmp[6] = 1; break;
            case "width": BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(18), 0); break;
            case "height": BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(22), int.MinValue); break;
            case "oversized": BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(18), int.MaxValue); break;
            case "planes": bmp[26] = 0; break;
            case "palette": BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(46), 257); break;
            case "pixel-index": bmp[BinaryPrimitives.ReadInt32LittleEndian(bmp.AsSpan(10))] = 4; break;
            case "pixel-offset": BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(10), 54); break;
            case "file-size": BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(2), 54); break;
            case "image-size": BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(34), 1); break;
        }
        using var stream = new MemoryStream(bmp);

        Assert.Throws<InvalidDataException>(() => RasterImageLoader.LoadGray8FromBmp(stream));
        Assert.That(stream.CanRead, Is.True);
    }

    [TestCase(0)]
    [TestCase(13)]
    [TestCase(53)]
    [TestCase(60)]
    [TestCase(80)]
    [TestCase(-1)]
    public void LoadFromBmp_WithTruncatedData_Throws(int length)
    {
        byte[] bmp = ReadFixture("gray8-bottom-up.bmp");
        using var stream = new ShortReadStream(bmp[..(length < 0 ? bmp.Length - 1 : length)]);

        Assert.Throws<InvalidDataException>(() => RasterImageLoader.LoadRgba8FromBmp(stream));
        Assert.That(stream.CanRead, Is.True);
    }

    [TestCase(14, 12)]
    [TestCase(28, 24)]
    [TestCase(30, 1)]
    public void LoadFromBmp_WithUnsupportedHeaderOrEncoding_Throws(int offset, int value)
    {
        byte[] bmp = ReadFixture("gray8-bottom-up.bmp");
        bmp[offset] = (byte)value;
        using var stream = new MemoryStream(bmp);

        Assert.Throws<NotSupportedException>(() => RasterImageLoader.LoadRgba8FromBmp(stream));
    }

    private static byte[] ReadFixture(string name) =>
        File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "Imaging", "Fixtures", name));

    private static void AssertFileRoundTrip<TValue>(Raster<TValue> source, Action<string> save,
        Func<string, Raster<TValue>> load)
    {
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid() + ".bmp");
        try
        {
            save(path);
            Raster<TValue> loaded = load(path);
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
}
