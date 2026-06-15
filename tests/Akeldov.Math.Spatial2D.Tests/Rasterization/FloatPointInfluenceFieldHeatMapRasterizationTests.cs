using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class FloatPointInfluenceFieldHeatMapRasterizationTests
{
    [Test]
    public void RasterizeHeatMap_WhenFieldValuesAreSampled_MapsValuesToHeatMapColors()
    {
        FloatPointInfluenceField field = CreateNearestField(
            new FloatPointInfluenceSource(1f, new PointXY(0.5f, 0.5f), 0f),
            new FloatPointInfluenceSource(1f, new PointXY(1.5f, 0.5f), 50f),
            new FloatPointInfluenceSource(1f, new PointXY(2.5f, 0.5f), 100f));
        var grid = new RasterGrid(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(3f, 1f),
            resolution: new VectorXYInt(3, 1));

        RGBA16BitRaster raster = field.RasterizeHeatMap(grid);

        Assert.That(raster[0, 0], Is.EqualTo(new RGBA16BitColor(0, 0, 32896, ushort.MaxValue)));
        Assert.That(raster[1, 0], Is.EqualTo(new RGBA16BitColor(32896, ushort.MaxValue, 32896, ushort.MaxValue)));
        Assert.That(raster[2, 0], Is.EqualTo(new RGBA16BitColor(32896, 0, 0, ushort.MaxValue)));
    }

    [Test]
    public void RasterizeHeatMap_WhenFieldRangeIsSingleValue_MapsValueToMiddleHeatMapColor()
    {
        FloatPointInfluenceField field = CreateNearestField(
            new FloatPointInfluenceSource(1f, new PointXY(0.5f, 0.5f), 7f),
            new FloatPointInfluenceSource(1f, new PointXY(1.5f, 0.5f), 7f));
        var grid = new RasterGrid(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(1f, 1f),
            resolution: new VectorXYInt(1, 1));

        RGBA16BitRaster raster = field.RasterizeHeatMap(grid);

        Assert.That(raster[0, 0], Is.EqualTo(new RGBA16BitColor(32896, ushort.MaxValue, 32896, ushort.MaxValue)));
    }

    [Test]
    public void ToHeatMapColor_WhenValueIsBetweenStops_InterpolatesChannels()
    {
        RGBA16BitColor color = FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer.ToHeatMapColor(0.125f);

        Assert.That(color, Is.EqualTo(new RGBA16BitColor(0, 0, 65439, ushort.MaxValue)));
    }

    [Test]
    public void ToHeatMapColor_WhenValueIsOutsideRange_Clamps()
    {
        Assert.That(
            FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer.ToHeatMapColor(-1f),
            Is.EqualTo(new RGBA16BitColor(0, 0, 32896, ushort.MaxValue)));
        Assert.That(
            FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer.ToHeatMapColor(2f),
            Is.EqualTo(new RGBA16BitColor(32896, 0, 0, ushort.MaxValue)));
    }

    [Test]
    public void ToHeatMapColor_WhenValueIsInvalid_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer.ToHeatMapColor(float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer.ToHeatMapColor(float.PositiveInfinity));
    }

    [Test]
    public void Rasterize_WhenSourceIsNull_Throws()
    {
        var rasterizer = new FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer();

        Assert.Throws<ArgumentNullException>(() =>
            rasterizer.Rasterize(null!, CreateGrid()));
    }

    [Test]
    public void Rasterize_WhenGridHasDefaultValue_Throws()
    {
        FloatPointInfluenceField field = CreateNearestField(
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f));
        var rasterizer = new FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            rasterizer.Rasterize(field, default));
    }

    [Test]
    public void Rasterize_WhenFieldRangeIsInvalid_Throws()
    {
        FloatPointInfluenceField field = CreateNearestField(
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), float.PositiveInfinity));
        var rasterizer = new FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer();

        var exception = Assert.Throws<ArgumentException>(() =>
            rasterizer.Rasterize(field, CreateGrid()));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    private static FloatPointInfluenceField CreateNearestField(params FloatPointInfluenceSource[] sources)
    {
        return new FloatPointInfluenceField(
            new NearestFloatInfluenceSampler<FloatPointInfluenceSource>(),
            sources);
    }

    private static RasterGrid CreateGrid()
    {
        return new RasterGrid(new PointXY(0f, 0f), new VectorXY(1f, 1f), new VectorXYInt(1, 1));
    }
}
