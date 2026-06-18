using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;
using BenchmarkDotNet.Attributes;

namespace Akeldov.Math.Spatial2D.Benchmarks.Regions;

[MemoryDiagnoser]
[ShortRunJob]
public class RegionBenchmarks
{
    private IContour[] _contours = null!;
    private ContourBasedRegion _region = null!;
    private PointXY[] _queries = null!;

    [Params(1, 4)]
    public int ContourCount { get; set; }

    [Params(1_000)]
    public int QueryCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _contours = CreateNestedSquareContours(ContourCount);
        _region = new ContourBasedRegion(_contours);
        _queries = CreateQueries(QueryCount, 120f, -10f);
    }

    [Benchmark]
    public ContourBasedRegion Construct()
    {
        return new ContourBasedRegion(_contours);
    }

    [Benchmark]
    public int ContainsQueries()
    {
        int containedCount = 0;

        for (int i = 0; i < _queries.Length; i++)
        {
            if (_region.Contains(_queries[i]))
                containedCount++;
        }

        return containedCount;
    }

    private static IContour[] CreateNestedSquareContours(int contourCount)
    {
        var contours = new IContour[contourCount];

        for (int i = 0; i < contours.Length; i++)
        {
            float margin = i * 12f;
            contours[i] = CreateSquareContour(margin, margin, 100f - margin, 100f - margin);
        }

        return contours;
    }

    private static CompositeContour CreateSquareContour(float left, float bottom, float right, float top)
    {
        return new CompositeContour(new IFinitePath[]
        {
            new ParameterizedSegment(new PointXY(left, bottom), new PointXY(right, bottom)),
            new ParameterizedSegment(new PointXY(right, bottom), new PointXY(right, top)),
            new ParameterizedSegment(new PointXY(right, top), new PointXY(left, top)),
            new ParameterizedSegment(new PointXY(left, top), new PointXY(left, bottom))
        });
    }

    private static PointXY[] CreateQueries(int queryCount, float range, float offset)
    {
        var random = new Random(56789);
        var queries = new PointXY[queryCount];

        for (int i = 0; i < queries.Length; i++)
        {
            queries[i] = new PointXY(
                offset + random.NextSingle() * range,
                offset + random.NextSingle() * range);
        }

        return queries;
    }
}
