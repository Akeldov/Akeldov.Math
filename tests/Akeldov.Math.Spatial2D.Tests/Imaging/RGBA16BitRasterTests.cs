using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class RGBA16BitRasterTests
{
    [Test]
    public void RGBA16BitRaster_WhenSourceBufferChanges_ReflectsMutation()
    {
        var values = new RGBA16BitColor[6];
        var raster = new Raster<RGBA16BitColor>(CreateGrid(), values);
        var color = new RGBA16BitColor(1, 2, 3, 4);

        values[5] = color;

        Assert.That(raster[1, 2], Is.EqualTo(color));
        Assert.That(raster.Values[5], Is.EqualTo(color));
    }

    [Test]
    public void RGBA16BitRasterClone_WhenCloneBuffersChange_DoesNotChangeSource()
    {
        var raster = new Raster<RGBA16BitColor>(CreateGrid(), new RGBA16BitColor[6]);

        var clone = raster.Clone();
        var color = new RGBA16BitColor(1, 2, 3, 4);

        clone[1, 2] = color;

        Assert.That(raster[1, 2], Is.EqualTo(default(RGBA16BitColor)));
        Assert.That(clone[1, 2], Is.EqualTo(color));
    }

    [Test]
    public void RGBA16BitRaster_WhenValueCountDoesNotMatchGrid_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Raster<RGBA16BitColor>(CreateGrid(), new RGBA16BitColor[5]));
    }

    [Test]
    public void RGBA16BitRasterIndexer_WhenCoordinatesAreUsed_MapsToRowMajorValue()
    {
        var raster = new Raster<RGBA16BitColor>(CreateGrid(), new RGBA16BitColor[6]);
        var color = new RGBA16BitColor(1, 2, 3, 4);

        raster[1, 2] = color;

        Assert.That(raster.Values[5], Is.EqualTo(color));
    }

    [Test]
    public void SaveAsPng_WhenRasterIsRGBA16Bit_WritesPng16WithAlpha()
    {
        var values = new RGBA16BitColor[6];
        values[0] = new RGBA16BitColor(0x1234, 0x5678, 0x9abc, 0xdef0);
        var raster = new Raster<RGBA16BitColor>(CreateGrid(), values);
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
        var raster = new Raster<RGBA16BitColor>(CreateGrid(), values);
        using var stream = new MemoryStream();

        raster.SaveAsPng(stream);

        byte[] bytes = stream.ToArray();
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(16));
        Assert.That(bytes[25], Is.EqualTo(6));
    }

    private static SpatialRasterGrid CreateGrid()
    {
        return new SpatialRasterGrid(new PointXY(0f, 0f), new VectorXY(2f, 3f), new VectorXYInt(2, 3));
    }
}
