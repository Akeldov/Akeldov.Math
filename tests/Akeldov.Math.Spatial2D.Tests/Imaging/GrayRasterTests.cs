using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

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
    public void Gray8BitRasterToRaster_ReturnsCallerOwnedRasterCopy()
    {
        Gray8BitColor[] values = { new(1), new(2), new(3), new(4) };
        var raster = new SpatialRaster<Gray8BitColor>(CreateGrid(), values);

        Raster<Gray8BitColor> nonSpatialRaster = raster.ToRaster();
        nonSpatialRaster[1, 0] = new Gray8BitColor(9);

        Assert.That(nonSpatialRaster, Is.TypeOf<Raster<Gray8BitColor>>());
        Assert.That(nonSpatialRaster.Resolution, Is.EqualTo(raster.Geometry.Resolution));
        Assert.That(nonSpatialRaster.Values, Is.Not.SameAs(raster.Values));
        Assert.That(raster[1, 0].Value, Is.EqualTo(2));
        Assert.That(nonSpatialRaster[1, 0].Value, Is.EqualTo(9));
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

    private static RasterGeometry CreateGrid()
    {
        return new RasterGeometry(new PointXY(0f, 0f), VectorXY.One, new VectorXYInt(2, 2));
    }
}
