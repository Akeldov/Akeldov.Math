using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using BenchmarkDotNet.Attributes;

namespace Akeldov.Math.Spatial2D.Benchmarks.Rasterization;

[MemoryDiagnoser]
[ShortRunJob]
public class CurveDrawingBenchmarks
{
    private SpatialRaster<RGBA8BitColor> _raster = null!;
    private ICurve _curve = null!;

    [Params("Segment", "Line", "Ray", "Arc", "Circle", "Rectangle", "Contour", "Chain", "Quadratic", "Cubic", "BSpline", "Nurbs")]
    public string Curve { get; set; } = "Circle";

    [GlobalSetup]
    public void Setup()
    {
        _raster = new SpatialRaster<RGBA8BitColor>(
            new RasterGeometry(default(PointXY), new VectorXY(160, 110), new VectorXYInt(160, 110)), new RGBA8BitColor[160 * 110]);
        var a = new PointXY(20, 20);
        var b = new PointXY(100, 20);
        var points = new[] { a, new PointXY(60, 80), b };
        var knots = new[] { 0f, 0f, 0f, 1f, 1f, 1f };
        _curve = Curve switch
        {
            "Segment" => new Segment(a, b),
            "Line" => new Line(a, b),
            "Ray" => new Ray(a, 0),
            "Arc" => new Arc(new PointXY(70, 55), 15, 0, MathF.PI),
            "Circle" => new Circle(new PointXY(70, 55), 15),
            "Rectangle" => new RectangleContour(a, new PointXY(100, 80)),
            "Contour" => new CompositeContour(points),
            "Chain" => new ParameterizedSegmentChain(points),
            "Quadratic" => new QuadraticBezier(a, points[1], b),
            "Cubic" => new CubicBezier(a, new PointXY(30, 80), new PointXY(90, 80), b),
            "BSpline" => new BSpline(2, points, knots),
            "Nurbs" => new Nurbs(2, points, new[] { 1f, 2f, 1f }, knots),
            _ => throw new InvalidOperationException(Curve),
        };
    }

    [Benchmark]
    public void Optimized() => _raster.Draw(_curve, 2, RGBA8BitColor.Black);

    // Original 4x4 full-raster algorithm retained as a reproducible performance baseline.
    [Benchmark(Baseline = true)]
    public void FullScan()
    {
        var grid = _raster.Geometry;
        for (int y = 0; y < grid.Resolution.Y; y++)
        {
            for (int x = 0; x < grid.Resolution.X; x++)
            {
                int count = 0;
                for (int sy = 0; sy < 4; sy++)
                    for (int sx = 0; sx < 4; sx++)
                        if (_curve.Distance(new PointXY(grid.Origin.X + (x + (sx + 0.5f) / 4) * grid.CellSize.X,
                            grid.Origin.Y + (y + (sy + 0.5f) / 4) * grid.CellSize.Y)) <= 1)
                            count++;
                if (count != 0)
                {
                    float alpha = count / 16f;
                    float background = _raster[x, y].A / 255f * (1 - alpha);
                    _raster[x, y] = new RGBA8BitColor(0, 0, 0, (byte)MathF.Round((alpha + background) * 255));
                }
            }
        }
    }
}
