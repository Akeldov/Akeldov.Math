using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.PerlinNoise;

public class RegionalPerlinNoisePartitionTests
{
    private static readonly PerlinNoisePartitionTarget[] Targets =
    {
        new(42, 0.5), new(7, 0.25), new(99, 0.25), new(100, 0)
    };

    private static readonly PerlinNoisePartitionParameters Parameters = new(42, 19f, 3, 0.4f, 2.25f, new VectorXY(2, -7));

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Partition_CalculatesSharesIndependentlyWithinEachRegion(Layout layout)
    {
        var geometry = new HexMapGeometry(40, 31, new VectorXY(-30, 50), 2.5f, layout);
        var noise = geometry.CreatePerlinNoise(Parameters.Seed, Parameters.Scale, Parameters.Octaves,
            Parameters.Persistence, Parameters.Lacunarity, Parameters.Offset);
        var participation = new BoolHexMap(geometry.Topology);
        var regions = new IntHexMap(geometry.Topology);
        var nullableRegions = new HexMap<int?>(geometry.Topology);
        var sortedIndexes = Enumerable.Range(0, geometry.Topology.Count).OrderBy(i => noise[i]).ToArray();
        // Each region occupies a different noise range, exposing accidental use of global quantiles.
        for (int rank = 0; rank < sortedIndexes.Length; rank++)
        {
            int index = sortedIndexes[rank];
            participation[index] = rank % 5 != 0;
            regions[index] = rank < 400 ? int.MinValue : rank < 800 ? 0 : int.MaxValue;
            nullableRegions[index] = participation[index] ? regions[index] : null;
        }

        var combined = geometry.ToPerlinNoiseHexPartitionMap(Targets, participation, regions, Parameters);
        var nullable = geometry.ToPerlinNoiseHexPartitionMap(Targets, nullableRegions, Parameters);

        Assert.That(PartialPerlinNoisePartitionTests.Values(nullable), Is.EqualTo(PartialPerlinNoisePartitionTests.Values(combined)));
        AssertRegionalPartition(combined, geometry, Targets, participation, regions, Parameters, $"layout={layout}");
        foreach (int region in new[] { int.MinValue, 0, int.MaxValue })
        {
            var indexes = Enumerable.Range(0, geometry.Topology.Count).Where(i => nullableRegions[i] == region).ToArray();
            foreach (var target in Targets)
                Assert.That(indexes.Count(i => combined[i] == target.Id),
                    Is.EqualTo(indexes.Length * target.AreaShare).Within(1), $"layout={layout}, region={region}, target={target.Id}");
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Partition_SingleRegionMatchesBooleanMask(bool fullMask)
    {
        var geometry = new HexMapGeometry(new HexMapTopology(12, 9, Layout.OddR), 1f);
        var participation = new BoolHexMap(geometry.Topology,
            Enumerable.Range(0, geometry.Topology.Count).Select(i => fullMask || i % 3 == 0).ToArray());
        var regions = new IntHexMap(geometry.Topology, Enumerable.Repeat(-17, geometry.Topology.Count).ToArray());
        var nullableRegions = new HexMap<int?>(geometry.Topology,
            Enumerable.Range(0, geometry.Topology.Count).Select(i => participation[i] ? (int?)-17 : null).ToArray());
        var expected = geometry.ToPerlinNoiseHexPartitionMap(Targets, participation, Parameters);

        foreach (var actual in new[]
        {
            geometry.ToPerlinNoiseHexPartitionMap(Targets, participation, regions, Parameters),
            geometry.ToPerlinNoiseHexPartitionMap(Targets, nullableRegions, Parameters)
        })
        {
            Assert.That(PartialPerlinNoisePartitionTests.Values(actual), Is.EqualTo(PartialPerlinNoisePartitionTests.Values(expected)));
            for (int i = 0; i < Targets.Length; i++)
                Assert.That(actual.Cells[i].HexIndexes, Is.EqualTo(expected.Cells[i].HexIndexes));
        }
    }

    [Test]
    public void Partition_DisconnectedHexesWithSameRegionIdShareQuantiles()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(9, 1, Layout.OddR), 1f);
        var regions = new HexMap<int?>(geometry.Topology);
        regions[0] = regions[8] = -7;
        var targets = new[] { new PerlinNoisePartitionTarget(9, 1), new PerlinNoisePartitionTarget(17, 1) };

        var result = geometry.ToPerlinNoiseHexPartitionMap(targets, regions, new PerlinNoisePartitionParameters(42, 7f));

        Assert.That(result.Cells.Select(cell => cell.HexIndexes.Count), Is.EqualTo(new[] { 1, 1 }));
        Assert.That(Enumerable.Range(1, 7).Select(i => result[i]), Is.All.Null);
        Assert.That(new[] { result[0], result[8] }, Is.EquivalentTo(new int?[] { 9, 17 }));
    }

