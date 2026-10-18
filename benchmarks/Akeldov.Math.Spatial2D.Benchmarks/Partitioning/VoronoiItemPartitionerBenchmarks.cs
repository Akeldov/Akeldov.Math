using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using BenchmarkDotNet.Attributes;

namespace Akeldov.Math.Spatial2D.Benchmarks.Partitioning;

[MemoryDiagnoser]
[ShortRunJob]
public class VoronoiItemPartitionerBenchmarks
{
    private Site[] _sites = null!;
    private PointItem[] _items = null!;
    private VoronoiItemPartitioner<PointItem> _partitioner = null!;

    [Params(16, 64)]
    public int SiteCount { get; set; }

    [Params(1_000, 10_000)]
    public int ItemCount { get; set; }

    [Params(0, 2)]
    public int RelaxationIterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(12345);

        _sites = new Site[SiteCount];
        for (int i = 0; i < _sites.Length; i++)
        {
            _sites[i] = new Site(
                NextPoint(random, 1000f),
                weight: 0.5f + random.NextSingle() * 2f);
        }

        _items = new PointItem[ItemCount];
        for (int i = 0; i < _items.Length; i++)
            _items[i] = new PointItem(NextPoint(random, 1000f));

        _partitioner = new VoronoiItemPartitioner<PointItem>(
            _sites,
            RelaxationIterations,
            EmptyCellPolicy.LeaveAsIs);
    }

    [Benchmark]
    public IReadOnlyList<VoronoiItemPartition<PointItem>> Partition()
    {
        return _partitioner.Partition(_items);
    }

    private static PointXY NextPoint(Random random, float size)
    {
        return new PointXY(random.NextSingle() * size, random.NextSingle() * size);
    }

    public sealed class PointItem : IHasPosition2D
    {
        public PointItem(PointXY position)
        {
            Position = position;
        }

        public PointXY Position { get; }
    }
}
