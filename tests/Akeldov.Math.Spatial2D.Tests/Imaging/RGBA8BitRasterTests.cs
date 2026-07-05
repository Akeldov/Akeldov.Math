using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class RGBA8BitRasterTests
{
    [Test]
    public void RGBA8BitRaster_WhenSourceBufferChanges_ReflectsMutation()
    {
        var values = new RGBA8BitColor[6];
        var raster = new Raster<RGBA8BitColor>(CreateGrid(), values);
        var color = new RGBA8BitColor(1, 2, 3, 4);

        values[5] = color;

        Assert.That(raster[1, 2], Is.EqualTo(color));
        Assert.That(raster.Values[5], Is.EqualTo(color));
    }

    [Test]
    public void RGBA8BitRasterClone_WhenCloneBuffersChange_DoesNotChangeSource()
    {
        var raster = new Raster<RGBA8BitColor>(CreateGrid(), new RGBA8BitColor[6]);

        var clone = raster.Clone();
        var color = new RGBA8BitColor(1, 2, 3, 4);

        clone[1, 2] = color;

        Assert.That(raster[1, 2], Is.EqualTo(default(RGBA8BitColor)));
        Assert.That(clone[1, 2], Is.EqualTo(color));
    }

    [Test]
    public void RGBA8BitRaster_WhenValueCountDoesNotMatchGrid_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Raster<RGBA8BitColor>(CreateGrid(), new RGBA8BitColor[5]));
    }

    [Test]
    public void RGBA8BitRasterIndexer_WhenCoordinatesAreUsed_MapsToRowMajorValue()
    {
        var raster = new Raster<RGBA8BitColor>(CreateGrid(), new RGBA8BitColor[6]);
        var color = new RGBA8BitColor(1, 2, 3, 4);

        raster[1, 2] = color;

        Assert.That(raster.Values[5], Is.EqualTo(color));
    }

    [Test]
    public void SaveAsPng_WhenRasterIsRGBA8Bit_WritesPng8WithAlpha()
    {
        Raster<RGBA8BitColor> raster = CreateRasterWithFirstPixel();
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
        Raster<RGBA8BitColor> raster = CreateRasterWithFirstPixel();
        using var stream = new MemoryStream();

        raster.SaveAsPng(stream);

        byte[] bytes = stream.ToArray();
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(8));
        Assert.That(bytes[25], Is.EqualTo(6));
    }

    [Test]
    public void SaveAsBmp_WhenRasterIsRGBA8Bit_WritesBmp32WithBgraPixels()
    {
        Raster<RGBA8BitColor> raster = CreateRasterWithFirstPixel();
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
    public void SaveAsBmp_WhenRGBA8BitStreamIsProvided_WritesBmp32WithBgraPixels()
    {
        Raster<RGBA8BitColor> raster = CreateRasterWithFirstPixel();
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

    private static Raster<RGBA8BitColor> CreateRasterWithFirstPixel()
    {
        var values = new RGBA8BitColor[6];
        values[0] = new RGBA8BitColor(0x12, 0x34, 0x56, 0x78);

        return new Raster<RGBA8BitColor>(CreateGrid(), values);
    }

    private static SpatialRasterGrid CreateGrid()
    {
        return new SpatialRasterGrid(new PointXY(0f, 0f), new VectorXY(2f, 3f), new VectorXYInt(2, 3));
    }
}