    [TestCase(0, 0)]
    [TestCase(0, 3)]
    [TestCase(3, 0)]
    [TestCase(4, 3)]
    public void Partition_NoParticipantsRetainsEmptyTargets(int width, int height)
    {
        var geometry = new HexMapGeometry(new HexMapTopology(width, height, Layout.EvenQ), 1f);
        foreach (var result in new[]
        {
            geometry.ToPerlinNoiseHexPartitionMap(Targets, new HexMap<int?>(geometry.Topology), Parameters),
            geometry.ToPerlinNoiseHexPartitionMap(Targets, new BoolHexMap(geometry.Topology), new IntHexMap(geometry.Topology), Parameters)
        })
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result.Cells.Select(cell => cell.Id), Is.EqualTo(Targets.Select(target => target.Id)));
            Assert.That(result.Cells.All(cell => cell.HexIndexes.Count == 0), Is.True);
            Assert.That(PartialPerlinNoisePartitionTests.Values(result), Is.All.Null);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Partition_ConstantNoisePreservesTiesWithinEveryRegion(bool singleTarget)
    {
        var geometry = new HexMapGeometry(new HexMapTopology(6, 1, Layout.EvenR), 1f);
        var regions = new IntHexMap(geometry.Topology, new[] { -7, 0, -7, 0, 42, 42 });
        var participation = new BoolHexMap(geometry.Topology, new[] { true, true, true, true, true, false });
        var nullableRegions = new HexMap<int?>(geometry.Topology, new int?[] { -7, 0, -7, 0, 42, null });
        var targets = singleTarget
            ? new[] { new PerlinNoisePartitionTarget(17, 1) }
            : new[] { new PerlinNoisePartitionTarget(9, 1), new PerlinNoisePartitionTarget(17, 1), new PerlinNoisePartitionTarget(99, 0) };
        var parameters = new PerlinNoisePartitionParameters(42, float.MaxValue);
        foreach (var result in new[]
        {
            geometry.ToPerlinNoiseHexPartitionMap(targets, nullableRegions, parameters),
            geometry.ToPerlinNoiseHexPartitionMap(targets, participation, regions, parameters)
        })
        {
            Assert.That(PartialPerlinNoisePartitionTests.Values(result), Is.EqualTo(new int?[] { 17, 17, 17, 17, 17, null }));
            Assert.That(result.Cells.Where(cell => cell.Id != 17).All(cell => cell.HexIndexes.Count == 0), Is.True);
        }
    }

