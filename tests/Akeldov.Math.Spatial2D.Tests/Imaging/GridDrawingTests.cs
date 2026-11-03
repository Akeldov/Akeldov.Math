using System.Globalization;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class GridDrawingTests
{
    private static SpatialRaster<RGBA8BitColor> CreateRaster() => new(
        new RasterGeometry(new PointXY(-3, -2), new VectorXY(6, 4), new VectorXYInt(24, 16)),
        new RGBA8BitColor[24 * 16]);

    [Test]
    public void DrawGrid_UsesWorldCoordinatesAndDoesNotDarkenCrossings()
    {
        var raster = CreateRaster();
        var color = new RGBA8BitColor(30, 100, 200, 128);
        raster.DrawGrid(new VectorXY(2, 1), 0.5f, color);
        Assert.That(raster[4, 12], Is.EqualTo(color), "Crossing at negative X.");
        Assert.That(raster[12, 8], Is.EqualTo(color), "Crossing at world zero.");
        Assert.That(raster[7, 10].A, Is.Zero, "Cell between grid lines.");
        Assert.That(raster.Values.Max(p => p.A), Is.EqualTo(128));
    }

    [Test]
    public void DrawGrid_MajorLinesTakePrecedenceInBothDirections()
    {
        var raster = CreateRaster();
        var minor = new RGBA8BitColor(255, 0, 0, 128);
        var major = new RGBA8BitColor(0, 0, 255, 128);
        raster.DrawGrid(new VectorXY(1, 1), 0.5f, minor, majorEvery: 2, majorColor: major);
        Assert.That(raster[4, 12], Is.EqualTo(major), "X=-2 is major even where it meets a minor Y line.");
        Assert.That(raster[8, 12], Is.EqualTo(minor), "X=-1, Y=1 are both minor.");
        Assert.That(raster[12, 8], Is.EqualTo(major), "World zero is major.");
        Assert.That(raster.Values.Max(p => p.A), Is.EqualTo(128));
    }

    [Test]
    public void DrawGrid_ClipsThickLinesToBoundsWithSampleCoverage()
    {
        var raster = CreateRaster();
        var rectangle = new Rectangle(new PointXY(-0.875f, -0.5f), new PointXY(0.875f, 0.5f));
        raster.DrawGrid(new VectorXY(1, 1), 10, RGBA8BitColor.Black, bounds: rectangle);
        Assert.That(raster[8, 8].A, Is.EqualTo(128), "Half a cell is inside the bounds.");
        Assert.That(raster[12, 8].A, Is.EqualTo(255));
        Assert.That(raster[7, 8].A, Is.Zero);
        Assert.That(raster[12, 5].A, Is.Zero);
        Assert.That(raster[12, 10].A, Is.Zero);
    }

    [Test]
    public void DrawGrid_AntialiasedCrossingUsesMajorColorOnlyAtSharedSamples()
    {
        var raster = CreateRaster();
        raster.DrawGrid(new VectorXY(1, 1), 0.125f, new RGBA8BitColor(255, 0, 0, 128),
            majorEvery: 2, majorColor: new RGBA8BitColor(0, 0, 255, 128));
        // Four blue samples at X=0, plus three red samples at Y=1: seven samples total.
        Assert.That(raster[12, 12], Is.EqualTo(new RGBA8BitColor(109, 0, 146, 56)));
    }

    [Test]
    public void DrawGrid_OffscreenLineCenterCanCoverViewport()
    {
        var raster = new SpatialRaster<RGBA8BitColor>(
            new RasterGeometry(new PointXY(0.1f, 0.1f), new VectorXY(0.2f, 0.2f), new VectorXYInt(2, 2)),
            new RGBA8BitColor[4]);
        raster.DrawGrid(new VectorXY(1, 1), 1, RGBA8BitColor.Black,
            bounds: new Rectangle(new PointXY(-1, -1), new PointXY(1, 1)));
        Assert.That(raster.Values, Is.All.EqualTo(RGBA8BitColor.Black));
    }

    [Test]
    public void DrawGrid_OverloadsAgreeAndDenseLinesDoNotAccumulateAlpha()
    {
        var a = CreateRaster();
        var b = CreateRaster();
        var c = CreateRaster();
        var color = new RGBA8BitColor(40, 90, 150, 128);
        a.DrawGrid(0.1f, 0.5f, color);
        b.DrawGrid(new VectorXY(0.1f, 0.1f), 0.5f, color);
        c.DrawGrid(new VectorXY(0.1f, 0.1f), 0.5f, color,
            bounds: new Rectangle(new PointXY(-3, -2), new PointXY(3, 2)));
        Assert.That(a.Values, Is.EqualTo(b.Values));
        Assert.That(a.Values, Is.EqualTo(c.Values));
        Assert.That(a.Values, Is.All.EqualTo(color));
    }

    [Test]
    public void DrawGrid_EmptyBoundsAndTransparentColorsPreserveRaster()
    {
        var raster = CreateRaster();
        raster.DrawGrid(1, 0.5f, default);
        raster.DrawGrid(new VectorXY(1, 1), 0.5f, RGBA8BitColor.Black, bounds: default(Rectangle));
        raster.DrawGrid(new VectorXY(1, 1), 0.5f, RGBA8BitColor.Black,
            bounds: new Rectangle(new PointXY(100, 100), new PointXY(101, 101)));
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }

    [Test]
    public void DrawGrid_LabelsUseInvariantCoordinatesAndStayInsideBounds()
    {
        SpatialRaster<RGBA8BitColor> Create() => new(
            new RasterGeometry(new PointXY(-5, -5), new VectorXY(10, 10), new VectorXYInt(100, 100)),
            new RGBA8BitColor[10000]);
        var a = Create();
        var b = Create();
        var bounds = new Rectangle(new PointXY(-4, -4), new PointXY(4, 4));
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            a.DrawGrid(new VectorXY(1.5f, 1.5f), 0.1f, default,
                labelHeight: 0.7f, labelColor: RGBA8BitColor.Black, bounds: bounds);
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            b.DrawGrid(new VectorXY(1.5f, 1.5f), 0.1f, default,
                labelHeight: 0.7f, labelColor: RGBA8BitColor.Black, bounds: bounds);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
        Assert.That(a.Values, Is.EqualTo(b.Values));
        Assert.That(a.Values.Any(p => p.A > 0), Is.True);
        for (int y = 0; y < 100; y++)
            for (int x = 0; x < 100; x++)
                if (x < 10 || x >= 90 || y < 10 || y >= 90)
                    Assert.That(a[x, y].A, Is.Zero);
    }

    [Test]
    public void DrawGrid_InvalidArgumentsThrow()
    {
        var raster = CreateRaster();
        Assert.Throws<ArgumentNullException>(() => SpatialRasterDrawingExtensions.DrawGrid(null!, 1, 1, default));
        foreach (float value in new[] { 0f, -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawGrid(value, 1, default));
            Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawGrid(new VectorXY(1, value), 1, default));
            Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawGrid(1, value, default));
        }
        foreach (float value in new[] { -1f, float.NaN, float.PositiveInfinity })
            Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawGrid(new VectorXY(1, 1), 1, default, labelHeight: value));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawGrid(new VectorXY(1, 1), 1, default, majorEvery: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawGrid(float.Epsilon, 1, default));
    }
}
