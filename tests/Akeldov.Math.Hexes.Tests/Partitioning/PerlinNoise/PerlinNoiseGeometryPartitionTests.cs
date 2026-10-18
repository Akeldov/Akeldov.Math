using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Spatial2D;
using static Akeldov.Math.Hexes.Tests.Partitioning.PerlinNoise.PerlinNoisePartitionTests;

namespace Akeldov.Math.Hexes.Tests.Partitioning.PerlinNoise;

public class PerlinNoiseGeometryPartitionTests
{
    private static readonly PerlinNoisePartitionTarget[] Targets =
    {
        new(7, 0.3), new(42, 0.5), new(99, 0.2), new(100, 0)
    };

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Partition_RetainsGeometryAndAssignsWorldNoiseBands(Layout layout)
    {
        var geometry = new HexMapGeometry(40, 31, new VectorXY(-30f, 50f), 2.5f, layout);
        var offset = new VectorXY(2.5f, -7f);
        var noise = geometry.CreatePerlinNoise(123, 19f, 3, 0.4f, 2.25f, offset);
        var partition = geometry.PartitionByPerlinNoise(Targets, new PerlinNoisePartitionParameters(19f, 3, 0.4f, 2.25f, offset), seed: 123);

        Assert.That(((ISpatialHexMap<int>)partition).Geometry, Is.EqualTo(geometry));
        AssertPartition(partition, noise, Targets, $"layout={layout}");
        for (int i = 0; i < Targets.Length; i++)
            Assert.That((double)partition.Cells[i].HexIndexes.Count / geometry.Topology.Count,
                Is.EqualTo(Targets[i].AreaShare).Within(0.01), $"{layout}, type={i}");
        Assert.That(Values(geometry.PartitionByPerlinNoise(Targets, new PerlinNoisePartitionParameters(19f, 3, 0.4f, 2.25f, offset), seed: 123)),
            Is.EqualTo(Values(partition)));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Partition_UnitGeometryUsesTopologyNoiseBands(Layout layout)
    {
        var topology = new HexMapTopology(20, 18, layout);
        var noise = topology.CreatePerlinNoise(42, 7f);
        var partition = new HexMapGeometry(topology, 1f).PartitionByPerlinNoise(Targets, new PerlinNoisePartitionParameters(7f), seed: 42);

        AssertPartition(partition, noise, Targets, $"layout={layout}");
    }

    [Test]
    public void Partition_OriginAndRadiusAffectAssignments()
    {
        var topology = new HexMapTopology(20, 18, Layout.OddR);
        var origin = new VectorXY(-10f, 15f);
        var expected = new HexMapGeometry(topology, origin, 2f).PartitionByPerlinNoise(Targets, new PerlinNoisePartitionParameters(9f), seed: 42);
        var shifted = new HexMapGeometry(topology, origin + new VectorXY(5f, 7f), 2f)
            .PartitionByPerlinNoise(Targets, new PerlinNoisePartitionParameters(9f), seed: 42);
        var resized = new HexMapGeometry(topology, origin, 4f).PartitionByPerlinNoise(Targets, new PerlinNoisePartitionParameters(9f), seed: 42);
        Assert.That(Values(shifted), Is.Not.EqualTo(Values(expected)));
        Assert.That(Values(resized), Is.Not.EqualTo(Values(expected)));
    }

    [Test]
    public void Partition_EmptyGeometryRetainsAllTypesAndValidatesInputs()
    {
        var geometry = new HexMapGeometry(0, 3, new VectorXY(-10f, 15f), 2f, Layout.OddR);
        var partition = geometry.PartitionByPerlinNoise(Targets, new PerlinNoisePartitionParameters(7f), seed: 42);
        Assert.That(partition.Geometry, Is.EqualTo(geometry));
        AssertPartition(partition, geometry.CreatePerlinNoise(42, 7f), Targets, "empty");
        Assert.Throws<ArgumentNullException>(() => geometry.PartitionByPerlinNoise(null!, new PerlinNoisePartitionParameters(7f), seed: 42));
        Assert.Throws<ArgumentException>(() => geometry.PartitionByPerlinNoise(Array.Empty<PerlinNoisePartitionTarget>(), new PerlinNoisePartitionParameters(7f), seed: 42));
        Assert.Throws<ArgumentException>(() => geometry.PartitionByPerlinNoise(new[] { Targets[0], Targets[0] }, new PerlinNoisePartitionParameters(7f), seed: 42));
        Assert.Throws<ArgumentException>(() => geometry.PartitionByPerlinNoise(new[] { Targets[3] }, new PerlinNoisePartitionParameters(7f), seed: 42));
        Assert.Throws<ArgumentOutOfRangeException>(() => geometry.PartitionByPerlinNoise(Targets, new PerlinNoisePartitionParameters(0f), seed: 42));
        Assert.Throws<ArgumentOutOfRangeException>(() => default(HexMapGeometry).PartitionByPerlinNoise(Targets, new PerlinNoisePartitionParameters(7f), seed: 42));
    }
}
