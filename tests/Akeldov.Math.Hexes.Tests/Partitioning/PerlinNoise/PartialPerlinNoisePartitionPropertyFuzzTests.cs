using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.PerlinNoise;

public class PartialPerlinNoisePartitionPropertyFuzzTests
{
    [TestCase(42)]
    [TestCase(173)]
    [TestCase(20261018)]
    public void RegionalPartition_PreservesCoverageAndIndependentRegionAssignments(int seed)
    {
        var random = new Random(seed);
        var regionIds = new[] { int.MinValue, -7, 0, 42, int.MaxValue };
        foreach (Layout layout in Enum.GetValues<Layout>())
        for (int iteration = 0; iteration < 12; iteration++)
        {
            string context = $"seed={seed}, layout={layout}, iteration={iteration}";
            var topology = new HexMapTopology(random.Next(0, 16), random.Next(0, 16), layout);
            var geometry = new HexMapGeometry(topology, new VectorXY(-10, 15), 2.5f);
            var participation = new BoolHexMap(topology);
            var regions = new IntHexMap(topology);
            var nullableRegions = new HexMap<int?>(topology);
            for (int i = 0; i < topology.Count; i++)
            {
                participation[i] = random.Next(3) != 0;
                regions[i] = regionIds[random.Next(regionIds.Length)];
                nullableRegions[i] = participation[i] ? regions[i] : null;
            }
            var targets = Enumerable.Range(0, random.Next(1, 7))
                .Select(i => new PerlinNoisePartitionTarget(i * 7 + 11, i == 0 ? 1 : random.Next(0, 10))).ToArray();
            var parameters = new PerlinNoisePartitionParameters(iteration % 4 == 0 ? float.MaxValue : (float)(1 + random.NextDouble() * 15),
                random.Next(1, 5),
                (float)random.NextDouble(), (float)(1 + random.NextDouble() * 2), new VectorXY(random.Next(-20, 20), random.Next(-20, 20)));

            var combined = geometry.PartitionByPerlinNoise(participation, regions, targets, parameters, seed: seed);
            var nullable = geometry.PartitionByPerlinNoise(nullableRegions, targets, parameters, seed: seed);

            Assert.That(PartialPerlinNoisePartitionTests.Values(nullable),
                Is.EqualTo(PartialPerlinNoisePartitionTests.Values(combined)), context);
            RegionalPerlinNoisePartitionTests.AssertRegionalPartition(combined, geometry, targets,
                participation, regions, parameters, context, seed);

            // Only region equality matters. Renaming regions and changing excluded values has no effect.
            for (int i = 0; i < topology.Count; i++)
                regions[i] = participation[i] ? ~regions[i] : 123;
            var renamed = geometry.PartitionByPerlinNoise(participation, regions, targets, parameters, seed: seed);
            Assert.That(PartialPerlinNoisePartitionTests.Values(renamed),
                Is.EqualTo(PartialPerlinNoisePartitionTests.Values(combined)), context);
        }
    }

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

            var partition = geometry.PartitionByPerlinNoise(mask, targets, new PerlinNoisePartitionParameters(scale, octaves, persistence, lacunarity, offset), seed: seed);

            PartialPerlinNoisePartitionTests.AssertPartition(partition, noise, mask, targets,
                $"seed={seed}, layout={layout}, iteration={iteration}");
        }
    }
}
