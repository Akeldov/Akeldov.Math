using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class HexFieldTopologyTests
{
    [Test]
    public void Constructor_ExposesDimensionsAndLayout()
    {
        var topology = new HexAdjacencyMap(3, 2, Layout.EvenQ);

        Assert.That(topology.Width, Is.EqualTo(3));
        Assert.That(topology.Height, Is.EqualTo(2));
        Assert.That(topology.Count, Is.EqualTo(6));
        Assert.That(topology.Layout, Is.EqualTo(Layout.EvenQ));
        Assert.That(topology.Adjacent, Has.Length.EqualTo(6));
    }

    [Test]
    public void HexFieldTopology_ImplementsIHexMap()
    {
        IHexMap<HexAdjacency> topology = new HexAdjacencyMap(3, 2, Layout.OddR);

        HexAdjacency adjacency = topology[new VectorXYInt(1, 0)];

        Assert.That(topology.Width, Is.EqualTo(3));
        Assert.That(topology.Height, Is.EqualTo(2));
        Assert.That(topology[1], Is.EqualTo(adjacency));
    }

    [Test]
    public void Constructor_CreatesAdjacency()
    {
        var topology = new HexAdjacencyMap(3, 2, Layout.OddR);
        HexAdjacency adjacency = topology[new VectorXYInt(1, 0)];

        Assert.That(adjacency.Flags, Is.EqualTo(
            HexAdjacencyFlags.Adjacent0 |
            HexAdjacencyFlags.Adjacent1 |
            HexAdjacencyFlags.Adjacent2 |
            HexAdjacencyFlags.Adjacent3));
        Assert.That(adjacency.Adjacent0Index, Is.EqualTo(2));
        Assert.That(adjacency.Adjacent1Index, Is.EqualTo(4));
        Assert.That(adjacency.Adjacent2Index, Is.EqualTo(3));
        Assert.That(adjacency.Adjacent3Index, Is.EqualTo(0));
        Assert.That(adjacency.Adjacent4Index, Is.EqualTo(1));
        Assert.That(adjacency.Adjacent5Index, Is.EqualTo(1));
    }

    [TestCase(Layout.OddQ, 5, 1, 3, 6, 7, 8)]
    [TestCase(Layout.EvenQ, 2, 1, 0, 3, 7, 5)]
    public void Constructor_ForFlatTopLayouts_UsesAdjacent0AsNorthEast(
        Layout layout,
        int adjacent0Index,
        int adjacent1Index,
        int adjacent2Index,
        int adjacent3Index,
        int adjacent4Index,
        int adjacent5Index)
    {
        var topology = new HexAdjacencyMap(3, 3, layout);

        HexAdjacency adjacency = topology[new VectorXYInt(1, 1)];

        Assert.That(adjacency.Flags, Is.EqualTo(HexAdjacencyFlags.AllAdjacent));
        Assert.That(adjacency.Adjacent0Index, Is.EqualTo(adjacent0Index));
        Assert.That(adjacency.Adjacent1Index, Is.EqualTo(adjacent1Index));
        Assert.That(adjacency.Adjacent2Index, Is.EqualTo(adjacent2Index));
        Assert.That(adjacency.Adjacent3Index, Is.EqualTo(adjacent3Index));
        Assert.That(adjacency.Adjacent4Index, Is.EqualTo(adjacent4Index));
        Assert.That(adjacency.Adjacent5Index, Is.EqualTo(adjacent5Index));
    }

    [Test]
    public void Indexer_WhenIndexIsOutsideTopology_Throws()
    {
        var topology = new HexAdjacencyMap(3, 2, Layout.OddR);

        Assert.Throws<IndexOutOfRangeException>(() => _ = topology[new VectorXYInt(3, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = topology[new VectorXYInt(0, 2)]);
    }

    [Test]
    public void Constructor_WhenDimensionIsNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexAdjacencyMap(-1, 1, Layout.OddR));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexAdjacencyMap(1, -1, Layout.OddR));
    }
}
