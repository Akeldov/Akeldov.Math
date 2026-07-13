using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using BenchmarkDotNet.Attributes;

namespace Akeldov.Math.Hexes.Benchmarks.Rasterization;

[MemoryDiagnoser]
[ShortRunJob]
public class HexRasterizationBenchmarks
{
    private IndexSeptupletMap _adjacencyMap = null!;
    private IndexSeptupletGrid _adjacencyGrid = null!;
    private VectorXYInt _topologyResolution;

    [Params(32, 128)]
    public int Size { get; set; }

    [Params(Layout.OddR, Layout.EvenQ)]
    public Layout Layout { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _adjacencyMap = new IndexSeptupletMap(new HexMapTopology(Size, Size, Layout));
        _topologyResolution = new VectorXYInt(Size * 8, Size * 8);
        _adjacencyGrid = new IndexSeptupletGrid(
            _adjacencyMap,
            resolution: _topologyResolution);
    }

    [Benchmark]
    public IndexSeptupletGrid ConstructAdjacencyGrid()
    {
        return new IndexSeptupletGrid(
            _adjacencyMap,
            resolution: new VectorXYInt(Size * 8, Size * 8));
    }

    [Benchmark]
    public Raster<RGBA16BitColor> RasterizeTopology()
    {
        return _adjacencyMap.Rasterize(
            _topologyResolution,
            (Septuplet<VectorXYInt> adjacency) => ToIndexColor(adjacency.Main));
    }

    [Benchmark]
    public Raster<RGBA16BitColor> RasterizeAdjacencyGrid()
    {
        return _adjacencyGrid.Rasterize((Septuplet<VectorXYInt> adjacency) => ToAdjacencyIndexColor(adjacency.Main));
    }

    private static RGBA16BitColor ToIndexColor(VectorXYInt index)
    {
        return new RGBA16BitColor(
            ToChannel(0.16f + 0.01f * index.X),
            ToChannel(0.24f + 0.01f * index.Y),
            ToChannel(0.80f - 0.001f * (index.X + index.Y)),
            ushort.MaxValue);
    }

    private RGBA16BitColor ToAdjacencyIndexColor(VectorXYInt index)
    {
        if ((uint)index.X >= (uint)Size || (uint)index.Y >= (uint)Size)
            return new RGBA16BitColor(0, 0, 0, ushort.MaxValue);

        int flatIndex = index.Y * Size + index.X;
        return new RGBA16BitColor(
            (ushort)(flatIndex * 97),
            (ushort)(flatIndex * 193),
            (ushort)(flatIndex * 389),
            ushort.MaxValue);
    }

    private static ushort ToChannel(float value)
    {
        value = MathF.Min(MathF.Max(value, 0f), 1f);
        return (ushort)MathF.Round(value * ushort.MaxValue);
    }
}
