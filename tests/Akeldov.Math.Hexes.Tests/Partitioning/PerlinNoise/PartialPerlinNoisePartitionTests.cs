using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.PerlinNoise;

public class PartialPerlinNoisePartitionTests
{
    private static readonly PerlinNoisePartitionTarget[] Targets =
    {
        new(7, 0.3), new(42, 0.5), new(99, 0.2), new(100, 0)
    };

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Partition_UsesOnlyParticipatingNoiseValuesForAreaShares(Layout layout)
    {
        var geometry = new HexMapGeometry(40, 31, new VectorXY(-30, 50), 2.5f, layout);
        var noise = geometry.CreatePerlinNoise(42, 19f);
        var mask = new BoolHexMap(geometry.Topology);
        // Selecting the lowest values distinguishes masked quantiles from filtering a full partition.
        foreach (int index in Enumerable.Range(0, geometry.Topology.Count).OrderBy(i => noise[i]).Take(600))
            mask[index] = true;
        var maskBefore = Enumerable.Range(0, mask.Topology.Count).Select(i => mask[i]).ToArray();

        PartialPerlinNoiseHexPartitionMap result = geometry.PartitionByPerlinNoise(mask, Targets, new PerlinNoisePartitionParameters(19f), seed: 42);

        AssertPartition(result, noise, mask, Targets, $"layout={layout}");
        for (int i = 0; i < Targets.Length; i++)
            Assert.That(result.Cells[i].HexIndexes.Count / 600d, Is.EqualTo(Targets[i].AreaShare).Within(0.01));
        Assert.That(Enumerable.Range(0, mask.Topology.Count).Select(i => mask[i]), Is.EqualTo(maskBefore));
        var originalAssignments = Values(result);
        var originalIndexes = result.Cells.SelectMany(cell => cell.HexIndexes).ToArray();
        for (int i = 0; i < mask.Topology.Count; i++)
            mask[i] = !mask[i];
        Assert.That(Values(result), Is.EqualTo(originalAssignments));
        Assert.That(result.Cells.SelectMany(cell => cell.HexIndexes), Is.EqualTo(originalIndexes));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Partition_WithFullMask_MatchesFullPartition(Layout layout)
    {
        var geometry = new HexMapGeometry(20, 18, new VectorXY(-10, 15), 2f, layout);
        var mask = new BoolHexMap(geometry.Topology, Enumerable.Repeat(true, geometry.Topology.Count).ToArray());
        var offset = new VectorXY(2.5f, -7f);
        var parameters = new PerlinNoisePartitionParameters(9f, 3, 0.4f, 2.25f, offset);
        var full = geometry.PartitionByPerlinNoise(Targets, parameters, seed: 173);

        var partial = geometry.PartitionByPerlinNoise(mask, Targets, parameters, seed: 173);

        Assert.That(Values(partial), Is.EqualTo(PerlinNoisePartitionTests.Values(full).Select(id => (int?)id)));
        for (int i = 0; i < Targets.Length; i++)
            Assert.That(partial.Cells[i].HexIndexes, Is.EqualTo(full.Cells[i].HexIndexes));
    }

    [TestCase(0, 0)]
    [TestCase(0, 3)]
    [TestCase(3, 0)]
    [TestCase(4, 3)]
    public void Partition_WithNoParticipants_RetainsEmptyTargets(int width, int height)
    {
        var geometry = new HexMapGeometry(new HexMapTopology(width, height, Layout.EvenR), 1f);
        var mask = new BoolHexMap(geometry.Topology);

        var result = geometry.PartitionByPerlinNoise(mask, Targets, new PerlinNoisePartitionParameters(7f), seed: 42);

        Assert.That(result.Geometry, Is.EqualTo(geometry));
        Assert.That(result.Cells.Select(cell => cell.Id), Is.EqualTo(Targets.Select(target => target.Id)));
        Assert.That(result.Cells.All(cell => cell.HexIndexes.Count == 0), Is.True);
        Assert.That(Values(result), Is.All.Null);
    }

    [Test]
    public void Partition_WithOneParticipant_LeavesOtherHexesUnassigned()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(5, 4, Layout.OddR), 1f);
        var mask = new BoolHexMap(geometry.Topology);
        mask[7] = true;
        var targets = new[] { new PerlinNoisePartitionTarget(9, 0.2), new PerlinNoisePartitionTarget(17, 0.8) };

