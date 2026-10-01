using BenchmarkDotNet.Attributes;

namespace Akeldov.Math.Hexes.Benchmarks.Maps;

[MemoryDiagnoser]
[ShortRunJob]
public class BooleanHexMapDilationBenchmarks
{
    private BoolHexMap _source = null!;

    [Params(2, 8, 32)]
    public int RingsCount { get; set; }

    [Params("SingleCell", "Dense")]
    public string Pattern { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        var topology = new HexMapTopology(256, 256, Layout.OddR);
        var values = new bool[topology.Count];
        if (Pattern == "SingleCell")
        {
            values[128 * 256 + 128] = true;
        }
        else
        {
            var random = new Random(1729);
            for (int index = 0; index < values.Length; index++)
                values[index] = random.NextDouble() < 0.5;
        }

        _source = new BoolHexMap(topology, values);
    }

    [Benchmark(Baseline = true)]
    public BoolHexMap RepeatedDilation()
    {
        BoolHexMap result = _source;
        for (int ring = 0; ring < RingsCount; ring++)
            result = result.Dilate();

        return result;
    }

    [Benchmark]
    public BoolHexMap BoundedDilation() => _source.Dilate(RingsCount);
}
