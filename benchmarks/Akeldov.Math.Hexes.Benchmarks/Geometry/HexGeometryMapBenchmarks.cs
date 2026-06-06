using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using BenchmarkDotNet.Attributes;

namespace Akeldov.Math.Hexes.Benchmarks.Geometry;

[MemoryDiagnoser]
[ShortRunJob]
public class HexGeometryMapBenchmarks
{
    private HexCenterMap _centerMap = null!;
    private VectorXYInt[] _indices = null!;

    [Params(32, 128)]
    public int Size { get; set; }

    [Params(Layout.OddR, Layout.EvenQ)]
    public Layout Layout { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _centerMap = new HexCenterMap(Size, Size, VectorXY.Zero, apothem: 8f, Layout);
        _indices = CreateIndices(Size, Size);
    }

    [Benchmark]
    public HexCenterMap ConstructCenterMap()
    {
        return new HexCenterMap(Size, Size, VectorXY.Zero, apothem: 8f, Layout);
    }

    [Benchmark]
    public HexCenterMap ConstructCenterMapWithDefaultOrigin()
    {
        return new VectorXYInt(Size, Size).ToHexCenterMap(Layout, apothem: 8f);
    }

    [Benchmark]
    public float SumCenters()
    {
        float sum = 0f;

        for (int i = 0; i < _indices.Length; i++)
        {
            VectorXY center = _centerMap[_indices[i]];
            sum += center.X;
            sum += center.Y;
        }

        return sum;
    }

    private static VectorXYInt[] CreateIndices(int width, int height)
    {
        var indices = new VectorXYInt[checked(width * height)];
        int index = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                indices[index] = new VectorXYInt(x, y);
                index++;
            }
        }

        return indices;
    }
}
