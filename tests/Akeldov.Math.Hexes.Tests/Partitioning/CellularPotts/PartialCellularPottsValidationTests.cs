using Akeldov.Math.Hexes.Partitioning.CellularPotts;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.CellularPotts;

public class PartialCellularPottsValidationTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void Diffuse_ValidatesSourceAndParameters(bool explicitTargets)
    {
        var full = CellularPottsTests.Partition(new HexMapTopology(3, 2, Layout.OddR), _ => 7, 7);
        var partial = new PartialHexPartitionMap<HexPartitionCell>(full.Topology, full.Cells);
        var parameters = new CellularPottsParameters(0);

        Assert.That(Assert.Throws<ArgumentNullException>(() =>
            Invoke(null!, parameters, explicitTargets))!.ParamName, Is.EqualTo("partition"));
        Assert.That(Assert.Throws<ArgumentNullException>(() =>
            Invoke(partial, null!, explicitTargets))!.ParamName, Is.EqualTo("parameters"));

        // A null assignment must still agree with the retained cells.
        partial[0] = null;
        Assert.That(Assert.Throws<ArgumentException>(() =>
            Invoke(partial, parameters, explicitTargets))!.ParamName, Is.EqualTo("partition"));
        Assert.That(partial[0], Is.Null);
    }

    [Test]
    public void Diffuse_ValidatesTargetsWhenEveryHexIsUnassigned()
    {
        var partial = new PartialHexPartitionMap<HexPartitionCell>(new HexMapTopology(3, 2, Layout.OddR),
            new[] { new HexPartitionCell(7, Array.Empty<VectorXYInt>()) });
        var parameters = new CellularPottsParameters(0);
        Assert.That(Assert.Throws<ArgumentException>(() => partial.DiffuseByCellularPotts(
            new Dictionary<int, float> { [99] = 1 }, parameters))!.ParamName, Is.EqualTo("targetAreas"));
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => partial.DiffuseByCellularPotts(
            new Dictionary<int, float> { [7] = float.NaN }, parameters))!.ParamName, Is.EqualTo("targetAreas"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Diffuse_ValidatesRegionalArgumentsEvenWithoutParticipants(bool explicitTargets)
    {
        var topology = new HexMapTopology(3, 2, Layout.OddR);
        var source = new PartialHexPartitionMap<HexPartitionCell>(topology,
            new[] { new HexPartitionCell(7, Array.Empty<VectorXYInt>()) });
        var regions = new HexMap<int>(topology);
        var parameters = new CellularPottsParameters(0);

        PartialHexPartitionMap<HexPartitionCell> Apply(PartialHexPartitionMap<HexPartitionCell> partition,
            HexMap<int> regionMap, CellularPottsParameters settings) => explicitTargets
            ? partition.DiffuseByCellularPotts(regionMap, new Dictionary<int, float> { [7] = 1 }, settings)
            : partition.DiffuseByCellularPotts(regionMap, settings);

        Assert.That(Assert.Throws<ArgumentNullException>(() => Apply(null!, regions, parameters))!.ParamName, Is.EqualTo("partition"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => Apply(source, null!, parameters))!.ParamName, Is.EqualTo("regions"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => Apply(source, regions, null!))!.ParamName, Is.EqualTo("parameters"));
        foreach (var other in new[] { new HexMapTopology(2, 3, Layout.OddR), new HexMapTopology(3, 2, Layout.EvenR) })
            Assert.That(Assert.Throws<ArgumentException>(() => Apply(source, new HexMap<int>(other), parameters))!.ParamName, Is.EqualTo("regions"));
        Assert.That(PartialCellularPottsTests.Values(Apply(source, regions, parameters)), Is.All.Null);

        source[0] = 7;
        Assert.That(Assert.Throws<ArgumentException>(() => Apply(source, regions, parameters))!.ParamName, Is.EqualTo("partition"));
    }

    private static PartialHexPartitionMap<HexPartitionCell> Invoke(PartialHexPartitionMap<HexPartitionCell> partial,
        CellularPottsParameters parameters, bool explicitTargets) => explicitTargets
        ? partial.DiffuseByCellularPotts(new Dictionary<int, float> { [7] = 6 }, parameters)
        : partial.DiffuseByCellularPotts(parameters);
}