    [Test]
    public void Partition_MasksAndTargetsAreNotModifiedOrRetained()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(6, 1, Layout.EvenR), 1f);
        var targets = Targets.ToList();
        var participation = new BoolHexMap(geometry.Topology, new[] { true, false, true, true, false, true });
        var regions = new IntHexMap(geometry.Topology, new[] { -7, 0, 0, -7, 7, 0 });
        var nullableValues = new int?[] { -7, null, 0, -7, null, 0 };
        var nullableRegions = new HexMap<int?>(geometry.Topology, nullableValues.ToArray());
        var combined = geometry.ToPerlinNoiseHexPartitionMap(targets, participation, regions, Parameters);
        var nullable = geometry.ToPerlinNoiseHexPartitionMap(targets, nullableRegions, Parameters);
        var assignments = PartialPerlinNoisePartitionTests.Values(combined);
        var indexes = combined.Cells.SelectMany(cell => cell.HexIndexes).ToArray();

        Assert.That(Enumerable.Range(0, 6).Select(i => participation[i]), Is.EqualTo(new[] { true, false, true, true, false, true }));
        Assert.That(Enumerable.Range(0, 6).Select(i => regions[i]), Is.EqualTo(new[] { -7, 0, 0, -7, 7, 0 }));
        Assert.That(Enumerable.Range(0, 6).Select(i => nullableRegions[i]), Is.EqualTo(nullableValues));
        targets.Clear();
        for (int i = 0; i < 6; i++)
        {
            participation[i] = !participation[i];
            regions[i] = int.MaxValue;
            nullableRegions[i] = null;
        }
        foreach (var result in new[] { combined, nullable })
        {
            Assert.That(PartialPerlinNoisePartitionTests.Values(result), Is.EqualTo(assignments));
            Assert.That(result.Cells.SelectMany(cell => cell.HexIndexes), Is.EqualTo(indexes));
            Assert.That(result.Cells.Select(cell => cell.Id), Is.EqualTo(Targets.Select(target => target.Id)));
        }
    }

    [Test]
    public void Partition_NullArgumentsAreRejected()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(3, 2, Layout.OddR), 1f);
        var participation = new BoolHexMap(geometry.Topology);
        var regions = new IntHexMap(geometry.Topology);
        var nullableRegions = new HexMap<int?>(geometry.Topology);
        Assert.That(Assert.Throws<ArgumentNullException>(() => geometry.ToPerlinNoiseHexPartitionMap(Targets, regionsMask: null!, Parameters))!.ParamName, Is.EqualTo("regionsMask"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => geometry.ToPerlinNoiseHexPartitionMap(Targets, null!, regions, Parameters))!.ParamName, Is.EqualTo("participationMask"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => geometry.ToPerlinNoiseHexPartitionMap(Targets, participation, null!, Parameters))!.ParamName, Is.EqualTo("regionsMask"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => geometry.ToPerlinNoiseHexPartitionMap(Targets, nullableRegions, null!))!.ParamName, Is.EqualTo("parameters"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => geometry.ToPerlinNoiseHexPartitionMap(Targets, participation, regions, null!))!.ParamName, Is.EqualTo("parameters"));
        Assert.Throws<ArgumentNullException>(() => geometry.ToPerlinNoiseHexPartitionMap(null!, nullableRegions, Parameters));
        Assert.Throws<ArgumentNullException>(() => geometry.ToPerlinNoiseHexPartitionMap(null!, participation, regions, Parameters));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Partition_IncompatibleTopologyIsRejected(bool differentLayout)
    {
        var geometry = new HexMapGeometry(new HexMapTopology(3, 2, Layout.OddR), 1f);
        var other = differentLayout ? new HexMapTopology(3, 2, Layout.EvenR) : new HexMapTopology(2, 3, Layout.OddR);
        Assert.That(Assert.Throws<ArgumentException>(() => geometry.ToPerlinNoiseHexPartitionMap(Targets, new HexMap<int?>(other), Parameters))!.ParamName, Is.EqualTo("regionsMask"));
        Assert.That(Assert.Throws<ArgumentException>(() => geometry.ToPerlinNoiseHexPartitionMap(Targets,
            new BoolHexMap(other), new IntHexMap(geometry.Topology), Parameters))!.ParamName, Is.EqualTo("participationMask"));
        Assert.That(Assert.Throws<ArgumentException>(() => geometry.ToPerlinNoiseHexPartitionMap(Targets,
            new BoolHexMap(geometry.Topology), new IntHexMap(other), Parameters))!.ParamName, Is.EqualTo("regionsMask"));
    }

    [Test]
    public void Partition_EmptyMasksStillValidateTargetsAndGeometry()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(3, 2, Layout.OddR), 1f);
        foreach (var targets in new[]
        {
            Array.Empty<PerlinNoisePartitionTarget>(), new PerlinNoisePartitionTarget[] { null! },
            new[] { Targets[0], Targets[0] }, new[] { new PerlinNoisePartitionTarget(7, 0) }
        })
        {
            Assert.Throws<ArgumentException>(() => geometry.ToPerlinNoiseHexPartitionMap(targets, new HexMap<int?>(geometry.Topology), Parameters));
            Assert.Throws<ArgumentException>(() => geometry.ToPerlinNoiseHexPartitionMap(targets,
                new BoolHexMap(geometry.Topology), new IntHexMap(geometry.Topology), Parameters));
        }
        Assert.Throws<ArgumentOutOfRangeException>(() => default(HexMapGeometry)
            .ToPerlinNoiseHexPartitionMap(Targets, new HexMap<int?>(default), Parameters));
        Assert.Throws<ArgumentOutOfRangeException>(() => default(HexMapGeometry)
            .ToPerlinNoiseHexPartitionMap(Targets, new BoolHexMap(default), new IntHexMap(default), Parameters));
    }

    internal static void AssertRegionalPartition(PartialPerlinNoiseHexPartitionMap partition, HexMapGeometry geometry,
        IReadOnlyList<PerlinNoisePartitionTarget> targets, IHexMap<bool> participation, IHexMap<int> regions,
        PerlinNoisePartitionParameters parameters, string context)
    {
        var indexes = Enumerable.Range(0, geometry.Topology.Count).ToArray();
        var rebuilt = new PartialHexPartitionMap<HexPartitionCell>(geometry.Topology, partition.Cells);
        Assert.That(partition.Geometry, Is.EqualTo(geometry), context);
        Assert.That(partition.Cells.Select(cell => cell.Id), Is.EqualTo(targets.Select(target => target.Id)), context);
        Assert.That(partition.Cells.Sum(cell => cell.HexIndexes.Count), Is.EqualTo(indexes.Count(i => participation[i])), context);
        foreach (int i in indexes)
        {
            Assert.That(partition[i].HasValue, Is.EqualTo(participation[i]), $"index={i}, {context}");
            Assert.That(partition[i], Is.EqualTo(rebuilt[i]), $"index={i}, {context}");
        }
        foreach (var cell in partition.Cells)
            Assert.That(cell.HexIndexes.Select(p => p.Y * geometry.Topology.Resolution.X + p.X), Is.Ordered, context);
        foreach (int region in indexes.Where(i => participation[i]).Select(i => regions[i]).Distinct())
        {
            var regionMask = new BoolHexMap(geometry.Topology, indexes.Select(i => participation[i] && regions[i] == region).ToArray());
            var expected = geometry.ToPerlinNoiseHexPartitionMap(targets, regionMask, parameters);
            foreach (int i in indexes.Where(i => regionMask[i]))
                Assert.That(partition[i], Is.EqualTo(expected[i]), $"index={i}, region={region}, {context}");
        }
    }
}
