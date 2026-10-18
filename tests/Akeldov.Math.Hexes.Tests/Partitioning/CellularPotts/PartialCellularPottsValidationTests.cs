using Akeldov.Math.Hexes.Partitioning.CellularPotts;

namespace Akeldov.Math.Hexes.Tests.Partitioning.CellularPotts;

public class PartialCellularPottsValidationTests
{
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    public void Apply_ValidatesSourceParametersAndTargetsEvenWithoutParticipants(int overload)
    {
        var full = CellularPottsTests.Partition(new HexMapTopology(3, 2, Layout.OddR), _ => 7, 7);
        var partial = new PartialHexPartitionMap<HexPartitionCell>(full.Topology, full.Cells);
        var mask = new BoolHexMap(full.Topology);
        var nullable = new HexMap<int?>(full.Topology);
        var regions = new IntHexMap(full.Topology);
        var parameters = new CellularPottsParameters(0);

        Assert.That(Assert.Throws<ArgumentNullException>(() =>
            Invoke(overload, null!, null!, mask, nullable, regions, parameters))!.ParamName, Is.EqualTo("partition"));
        Assert.That(Assert.Throws<ArgumentNullException>(() =>
            Invoke(overload, full, partial, mask, nullable, regions, null!))!.ParamName, Is.EqualTo("parameters"));
        var unknownTarget = new Dictionary<int, float> { [99] = 1 };
        Assert.That(Assert.Throws<ArgumentException>(() =>
            Invoke(overload, full, partial, mask, nullable, regions, parameters, unknownTarget))!.ParamName, Is.EqualTo("targetAreas"));

        // The stale source assignment is excluded by every supplied mask, but must still be rejected.
        full[0] = 42;
        partial[0] = null;
        Assert.That(Assert.Throws<ArgumentException>(() =>
            Invoke(overload, full, partial, mask, nullable, regions, parameters))!.ParamName, Is.EqualTo("partition"));
        Assert.That(full[0], Is.EqualTo(42));
        Assert.That(partial[0], Is.Null);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    public void Apply_RejectsNullAndIncompatibleMasks(int overload)
    {
        var full = CellularPottsTests.Partition(new HexMapTopology(3, 2, Layout.OddR), _ => 7, 7);
        var partial = new PartialHexPartitionMap<HexPartitionCell>(full.Topology, full.Cells);
        var mask = new BoolHexMap(full.Topology);
        var nullable = new HexMap<int?>(full.Topology);
        var regions = new IntHexMap(full.Topology);
        var parameters = new CellularPottsParameters(0);
        bool hasParticipation = overload is 0 or 2 or 4 or 6;
        bool hasRegions = overload is 1 or 2 or 5 or 6;
        if (hasParticipation)
            Assert.That(Assert.Throws<ArgumentNullException>(() =>
                Invoke(overload, full, partial, null!, nullable, regions, parameters))!.ParamName, Is.EqualTo("participationMask"));
        if (hasRegions)
            Assert.That(Assert.Throws<ArgumentNullException>(() =>
                Invoke(overload, full, partial, mask, null!, null!, parameters))!.ParamName, Is.EqualTo("regionsMask"));
        foreach (var other in new[] { new HexMapTopology(2, 3, Layout.OddR), new HexMapTopology(3, 2, Layout.EvenR) })
        {
            if (hasParticipation)
                Assert.That(Assert.Throws<ArgumentException>(() =>
                    Invoke(overload, full, partial, new BoolHexMap(other), nullable, regions, parameters))!.ParamName, Is.EqualTo("participationMask"));
            if (hasRegions)
                Assert.That(Assert.Throws<ArgumentException>(() =>
                    Invoke(overload, full, partial, mask, new HexMap<int?>(other), new IntHexMap(other), parameters))!.ParamName, Is.EqualTo("regionsMask"));
        }
    }

    private static PartialHexPartitionMap<HexPartitionCell> Invoke(int overload,
        HexPartitionMap<HexPartitionCell> full, PartialHexPartitionMap<HexPartitionCell> partial,
        IHexMap<bool> mask, IHexMap<int?> nullable, IHexMap<int> regions, CellularPottsParameters parameters, IReadOnlyDictionary<int, float>? targetAreas = null) => overload switch
    {
        0 => full.ApplyCellularPotts(mask, targetAreas, parameters),
        1 => full.ApplyCellularPotts(nullable, targetAreas, parameters),
        2 => full.ApplyCellularPotts(mask, regions, targetAreas, parameters),
        3 => partial.ApplyCellularPotts(targetAreas, parameters),
        4 => partial.ApplyCellularPotts(mask, targetAreas, parameters),
        5 => partial.ApplyCellularPotts(nullable, targetAreas, parameters),
        6 => partial.ApplyCellularPotts(mask, regions, targetAreas, parameters),
        _ => throw new ArgumentOutOfRangeException(nameof(overload))
    };
}
