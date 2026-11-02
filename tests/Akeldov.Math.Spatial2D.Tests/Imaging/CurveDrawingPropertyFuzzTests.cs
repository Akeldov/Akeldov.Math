using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class CurveDrawingPropertyFuzzTests
{
    [TestCase(7101)]
    [TestCase(7102)]
    [TestCase(7103)]
    public void OptimizedCoverage_MatchesExhaustiveSampling(int seed)
    {
        var random = new Random(seed);
        var grid = new RasterGeometry(new PointXY(-10.3f, 2.7f), new VectorXY(14.7f, 9.1f), new VectorXYInt(24, 16));
        for (int scenario = 0; scenario < 3; scenario++)
        {
            var center = new PointXY(-12 + (float)random.NextDouble() * 20, (float)random.NextDouble() * 15);
            float width = scenario == 2 ? 15f : 0.1f + (float)random.NextDouble() * 3;
            foreach (ICurve curve in CreateCurves(center))
            {
                var raster = new SpatialRaster<RGBA8BitColor>(grid, new RGBA8BitColor[384]);
                raster.Draw(curve, width, new RGBA8BitColor(20, 80, 160, 173));
                AssertCoverage(raster, point => curve.Distance(point), width,
                    $"seed={seed}, scenario={scenario}, curve={curve.GetType().Name}");
            }
        }
    }

    [TestCase(7201)]
    [TestCase(7202)]
    public void DashedCoverage_MatchesOriginalUnionOfStrokes(int seed)
    {
        var random = new Random(seed);
        var grid = new RasterGeometry(new PointXY(-5, -4), new VectorXY(10, 8), new VectorXYInt(24, 16));
        var contour = new ParameterizedCircle(new PointXY(0.3f, -0.2f), 2.6f);
        float dash = 0.3f + (float)random.NextDouble();
        float width = 2f; // Overlapping caps must not accumulate opacity.
        var segments = new List<Segment>();
        float step = MathF.Min(MathF.Min(grid.CellSize.X, grid.CellSize.Y) * 0.5f, dash);
        for (float offset = 0; offset < contour.Length; offset += dash * 2)
        {
            float stop = MathF.Min(contour.Length, offset + dash);
            for (float position = offset; position < stop;)
            {
                float next = MathF.Min(stop, position + step);
                segments.Add(new Segment(contour.GetPoint(position), contour.GetPoint(next)));
                position = next;
            }
        }
        var raster = new SpatialRaster<RGBA8BitColor>(grid, new RGBA8BitColor[384]);
        raster.DrawDashed(contour, width, new RGBA8BitColor(20, 80, 160, 173), dash);
        AssertCoverage(raster, point => segments.Min(segment => segment.Distance(point)), width, $"seed={seed}, dashed circle");
    }

    [Test]
    public void CustomCurve_TileRejectionReducesQueries()
    {
        var curve = new CountingCurve(new Circle(new PointXY(50, 40), 5));
        var raster = new SpatialRaster<RGBA8BitColor>(
            new RasterGeometry(default(PointXY), new VectorXY(100, 80), new VectorXYInt(100, 80)), new RGBA8BitColor[8000]);
        raster.Draw(curve, 1, RGBA8BitColor.Black);
        Assert.That(curve.Calls, Is.LessThan(8000));
        Assert.That(raster.Values.Any(pixel => pixel.A != 0), Is.True);
    }

    private static IEnumerable<ICurve> CreateCurves(PointXY c)
    {
        var a = c + new VectorXY(-3, -1);
        var b = c + new VectorXY(3, 1);
        var control = c + new VectorXY(-1, 4);
        var points = new[] { a, control, b };
        yield return new Line(a, b);
        yield return new Ray(c, 0.7f);
        yield return new Segment(a, b);
        yield return new Segment(c, c);
        yield return new ParameterizedSegment(a, b);
        yield return new ParameterizedLine(a, b - a);
        yield return new ParameterizedSegmentChain(points);
        yield return new Arc(c, 3, 0.2f, 4.3f);
        yield return new ParameterizedArc(c, 3, 0.2f, 4.3f, AngularDirection.Counterclockwise);
        yield return new Circle(c, 3);
        yield return new Circle(c, 0);
        yield return new ParameterizedCircle(c, 3);
        yield return new RectangleContour(a, b);
        yield return new RectangleContour(c, c);
        yield return new ParameterizedRectangleContour(a, b);
        yield return new OrientedRectangleContour(c, new VectorXY(4, 2), 0.7f);
        yield return new ParameterizedOrientedRectangleContour(c, new VectorXY(4, 2), 0.7f);
        yield return new CompositeContour(points);
        yield return new QuadraticBezier(a, control, b);
        yield return new CubicBezier(a, control, c + new VectorXY(2, -4), b);
        yield return new BSpline(2, points, new[] { 0f, 0f, 0f, 1f, 1f, 1f });
        yield return new Nurbs(2, points, new[] { 1f, 2f, 1f }, new[] { 0f, 0f, 0f, 1f, 1f, 1f });
    }

    private static void AssertCoverage(SpatialRaster<RGBA8BitColor> raster, Func<PointXY, float> distance, float width, string context)
    {
        var grid = raster.Geometry;
        for (int y = 0; y < raster.Resolution.Y; y++)
        {
            for (int x = 0; x < raster.Resolution.X; x++)
            {
                int covered = 0;
                for (int sy = 0; sy < 4; sy++)
                    for (int sx = 0; sx < 4; sx++)
                        if (distance(new PointXY(grid.Origin.X + (x + (sx + 0.5f) / 4) * grid.CellSize.X,
                            grid.Origin.Y + (y + (sy + 0.5f) / 4) * grid.CellSize.Y)) <= width * 0.5f)
                            covered++;
                byte alpha = (byte)MathF.Round((173 / 255f * (covered / 16f)) * 255f);
                var expected = covered == 0 ? default : new RGBA8BitColor(20, 80, 160, alpha);
                Assert.That(raster[x, y], Is.EqualTo(expected), $"{context}, x={x}, y={y}");
            }
        }
    }

    private sealed class CountingCurve : ICurve
    {
        private readonly ICurve _curve;
        public CountingCurve(ICurve curve) { _curve = curve; }
        public int Calls { get; private set; }
        public float Distance(PointXY point) { Calls++; return _curve.Distance(point); }
        public CurveProjection Project(PointXY point) => _curve.Project(point);
    }
}
