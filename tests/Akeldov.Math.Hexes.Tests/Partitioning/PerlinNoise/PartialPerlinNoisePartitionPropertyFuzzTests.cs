using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.PerlinNoise;

public class PartialPerlinNoisePartitionPropertyFuzzTests
{
    [TestCase(42)]
    [TestCase(173)]
    [TestCase(20261017)]
    public void Partition_PreservesMaskCoverageBandOrderAndApproximateShares(int seed)
    {
        var random = new Random(seed);
        foreach (Layout layout in Enum.GetValues<Layout>())
        for (int iteration = 0; iteration < 12; iteration++)
        {
            var topology = new HexMapTopology(random.Next(1, 20), random.Next(1, 20), layout);
            var geometry = new HexMapGeometry(topology, new VectorXY(-10, 15), 2.5f);
            var mask = new BoolHexMap(topology, Enumerable.Range(0, topology.Count).Select(_ => random.Next(3) == 0).ToArray());
            var targets = Enumerable.Range(0, random.Next(1, 9))
                .Select(i => new PerlinNoisePartitionTarget(i * 7 + 11, i == 0 ? 1 : random.Next(0, 10))).ToArray();
            float scale = (float)(1 + random.NextDouble() * 15);
            int octaves = random.Next(1, 5);
            float persistence = (float)random.NextDouble();
            float lacunarity = (float)(1 + random.NextDouble() * 2);
            var offset = new VectorXY(random.Next(-20, 20), random.Next(-20, 20));
            var noise = geometry.CreatePerlinNoise(seed, scale, octaves, persistence, lacunarity, offset);

            var partition = geometry.ToPerlinNoiseHexPartitionMap(targets, mask, new PerlinNoisePartitionParameters(seed, scale, octaves, persistence, lacunarity, offset));

            PartialPerlinNoisePartitionTests.AssertPartition(partition, noise, mask, targets,
                $"seed={seed}, layout={layout}, iteration={iteration}");
        }
    }
}
