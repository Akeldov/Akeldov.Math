using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class CurveDrawingTests
{
    private static SpatialRaster<RGBA8BitColor> CreateRaster() => new(
        new RasterGeometry(new PointXY(-2, -2), new VectorXY(4, 4), new VectorXYInt(8, 8)), new RGBA8BitColor[64]);

    [Test]
    public void Draw_LineAndRayRespectDirectionAndRasterBounds()
    {
        var line = CreateRaster();
        var ray = CreateRaster();
        line.Draw(new Line(default(PointXY), 0f), 1, RGBA8BitColor.Black);
        ray.Draw(new Ray(default(PointXY), 0f), 1, RGBA8BitColor.Black);
        Assert.That(line[0, 4].A, Is.EqualTo(255));
        Assert.That(line[7, 4].A, Is.EqualTo(255));
        Assert.That(ray[0, 4].A, Is.Zero);
        Assert.That(ray[7, 4].A, Is.EqualTo(255));
        Assert.That(line[4, 7].A, Is.Zero);
    }

    [TestCase(0f)]
    [TestCase(1.5707964f)]
    [TestCase(0.7f)]
    public void DrawArrow_MatchesUnionOfShaftAndOpenHead(float angle)
    {
        var raster = CreateRaster();
        var viaSegment = CreateRaster();
        var direction = new VectorXY(MathF.Cos(angle), MathF.Sin(angle));
        var start = new PointXY(-direction.X, -direction.Y);
        var end = new PointXY(direction.X, direction.Y);
        var left = new PointXY(-direction.Y * 0.5f, direction.X * 0.5f);
        var right = new PointXY(direction.Y * 0.5f, -direction.X * 0.5f);
        var strokes = new[] { new Segment(start, end), new Segment(end, left), new Segment(end, right) };
        var color = new RGBA8BitColor(30, 100, 200, 128);
        raster.DrawArrow(start, end, 0.5f, 1f, 1f, color);
        viaSegment.DrawArrow(new ParameterizedSegment(start, end), 0.5f, 1f, 1f, color);
        Assert.That(viaSegment.Values, Is.EqualTo(raster.Values));
        var zeroDash = CreateRaster();
        zeroDash.DrawArrow(start, end, 0.5f, 1f, 1f, 0f, color);
        Assert.That(zeroDash.Values, Is.EqualTo(raster.Values));
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int covered = 0;
                for (int sy = 0; sy < 4; sy++)
                    for (int sx = 0; sx < 4; sx++)
                    {
                        var point = new PointXY(-2f + (x + (sx + 0.5f) / 4f) * 0.5f,
                            -2f + (y + (sy + 0.5f) / 4f) * 0.5f);
                        if (strokes.Any(s => s.Distance(point) <= 0.25f))
                            covered++;
                    }
                Assert.That(raster[x, y].A, Is.EqualTo((byte)MathF.Round(128f * covered / 16f)),
                    $"Pixel ({x}, {y}), angle {angle}");
            }
    }

    [TestCase(false, 10f)]
    [TestCase(true, 10f)]
    [TestCase(false, 8.5f)]
    [TestCase(true, 8.5f)]
    public void DrawArrow_DashedShaftAndSolidHeadMatchCoverage(bool reverse, float tipX)
    {
        SpatialRaster<RGBA8BitColor> Create() => new(
            new RasterGeometry(default(PointXY), new VectorXY(12, 8), new VectorXYInt(24, 16)),
            new RGBA8BitColor[24 * 16]);
        PointXY Point(float x, float y) => reverse ? new PointXY(12 - x, 8 - y) : new PointXY(x, y);
        var strokes = new List<Segment>
        {
            new(Point(1, 4), Point(3, 4)),
            new(Point(5, 4), Point(7, 4)),
            new(Point(tipX, 4), Point(tipX - 1, 5)),
            new(Point(tipX, 4), Point(tipX - 1, 3))
        };
        // A shaft ending at X=10 has a shortened last dash; X=8.5 ends in a gap.
        if (tipX == 10f)
            strokes.Add(new Segment(Point(9, 4), Point(10, 4)));
        var raster = Create();
        var viaSegment = Create();
        var color = new RGBA8BitColor(30, 100, 200, 128);
        raster.DrawArrow(Point(1, 4), Point(tipX, 4), 0.5f, 1, 2, 2, color);
        viaSegment.DrawArrow(new ParameterizedSegment(Point(1, 4), Point(tipX, 4)), 0.5f, 1, 2, 2, color);
        Assert.That(viaSegment.Values, Is.EqualTo(raster.Values));
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 24; x++)
            {
                int covered = 0;
                for (int sy = 0; sy < 4; sy++)
                    for (int sx = 0; sx < 4; sx++)
                    {
                        var sample = new PointXY((x + (sx + 0.5f) / 4f) * 0.5f, (y + (sy + 0.5f) / 4f) * 0.5f);
                        if (strokes.Any(s => s.Distance(sample) <= 0.25f))
                            covered++;
                    }
                Assert.That(raster[x, y].A, Is.EqualTo((byte)MathF.Round(128f * covered / 16f)),
                    $"Pixel ({x}, {y}), reverse={reverse}, tipX={tipX}");
            }
    }

    [Test]
    public void DrawArrow_DashesValidateSpacingAndCompositeOverlapsOnce()
    {
        var raster = CreateRaster();
        var start = new PointXY(-1, 0);
        var end = new PointXY(1, 0);
        foreach (float dash in new[] { -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.Epsilon })
        {
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(
                () => raster.DrawArrow(start, end, 0.5f, 1, 1, dash, default))!.ParamName, Is.EqualTo("dash"));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => raster.DrawArrow(new ParameterizedSegment(start, end), 0.5f, 1, 1, dash, default));
        }
        raster.DrawArrow(start, end, 0.5f, 1, 1, 0.25f, default);
        raster.DrawArrow(start, start, 0.5f, 1, 1, 0.25f, RGBA8BitColor.Black);
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
        raster.DrawArrow(start, end, 0.5f, 1, 1, 0.25f, new RGBA8BitColor(30, 100, 200, 128));
        Assert.That(raster.Values.Any(p => p.A > 0), Is.True);
        Assert.That(raster.Values.All(p => p.A <= 128), Is.True);
    }

    [Test]
    public void DrawArrow_ValidatesDimensionsAndHandlesEmptyDrawing()
    {
        var raster = CreateRaster();
        var end = new PointXY(1, 0);
        Assert.Throws<ArgumentNullException>(() =>
            SpatialRasterDrawingExtensions.DrawArrow(null!, default(PointXY), end, 1, 1, 1, default));
        foreach (float invalid in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawArrow(default, end, invalid, 1, 1, default));
            Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawArrow(default, end, 1, invalid, 1, default));
            Assert.Throws<ArgumentOutOfRangeException>(() => raster.DrawArrow(default, end, 1, 1, invalid, default));
        }
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            raster.DrawArrow(new PointXY(float.NaN, 0), end, 1, 1, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            raster.DrawArrow(default, new PointXY(float.PositiveInfinity, 0), 1, 1, 1, default));
        raster.DrawArrow(default(PointXY), default(PointXY), 1, 1, 1, RGBA8BitColor.Black);
        raster.DrawArrow(default, end, 1, 1, 1, default);
        raster.DrawArrow(new PointXY(100, 100), new PointXY(110, 100), 1, 1, 1, RGBA8BitColor.Black);
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }

    [Test]
    public void Draw_AllConcreteOverloadsRenderStrokes()
    {
        var a = new PointXY(-1, 0);
        var b = new PointXY(1, 0);
        var control = new PointXY(0, 1);
        var points = new[] { a, control, b };
        var knots = new[] { 0f, 0f, 0f, 1f, 1f, 1f };
        var color = new RGBA8BitColor(30, 100, 200, 128);
        Action<SpatialRaster<RGBA8BitColor>>[] draw =
        {
            r => r.Draw(new Line(a, b), 0.5f, color),
            r => r.Draw(new Ray(a, 0), 0.5f, color),
            r => r.Draw(new Arc(default, 1, 0, MathF.PI), 0.5f, color),
            r => r.Draw(new ParameterizedLine(a, new VectorXY(1, 0)), 0.5f, color),
            r => r.Draw(new ParameterizedSegment(a, b), 0.5f, color),
            r => r.Draw(new ParameterizedArc(default, 1, 0, MathF.PI, AngularDirection.Counterclockwise), 0.5f, color),
            r => r.Draw(new ParameterizedSegmentChain(points), 0.5f, color),
            r => r.Draw(new QuadraticBezier(a, control, b), 0.5f, color),
            r => r.Draw(new CubicBezier(a, a + new VectorXY(0, 1), b + new VectorXY(0, 1), b), 0.5f, color),
            r => r.Draw(new BSpline(2, points, knots), 0.5f, color),
            r => r.Draw(new Nurbs(2, points, new[] { 1f, 2f, 1f }, knots), 0.5f, color),
        };
        for (int i = 0; i < draw.Length; i++)
        {
            var raster = CreateRaster();
            draw[i](raster);
            Assert.That(raster.Values.Any(p => p.A > 0), Is.True, $"Overload {i} must draw.");
            Assert.That(raster.Values.All(p => p.A <= 128), Is.True, $"Overload {i} must composite only once.");
        }
    }

    [Test]
    public void Draw_InvalidInputsThrowAndTransparentColorPreservesRaster()
    {
        var raster = CreateRaster();
        ICurve curve = new Line(default(PointXY), 0);
        Assert.Throws<ArgumentNullException>(() => raster.Draw((ICurve)null!, 1, default));
        Assert.Throws<ArgumentNullException>(() => raster.Draw((BSpline)null!, 1, default));
        Assert.Throws<ArgumentNullException>(() => SpatialRasterDrawingExtensions.Draw(null!, curve, 1, default));
        foreach (float width in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
            Assert.Throws<ArgumentOutOfRangeException>(() => raster.Draw(curve, width, default));
        raster.Draw(curve, 1, default);
        Assert.That(raster.Values, Is.All.EqualTo(default(RGBA8BitColor)));
    }
}
