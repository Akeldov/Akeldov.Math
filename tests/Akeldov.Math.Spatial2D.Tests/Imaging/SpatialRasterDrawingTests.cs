using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Regions;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class SpatialRasterDrawingTests
{
    [TestCase(0f)]
    [TestCase(2f)]
    public void DrawContour_PointsAndContourAgreeAndCloseBoundary(float dash)
    {
        var points = new[] { new PointXY(10, 20), new PointXY(18, 20), new PointXY(18, 24), new PointXY(10, 24) };
        var a = CreateRaster();
        var b = CreateRaster();
        var color = new RGBA8BitColor(255, 0, 0, 128);
        a.DrawContour(points, 1, color, dash);
        b.DrawContour(new CompositeContour(points), 1, color, dash);
        Assert.That(a.Values, Is.EqualTo(b.Values));
        Assert.That(a.Values.Any(p => p.A > 0), Is.True);
        Assert.That(a.Values.All(p => p.A <= 128), Is.True, "Overlapping strokes must not accumulate opacity.");
        if (dash == 0)
        {
            Assert.That(a[0, 1].A, Is.GreaterThan(0), "Closing edge is drawn.");
            Assert.That(a[1, 1].A, Is.Zero, "Interior is not filled.");
        }
    }

    [Test]
    public void DrawContour_DashesLeaveGapsOnRectangle()
    {
        var raster = new SpatialRaster<RGBA8BitColor>(
            new RasterGeometry(new PointXY(0, 0), new VectorXY(10, 10), new VectorXYInt(20, 20)), new RGBA8BitColor[400]);
        raster.DrawContour(new RectangleContour(new PointXY(1, 1), new PointXY(9, 9)), 0.5f, new RGBA8BitColor(255, 0, 0, 255), 2);
        Assert.That(raster.Values.Any(p => p.A > 0), Is.True);
        Assert.That(raster[8, 2].A, Is.Zero, "Middle of first gap along bottom edge.");
        Assert.That(raster[4, 2].A, Is.GreaterThan(0), "First bottom-edge dash.");
    }

    [TestCase(0f)]
    [TestCase(1f)]
    public void DrawContour_CircleDrawsBoundaryWithoutFillingCenter(float dash)
    {
        var raster = CreateRaster();
        raster.DrawContour(new Circle(new PointXY(13, 21.5f), 1.5f), 0.5f, new RGBA8BitColor(255, 0, 0, 255), dash);
        Assert.That(raster[1, 1].A, Is.Zero);
        Assert.That(raster.Values.Any(p => p.A > 0), Is.True);
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void DrawContour_InvalidDashThrows(float dash)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateRaster().DrawContour(new Circle(default, 1), 1, default, dash));
    }

    [Test]
    public void DrawContour_InvalidInputsThrow()
    {
        var raster = CreateRaster();
        Assert.Throws<ArgumentNullException>(() => raster.DrawContour((IContour)null!, 1, default));
        Assert.Throws<ArgumentNullException>(() => raster.DrawContour((PointXY[])null!, 1, default));
        Assert.Throws<ArgumentException>(() => raster.DrawContour(new[] { default(PointXY) }, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawContour(new Circle(default, 1), 0, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawContour(new Circle(default, 1), 1, default, float.Epsilon));
    }

    [Test]
    public void FillRectangle_UsesWorldCoordinatesAndExactCellCoverage()
    {
        var raster = CreateRaster();
        var buffer = raster.Values;
        raster.FillRectangle(new PointXY(11, 20.5f), new PointXY(14, 22), new RGBA8BitColor(255, 0, 0, 255));
        Assert.That(raster.Values, Is.SameAs(buffer));
        Assert.That(raster[0, 0], Is.EqualTo(new RGBA8BitColor(255, 0, 0, 64)));
        Assert.That(raster[1, 0].A, Is.EqualTo(128));
        Assert.That(raster[0, 1].A, Is.EqualTo(128));
        Assert.That(raster[1, 1].A, Is.EqualTo(255));
        Assert.That(raster[2, 1].A, Is.Zero);
        Assert.That(raster[1, 2].A, Is.Zero);
    }

    [Test]
    public void FillRectangle_RectangleOverloadClipsAndComposites()
    {
        var raster = CreateRaster();
        raster[0, 0] = new RGBA8BitColor(0, 0, 255, 128);
        raster.FillRectangle(new Rectangle(new PointXY(14, 22), new PointXY(8, 18)), new RGBA8BitColor(255, 0, 0, 128));
        Assert.That(raster[0, 0], Is.EqualTo(new RGBA8BitColor(170, 0, 85, 192)));
        Assert.That(raster[1, 1], Is.EqualTo(new RGBA8BitColor(255, 0, 0, 128)));
        Assert.That(raster[2, 1].A, Is.Zero);
    }

    [Test]
    public void FillRectangle_ReversedCornersProduceSameImage()
    {
        var a = CreateRaster();
        var b = CreateRaster();
        var color = new RGBA8BitColor(50, 100, 150, 200);
        a.FillRectangle(new PointXY(11, 20.25f), new PointXY(17, 23.75f), color);
        b.FillRectangle(new PointXY(17, 23.75f), new PointXY(11, 20.25f), color);
        Assert.That(a.Values, Is.EqualTo(b.Values));
    }

    [TestCase(0, 20, 9, 24)]
    [TestCase(19, 20, 25, 24)]
    [TestCase(10, 0, 18, 19)]
    [TestCase(10, 25, 18, 30)]
    [TestCase(12, 21, 12, 23)]
    [TestCase(12, 21, 16, 21)]
    public void FillRectangle_OutsideOrZeroAreaDoesNotDraw(float x0, float y0, float x1, float y1)
    {
        var raster = CreateRaster();
        raster.FillRectangle(new PointXY(x0, y0), new PointXY(x1, y1), new RGBA8BitColor(255, 0, 0, 255));
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }

    [Test]
    public void FillRectangle_TransparentFillPreservesPixels()
    {
        var raster = CreateRaster();
        raster[0, 0] = new RGBA8BitColor(10, 20, 30, 40);
        var before = raster.Values.ToArray();
        raster.FillRectangle(new PointXY(10, 20), new PointXY(18, 24), default);
        Assert.That(raster.Values, Is.EqualTo(before));
    }

    [Test]
    public void FillRectangle_InvalidInputsThrow()
    {
        Assert.Throws<ArgumentNullException>(() => SpatialRasterDrawingExtensions.FillRectangle(null!, default(Rectangle), default));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateRaster().FillRectangle(new PointXY(float.NaN, 0), default, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateRaster().FillRectangle(default, new PointXY(0, float.PositiveInfinity), default));
    }

    [Test]
    public void FillCircle_UsesWorldCoordinatesAndAntialiasesRectangularCells()
    {
        var raster = CreateRaster();
        var values = raster.Values;
        raster.FillCircle(new Circle(new PointXY(13, 21.5f), 1.25f), new RGBA8BitColor(255, 0, 0, 255));
        Assert.That(raster.Values, Is.SameAs(values));
        Assert.That(raster[1, 1], Is.EqualTo(new RGBA8BitColor(255, 0, 0, 255)));
        Assert.That(raster[1, 0].A, Is.InRange(1, 254));
        Assert.That(raster[1, 2], Is.EqualTo(raster[1, 0]));
        Assert.That(raster[0, 1], Is.EqualTo(raster[2, 1]));
        Assert.That(raster[3, 1].A, Is.Zero);
        Assert.That(raster[1, 3].A, Is.Zero);
    }

    [Test]
    public void FillCircle_CompositesAndClipsLargeCircle()
    {
        var raster = CreateRaster();
        raster[1, 1] = new RGBA8BitColor(0, 0, 255, 128);
        raster.FillCircle(new PointXY(14, 22), float.MaxValue, new RGBA8BitColor(255, 0, 0, 128));
        Assert.That(raster[0, 0], Is.EqualTo(new RGBA8BitColor(255, 0, 0, 128)));
        Assert.That(raster[1, 1], Is.EqualTo(new RGBA8BitColor(170, 0, 85, 192)));
        Assert.That(raster[3, 3], Is.EqualTo(raster[0, 0]));
    }

    [Test]
    public void FillCircle_OutsideZeroRadiusOrTransparent_DoesNotDraw()
    {
        var raster = CreateRaster();
        var color = new RGBA8BitColor(255, 0, 0, 255);
        raster.FillCircle(new PointXY(-10, 22), 1, color);
        raster.FillCircle(new PointXY(13, 21.5f), 0, color);
        raster.FillCircle(new PointXY(13, 21.5f), 10, default);
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void FillCircle_InvalidRadius_Throws(float radius)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateRaster().FillCircle(default(PointXY), radius, default));
    }

    [Test]
    public void FillCircle_InvalidRasterOrCenter_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SpatialRasterDrawingExtensions.FillCircle(null!, default(Circle), default));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateRaster().FillCircle(new PointXY(float.NaN, 0), 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateRaster().FillCircle(new PointXY(0, float.PositiveInfinity), 1, default));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void DrawDashedSegment_MatchesSeparatedStrokesAndShortensLastDash(bool vertical)
    {
        var actual = CreateRaster();
        var expected = CreateRaster();
        var color = new RGBA8BitColor(200, 60, 30, 128);
        PointXY At(float distance) => vertical ? new PointXY(11, 19 + distance) : new PointXY(9 + distance, 21);
        actual.DrawDashedSegment(new Segment(At(0), At(5)), 0.5f, 2, color);
        expected.DrawSegment(At(0), At(2), 0.5f, color);
        expected.DrawSegment(At(4), At(5), 0.5f, color);
        Assert.That(actual.Values, Is.EqualTo(expected.Values));
        Assert.That(actual.Values.Any(pixel => pixel.A > 0), Is.True);
    }

    [Test]
    public void DrawDashedSegment_LongDashMatchesSolidLine()
    {
        var actual = CreateRaster();
        var expected = CreateRaster();
        var color = new RGBA8BitColor(200, 60, 30, 255);
        var start = new PointXY(10, 20);
        var end = new PointXY(18, 24);
        actual.DrawDashedSegment(start, end, 1, float.MaxValue, color);
        expected.DrawSegment(start, end, 1, color);
        Assert.That(actual.Values, Is.EqualTo(expected.Values));
    }

    [Test]
    public void DrawDashedSegment_ZeroLengthOrTransparentColor_DoesNotDraw()
    {
        var raster = CreateRaster();
        var point = new PointXY(11, 21);
        raster.DrawDashedSegment(point, point, 1, 1, new RGBA8BitColor(255, 0, 0, 255));
        raster.DrawDashedSegment(point, new PointXY(17, 23), 1, 1, default);
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.Epsilon)]
    public void DrawDashedSegment_InvalidDash_ThrowsWithoutChangingRaster(float dash)
    {
        var raster = CreateRaster();
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawDashedSegment(new PointXY(10, 20), new PointXY(18, 24), 1, dash, default));
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }

    [Test]
    public void DrawDashedSegment_InvalidInputs_Throw()
    {
        var raster = CreateRaster();
        Assert.Throws<ArgumentNullException>(() => SpatialRasterDrawingExtensions.DrawDashedSegment(null!, default, default, 1, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawDashedSegment(new PointXY(float.NaN, 0), default, 1, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawDashedSegment(default, new PointXY(0, float.PositiveInfinity), 1, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawDashedSegment(default, default, 0, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawDashedSegment(new PointXY(-float.MaxValue, 0), new PointXY(float.MaxValue, 0), 1, 1, default));
    }

    private static SpatialRaster<RGBA8BitColor> CreateRaster() => new(
        new RasterGeometry(new PointXY(10, 20), new VectorXY(8, 4), new VectorXYInt(4, 4)),
        new RGBA8BitColor[16]);

    [Test]
    public void DrawSegment_UsesWorldCoordinatesAndRectangularCells()
    {
        var raster = CreateRaster();
        var buffer = raster.Values;
        var color = new RGBA8BitColor(255, 0, 0, 255);
        raster.DrawSegment(new Segment(new PointXY(8, 20.5f), new PointXY(20, 20.5f)), 1, color);
        Assert.That(raster.Values, Is.SameAs(buffer));
        for (int x = 0; x < 4; x++)
        {
            Assert.That(raster[x, 0], Is.EqualTo(color));
            Assert.That(raster[x, 1], Is.EqualTo(default(RGBA8BitColor)));
        }
    }

    [Test]
    public void DrawSegment_OutsideBounds_DoesNotPaintBorder()
    {
        var raster = CreateRaster();
        raster.DrawSegment(new PointXY(-10, 20), new PointXY(-10, 24), 1, new RGBA8BitColor(255, 0, 0, 255));
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }

    [Test]
    public void DrawSegment_CompositesOverTransparentAndTranslucentPixels()
    {
        var raster = CreateRaster();
        raster[1, 0] = new RGBA8BitColor(0, 0, 255, 128);
        raster.DrawSegment(new PointXY(8, 20.5f), new PointXY(20, 20.5f), 1, new RGBA8BitColor(255, 0, 0, 128));
        Assert.That(raster[0, 0], Is.EqualTo(new RGBA8BitColor(255, 0, 0, 128)));
        Assert.That(raster[1, 0], Is.EqualTo(new RGBA8BitColor(170, 0, 85, 192)));
    }

    [Test]
    public void DrawSegment_ZeroLength_HasPartialCoverageAndRoundBoundary()
    {
        var raster = CreateRaster();
        raster.DrawSegment(new PointXY(11, 20.5f), new PointXY(11, 20.5f), 1, new RGBA8BitColor(255, 0, 0, 255));
        Assert.That(raster[0, 0].A, Is.InRange(1, 254));
        Assert.That(raster[1, 0].A, Is.Zero);
    }

    [Test]
    public void DrawSegment_ReversingDiagonalEndpoints_ProducesSameImage()
    {
        var a = CreateRaster();
        var b = CreateRaster();
        var start = new PointXY(9, 19);
        var end = new PointXY(19, 25);
        var color = new RGBA8BitColor(30, 100, 200, 150);
        a.DrawSegment(start, end, 1, color);
        b.DrawSegment(end, start, 1, color);
        Assert.That(a.Values, Is.EqualTo(b.Values));
        Assert.That(a.Values.Any(pixel => pixel.A > 0), Is.True);
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void DrawSegment_InvalidWidth_Throws(float width)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateRaster().DrawSegment(default, default, width, default));
    }

    [Test]
    public void DrawSegment_InvalidInputs_ThrowBeforeDrawing()
    {
        Assert.Throws<ArgumentNullException>(() => SpatialRasterDrawingExtensions.DrawSegment(null!, default, default, 1, default));
        var raster = CreateRaster();
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawSegment(new PointXY(float.NaN, 0), default, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawSegment(default, new PointXY(0, float.PositiveInfinity), 1, default));
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }
}
