using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.CellularPotts;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.CellularPotts;

public class PartialCellularPottsTests
{
    [Test]
    public void Diffuse_AcceptsPartialPerlinResultAndPreservesItsUnassignedHexes()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(15, 12, Layout.OddR), 1f);
        var mask = new HexMap<int?>(geometry.Topology,
            Enumerable.Range(0, geometry.Topology.Count).Select(i => i % 15 == 7 ? null : (int?)(i % 15 < 7 ? -7 : 0)).ToArray());
        var targets = new[] { new PerlinNoisePartitionTarget(7, 1), new PerlinNoisePartitionTarget(42, 1) };
        var source = geometry.PartitionByPerlinNoise(mask, targets, new PerlinNoisePartitionParameters(7), seed: 42);
        var before = Enumerable.Range(0, geometry.Topology.Count).Select(i => source[i]).ToArray();

        var result = source.DiffuseByCellularPotts(
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
    public void Diffuse_FullyAssignedPartialSourceMatchesFullSeededEvolution(Layout layout, CellularPottsConnectivity connectivity)
    {
        var source = CellularPottsTests.Partition(new HexMapTopology(12, 8, layout), p => p.X < 6 ? 7 : 42, 7, 42, 99);
        var partial = new PartialHexPartitionMap<HexPartitionCell>(source.Topology, source.Cells);
        var parameters = new CellularPottsParameters(10, 5, 2, 0.5f, connectivity: connectivity);
        var expected = source.DiffuseByCellularPotts(parameters, new Random(42));

        var variants = new[]
        {
            partial.DiffuseByCellularPotts(parameters, new Random(42)),
            partial.DiffuseByCellularPotts(targetAreas: null, parameters, new Random(42)),
            partial.DiffuseByCellularPotts(new HexMap<int>(source.Topology), parameters, new Random(42)),
            partial.DiffuseByCellularPotts(new HexMap<int>(source.Topology), targetAreas: null, parameters, new Random(42))
        };
        foreach (var result in variants)
        {
            Assert.That(Values(result), Is.EqualTo(CellularPottsTests.Values(expected).Select(id => (int?)id)));
            Assert.That(result.Cells.Select(c => c.Id), Is.EqualTo(new[] { 7, 42, 99 }));
            for (int i = 0; i < result.Cells.Count; i++)
                Assert.That(result.Cells[i].HexIndexes, Is.EqualTo(expected.Cells[i].HexIndexes));
        }
    }

    [Test]
    public void Diffuse_CanContinueWhilePreservingUnassignedHexes()
    {
        var full = CellularPottsTests.Partition(new HexMapTopology(12, 8, Layout.OddR), p => p.X < 6 ? 7 : 42, 7, 42, 99);
        var source = new PartialHexPartitionMap<HexPartitionCell>(full.Topology,
            full.Cells.Select(c => new HexPartitionCell(c.Id, c.HexIndexes.Where(p => p.Y > 0).ToArray())).ToArray());
        var before = Values(source);
        var parameters = new CellularPottsParameters(5, 2, 1, 0.5f);
        var result = source.DiffuseByCellularPotts(parameters, new Random(42));
        var continued = result.DiffuseByCellularPotts(parameters, new Random(173));

        Assert.That(Values(result).Select(id => id.HasValue), Is.EqualTo(before.Select(id => id.HasValue)));
        Assert.That(Values(continued).Select(id => id.HasValue), Is.EqualTo(before.Select(id => id.HasValue)));
        Assert.That(Values(source), Is.EqualTo(before));
    }

    [TestCase(0, 0)]
    [TestCase(0, 3)]
    [TestCase(3, 0)]
    [TestCase(4, 3)]
    public void Diffuse_UnassignedMapRetainsEmptyCellsAndDoesNotUseRandom(int width, int height)
    {
        var topology = new HexMapTopology(width, height, Layout.OddR);
        var source = new PartialHexPartitionMap<HexPartitionCell>(topology, new[]
        {
            new HexPartitionCell(7, Array.Empty<VectorXYInt>()),
            new HexPartitionCell(42, Array.Empty<VectorXYInt>())
        });
        var parameters = new CellularPottsParameters(20);
        var result = source.DiffuseByCellularPotts(parameters, new NoDrawRandom());

        Assert.That(Values(result), Is.All.Null);
        Assert.That(result.Cells.Select(c => c.Id), Is.EqualTo(new[] { 7, 42 }));
        Assert.That(result.Cells.All(c => c.HexIndexes.Count == 0), Is.True);
        Assert.That(result.Cells[0], Is.Not.SameAs(source.Cells[0]));
        var empty = new PartialHexPartitionMap<HexPartitionCell>(topology, Array.Empty<HexPartitionCell>());
        Assert.That(Values(empty.DiffuseByCellularPotts(parameters, new NoDrawRandom())), Is.All.Null);
    }

    [Test]
    public void Diffuse_AttemptCountUsesAssignedHexesOnly()
    {
        var source = Partition(null, 7, 7, 7, null);
        var random = new FixedAttemptRandom(0, 0);
        var result = source.DiffuseByCellularPotts(new CellularPottsParameters(4), random);

        Assert.That(random.Bounds, Has.Count.EqualTo(24));
        Assert.That(random.Bounds.Where((_, i) => i % 2 == 0), Is.All.EqualTo(3));
        Assert.That(random.Bounds.Where((_, i) => i % 2 == 1), Is.All.EqualTo(1));
        Assert.That(Values(result), Is.EqualTo(new int?[] { null, 7, 7, 7, null }));
    }

    [Test]
    public void Diffuse_UnassignedHexesAreBarriers()
    {
        var source = Partition(7, 7, null, 42, 42);
        var result = source.DiffuseByCellularPotts(
            new CellularPottsParameters(100, 1e6f, 0, 0, CellularPottsConnectivity.None), new Random(42));
        Assert.That(Values(result), Is.EqualTo(Values(source)));
    }

    [Test]
    public void Diffuse_UnassignedNeighborsContributeNoBoundaryEnergy()
    {
        var topology = new HexMapTopology(5, 5, Layout.OddR);
        var center = new VectorXYInt(2, 2);
        var receiver = center.GetAdjacents(topology.Layout)[0];
        var source = new PartialHexPartitionMap<HexPartitionCell>(topology, new[]
        {
            new HexPartitionCell(7, new[] { center }),
            new HexPartitionCell(42, new[] { receiver })
        });
        var active = Enumerable.Range(0, topology.Count).Where(i => source[i].HasValue).ToList();
        var random = new FixedAttemptRandom(active.IndexOf(center.Y * 5 + center.X), 0);
        var result = source.DiffuseByCellularPotts(new CellularPottsParameters(1, 0, 1, 0, CellularPottsConnectivity.None), random);

        // The only participating edge disappears when the center copies its assigned neighbor.
        Assert.That(result[center], Is.EqualTo(42));
        Assert.That(Values(result).Count(id => id.HasValue), Is.EqualTo(2));
        Assert.That(source[center], Is.EqualTo(7));
    }

    [Test]
    public void Diffuse_AreaEnergyCountsAssignedHexesAcrossTheEntireMap()
    {
        var source = Partition(7, 42, null, 7, 7, 42);
        var targets = new Dictionary<int, float> { [7] = 3, [42] = 2 };
        var parameters = new CellularPottsParameters(1, 0, 0, 1, CellularPottsConnectivity.None);
        var result = source.DiffuseByCellularPotts(targets, parameters, new FixedAttemptRandom(0, 0));

        // Global areas already match 3/2. Per-island areas 1/1 would incorrectly accept this copy.
        Assert.That(Values(result), Is.EqualTo(Values(source)));
    }

    [Test]
    public void Diffuse_DisconnectedCellIsRejectedOnlyInPreserveMode()
    {
        var source = Partition(7, null, 7);
        Assert.That(Assert.Throws<ArgumentException>(() => source.DiffuseByCellularPotts(new CellularPottsParameters(0)))!.ParamName,
            Is.EqualTo("partition"));
        var result = source.DiffuseByCellularPotts(new CellularPottsParameters(1, connectivity: CellularPottsConnectivity.None));
        Assert.That(Values(result), Is.EqualTo(new int?[] { 7, null, 7 }));
    }

    [Test]
    public void Diffuse_ResultDoesNotRetainSourceAndHasConsistentMutableAssignments()
    {
        var source = Partition(null, 7, 7, 42, 42, null);
        var result = source.DiffuseByCellularPotts(new CellularPottsParameters(0));
        Assert.That(Values(source), Is.EqualTo(new int?[] { null, 7, 7, 42, 42, null }));
        for (int i = 0; i < source.Topology.Count; i++) source[i] = null;

        Assert.That(Values(result), Is.EqualTo(new int?[] { null, 7, 7, 42, 42, null }));
        var rebuilt = new PartialHexPartitionMap<HexPartitionCell>(result.Topology, result.Cells);
        Assert.That(Values(rebuilt), Is.EqualTo(Values(result)));
        Assert.That(result.Cells[0], Is.Not.SameAs(source.Cells[0]));
        result[1] = null;
        Assert.That(result[1], Is.Null);
    }

    [TestCase(0, 0, 42)]
    [TestCase(-7, -7, 42)]
    [TestCase(0, -7, 7)]
    [TestCase(int.MinValue, int.MaxValue, 7)]
    public void Diffuse_RegionsAllowCopiesOnlyWithinTheSameRegion(int firstRegion, int secondRegion, int expected)
    {
        var source = Partition(7, 42, null);
        var regions = new HexMap<int>(source.Topology, new[] { firstRegion, secondRegion, secondRegion });
        var result = source.DiffuseByCellularPotts(regions,
            new CellularPottsParameters(5, 0, 0, 0, CellularPottsConnectivity.None), new FixedAttemptRandom(0, 0));

        Assert.That(result[0], Is.EqualTo(expected));
        Assert.That(result[2], Is.Null);
        Assert.That(Values(source), Is.EqualTo(new int?[] { 7, 42, null }));
        Assert.That(Enumerable.Range(0, 3).Select(i => regions[i]), Is.EqualTo(new[] { firstRegion, secondRegion, secondRegion }));
    }

    [Test]
    public void Diffuse_RegionsKeepGlobalAreasAndConnectivity()
    {
        var source = Partition(7, 42, 42, 42);
        var regions = new HexMap<int>(source.Topology, new[] { 0, 0, -7, -7 });
        // The receiver spans two regions but is connected across the source map.
        Assert.DoesNotThrow(() => source.DiffuseByCellularPotts(regions, new CellularPottsParameters(0)));
        var result = source.DiffuseByCellularPotts(regions, new Dictionary<int, float> { [7] = 1, [42] = 3 },
            new CellularPottsParameters(1, 0, 0, 1, CellularPottsConnectivity.None), new FixedAttemptRandom(0, 0));

        // Global areas match the targets; per-region areas would incorrectly accept this copy.
        Assert.That(Values(result), Is.EqualTo(Values(source)));
    }

    [Test]
    public void Diffuse_RegionsKeepBoundaryEnergyAcrossTheirBorders()
    {
        var topology = new HexMapTopology(5, 5, Layout.OddR);
        var center = new VectorXYInt(2, 2);
        var receiver = center.GetAdjacents(topology.Layout)[0];
        var full = CellularPottsTests.Partition(topology, p => p == receiver ? 42 : 7, 7, 42);
        var source = new PartialHexPartitionMap<HexPartitionCell>(topology, full.Cells);
        var regions = new HexMap<int>(topology);
        regions[center] = -7;
        regions[receiver] = -7;
        var result = source.DiffuseByCellularPotts(regions,
            new CellularPottsParameters(1, 0, 1, 0, CellularPottsConnectivity.None),
            new FixedAttemptRandom(center.Y * 5 + center.X, 0));

        // Five donor edges across the region boundary outweigh the one disappearing receiver edge.
        Assert.That(result[center], Is.EqualTo(7));
    }

    [Test]
    public void Diffuse_CapturesRegionsBeforeTheFirstRandomDraw()
    {
        var source = Partition(7, 42);
        var regions = new HexMap<int>(source.Topology, new[] { 0, -7 });
        var result = source.DiffuseByCellularPotts(regions,
            new CellularPottsParameters(3, 0, 0, 0, CellularPottsConnectivity.None), new RegionChangingRandom(regions));

        Assert.That(Values(result), Is.EqualTo(new int?[] { 7, 42 }));
        Assert.That(regions[1], Is.Zero);
    }

    private sealed class RegionChangingRandom : Random
    {
        private readonly HexMap<int> _regions;
        internal RegionChangingRandom(HexMap<int> regions) => _regions = regions;
        public override int Next(int maxValue)
        {
            _regions[1] = 0;
            return 0;
        }
    }

    internal static int?[] Values(PartialHexPartitionMap<HexPartitionCell> partition) =>
        Enumerable.Range(0, partition.Topology.Count).Select(i => partition[i]).ToArray();

    private static PartialHexPartitionMap<HexPartitionCell> Partition(params int?[] assignments) =>
        new(new HexMapTopology(assignments.Length, 1, Layout.OddR),
            assignments.Where(id => id.HasValue).Select(id => id!.Value).Distinct()
                .Select(id => new HexPartitionCell(id, Enumerable.Range(0, assignments.Length)
                    .Where(i => assignments[i] == id).Select(i => new VectorXYInt(i, 0)).ToArray())).ToArray());

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
        public override int Next(int maxValue) => throw new AssertionException("An unassigned map must not draw random numbers.");
        public override double NextDouble() => throw new AssertionException("An unassigned map must not draw random numbers.");
    }
}
