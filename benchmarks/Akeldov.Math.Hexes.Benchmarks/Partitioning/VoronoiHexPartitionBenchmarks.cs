using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using BenchmarkDotNet.Attributes;

namespace Akeldov.Math.Hexes.Benchmarks.Partitioning;

[MemoryDiagnoser]
[ShortRunJob]
public class VoronoiHexPartitionBenchmarks
{
    private HexCenterMap _centers = null!;
    private Site[] _sites = null!;
    private BoolHexMap _participation = null!;
    private IntHexMap _regions = null!;
    private HexMap<int?> _nullableRegions = null!;

    [Params(32, 128)]
    public int Size { get; set; }

    [Params(16, 64)]
    public int SiteCount { get; set; }

    [Params(0, 2)]
    public int RelaxationIterations { get; set; }

    [Params(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude)]
    public EmptyCellPolicy EmptyCellPolicy { get; set; }

    [Params(ExclavePolicy.LeaveAsIs, ExclavePolicy.ReassignToClosestCell)]
    public ExclavePolicy ExclavePolicy { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _centers = new HexCenterMap(new HexMapGeometry(Size, Size, new VectorXY(13f, -21f), 1f, Layout.OddR));
        var topology = _centers.Topology;
        var participation = new bool[topology.Count];
        var regions = new int[topology.Count];
        var nullableRegions = new int?[topology.Count];
        var regionIds = new[] { -7, 0, 42, 71 };
        var indexesByRegion = new List<int>[regionIds.Length];
        var participatingIndexesByRegion = new List<int>[regionIds.Length];
        for (int i = 0; i < regionIds.Length; i++)
        {
            indexesByRegion[i] = new List<int>();
            participatingIndexesByRegion[i] = new List<int>();
        }

        // Four regions have equal area; the partial maps exclude one quarter of the hexes.
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                int index = y * Size + x;
                int regionIndex = (x < Size / 2 ? 0 : 1) + (y < Size / 2 ? 0 : 2);
                participation[index] = (x + 2 * y) % 4 != 0;
                regions[index] = regionIds[regionIndex];
                nullableRegions[index] = participation[index] ? regions[index] : null;
                indexesByRegion[regionIndex].Add(index);
                if (participation[index])
                    participatingIndexesByRegion[regionIndex].Add(index);
            }
        }

        _participation = new BoolHexMap(topology, participation);
        _regions = new IntHexMap(topology, regions);
        _nullableRegions = new HexMap<int?>(topology, nullableRegions);

        var random = new Random(12345);
        _sites = new Site[SiteCount];
        for (int i = 0; i < _sites.Length; i++)
        {
            int regionIndex = i % regionIds.Length;
            // Seed every region with an eligible site, then also sample excluded hexes.
            // Nullable regions ignore those sites; the combined-mask overload retains them.
            var candidates = i < regionIds.Length
                ? participatingIndexesByRegion[regionIndex]
                : indexesByRegion[regionIndex];
            int index = candidates[random.Next(candidates.Count)];
            _sites[i] = new Site(_centers[index], 0.5f + random.NextSingle() * 2f);
        }
    }

    [Benchmark]
    public VoronoiHexPartitionMap Partition()
    {
        return _centers.ToVoronoiHexPartitionMap(
            _sites, RelaxationIterations, EmptyCellPolicy, ExclavePolicy);
    }

    [Benchmark]
    public VoronoiHexPartitionMap PartitionByRegions()
    {
        return _centers.ToVoronoiHexPartitionMap(
            _sites, _regions, RelaxationIterations, EmptyCellPolicy, ExclavePolicy);
    }

    [Benchmark]
    public PartialVoronoiHexPartitionMap PartitionParticipatingHexes()
    {
        return _centers.ToPartialVoronoiHexPartitionMap(
            _sites, _participation, RelaxationIterations, EmptyCellPolicy, ExclavePolicy);
    }

    [Benchmark]
    public PartialVoronoiHexPartitionMap PartitionNullableRegions()
    {
        return _centers.ToPartialVoronoiHexPartitionMap(
            _sites, _nullableRegions, RelaxationIterations, EmptyCellPolicy, ExclavePolicy);
    }

    [Benchmark]
    public PartialVoronoiHexPartitionMap PartitionParticipatingHexesByRegions()
    {
        return _centers.ToPartialVoronoiHexPartitionMap(
            _sites, _participation, _regions, RelaxationIterations, EmptyCellPolicy, ExclavePolicy);
    }
}
