using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class GrayRasterTests
{
    [Test]
    public void Gray8BitRaster_WhenSourceBufferChanges_ReflectsMutation()
    {
        byte[] values = { 1, 2, 3, 4 };
        var raster = new Raster<byte>(CreateGrid(), values);

        values[1] = 9;

        Assert.That(raster[1, 0], Is.EqualTo(9));
        Assert.That(raster.Values[1], Is.EqualTo(9));
    }

    [Test]
    public void Gray8BitRasterClone_WhenCloneBufferChanges_DoesNotChangeSource()
    {
        byte[] values = { 1, 2, 3, 4 };
        var raster = new Raster<byte>(CreateGrid(), values);

        var clone = raster.Clone();
        clone[1, 0] = 9;

        Assert.That(raster[1, 0], Is.EqualTo(2));
        Assert.That(clone[1, 0], Is.EqualTo(9));
    }

    [Test]
    public void Gray8BitRaster_WhenValueCountDoesNotMatchGrid_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Raster<byte>(CreateGrid(), new byte[3]));
    }

    [Test]
    public void Gray8BitRasterIndexer_WhenCoordinatesAreUsed_MapsToRowMajorValue()
    {
        var raster = new Raster<byte>(CreateGrid(), new byte[4]);

        raster[1, 0] = 9;

        Assert.That(raster[1, 0], Is.EqualTo(9));
        Assert.That(raster.Values[1], Is.EqualTo(9));
    }

    [Test]
    public void SaveAsPng_WhenRasterIsGray8Bit_WritesPng8()
    {
        byte[] values = { 0x12, 0x56, 0x34, 0x78 };
        var raster = new Raster<byte>(CreateGrid(), values);
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
        var raster = new Raster<byte>(CreateGrid(), new byte[] { 0x12, 0x56, 0x34, 0x78 });
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
        var raster = new Raster<byte>(CreateGrid(), new byte[] { 0x12, 0x56, 0x34, 0x78 });
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
        ushort[] values = { 1, 2, 3, 4 };
        var raster = new Raster<ushort>(CreateGrid(), values);

        values[1] = 9;

        Assert.That(raster[1, 0], Is.EqualTo(9));
        Assert.That(raster.Values[1], Is.EqualTo(9));
    }

    [Test]
    public void Gray16BitRasterClone_WhenCloneBufferChanges_DoesNotChangeSource()
    {
        ushort[] values = { 1, 2, 3, 4 };
        var raster = new Raster<ushort>(CreateGrid(), values);

        var clone = raster.Clone();
        clone[1, 0] = 9;

        Assert.That(raster[1, 0], Is.EqualTo(2));
        Assert.That(clone[1, 0], Is.EqualTo(9));
    }

    [Test]
    public void Gray16BitRaster_WhenValueCountDoesNotMatchGrid_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Raster<ushort>(CreateGrid(), new ushort[3]));
    }

    [Test]
    public void Gray16BitRasterIndexer_WhenCoordinatesAreUsed_MapsToRowMajorValue()
    {
        var raster = new Raster<ushort>(CreateGrid(), new ushort[4]);

        raster[1, 0] = 9;

        Assert.That(raster[1, 0], Is.EqualTo(9));
        Assert.That(raster.Values[1], Is.EqualTo(9));
    }

    [Test]
    public void SaveAsPng_WhenGray16BitStreamIsProvided_WritesPng16()
    {
        var raster = new Raster<ushort>(CreateGrid(), new ushort[] { 0x1234, 0x5678, 0x9abc, 0xdef0 });
        using var stream = new MemoryStream();

        raster.SaveAsPng(stream);

        byte[] bytes = stream.ToArray();
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(16));
        Assert.That(bytes[25], Is.EqualTo(0));
    }

    private static SpatialRasterGrid CreateGrid()
    {
        return new SpatialRasterGrid(new PointXY(0f, 0f), VectorXY.One, new VectorXYInt(2, 2));
    }
}
