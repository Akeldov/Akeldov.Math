using Akeldov.Math.Hexes.Rasterization;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Topology.Grids.Rasterization;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using BenchmarkDotNet.Attributes;

namespace Akeldov.Math.Hexes.Benchmarks.Rasterization;

[MemoryDiagnoser]
[ShortRunJob]
public class HexRasterizationBenchmarks
{
    private HexAdjacencyMap _adjacencyMap = null!;
    private HexFieldTopologyRGBA16BitRasterizer _topologyRasterizer = null!;
    private IndexedHexAdjacencyGrid _adjacencyGrid = null!;
    private RasterGrid _topologyGrid;

    [Params(32, 128)]
    public int Size { get; set; }

    [Params(Layout.OddR, Layout.EvenQ)]
    public Layout Layout { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _adjacencyMap = new HexAdjacencyMap(Size, Size, Layout);
        _topologyRasterizer = new HexFieldTopologyRGBA16BitRasterizer(
            origin: VectorXY.Zero,
            apothem: 8f,
            indexToColor: ToIndexColor);
        _topologyGrid = _topologyRasterizer.CreateGrid(_adjacencyMap, pixelsPerApothem: 2f);
        _adjacencyGrid = new IndexedHexAdjacencyGrid(
            _adjacencyMap,
            resolution: new VectorXYInt(Size * 8, Size * 8));
    }

    [Benchmark]
    public IndexedHexAdjacencyGrid ConstructAdjacencyGrid()
    {
        return new IndexedHexAdjacencyGrid(
            _adjacencyMap,
            resolution: new VectorXYInt(Size * 8, Size * 8));
    }

    [Benchmark]
    public RGBA16BitRaster RasterizeTopology()
    {
        return _topologyRasterizer.Rasterize(_adjacencyMap, _topologyGrid);
    }

    [Benchmark]
    public RGBA16BitRaster RasterizeAdjacencyGrid()
    {
        return _adjacencyGrid.Rasterize(adjacency => ToFlatIndexColor(adjacency.Main));
    }

    private static RGBA16BitColor ToIndexColor(VectorXYInt index)
    {
        return new RGBA16BitColor(
            ToChannel(0.16f + 0.01f * index.X),
            ToChannel(0.24f + 0.01f * index.Y),
            ToChannel(0.80f - 0.001f * (index.X + index.Y)),
            ushort.MaxValue);
    }

    private static RGBA16BitColor ToFlatIndexColor(int flatIndex)
    {
        if (flatIndex < 0)
            return new RGBA16BitColor(0, 0, 0, ushort.MaxValue);

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
