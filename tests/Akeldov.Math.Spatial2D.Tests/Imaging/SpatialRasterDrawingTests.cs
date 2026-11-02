using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class SpatialRasterDrawingTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void DrawDashedLine_MatchesSeparatedStrokesAndShortensLastDash(bool vertical)
    {
        var actual = CreateRaster();
        var expected = CreateRaster();
        var color = new RGBA8BitColor(200, 60, 30, 128);
        PointXY At(float distance) => vertical ? new PointXY(11, 19 + distance) : new PointXY(9 + distance, 21);
        actual.DrawDashedLine(At(0), At(5), 0.5f, 2, color);
        expected.DrawLine(At(0), At(2), 0.5f, color);
        expected.DrawLine(At(4), At(5), 0.5f, color);
        Assert.That(actual.Values, Is.EqualTo(expected.Values));
        Assert.That(actual.Values.Any(pixel => pixel.A > 0), Is.True);
    }

    [Test]
    public void DrawDashedLine_LongDashMatchesSolidLine()
    {
        var actual = CreateRaster();
        var expected = CreateRaster();
        var color = new RGBA8BitColor(200, 60, 30, 255);
        var start = new PointXY(10, 20);
        var end = new PointXY(18, 24);
        actual.DrawDashedLine(start, end, 1, float.MaxValue, color);
        expected.DrawLine(start, end, 1, color);
        Assert.That(actual.Values, Is.EqualTo(expected.Values));
    }

    [Test]
    public void DrawDashedLine_ZeroLengthOrTransparentColor_DoesNotDraw()
    {
        var raster = CreateRaster();
        var point = new PointXY(11, 21);
        raster.DrawDashedLine(point, point, 1, 1, new RGBA8BitColor(255, 0, 0, 255));
        raster.DrawDashedLine(point, new PointXY(17, 23), 1, 1, default);
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.Epsilon)]
    public void DrawDashedLine_InvalidDash_ThrowsWithoutChangingRaster(float dash)
    {
        var raster = CreateRaster();
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawDashedLine(new PointXY(10, 20), new PointXY(18, 24), 1, dash, default));
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }

    [Test]
    public void DrawDashedLine_InvalidInputs_Throw()
    {
        var raster = CreateRaster();
        Assert.Throws<ArgumentNullException>(() => SpatialRasterDrawingExtensions.DrawDashedLine(null!, default, default, 1, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawDashedLine(new PointXY(float.NaN, 0), default, 1, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawDashedLine(default, new PointXY(0, float.PositiveInfinity), 1, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawDashedLine(default, default, 0, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawDashedLine(new PointXY(-float.MaxValue, 0), new PointXY(float.MaxValue, 0), 1, 1, default));
    }

    private static SpatialRaster<RGBA8BitColor> CreateRaster() => new(
        new RasterGeometry(new PointXY(10, 20), new VectorXY(8, 4), new VectorXYInt(4, 4)),
        new RGBA8BitColor[16]);

    [Test]
    public void DrawLine_UsesWorldCoordinatesAndRectangularCells()
    {
        var raster = CreateRaster();
        var buffer = raster.Values;
        var color = new RGBA8BitColor(255, 0, 0, 255);
        raster.DrawLine(new PointXY(8, 20.5f), new PointXY(20, 20.5f), 1, color);
        Assert.That(raster.Values, Is.SameAs(buffer));
        for (int x = 0; x < 4; x++)
        {
            Assert.That(raster[x, 0], Is.EqualTo(color));
            Assert.That(raster[x, 1], Is.EqualTo(default(RGBA8BitColor)));
        }
    }

    [Test]
    public void DrawLine_OutsideBounds_DoesNotPaintBorder()
    {
        var raster = CreateRaster();
        raster.DrawLine(new PointXY(-10, 20), new PointXY(-10, 24), 1, new RGBA8BitColor(255, 0, 0, 255));
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }

    [Test]
    public void DrawLine_CompositesOverTransparentAndTranslucentPixels()
    {
        var raster = CreateRaster();
        raster[1, 0] = new RGBA8BitColor(0, 0, 255, 128);
        raster.DrawLine(new PointXY(8, 20.5f), new PointXY(20, 20.5f), 1, new RGBA8BitColor(255, 0, 0, 128));
        Assert.That(raster[0, 0], Is.EqualTo(new RGBA8BitColor(255, 0, 0, 128)));
        Assert.That(raster[1, 0], Is.EqualTo(new RGBA8BitColor(170, 0, 85, 192)));
    }

    [Test]
    public void DrawLine_ZeroLength_HasPartialCoverageAndRoundBoundary()
    {
        var raster = CreateRaster();
        raster.DrawLine(new PointXY(11, 20.5f), new PointXY(11, 20.5f), 1, new RGBA8BitColor(255, 0, 0, 255));
        Assert.That(raster[0, 0].A, Is.InRange(1, 254));
        Assert.That(raster[1, 0].A, Is.Zero);
    }

    [Test]
    public void DrawLine_ReversingDiagonalEndpoints_ProducesSameImage()
    {
        var a = CreateRaster();
        var b = CreateRaster();
        var start = new PointXY(9, 19);
        var end = new PointXY(19, 25);
        var color = new RGBA8BitColor(30, 100, 200, 150);
        a.DrawLine(start, end, 1, color);
        b.DrawLine(end, start, 1, color);
        Assert.That(a.Values, Is.EqualTo(b.Values));
        Assert.That(a.Values.Any(pixel => pixel.A > 0), Is.True);
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void DrawLine_InvalidWidth_Throws(float width)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateRaster().DrawLine(default, default, width, default));
    }

    [Test]
    public void DrawLine_InvalidInputs_ThrowBeforeDrawing()
    {
        Assert.Throws<ArgumentNullException>(() => SpatialRasterDrawingExtensions.DrawLine(null!, default, default, 1, default));
        var raster = CreateRaster();
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawLine(new PointXY(float.NaN, 0), default, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawLine(default, new PointXY(0, float.PositiveInfinity), 1, default));
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }
}