        var result = geometry.PartitionByPerlinNoise(mask, targets, new PerlinNoisePartitionParameters(7f), seed: 42);

        Assert.That(result[7], Is.EqualTo(17));
        Assert.That(result.Cells[0].HexIndexes, Is.Empty);
        Assert.That(result.Cells[1].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(2, 1) }));
        Assert.That(Values(result).Where((_, index) => index != 7), Is.All.Null);
    }

    [Test]
    public void Partition_WithConstantNoise_KeepsEqualValuesTogetherAndBreaksTiesDownward()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(5, 4, Layout.OddQ), 1f);
        var mask = new BoolHexMap(geometry.Topology);
        mask[1] = mask[7] = mask[12] = mask[19] = true;
        var targets = new[] { new PerlinNoisePartitionTarget(9, 1), new PerlinNoisePartitionTarget(17, 1) };

        var result = geometry.PartitionByPerlinNoise(mask, targets, new PerlinNoisePartitionParameters(float.MaxValue), seed: 42);

        Assert.That(result.Cells[0].HexIndexes, Is.Empty);
        Assert.That(result.Cells[1].HexIndexes, Has.Count.EqualTo(4));
        AssertPartition(result, geometry.CreatePerlinNoise(42, float.MaxValue), mask, targets, "constant field");
    }

    [Test]
    public void Partition_WithSingleTarget_CopiesInputsAndReturnsMutablePartialMap()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(3, 2, Layout.OddR), 1f);
        var mask = new BoolHexMap(geometry.Topology, new[] { true, false, true, false, true, false });
        var targets = new List<PerlinNoisePartitionTarget> { new(42, 1) };

        var result = geometry.PartitionByPerlinNoise(mask, targets, new PerlinNoisePartitionParameters(5f), seed: 42);

        IPartialHexPartitionMap<HexPartitionCell> partial = result;
        SpatialPartialHexPartitionMap<HexPartitionCell> spatialPartition = result;
        ISpatialHexMap<int?> spatial = result;
        Assert.That(spatial.Geometry, Is.EqualTo(geometry));
        Assert.That(spatialPartition.Geometry, Is.EqualTo(result.Geometry));
        Assert.That(spatialPartition.Cells, Is.SameAs(result.Cells));
        Assert.That(Values(result), Is.EqualTo(new int?[] { 42, null, 42, null, 42, null }));
        targets.Clear();
        result[0] = null;
        result[1] = 42;
        Assert.That(partial[0], Is.Null);
        Assert.That(spatial[1], Is.EqualTo(42));
        Assert.That(result.Cells.Single().HexIndexes, Has.Count.EqualTo(3));
        Assert.That(mask[0], Is.True);
        Assert.That(mask[1], Is.False);
    }

    [Test]
    public void Partition_WhenMaskIsNullOrTopologyDiffers_Throws()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(3, 2, Layout.OddR), 1f);
        Assert.That(Assert.Throws<ArgumentNullException>(() =>
            geometry.PartitionByPerlinNoise(participationMask: null!, Targets, new PerlinNoisePartitionParameters(5f), seed: 42))!.ParamName, Is.EqualTo("participationMask"));
        foreach (var topology in new[] { new HexMapTopology(2, 3, Layout.OddR), new HexMapTopology(3, 2, Layout.EvenR) })
            Assert.That(Assert.Throws<ArgumentException>(() =>
                geometry.PartitionByPerlinNoise(new BoolHexMap(topology), Targets, new PerlinNoisePartitionParameters(5f), seed: 42))!.ParamName,
                Is.EqualTo("participationMask"));
    }

    [Test]
    public void Partition_WithEmptyMask_StillValidatesTargetsGeometryAndNoise()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(3, 2, Layout.OddR), 1f);
        var mask = new BoolHexMap(geometry.Topology);
        Assert.Throws<ArgumentNullException>(() => geometry.PartitionByPerlinNoise(mask, null!, new PerlinNoisePartitionParameters(5f), seed: 42));
        foreach (var targets in new[]
        {
            Array.Empty<PerlinNoisePartitionTarget>(), new PerlinNoisePartitionTarget[] { null! },
            new[] { Targets[0], Targets[0] }, new[] { new PerlinNoisePartitionTarget(7, 0) }
        })
            Assert.Throws<ArgumentException>(() => geometry.PartitionByPerlinNoise(mask, targets, new PerlinNoisePartitionParameters(5f), seed: 42));
        Assert.Throws<ArgumentOutOfRangeException>(() => default(HexMapGeometry)
            .PartitionByPerlinNoise(new BoolHexMap(default), Targets, new PerlinNoisePartitionParameters(5f), seed: 42));
        Assert.Throws<ArgumentOutOfRangeException>(() => geometry.PartitionByPerlinNoise(mask, Targets, new PerlinNoisePartitionParameters(0), seed: 42));
        Assert.Throws<ArgumentOutOfRangeException>(() => geometry.PartitionByPerlinNoise(mask, Targets, new PerlinNoisePartitionParameters(5f, octaves: 0), seed: 42));
        Assert.Throws<ArgumentOutOfRangeException>(() => geometry.PartitionByPerlinNoise(mask, Targets, new PerlinNoisePartitionParameters(5f, persistence: float.NaN), seed: 42));
        Assert.Throws<ArgumentOutOfRangeException>(() => geometry.PartitionByPerlinNoise(mask, Targets, new PerlinNoisePartitionParameters(5f, lacunarity: 0), seed: 42));
        Assert.Throws<ArgumentOutOfRangeException>(() => geometry.PartitionByPerlinNoise(mask, Targets, new PerlinNoisePartitionParameters(5f, offset: new VectorXY(float.PositiveInfinity, 0)), seed: 42));
    }

    internal static int?[] Values(PartialPerlinNoiseHexPartitionMap partition) =>
        Enumerable.Range(0, partition.Topology.Count).Select(i => partition[i]).ToArray();

    internal static void AssertPartition(PartialPerlinNoiseHexPartitionMap partition, SpatialFloatHexMap noise,
        IHexMap<bool> mask, IReadOnlyList<PerlinNoisePartitionTarget> targets, string context)
    {
        Assert.That(partition.Geometry, Is.EqualTo(noise.Geometry), context);
        Assert.That(partition.Cells.Select(cell => cell.Id), Is.EqualTo(targets.Select(target => target.Id)), context);
        var participants = Enumerable.Range(0, mask.Topology.Count).Where(i => mask[i]).ToArray();
        Assert.That(partition.Cells.Sum(cell => cell.HexIndexes.Count), Is.EqualTo(participants.Length), context);
        var rebuilt = new PartialHexPartitionMap<HexPartitionCell>(partition.Topology, partition.Cells);
        for (int i = 0; i < mask.Topology.Count; i++)
        {
            Assert.That(partition[i].HasValue, Is.EqualTo(mask[i]), $"index={i}, {context}");
            Assert.That(partition[i], Is.EqualTo(rebuilt[i]), $"index={i}, {context}");
        }
        foreach (var cell in partition.Cells)
        {
            var indexes = cell.HexIndexes.Select(p => p.Y * partition.Topology.Resolution.X + p.X).ToArray();
            Assert.That(indexes, Is.Ordered, context);
            Assert.That(indexes.Distinct().Count(), Is.EqualTo(indexes.Length), context);
        }
        var order = targets.Select((target, i) => (target.Id, Index: i)).ToDictionary(t => t.Id, t => t.Index);
        int previous = -1;
        var attainableCounts = new List<int> { 0 };
        foreach (var group in participants.GroupBy(i => noise[i]).OrderBy(group => group.Key))
        {
            var owners = group.Select(i => order[partition[i]!.Value]).Distinct().ToArray();
            Assert.That(owners, Has.Length.EqualTo(1), context);
            Assert.That(owners[0], Is.GreaterThanOrEqualTo(previous), context);
            previous = owners[0];
            attainableCounts.Add(attainableCounts[^1] + group.Count());
        }
        double total = targets.Sum(target => target.AreaShare);
        double share = 0;
        int count = 0;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].AreaShare == 0)
                Assert.That(partition.Cells[i].HexIndexes, Is.Empty, context);
            share += targets[i].AreaShare;
            count += partition.Cells[i].HexIndexes.Count;
            double desired = share / total * participants.Length;
            // Share normalization can move a half-integer target by a few floating-point ulps.
            // Compare the quantile error, as in full-partition tests; exact ties have a dedicated test.
            double closestError = attainableCounts.Min(n => System.Math.Abs(n - desired));
            Assert.That(System.Math.Abs(count - desired), Is.EqualTo(closestError).Within(1e-9),
                $"target={i}, {context}");
        }
    }
}
