using Akeldov.Math.Hexes.Partitioning.CellularPotts;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.CellularPotts;

public class CellularPottsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Apply_AtZeroSteps_CopiesSparseIdsEmptyCellsAndDuplicateIndexes(Layout layout)
    {
        var topology = new HexMapTopology(2, 1, layout);
        var source = new HexPartitionMap<HexPartitionCell>(topology, new[]
        {
            new HexPartitionCell(int.MaxValue, new[] { new VectorXYInt(0, 0), new VectorXYInt(0, 0) }),
            new HexPartitionCell(7, Array.Empty<VectorXYInt>()),
            new HexPartitionCell(42, new[] { new VectorXYInt(1, 0) })
        });

        var result = source.ApplyCellularPotts(new CellularPottsParameters(0));

        Assert.That(result, Is.Not.SameAs(source));
        Assert.That(result.Topology, Is.EqualTo(topology));
        Assert.That(result.Cells.Select(c => c.Id), Is.EqualTo(new[] { int.MaxValue, 7, 42 }));
        Assert.That(result.Cells[0], Is.Not.SameAs(source.Cells[0]));
        Assert.That(result.Cells[0].HexIndexes, Has.Count.EqualTo(1));
        Assert.That(result.Cells[1].HexIndexes, Is.Empty);
        Assert.That(Values(result), Is.EqualTo(Values(source)));
        result[0] = 42;
        Assert.That(source[0], Is.EqualTo(int.MaxValue));
        Assert.That(source.Cells[0].HexIndexes, Has.Count.EqualTo(2));
    }

    [TestCase(0, 0)]
    [TestCase(0, 3)]
    [TestCase(3, 0)]
    [TestCase(1, 1)]
    public void Apply_EmptyAndSingleCellMaps_AreUnchanged(int width, int height)
    {
        var source = Partition(new HexMapTopology(width, height, Layout.OddR), _ => 7, 7, 42);
        var result = source.ApplyCellularPotts(new CellularPottsParameters(10));

        Assert.That(Values(result), Is.EqualTo(Values(source)));
        Assert.That(result.Cells[1].HexIndexes, Is.Empty);
    }

    [TestCase(CellularPottsConnectivity.Preserve)]
    [TestCase(CellularPottsConnectivity.None)]
    public void Apply_SingletonCell_DisappearsOnlyWithoutConnectivityPreservation(CellularPottsConnectivity connectivity)
    {
        var source = Partition(new HexMapTopology(3, 1, Layout.OddR), p => p.X == 0 ? 7 : 42, 7, 42);
        var random = new FixedAttemptRandom(0, 0);
        var parameters = new CellularPottsParameters(4, 0, 0, 1, new Dictionary<int, double> { [7] = 0, [42] = 3 }, connectivity);

        var result = source.ApplyCellularPotts(parameters, random: random);

        bool preserve = connectivity == CellularPottsConnectivity.Preserve;
        Assert.That(result[0], Is.EqualTo(preserve ? 7 : 42));
        Assert.That(result.Cells[0].HexIndexes, Has.Count.EqualTo(preserve ? 1 : 0));
        Assert.That(result.Cells.Select(c => c.Id), Is.EqualTo(new[] { 7, 42 }));
        Assert.That(source[0], Is.EqualTo(7));
        Assert.That(random.SiteDraws, Is.EqualTo(12));
        var continued = result.ApplyCellularPotts(parameters, random: new Random(42));
        Assert.That(continued.Cells[0].HexIndexes, Has.Count.EqualTo(preserve ? 1 : 0));
    }

    [TestCase(CellularPottsConnectivity.Preserve)]
    [TestCase(CellularPottsConnectivity.None)]
    public void Apply_DonorBridge_IsProtectedOnlyWithConnectivityPreservation(CellularPottsConnectivity connectivity)
    {
        var center = new VectorXYInt(2, 2);
        var neighbors = center.GetAdjacents(Layout.OddR);
        var donor = new HashSet<VectorXYInt> { center, neighbors[0], neighbors[3] };
        var source = Partition(new HexMapTopology(5, 5, Layout.OddR), p => donor.Contains(p) ? 7 : 42, 7, 42);
        var random = Attempt(source, center, 42);

        var result = source.ApplyCellularPotts(new CellularPottsParameters(1, 1e100, 0, 0, connectivity: connectivity), random: random);

        bool preserve = connectivity == CellularPottsConnectivity.Preserve;
        Assert.That(result[center], Is.EqualTo(preserve ? 7 : 42));
        Assert.That(result.Cells[0].HexIndexes, Has.Count.EqualTo(preserve ? 3 : 2));
        if (!preserve)
            Assert.That(result.Cells[0].HexIndexes, Is.EquivalentTo(new[] { neighbors[0], neighbors[3] }));
        Assert.That(source[center], Is.EqualTo(7));
        Assert.That(random.EnergyDraws, Is.Zero);
    }

    [TestCase(CellularPottsConnectivity.Preserve)]
    [TestCase(CellularPottsConnectivity.None)]
    public void Apply_TargetWithSeparateLocalArcs_CanReceiveAHexInBothModes(CellularPottsConnectivity connectivity)
    {
        var center = new VectorXYInt(2, 2);
        var neighbors = center.GetAdjacents(Layout.OddR);
        var donor = new HashSet<VectorXYInt> { center, neighbors[0], neighbors[1] };
        var third = new HashSet<VectorXYInt> { neighbors[3] };
        var source = Partition(new HexMapTopology(5, 5, Layout.OddR),
            p => donor.Contains(p) ? 7 : third.Contains(p) ? 99 : 42, 7, 42, 99);
        var random = Attempt(source, center, 42);

        var result = source.ApplyCellularPotts(new CellularPottsParameters(1, 1e100, 0, 0, connectivity: connectivity), random: random);

        Assert.That(result[center], Is.EqualTo(42));
        Assert.That(random.EnergyDraws, Is.Zero);
        AssertConsistentAndConnected(result);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Apply_Preserve_RejectsRemovalWhenTheOnlyAlternatePathLeavesTheNeighborhood(Layout layout)
    {
        var topology = new HexMapTopology(5, 5, layout);
        var center = new VectorXYInt(2, 2);
        var ring = center.GetAdjacents(layout).ToHashSet();
        var site = center.GetAdjacents(layout)[0];
        var source = Partition(topology, p => ring.Contains(p) ? 7 : p == center ? 99 : 42, 7, 42, 99);

        var result = source.ApplyCellularPotts(new CellularPottsParameters(1, 0, 0, 0), random: Attempt(source, site, 42));

        Assert.That(result[site], Is.EqualTo(7));
        Assert.That(result.Cells[0].HexIndexes, Has.Count.EqualTo(6));
        Assert.That(result[center], Is.EqualTo(99));
        AssertConsistentAndConnected(result);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Apply_Preserve_MapBoundaryDoesNotJoinDisconnectedNeighborArcs(Layout layout)
    {
        var site = new VectorXYInt(2, 0);
        var donor = new HashSet<VectorXYInt> { site, new VectorXYInt(1, 0), new VectorXYInt(3, 0) };
        var source = Partition(new HexMapTopology(5, 5, layout), p => donor.Contains(p) ? 7 : 42, 7, 42);

        var result = source.ApplyCellularPotts(new CellularPottsParameters(1, 0, 0, 0), random: Attempt(source, site, 42));

        Assert.That(Values(result), Is.EqualTo(Values(source)));
        AssertConsistentAndConnected(result);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Apply_Preserve_NeighborDirectionsFiveAndZeroFormOneConnectedRun(Layout layout)
    {
        var site = new VectorXYInt(2, 2);
        var neighbors = site.GetAdjacents(layout);
        var donor = new HashSet<VectorXYInt> { site, neighbors[5], neighbors[0] };
        var source = Partition(new HexMapTopology(5, 5, layout), p => donor.Contains(p) ? 7 : 42, 7, 42);

        var result = source.ApplyCellularPotts(new CellularPottsParameters(1, 0, 0, 0), random: Attempt(source, site, 42));

        Assert.That(result[site], Is.EqualTo(42));
        Assert.That(result.Cells[0].HexIndexes, Is.EquivalentTo(new[] { neighbors[5], neighbors[0] }));
        AssertConsistentAndConnected(result);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Apply_LegalNeutralCopy_UpdatesAssignmentsAndCellSnapshots(Layout layout)
    {
        var source = Partition(new HexMapTopology(5, 5, layout), p => p.X < 2 ? 7 : 42, 7, 42, 99);
        var before = Values(source);
        var site = new VectorXYInt(1, 2);
        var random = Attempt(source, site, 42);

        var result = source.ApplyCellularPotts(new CellularPottsParameters(2, 0, 0, 0), random: random);

        Assert.That(result[site], Is.EqualTo(42));
        Assert.That(Values(source), Is.EqualTo(before));
        Assert.That(result.Cells[0].HexIndexes, Has.Count.EqualTo(9));
        Assert.That(result.Cells[1].HexIndexes, Has.Count.EqualTo(16));
        Assert.That(result.Cells[2].HexIndexes, Is.Empty);
        Assert.That(random.SiteDraws, Is.EqualTo(2 * source.Topology.Count));
        Assert.That(random.TargetDraws, Is.EqualTo(random.SiteDraws));
        AssertConsistentAndConnected(result);
    }

    [TestCase(0d, false)]
    [TestCase(0.36d, true)]
    [TestCase(0.37d, false)]
    [TestCase(0d, false, CellularPottsConnectivity.None)]
    [TestCase(0.36d, true, CellularPottsConnectivity.None)]
    [TestCase(0.37d, false, CellularPottsConnectivity.None)]
    public void Apply_AreaEnergy_UsesMetropolisThreshold(double draw, bool accepted,
        CellularPottsConnectivity connectivity = CellularPottsConnectivity.Preserve)
    {
        // Copying one hex from areas 2/2 with targets 2/2 costs 2. At T=2, acceptance is exp(-1).
        var source = Partition(new HexMapTopology(4, 1, Layout.OddR), p => p.X < 2 ? 7 : 42, 7, 42);
        var site = new VectorXYInt(1, 0);
        var random = Attempt(source, site, 42, draw);
        double temperature = draw == 0d ? 0d : 2d;

        var result = source.ApplyCellularPotts(new CellularPottsParameters(1, temperature, 0, 1, connectivity: connectivity), random: random);

        Assert.That(result[site], Is.EqualTo(accepted ? 42 : 7));
        Assert.That(random.EnergyDraws, Is.EqualTo(temperature == 0d ? 0 : accepted ? 1 : 4));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Apply_ExplicitTargets_DetermineDownhillDirection(bool favorsCopy)
    {
        var source = Partition(new HexMapTopology(4, 1, Layout.OddR), p => p.X < 2 ? 7 : 42, 7, 42);
        var site = new VectorXYInt(1, 0);
        var targets = new Dictionary<int, double> { [7] = favorsCopy ? 1 : 3, [42] = favorsCopy ? 3 : 1 };

        var result = source.ApplyCellularPotts(new CellularPottsParameters(1, 0, 0, 1, targets), random: Attempt(source, site, 42));

        Assert.That(result[site], Is.EqualTo(favorsCopy ? 42 : 7));
    }

    [TestCase(0.13d, true)]
    [TestCase(0.14d, false)]
    public void Apply_BoundaryEnergy_CountsEachChangedEdgeOnce(double draw, bool accepted)
    {
        var center = new VectorXYInt(2, 2);
        var neighbors = center.GetAdjacents(Layout.OddR);
        var donor = new HashSet<VectorXYInt> { center, neighbors[0], neighbors[1], neighbors[2], neighbors[3] };
        var source = Partition(new HexMapTopology(5, 5, Layout.OddR), p => donor.Contains(p) ? 7 : 42, 7, 42);
        // Four donor contacts versus two target contacts: delta H = J * (4 - 2) = 2.
        var result = source.ApplyCellularPotts(new CellularPottsParameters(1, 1, 1, 0), random: Attempt(source, center, 42, draw));

        Assert.That(result[center], Is.EqualTo(accepted ? 42 : 7));
    }

    [Test]
    public void Apply_EnergyOverflow_ThrowsWithoutChangingSource()
    {
        var center = new VectorXYInt(2, 2);
        var neighbors = center.GetAdjacents(Layout.OddR);
        var donor = new HashSet<VectorXYInt> { center, neighbors[0], neighbors[1], neighbors[2], neighbors[3] };
        var source = Partition(new HexMapTopology(5, 5, Layout.OddR), p => donor.Contains(p) ? 7 : 42, 7, 42);
        var before = Values(source);

        Assert.Throws<InvalidOperationException>(() => source.ApplyCellularPotts(new CellularPottsParameters(1, 1, double.MaxValue, 0), random: Attempt(source, center, 42)));
        Assert.That(Values(source), Is.EqualTo(before));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Apply_MapEdgeCopy_HasNoExteriorEnergyOrWrapping(Layout layout)
    {
        var source = Partition(new HexMapTopology(4, 1, layout), p => p.X < 2 ? 7 : 42, 7, 42);
        var site = new VectorXYInt(1, 0);
        // One donor edge is removed and one target edge is added: delta H = 0.
        var result = source.ApplyCellularPotts(new CellularPottsParameters(1, 0, 10, 0), random: Attempt(source, site, 42));

        Assert.That(result[site], Is.EqualTo(42));
        AssertConsistentAndConnected(result);
    }

    [TestCase(CellularPottsConnectivity.Preserve)]
    [TestCase(CellularPottsConnectivity.None)]
    public void Apply_TargetDraw_SamplesTheSameNeighborSitesInBothModes(CellularPottsConnectivity connectivity)
    {
        var center = new VectorXYInt(2, 2);
        var neighbors = center.GetAdjacents(Layout.OddR);
        var source = Partition(new HexMapTopology(5, 5, Layout.OddR),
            p => p == center || p == neighbors[0] ? 7 : 42, 7, 42);
        var random = Attempt(source, center, 42);

        var result = source.ApplyCellularPotts(new CellularPottsParameters(1, 0, 0, 0, connectivity: connectivity), random: random);

        Assert.That(result[center], Is.EqualTo(42));
        Assert.That(random.TargetBounds, Is.All.EqualTo(6));
    }

    [TestCase(CellularPottsConnectivity.Preserve, 0, 7)]
    [TestCase(CellularPottsConnectivity.Preserve, 1, 42)]
    [TestCase(CellularPottsConnectivity.Preserve, 2, 42)]
    [TestCase(CellularPottsConnectivity.Preserve, 3, 42)]
    [TestCase(CellularPottsConnectivity.Preserve, 4, 42)]
    [TestCase(CellularPottsConnectivity.Preserve, 5, 99)]
    [TestCase(CellularPottsConnectivity.None, 0, 7)]
    [TestCase(CellularPottsConnectivity.None, 1, 42)]
    [TestCase(CellularPottsConnectivity.None, 2, 42)]
    [TestCase(CellularPottsConnectivity.None, 3, 42)]
    [TestCase(CellularPottsConnectivity.None, 4, 42)]
    [TestCase(CellularPottsConnectivity.None, 5, 99)]
    public void Apply_TargetDraw_WeightsIdentifiersByTheirNeighborCountsInBothModes(
        CellularPottsConnectivity connectivity, int targetChoice, int expectedId)
    {
        var center = new VectorXYInt(2, 2);
        var neighbors = center.GetAdjacents(Layout.OddR);
        // Every neighbor has one sampling slot, even when several neighbors have the same owner.
        var source = Partition(new HexMapTopology(5, 5, Layout.OddR),
            p => p == center || p == neighbors[0] ? 7 : p == neighbors[5] ? 99 : 42, 7, 42, 99);
        var random = new FixedAttemptRandom(center.Y * source.Topology.Resolution.X + center.X, targetChoice);

        var result = source.ApplyCellularPotts(new CellularPottsParameters(1, 0, 0, 0, connectivity: connectivity), random: random);

        Assert.That(result[center], Is.EqualTo(expectedId));
        Assert.That(random.TargetBounds, Is.All.EqualTo(6));
        Assert.That(random.SiteDraws, Is.EqualTo(source.Topology.Count));
        Assert.That(random.EnergyDraws, Is.Zero);
        AssertConsistentAndConnected(result);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Apply_DisconnectedInput_IsRejected(Layout layout)
    {
        var source = Partition(new HexMapTopology(5, 1, layout), p => p.X == 0 || p.X == 4 ? 7 : 42, 7, 42);

        var exception = Assert.Throws<ArgumentException>(() => source.ApplyCellularPotts(new CellularPottsParameters(0)));

        Assert.That(exception!.ParamName, Is.EqualTo("partition"));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Apply_WithoutConnectivity_AcceptsDisconnectedInput(Layout layout)
    {
        var source = Partition(new HexMapTopology(5, 1, layout), p => p.X == 0 || p.X == 4 ? 7 : 42, 7, 42);
        var copy = source.ApplyCellularPotts(new CellularPottsParameters(0, connectivity: CellularPottsConnectivity.None));
        Assert.That(Values(copy), Is.EqualTo(Values(source)));

        var result = source.ApplyCellularPotts(new CellularPottsParameters(1, 0, 0, 0, connectivity: CellularPottsConnectivity.None), random: new FixedAttemptRandom(0, 0));

        Assert.That(result[0], Is.EqualTo(42));
        Assert.That(result[4], Is.EqualTo(7));
        Assert.That(result.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(4, 0) }));
    }

    [TestCase(-1)]
    [TestCase(2)]
    [TestCase(int.MaxValue)]
    public void Parameters_InvalidConnectivity_IsRejected(int value)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new CellularPottsParameters(0, connectivity: (CellularPottsConnectivity)value));

        Assert.That(exception!.ParamName, Is.EqualTo("connectivity"));
    }

    [Test]
    public void Apply_StaleAssignmentSnapshot_IsRejectedWithoutChangingSource()
    {
        var source = Partition(new HexMapTopology(4, 1, Layout.OddR), p => p.X < 2 ? 7 : 42, 7, 42);
        source[0] = 42;
        var before = Values(source);

        Assert.Throws<ArgumentException>(() => source.ApplyCellularPotts(new CellularPottsParameters(1)));
        Assert.That(Values(source), Is.EqualTo(before));
    }

    [Test]
    public void Apply_InvalidArguments_AreRejected()
    {
        HexPartitionMap<HexPartitionCell> missing = null!;
        Assert.Throws<ArgumentNullException>(() => missing.ApplyCellularPotts(new CellularPottsParameters(0)));
        var source = Partition(new HexMapTopology(1, 1, Layout.OddR), _ => 7, 7);
        Assert.That(Assert.Throws<ArgumentNullException>(() => source.ApplyCellularPotts(null!))!.ParamName, Is.EqualTo("parameters"));
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => new CellularPottsParameters(-1))!.ParamName, Is.EqualTo("stepCount"));
        var parameters = new CellularPottsParameters(0, targetAreas: new Dictionary<int, double> { [42] = 1 });
        Assert.Throws<ArgumentException>(() => source.ApplyCellularPotts(parameters));
    }

    [TestCase(-1d)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void Parameters_InvalidEnergyParameters_AreRejected(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CellularPottsParameters(0, temperature: value));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CellularPottsParameters(0, boundaryEnergy: value));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CellularPottsParameters(0, areaPenalty: value));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CellularPottsParameters(0, targetAreas: new Dictionary<int, double> { [7] = value }));
    }

    [Test]
    public void Parameters_TargetAreas_AreReadOnlySnapshot()
    {
        var targets = new Dictionary<int, double> { [7] = 2.5 };
        var parameters = new CellularPottsParameters(0, targetAreas: targets);
        targets[7] = 100;

        Assert.That(parameters.TargetAreas[7], Is.EqualTo(2.5));
        Assert.Throws<NotSupportedException>(() => ((IDictionary<int, double>)parameters.TargetAreas)[7] = 10);
        Assert.Throws<ArgumentException>(() => new CellularPottsParameters(0, targetAreas: new Dictionary<int, double> { [-1] = 1 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CellularPottsParameters(0, targetAreas: new Dictionary<int, double> { [7] = (double)int.MaxValue + 1 }));
    }

    [Test]
    public void Parameters_DefaultsPreserveConnectivityWithUnitEnergyCoefficients()
    {
        var parameters = new CellularPottsParameters(17);
        Assert.Multiple(() =>
        {
            Assert.That(parameters.StepCount, Is.EqualTo(17));
            Assert.That(parameters.Connectivity, Is.EqualTo(CellularPottsConnectivity.Preserve));
            Assert.That(parameters.Temperature, Is.EqualTo(1d));
            Assert.That(parameters.BoundaryEnergy, Is.EqualTo(1d));
            Assert.That(parameters.AreaPenalty, Is.EqualTo(1d));
            Assert.That(parameters.TargetAreas, Is.Empty);
        });
    }

    internal static HexPartitionMap<HexPartitionCell> Partition(HexMapTopology topology, Func<VectorXYInt, int> assign, params int[] ids)
    {
        var indexes = ids.ToDictionary(id => id, _ => new List<VectorXYInt>());
        for (int y = 0; y < topology.Resolution.Y; y++)
        for (int x = 0; x < topology.Resolution.X; x++)
        {
            var index = new VectorXYInt(x, y);
            indexes[assign(index)].Add(index);
        }
        return new HexPartitionMap<HexPartitionCell>(topology, ids.Select(id => new HexPartitionCell(id, indexes[id])).ToArray());
    }

    internal static int[] Values(HexPartitionMap<HexPartitionCell> partition) =>
        Enumerable.Range(0, partition.Topology.Count).Select(i => partition[i]).ToArray();

    internal static void AssertConsistentAndConnected(HexPartitionMap<HexPartitionCell> partition, string context = "")
    {
        int total = 0;
        foreach (var cell in partition.Cells)
        {
            var remaining = cell.HexIndexes.ToHashSet();
            total += remaining.Count;
            foreach (var index in remaining)
                Assert.That(partition[index], Is.EqualTo(cell.Id), context);
            if (remaining.Count == 0)
                continue;
            var queue = new Queue<VectorXYInt>();
            var seed = remaining.First();
            queue.Enqueue(seed);
            remaining.Remove(seed);
            while (queue.Count > 0)
            {
                foreach (var adjacent in queue.Dequeue().GetAdjacents(partition.Topology.Layout))
                    if (remaining.Remove(adjacent))
                        queue.Enqueue(adjacent);
            }
            Assert.That(remaining, Is.Empty, $"Cell {cell.Id} fragmented. {context}");
        }
        Assert.That(total, Is.EqualTo(partition.Topology.Count), context);
    }

    private static FixedAttemptRandom Attempt(HexPartitionMap<HexPartitionCell> source, VectorXYInt site, int target,
        double draw = 0d)
    {
        var ids = site.GetAdjacents(source.Topology.Layout)
            .Where(p => p.X >= 0 && p.Y >= 0 && p.X < source.Topology.Resolution.X && p.Y < source.Topology.Resolution.Y)
            .Select(p => source[p]);
        var targets = ids.ToList();
        return new FixedAttemptRandom(site.Y * source.Topology.Resolution.X + site.X, targets.IndexOf(target), draw);
    }

    private sealed class FixedAttemptRandom : Random
    {
        private readonly int _site;
        private readonly int _target;
        private readonly double _draw;
        private bool _drawSite = true;

        internal FixedAttemptRandom(int site, int target, double draw = 0d)
        {
            _site = site;
            _target = target;
            _draw = draw;
        }

        internal int SiteDraws { get; private set; }
        internal int TargetDraws { get; private set; }
        internal int EnergyDraws { get; private set; }
        internal List<int> TargetBounds { get; } = new List<int>();

        public override int Next(int maxValue)
        {
            int result;
            if (_drawSite)
            {
                SiteDraws++;
                result = _site;
            }
            else
            {
                TargetDraws++;
                TargetBounds.Add(maxValue);
                result = _target;
            }
            _drawSite = !_drawSite;
            Assert.That(result, Is.InRange(0, maxValue - 1));
            return result;
        }

        public override double NextDouble()
        {
            EnergyDraws++;
            return _draw;
        }
    }
}
