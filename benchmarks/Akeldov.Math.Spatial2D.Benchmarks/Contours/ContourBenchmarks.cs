using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using BenchmarkDotNet.Attributes;

namespace Akeldov.Math.Spatial2D.Benchmarks.Contours;

[MemoryDiagnoser]
[ShortRunJob]
public class ContourBenchmarks
{
    private CompositeContour _contour = null!;
    private PointXY[] _queries = null!;

    [Params(8, 64)]
    public int SegmentCount { get; set; }

    [Params(1_000)]
    public int QueryCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _contour = CreateRegularPolygonContour(SegmentCount, 100f);
        _queries = CreateQueries(QueryCount, 120f);
    }

    [Benchmark]
    public int EnclosesQueries()
    {
        int enclosedCount = 0;

        for (int i = 0; i < _queries.Length; i++)
        {
            if (_contour.Encloses(_queries[i]))
                enclosedCount++;
        }

        return enclosedCount;
    }

    [Benchmark]
    public CompositeContour FilletCorners()
    {
        return _contour.FilletCorners(2f);
    }

    private static CompositeContour CreateRegularPolygonContour(int segmentCount, float radius)
    {
        var curves = new IFinitePath[segmentCount];

        for (int i = 0; i < curves.Length; i++)
        {
            PointXY start = GetRegularPolygonVertex(i, segmentCount, radius);
            PointXY end = GetRegularPolygonVertex((i + 1) % segmentCount, segmentCount, radius);
            curves[i] = new ParameterizedSegment(start, end);
        }

        return new CompositeContour(curves);
    }

    private static PointXY GetRegularPolygonVertex(int index, int segmentCount, float radius)
    {
        float angle = index * 2f * MathF.PI / segmentCount;
        return new PointXY(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
    }

    private static PointXY[] CreateQueries(int queryCount, float range)
    {
        var random = new Random(45678);
        var queries = new PointXY[queryCount];
        float offset = range * 0.5f;

        for (int i = 0; i < queries.Length; i++)
        {
            queries[i] = new PointXY(
                random.NextSingle() * range - offset,
                random.NextSingle() * range - offset);
        }

        return queries;
    }
}
