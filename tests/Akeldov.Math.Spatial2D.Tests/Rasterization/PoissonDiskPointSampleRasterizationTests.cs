using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class PoissonDiskPointSampleRasterizationTests
{
    [Test]
    public void Rasterize_WhenPoissonDiskSamplesAreProvided_MapsNearestSampleAndDistanceToColor()
    {
        var samples = new[]
        {
            new PoissonDiskPointSample(new PointXY(0.5f, 0.5f), 1f),
            new PoissonDiskPointSample(new PointXY(2.5f, 0.5f), 2f)
        };
        var grid = new RasterGeometry(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(3f, 1f),
            resolution: new VectorXYInt(3, 1));

        SpatialRaster<RGBA16BitColor> raster = samples.Rasterize(grid, ToColor);

        Assert.That(raster[0, 0], Is.EqualTo(new RGBA16BitColor(1000, 0, 0, ushort.MaxValue)));
        Assert.That(raster[1, 0], Is.EqualTo(new RGBA16BitColor(1000, 1000, 0, ushort.MaxValue)));
        Assert.That(raster[2, 0], Is.EqualTo(new RGBA16BitColor(2000, 0, 0, ushort.MaxValue)));
    }

    [Test]
    public void Rasterize_WhenPoissonDiskSamplesAreProvided_MapsNearestSampleAndDistanceToGray16()
    {
        var samples = new[]
        {
            new PoissonDiskPointSample(new PointXY(0.5f, 0.5f), 1f),
            new PoissonDiskPointSample(new PointXY(2.5f, 0.5f), 2f)
        };
        var grid = new RasterGeometry(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(3f, 1f),
            resolution: new VectorXYInt(3, 1));

        SpatialRaster<Gray16BitColor> raster = samples.Rasterize(grid, ToGray16);

        Assert.That(raster[0, 0].Value, Is.EqualTo(1000));
        Assert.That(raster[1, 0].Value, Is.EqualTo(1100));
        Assert.That(raster[2, 0].Value, Is.EqualTo(2000));
    }

    [Test]
    public void Rasterize_WhenPoissonDiskSamplesAreRenderedAsRings_OverlaysPointsAndMinimalDistanceRings()
    {
        var samples = new[]
        {
            new PoissonDiskPointSample(new PointXY(0.5f, 0.5f), 1f)
        };
        var grid = new RasterGeometry(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(3f, 1f),
            resolution: new VectorXYInt(3, 1));
        var rasterizer = new PoissonDiskPointSampleCollectionRingsGray16BitRasterizer(
            pointRadius: 0.2f,
            ringThickness: 0.2f,
            backgroundGrayLevel: new Gray16BitColor(10),
            ringGrayLevel: new Gray16BitColor(100),
            pointGrayLevel: new Gray16BitColor(200));

        SpatialRaster<Gray16BitColor> raster = samples.Rasterize(grid, rasterizer);

        Assert.That(raster[0, 0].Value, Is.EqualTo(200));
        Assert.That(raster[1, 0].Value, Is.EqualTo(100));
        Assert.That(raster[2, 0].Value, Is.EqualTo(10));
    }

    [Test]
    public void Constructor_WhenRGBA16ColorMapperIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PoissonDiskPointSampleCollectionDistanceRGBA16BitRasterizer(null!));
    }

    [Test]
    public void Constructor_WhenGray16MapperIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PoissonDiskPointSampleCollectionDistanceGray16BitRasterizer(null!));
    }

    [Test]
    public void Constructor_WhenRingRasterizerMetricsAreInvalid_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PoissonDiskPointSampleCollectionRingsGray16BitRasterizer(0f, 0.2f, new(0), new(1), new(2)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PoissonDiskPointSampleCollectionRingsGray16BitRasterizer(0.2f, 0f, new(0), new(1), new(2)));
    }

    [Test]
    public void Rasterize_WhenSourceIsEmpty_Throws()
    {
        var rasterizer = new PoissonDiskPointSampleCollectionDistanceRGBA16BitRasterizer(ToColor);

        var exception = Assert.Throws<ArgumentException>(() =>
            rasterizer.Rasterize(Array.Empty<PoissonDiskPointSample>(), CreateGrid()));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Rasterize_WhenSourceContainsDefaultSample_Throws()
    {
        var rgbaRasterizer = new PoissonDiskPointSampleCollectionDistanceRGBA16BitRasterizer(ToColor);
        var grayRasterizer = new PoissonDiskPointSampleCollectionDistanceGray16BitRasterizer(ToGray16);
        var ringsRasterizer = new PoissonDiskPointSampleCollectionRingsGray16BitRasterizer(0.2f, 0.1f, new(0), new(1), new(2));
        var samples = new[] { default(PoissonDiskPointSample) };

        var exception = Assert.Throws<ArgumentException>(() =>
            rgbaRasterizer.Rasterize(samples, CreateGrid()));
        Assert.That(exception!.ParamName, Is.EqualTo("source"));

        exception = Assert.Throws<ArgumentException>(() =>
            grayRasterizer.Rasterize(samples, CreateGrid()));
        Assert.That(exception!.ParamName, Is.EqualTo("source"));

        exception = Assert.Throws<ArgumentException>(() =>
            ringsRasterizer.Rasterize(samples, CreateGrid()));
        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Rasterize_WhenGridHasDefaultValue_Throws()
    {
        var rgbaRasterizer = new PoissonDiskPointSampleCollectionDistanceRGBA16BitRasterizer(ToColor);
        var grayRasterizer = new PoissonDiskPointSampleCollectionDistanceGray16BitRasterizer(ToGray16);
        var ringsRasterizer = new PoissonDiskPointSampleCollectionRingsGray16BitRasterizer(0.2f, 0.1f, new(0), new(1), new(2));
        var samples = new[] { new PoissonDiskPointSample(new PointXY(0f, 0f), 1f) };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            rgbaRasterizer.Rasterize(samples, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            grayRasterizer.Rasterize(samples, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ringsRasterizer.Rasterize(samples, default));
    }

    [Test]
    public void RasterizeGray16_WhenSourceIsEmpty_Throws()
    {
        var rasterizer = new PoissonDiskPointSampleCollectionDistanceGray16BitRasterizer(ToGray16);

        var exception = Assert.Throws<ArgumentException>(() =>
            rasterizer.Rasterize(Array.Empty<PoissonDiskPointSample>(), CreateGrid()));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void RasterizeRings_WhenSourceIsEmpty_Throws()
    {
        var rasterizer = new PoissonDiskPointSampleCollectionRingsGray16BitRasterizer(0.2f, 0.1f, new(0), new(1), new(2));

        var exception = Assert.Throws<ArgumentException>(() =>
            rasterizer.Rasterize(Array.Empty<PoissonDiskPointSample>(), CreateGrid()));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    private static RasterGeometry CreateGrid()
    {
        return new RasterGeometry(new PointXY(0f, 0f), new VectorXY(1f, 1f), new VectorXYInt(1, 1));
    }

    private static RGBA16BitColor ToColor(PoissonDiskPointSample sample, float distance)
    {
        return new RGBA16BitColor(
            (ushort)MathF.Round(sample.MinimalDistance * 1000f),
            (ushort)MathF.Round(distance * 1000f),
            0,
            ushort.MaxValue);
    }

    private static Gray16BitColor ToGray16(PoissonDiskPointSample sample, float distance)
    {
        return new Gray16BitColor((ushort)MathF.Round(sample.MinimalDistance * 1000f + distance * 100f));
    }
}
