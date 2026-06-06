using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using BenchmarkDotNet.Attributes;

namespace Akeldov.Math.Hexes.Benchmarks.Topology;

[MemoryDiagnoser]
[ShortRunJob]
public class HexAdjacencyMapBenchmarks
{
    private HexAdjacencyMap _adjacencyMap = null!;
    private VectorXYInt[] _indices = null!;

    [Params(32, 128)]
    public int Size { get; set; }

    [Params(Layout.OddR, Layout.EvenQ)]
    public Layout Layout { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _adjacencyMap = new HexAdjacencyMap(Size, Size, Layout);
        _indices = CreateIndices(Size, Size);
    }

    [Benchmark]
    public HexAdjacencyMap ConstructAdjacencyMap()
    {
        return new HexAdjacencyMap(Size, Size, Layout);
    }

    [Benchmark]
    public int SumAdjacencyMapNeighbors()
    {
        int sum = 0;

        for (int i = 0; i < _indices.Length; i++)
        {
            Septuplet<int> adjacency = _adjacencyMap[_indices[i]];
            sum += adjacency.Adjacent0;
            sum += adjacency.Adjacent1;
            sum += adjacency.Adjacent2;
            sum += adjacency.Adjacent3;
            sum += adjacency.Adjacent4;
            sum += adjacency.Adjacent5;
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
