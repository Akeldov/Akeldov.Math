using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.PerlinNoise;

public class PerlinNoisePartitionTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Partition_CoversMapWithOrderedNoiseBandsAndRequestedShares(Layout layout)
    {
        var topology = new HexMapTopology(40, 31, layout);
        var targets = new[]
        {
            new PerlinNoisePartitionTarget(int.MaxValue, 0.2),
            new PerlinNoisePartitionTarget(7, 0.5),
            new PerlinNoisePartitionTarget(42, 0.3)
        };
        var offset = new VectorXY(-2.5f, 7.25f);
        var noise = topology.CreatePerlinNoise(123, 9f, 3, 0.4f, 2.25f, offset);

        var partition = topology.ToPerlinNoiseHexPartitionMap(targets, 123, 9f, 3, 0.4f, 2.25f, offset);

        AssertPartition(partition, noise, targets, $"layout={layout}");
        for (int i = 0; i < targets.Length; i++)
            Assert.That((double)partition.Cells[i].HexIndexes.Count / topology.Count,
                Is.EqualTo(targets[i].AreaShare).Within(0.01), $"layout={layout}, target={i}");
    }

    [Test]
    public void Partition_IsReproducibleAndSeedChangesTheMap()
    {
        var topology = new HexMapTopology(20, 18, Layout.OddR);
        var targets = new[] { new PerlinNoisePartitionTarget(7, 2), new PerlinNoisePartitionTarget(42, 3) };

        var first = topology.ToPerlinNoiseHexPartitionMap(targets, 37, 6f);
        var replay = topology.ToPerlinNoiseHexPartitionMap(targets, 37, 6f);
        var other = topology.ToPerlinNoiseHexPartitionMap(targets, 38, 6f);

        Assert.That(Values(replay), Is.EqualTo(Values(first)));
        Assert.That(Values(other), Is.Not.EqualTo(Values(first)));
    }

    [Test]
    public void Partition_NormalizesSharesWithoutOverflowOrUnderflowOfTheTotal()
    {
        var topology = new HexMapTopology(20, 18, Layout.EvenQ);
        var fractions = new[] { new PerlinNoisePartitionTarget(7, 0.4), new PerlinNoisePartitionTarget(42, 0.6) };
        var percentages = new[] { new PerlinNoisePartitionTarget(7, 40), new PerlinNoisePartitionTarget(42, 60) };
        var enormous = new[] { new PerlinNoisePartitionTarget(7, 1e308), new PerlinNoisePartitionTarget(42, 1.5e308) };
        var small = new[] { new PerlinNoisePartitionTarget(7, 2 * double.Epsilon), new PerlinNoisePartitionTarget(42, 3 * double.Epsilon) };
        var expected = topology.ToPerlinNoiseHexPartitionMap(fractions, 42, 7f);

        foreach (var targets in new[] { percentages, enormous, small })
            Assert.That(Values(topology.ToPerlinNoiseHexPartitionMap(targets, 42, 7f)), Is.EqualTo(Values(expected)));
    }

    [Test]
    public void Partition_ZeroSharesStayEmptyAtEveryPosition()
    {
        var topology = new HexMapTopology(13, 9, Layout.OddQ);
        var targets = new[]
        {
            new PerlinNoisePartitionTarget(0, 0), new PerlinNoisePartitionTarget(7, 1),
            new PerlinNoisePartitionTarget(1, 0), new PerlinNoisePartitionTarget(42, 1),
            new PerlinNoisePartitionTarget(2, 0)
        };

        var partition = topology.ToPerlinNoiseHexPartitionMap(targets, 42, 5f);

        AssertPartition(partition, topology.CreatePerlinNoise(42, 5f), targets, "zero shares");
        foreach (int index in new[] { 0, 2, 4 })
            Assert.That(partition.Cells[index].HexIndexes, Is.Empty);
    }

    [Test]
    public void Partition_ConstantField_KeepsEqualValuesTogetherAndBreaksCutTiesDownward()
    {
        var topology = new HexMapTopology(7, 5, Layout.OddR);
        var targets = new[] { new PerlinNoisePartitionTarget(7, 1), new PerlinNoisePartitionTarget(42, 1) };
        var noise = topology.CreatePerlinNoise(42, float.MaxValue);
        Assert.That(Enumerable.Range(0, topology.Count).Select(i => noise[i]).Distinct().Count(), Is.EqualTo(1));

        var partition = topology.ToPerlinNoiseHexPartitionMap(targets, 42, float.MaxValue);

        Assert.That(partition.Cells[0].HexIndexes, Is.Empty);
        Assert.That(Values(partition), Is.All.EqualTo(42));
        AssertPartition(partition, noise, targets, "constant field");
    }

    [TestCase(0, 0)]
    [TestCase(0, 3)]
    [TestCase(3, 0)]
    public void Partition_EmptyMap_RetainsAllTargets(int width, int height)
    {
        var topology = new HexMapTopology(width, height, Layout.EvenR);
        var targets = new[] { new PerlinNoisePartitionTarget(42, 0), new PerlinNoisePartitionTarget(7, 1) };

        var partition = topology.ToPerlinNoiseHexPartitionMap(targets, 42, 5f);

        Assert.That(partition.Topology, Is.EqualTo(topology));
        Assert.That(partition.Cells.Select(c => c.Id), Is.EqualTo(new[] { 42, 7 }));
        Assert.That(partition.Cells.All(c => c.HexIndexes.Count == 0), Is.True);
    }

    [Test]
    public void Partition_SingleTargetOwnsEveryHexAndResultDoesNotRetainInputList()
    {
        var topology = new HexMapTopology(5, 4, Layout.EvenQ);
        var targets = new List<PerlinNoisePartitionTarget> { new PerlinNoisePartitionTarget(42, 0.1) };

        var partition = topology.ToPerlinNoiseHexPartitionMap(targets, 42, 5f);
        targets.Clear();

        Assert.That(Values(partition), Is.All.EqualTo(42));
        Assert.That(partition.Cells.Single().HexIndexes, Has.Count.EqualTo(topology.Count));
        var other = topology.ToPerlinNoiseHexPartitionMap(new[] { new PerlinNoisePartitionTarget(42, 1) }, 42, 5f);
        partition[0] = 99;
        Assert.That(other[0], Is.EqualTo(42));
    }

    [Test]
    public void Partition_SingleHex_CanLeavePositiveShareTargetsEmpty()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var targets = new[] { new PerlinNoisePartitionTarget(7, 0.2), new PerlinNoisePartitionTarget(42, 0.8) };

        var partition = topology.ToPerlinNoiseHexPartitionMap(targets, 42, 5f);

        Assert.That(partition[0], Is.EqualTo(42));
        Assert.That(partition.Cells[0].HexIndexes, Is.Empty);
    }

    [Test]
    public void Partition_InvalidTargetListsAreRejected()
    {
        var topology = new HexMapTopology(2, 2, Layout.OddR);
        Assert.Throws<ArgumentNullException>(() => topology.ToPerlinNoiseHexPartitionMap(null!, 42, 5f));
        Assert.Throws<ArgumentException>(() => topology.ToPerlinNoiseHexPartitionMap(Array.Empty<PerlinNoisePartitionTarget>(), 42, 5f));
        Assert.Throws<ArgumentException>(() => topology.ToPerlinNoiseHexPartitionMap(new PerlinNoisePartitionTarget[] { null! }, 42, 5f));
        Assert.Throws<ArgumentException>(() => topology.ToPerlinNoiseHexPartitionMap(
            new[] { new PerlinNoisePartitionTarget(7, 1), new PerlinNoisePartitionTarget(7, 2) }, 42, 5f));
        Assert.Throws<ArgumentException>(() => topology.ToPerlinNoiseHexPartitionMap(
            new[] { new PerlinNoisePartitionTarget(7, 0), new PerlinNoisePartitionTarget(42, 0) }, 42, 5f));
    }

    [TestCase(-1d)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void Target_InvalidShareIsRejected(double share) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new PerlinNoisePartitionTarget(7, share));

    [Test]
    public void Target_NegativeIdentifierIsRejected() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new PerlinNoisePartitionTarget(-1, 1));

    [TestCase(0)]
    [TestCase(1)]
    public void Partition_ValidatesNoiseArgumentsEvenForEmptyMapsAndSingleTargets(int width)
    {
        var topology = new HexMapTopology(width, 1, Layout.OddR);
        var targets = new[] { new PerlinNoisePartitionTarget(7, 1) };
        Assert.Throws<ArgumentOutOfRangeException>(() => topology.ToPerlinNoiseHexPartitionMap(targets, 1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => topology.ToPerlinNoiseHexPartitionMap(targets, 1, 1, octaves: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => topology.ToPerlinNoiseHexPartitionMap(targets, 1, 1, persistence: float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => topology.ToPerlinNoiseHexPartitionMap(targets, 1, 1, lacunarity: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => topology.ToPerlinNoiseHexPartitionMap(targets, 1, 1,
            offset: new VectorXY(float.PositiveInfinity, 0)));
    }

    internal static int[] Values(HexPartitionMap<HexPartitionCell> partition) =>
        Enumerable.Range(0, partition.Topology.Count).Select(i => partition[i]).ToArray();

    internal static void AssertPartition(HexPartitionMap<HexPartitionCell> partition, FloatHexMap noise,
        IReadOnlyList<PerlinNoisePartitionTarget> targets, string context)
    {
        Assert.That(partition.Topology, Is.EqualTo(noise.Topology), context);
        Assert.That(partition.Cells.Select(c => c.Id), Is.EqualTo(targets.Select(t => t.Id)), context);
        Assert.That(partition.Cells.Sum(c => c.HexIndexes.Count), Is.EqualTo(noise.Topology.Count), context);
        var rebuilt = new HexPartitionMap<HexPartitionCell>(partition.Topology, partition.Cells);
        Assert.That(Values(rebuilt), Is.EqualTo(Values(partition)), context);
        foreach (var cell in partition.Cells)
        {
            var indexes = cell.HexIndexes.Select(p => p.Y * partition.Topology.Resolution.X + p.X).ToArray();
            Assert.That(indexes, Is.Ordered, context);
            Assert.That(indexes.Distinct().Count(), Is.EqualTo(indexes.Length), context);
        }

        var targetIndexes = targets.Select((target, index) => (target.Id, index)).ToDictionary(t => t.Id, t => t.index);
        var groups = Enumerable.Range(0, noise.Topology.Count).GroupBy(i => noise[i]).OrderBy(g => g.Key).ToArray();
        int previousTarget = -1;
        var attainableCounts = new List<int> { 0 };
        foreach (var group in groups)
        {
            int[] owners = group.Select(i => targetIndexes[partition[i]]).Distinct().ToArray();
            Assert.That(owners, Has.Length.EqualTo(1), $"Equal values must share an owner: {context}");
            Assert.That(owners[0], Is.GreaterThanOrEqualTo(previousTarget), $"Bands must be ordered: {context}");
            previousTarget = owners[0];
            attainableCounts.Add(attainableCounts[^1] + group.Count());
        }

        double totalShare = targets.Sum(t => t.AreaShare);
        double cumulativeShare = 0;
        int cumulativeCount = 0;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].AreaShare == 0)
                Assert.That(partition.Cells[i].HexIndexes, Is.Empty, context);
            cumulativeShare += targets[i].AreaShare;
            cumulativeCount += partition.Cells[i].HexIndexes.Count;
            double desired = cumulativeShare / totalShare * noise.Topology.Count;
            double closestError = attainableCounts.Min(count => System.Math.Abs(count - desired));
            Assert.That(System.Math.Abs(cumulativeCount - desired), Is.EqualTo(closestError).Within(1e-9),
                $"Cumulative share at target {i}: {context}");
        }
    }
}
