using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.CellularPotts;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.CellularPotts;

public class PartialCellularPottsTests
{
    [Test]
    public void Apply_AcceptsPartialPerlinResultAndPreservesItsUnassignedHexes()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(15, 12, Layout.OddR), 1f);
        var mask = new HexMap<int?>(geometry.Topology,
            Enumerable.Range(0, geometry.Topology.Count).Select(i => i % 15 == 7 ? null : (int?)(i % 15 < 7 ? -7 : 0)).ToArray());
        var targets = new[] { new PerlinNoisePartitionTarget(7, 1), new PerlinNoisePartitionTarget(42, 1) };
        var source = geometry.ToPerlinNoiseHexPartitionMap(mask, targets, new PerlinNoisePartitionParameters(7), seed: 42);
        var before = Enumerable.Range(0, geometry.Topology.Count).Select(i => source[i]).ToArray();

        var result = source.ApplyCellularPotts(mask,
            new CellularPottsParameters(8, connectivity: CellularPottsConnectivity.None), new Random(42));

        Assert.That(Values(result).Select(id => id.HasValue), Is.EqualTo(before.Select(id => id.HasValue)));
        Assert.That(result.Cells.Select(c => c.Id), Is.EqualTo(new[] { 7, 42 }));
        Assert.That(Enumerable.Range(0, geometry.Topology.Count).Select(i => source[i]), Is.EqualTo(before));
    }

    [TestCase(Layout.OddR, CellularPottsConnectivity.Preserve)]
    [TestCase(Layout.EvenR, CellularPottsConnectivity.Preserve)]
    [TestCase(Layout.OddQ, CellularPottsConnectivity.Preserve)]
    [TestCase(Layout.EvenQ, CellularPottsConnectivity.Preserve)]
    [TestCase(Layout.OddR, CellularPottsConnectivity.None)]
    [TestCase(Layout.EvenR, CellularPottsConnectivity.None)]
    [TestCase(Layout.OddQ, CellularPottsConnectivity.None)]
    [TestCase(Layout.EvenQ, CellularPottsConnectivity.None)]
    public void Apply_FullMasksAndPartialSourceMatchOriginalSeededEvolution(Layout layout, CellularPottsConnectivity connectivity)
    {
        var source = CellularPottsTests.Partition(new HexMapTopology(12, 8, layout), p => p.X < 6 ? 7 : 42, 7, 42, 99);
        var partial = new PartialHexPartitionMap<HexPartitionCell>(source.Topology, source.Cells);
        var participation = new BoolHexMap(source.Topology, Enumerable.Repeat(true, source.Topology.Count).ToArray());
        var regions = new IntHexMap(source.Topology, Enumerable.Repeat(-7, source.Topology.Count).ToArray());
        var parameters = new CellularPottsParameters(10, 5, 2, 0.5f, connectivity: connectivity);
        var expected = source.ApplyCellularPotts(parameters, new Random(42));

        var variants = Variants(source, partial, participation, regions, parameters, includeBoolean: true).ToList();
        variants.Add(partial.ApplyCellularPotts(parameters, new Random(42)));
        foreach (var result in variants)
        {
            Assert.That(Values(result), Is.EqualTo(CellularPottsTests.Values(expected).Select(id => (int?)id)));
            Assert.That(result.Cells.Select(c => c.Id), Is.EqualTo(new[] { 7, 42, 99 }));
            for (int i = 0; i < result.Cells.Count; i++)
                Assert.That(result.Cells[i].HexIndexes, Is.EqualTo(expected.Cells[i].HexIndexes));
        }
    }

    [Test]
    public void Apply_PartialSourceIntersectsMasksAndCanContinueWithoutAMask()
    {
        var source = CellularPottsTests.Partition(new HexMapTopology(12, 8, Layout.OddR), p => p.X < 6 ? 7 : 42, 7, 42, 99);
        var partial = new PartialHexPartitionMap<HexPartitionCell>(source.Topology,
            source.Cells.Select(c => new HexPartitionCell(c.Id, c.HexIndexes.Where(p => p.Y > 0).ToArray())).ToArray());
        var participation = new BoolHexMap(source.Topology,
            Enumerable.Range(0, source.Topology.Count).Select(i => i % 12 != 0).ToArray());
        var effective = new BoolHexMap(source.Topology,
            Enumerable.Range(0, source.Topology.Count).Select(i => participation[i] && partial[i].HasValue).ToArray());
        var regions = new IntHexMap(source.Topology);
        var parameters = new CellularPottsParameters(5, 2, 1, 0.5f);
        var expected = source.ApplyCellularPotts(effective, parameters, new Random(42));

        foreach (var result in Variants(null, partial, participation, regions, parameters, includeBoolean: true))
            Assert.That(Values(result), Is.EqualTo(Values(expected)));
        var continued = expected.ApplyCellularPotts(parameters, new Random(173));
        Assert.That(Values(continued).Select(id => id.HasValue), Is.EqualTo(Values(expected).Select(id => id.HasValue)));
        Assert.That(Values(partial).Count(id => id == null), Is.EqualTo(12));
    }

    [TestCase(0, 0)]
    [TestCase(0, 3)]
    [TestCase(3, 0)]
    [TestCase(4, 3)]
    public void Apply_EmptyParticipationRetainsCellsAndDoesNotUseRandom(int width, int height)
    {
        var source = CellularPottsTests.Partition(new HexMapTopology(width, height, Layout.OddR), _ => 7, 7, 42);
        var partial = new PartialHexPartitionMap<HexPartitionCell>(source.Topology, source.Cells);
        var participation = new BoolHexMap(source.Topology);
        var regions = new IntHexMap(source.Topology);
        var parameters = new CellularPottsParameters(20);
        foreach (var result in Variants(source, partial, participation, regions, parameters, true, () => new NoDrawRandom()))
        {
            Assert.That(Values(result), Is.All.Null);
            Assert.That(result.Cells.Select(c => c.Id), Is.EqualTo(new[] { 7, 42 }));
            Assert.That(result.Cells.All(c => c.HexIndexes.Count == 0), Is.True);
            Assert.That(result.Cells[0], Is.Not.SameAs(source.Cells[0]));
        }
        var emptyPartial = new PartialHexPartitionMap<HexPartitionCell>(source.Topology, Array.Empty<HexPartitionCell>());
        Assert.That(Values(emptyPartial.ApplyCellularPotts(parameters, new NoDrawRandom())), Is.All.Null);
    }

    [Test]
    public void Apply_AttemptCountUsesParticipatingHexesOnly()
    {
        var source = CellularPottsTests.Partition(new HexMapTopology(5, 1, Layout.OddR), _ => 7, 7);
        var mask = new BoolHexMap(source.Topology, new[] { false, true, true, true, false });
        var random = new FixedAttemptRandom(0, 0);

        var result = source.ApplyCellularPotts(mask, new CellularPottsParameters(4), random);

        Assert.That(random.Bounds, Has.Count.EqualTo(24));
        Assert.That(random.Bounds.Where((_, i) => i % 2 == 0), Is.All.EqualTo(3));
        Assert.That(random.Bounds.Where((_, i) => i % 2 == 1), Is.All.EqualTo(1));
        Assert.That(Values(result), Is.EqualTo(new int?[] { null, 7, 7, 7, null }));
    }

    [Test]
    public void Apply_RegionsAreBarriersEvenWhenAdjacentAndHaveExtremeIdentifiers()
    {
        var source = CellularPottsTests.Partition(new HexMapTopology(4, 1, Layout.OddR), p => p.X < 2 ? 7 : 42, 7, 42);
        var participation = new BoolHexMap(source.Topology, new[] { true, true, true, true });
        var regions = new IntHexMap(source.Topology, new[] { int.MinValue, int.MinValue, int.MaxValue, int.MaxValue });
        var parameters = new CellularPottsParameters(100, 1e6f, 0, 0, connectivity: CellularPottsConnectivity.None);
        foreach (var result in Variants(source, new PartialHexPartitionMap<HexPartitionCell>(source.Topology, source.Cells),
            participation, regions, parameters, false))
            Assert.That(Values(result), Is.EqualTo(new int?[] { 7, 7, 42, 42 }));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Apply_ExcludedHexesAndOtherRegionsContributeNoBoundaryEnergy(bool regional)
    {
        var topology = new HexMapTopology(5, 5, Layout.OddR);
        var center = new VectorXYInt(2, 2);
        var receiver = center.GetAdjacents(topology.Layout)[0];
        var source = CellularPottsTests.Partition(topology, p => p == receiver ? 42 : 7, 7, 42);
        var participation = new BoolHexMap(topology);
        var regions = new IntHexMap(topology);
        for (int i = 0; i < topology.Count; i++)
        {
            var p = new VectorXYInt(i % 5, i / 5);
            bool selected = p == center || p == receiver;
            participation[i] = regional || selected;
            regions[i] = selected ? -7 : 0;
        }
        var parameters = new CellularPottsParameters(1, 0, 1, 0, connectivity: CellularPottsConnectivity.None);
        var random = Attempt(topology, participation, regional ? regions : null, center, receiver);
        var result = regional
            ? source.ApplyCellularPotts(participation, regions, parameters, random)
            : source.ApplyCellularPotts(participation, parameters, random);

        // Only the receiver edge contributes, so the copy is downhill despite five excluded donor neighbors.
        Assert.That(result[center], Is.EqualTo(42));
        Assert.That(source[center], Is.EqualTo(7));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Apply_AreaEnergyUsesOnlyTheParticipatingCellAreaInItsRegion(bool regional)
    {
        var topology = new HexMapTopology(8, 1, Layout.OddR);
        var source = CellularPottsTests.Partition(topology, p => p.X == 2 || p.X == 3 || p.X == 7 ? 42 : 7, 7, 42);
        var participation = new BoolHexMap(topology, Enumerable.Range(0, 8).Select(i => regional || i < 4).ToArray());
        var regions = new IntHexMap(topology, new[] { -7, -7, -7, -7, 0, 0, 0, 0 });
        var parameters = new CellularPottsParameters(1, 0, 0, 1, new Dictionary<int, float> { [7] = 2, [42] = 2 }, CellularPottsConnectivity.None);
        var random = Attempt(topology, participation, regional ? regions : null, new VectorXYInt(1, 0), new VectorXYInt(2, 0));
        var result = regional
            ? source.ApplyCellularPotts(participation, regions, parameters, random)
            : source.ApplyCellularPotts(participation, parameters, random);

        // Areas 2/2 already match the targets: a transfer costs 2 and must be rejected at zero temperature.
        Assert.That(result[1], Is.EqualTo(7));
    }

    [Test]
    public void Apply_PreserveChecksConnectivitySeparatelyInEachRegion()
    {
        var source = CellularPottsTests.Partition(new HexMapTopology(4, 1, Layout.OddR), p => p.X % 2 == 0 ? 7 : 42, 7, 42);
        var regions = new HexMap<int?>(source.Topology, new int?[] { -7, -7, 0, 0 });
        var parameters = new CellularPottsParameters(10, 0, 0, 0);
        var result = source.ApplyCellularPotts(regions, parameters, new Random(42));
        Assert.That(Values(result), Is.EqualTo(new int?[] { 7, 42, 7, 42 }));

        for (int i = 0; i < 4; i++) regions[i] = -7;
        Assert.That(Assert.Throws<ArgumentException>(() => source.ApplyCellularPotts(regions, parameters))!.ParamName, Is.EqualTo("partition"));
    }

    [Test]
    public void Apply_MaskThatDisconnectsACellIsRejectedOnlyInPreserveMode()
    {
        var source = CellularPottsTests.Partition(new HexMapTopology(3, 1, Layout.OddR), _ => 7, 7);
        var mask = new BoolHexMap(source.Topology, new[] { true, false, true });
        Assert.Throws<ArgumentException>(() => source.ApplyCellularPotts(mask, new CellularPottsParameters(0)));
        var result = source.ApplyCellularPotts(mask, new CellularPottsParameters(1, connectivity: CellularPottsConnectivity.None));
        Assert.That(Values(result), Is.EqualTo(new int?[] { 7, null, 7 }));
    }

    [Test]
    public void Apply_ResultDoesNotRetainMasksOrSourceAndHasConsistentMutableAssignments()
    {
        var source = CellularPottsTests.Partition(new HexMapTopology(6, 1, Layout.OddR), p => p.X < 3 ? 7 : 42, 7, 42, 99);
        var partial = new PartialHexPartitionMap<HexPartitionCell>(source.Topology, source.Cells);
        var mask = new BoolHexMap(source.Topology, new[] { false, true, true, true, true, false });
        var regions = new IntHexMap(source.Topology);
        var results = Variants(source, partial, mask, regions, new CellularPottsParameters(0), true).ToArray();
        Assert.That(CellularPottsTests.Values(source), Is.EqualTo(new[] { 7, 7, 7, 42, 42, 42 }));
        Assert.That(Enumerable.Range(0, 6).Select(i => mask[i]), Is.EqualTo(new[] { false, true, true, true, true, false }));
        for (int i = 0; i < 6; i++)
        {
            source[i] = 99;
            partial[i] = null;
            mask[i] = false;
            regions[i] = 123;
        }
        foreach (var result in results)
        {
            Assert.That(Values(result), Is.EqualTo(new int?[] { null, 7, 7, 42, 42, null }));
            var rebuilt = new PartialHexPartitionMap<HexPartitionCell>(result.Topology, result.Cells);
            Assert.That(Values(rebuilt), Is.EqualTo(Values(result)));
            Assert.That(result.Cells[2].HexIndexes, Is.Empty);
            result[1] = null;
            Assert.That(result[1], Is.Null);
        }
    }

    internal static IEnumerable<PartialHexPartitionMap<HexPartitionCell>> Variants(
        HexPartitionMap<HexPartitionCell>? full, PartialHexPartitionMap<HexPartitionCell> partial,
        BoolHexMap participation, IntHexMap regions, CellularPottsParameters parameters,
        bool includeBoolean, Func<Random>? randomFactory = null)
    {
        randomFactory ??= () => new Random(42);
        var nullable = new HexMap<int?>(partial.Topology,
            Enumerable.Range(0, partial.Topology.Count).Select(i => participation[i] ? (int?)regions[i] : null).ToArray());
        if (full is not null)
        {
            if (includeBoolean) yield return full.ApplyCellularPotts(participation, parameters, randomFactory());
            yield return full.ApplyCellularPotts(nullable, parameters, randomFactory());
            yield return full.ApplyCellularPotts(participation, regions, parameters, randomFactory());
        }
        if (includeBoolean) yield return partial.ApplyCellularPotts(participation, parameters, randomFactory());
        yield return partial.ApplyCellularPotts(nullable, parameters, randomFactory());
        yield return partial.ApplyCellularPotts(participation, regions, parameters, randomFactory());
    }

    internal static int?[] Values(PartialHexPartitionMap<HexPartitionCell> partition) =>
        Enumerable.Range(0, partition.Topology.Count).Select(i => partition[i]).ToArray();

    private static FixedAttemptRandom Attempt(HexMapTopology topology, BoolHexMap participation,
        IntHexMap? regions, VectorXYInt site, VectorXYInt receiver)
    {
        int width = topology.Resolution.X;
        var active = Enumerable.Range(0, topology.Count).Where(i => participation[i]).ToList();
        var neighbors = site.GetAdjacents(topology.Layout)
            .Where(p => p.X >= 0 && p.Y >= 0 && p.X < width && p.Y < topology.Resolution.Y)
            .Where(p => participation[p] && (regions is null || regions[p] == regions[site])).ToList();
        return new FixedAttemptRandom(active.IndexOf(site.Y * width + site.X), neighbors.IndexOf(receiver));
    }

    private sealed class FixedAttemptRandom : Random
    {
        private readonly int _site;
        private readonly int _neighbor;
        internal List<int> Bounds { get; } = new();
        internal FixedAttemptRandom(int site, int neighbor) { _site = site; _neighbor = neighbor; }
        public override int Next(int maxValue)
        {
            int result = Bounds.Count % 2 == 0 ? _site : _neighbor;
            Bounds.Add(maxValue);
            Assert.That(result, Is.InRange(0, maxValue - 1));
            return result;
        }
        public override double NextDouble() => throw new AssertionException("No thermal draw is expected at zero temperature.");
    }

    private sealed class NoDrawRandom : Random
    {
        public override int Next(int maxValue) => throw new AssertionException("An empty participation mask must not draw random numbers.");
        public override double NextDouble() => throw new AssertionException("An empty participation mask must not draw random numbers.");
    }
}
