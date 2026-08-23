using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using BenchmarkDotNet.Attributes;

namespace Akeldov.Math.Spatial2D.Benchmarks.Fields;

[MemoryDiagnoser]
[ShortRunJob]
public class DelaunayInfluenceSourceIndexBenchmarks
{
    private FloatPointInfluenceSource[] _sources = null!;
    private PointXY[] _queries = null!;
    private DelaunayInfluenceSourceIndex<FloatPointInfluenceSource> _index = null!;

    [Params(32, 128, 512)]
    public int SourceCount { get; set; }

    [Params(1_000)]
    public int QueryCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(23456);

        _sources = new FloatPointInfluenceSource[SourceCount];
        for (int i = 0; i < _sources.Length; i++)
        {
            _sources[i] = new FloatPointInfluenceSource(
                weight: 1f,
                position: NextPoint(random, 1000f),
                value: random.NextSingle());
        }

        _queries = new PointXY[QueryCount];
        for (int i = 0; i < _queries.Length; i++)
            _queries[i] = NextPoint(random, 1000f);

        _index = new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(_sources);
    }

    [Benchmark]
    public DelaunayInfluenceSourceIndex<FloatPointInfluenceSource> Build()
    {
        return new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(_sources);
    }

    [Benchmark]
    public int SelectSources()
    {
        int selectedSourceCount = 0;

        for (int i = 0; i < _queries.Length; i++)
            selectedSourceCount += _index.SelectSources(_queries[i]).Count;

        return selectedSourceCount;
    }

    private static PointXY NextPoint(Random random, float size)
    {
        return new PointXY(random.NextSingle() * size, random.NextSingle() * size);
    }
}
