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
