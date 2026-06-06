using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class IndexPartialSeptupletMapTests
{
    [Test]
    public void Constructor_ExposesDimensionsAndLayout()
    {
        var topology = new IndexPartialSeptupletMap(3, 2, Layout.EvenQ);

        Assert.That(topology.Width, Is.EqualTo(3));
        Assert.That(topology.Height, Is.EqualTo(2));
        Assert.That(topology.Count, Is.EqualTo(6));
        Assert.That(topology.Layout, Is.EqualTo(Layout.EvenQ));
        Assert.That(topology.Adjacent, Has.Length.EqualTo(6));
    }

    [Test]
    public void IndexPartialSeptupletMap_ImplementsIHexMap()
    {
        IHexMap<PartialSeptuplet<VectorXYInt>> topology = new IndexPartialSeptupletMap(3, 2, Layout.OddR);

        PartialSeptuplet<VectorXYInt> adjacency = topology[new VectorXYInt(1, 0)];

        Assert.That(topology.Width, Is.EqualTo(3));
        Assert.That(topology.Height, Is.EqualTo(2));
        Assert.That(topology[1], Is.EqualTo(adjacency));
    }

    [Test]
    public void Constructor_CreatesAdjacencyAndPresence()
    {
        var topology = new IndexPartialSeptupletMap(3, 2, Layout.OddR);

        PartialSeptuplet<VectorXYInt> adjacency = topology[new VectorXYInt(1, 0)];

        Assert.That(adjacency.Main, Is.EqualTo(new VectorXYInt(1, 0)));
        Assert.That(adjacency.Adjacent0, Is.EqualTo(new VectorXYInt(2, 0)));
        Assert.That(adjacency.Adjacent1, Is.EqualTo(new VectorXYInt(1, 1)));
        Assert.That(adjacency.Adjacent2, Is.EqualTo(new VectorXYInt(0, 1)));
        Assert.That(adjacency.Adjacent3, Is.EqualTo(new VectorXYInt(0, 0)));
        Assert.That(adjacency.Adjacent4, Is.EqualTo(new VectorXYInt(0, -1)));
        Assert.That(adjacency.Adjacent5, Is.EqualTo(new VectorXYInt(1, -1)));
        Assert.That(adjacency.Presence, Is.EqualTo(
            SeptupletPresenceFlags.Main |
            SeptupletPresenceFlags.Adjacent0 |
            SeptupletPresenceFlags.Adjacent1 |
            SeptupletPresenceFlags.Adjacent2 |
            SeptupletPresenceFlags.Adjacent3));
        Assert.That(adjacency.HasMain, Is.True);
        Assert.That(adjacency.HasAdjacent0, Is.True);
        Assert.That(adjacency.HasAdjacent1, Is.True);
        Assert.That(adjacency.HasAdjacent2, Is.True);
        Assert.That(adjacency.HasAdjacent3, Is.True);
        Assert.That(adjacency.HasAdjacent4, Is.False);
        Assert.That(adjacency.HasAdjacent5, Is.False);
    }

    [Test]
    public void Constructor_WhenNeighborsAreOutside_KeepsLogicalIndices()
    {
        var topology = new IndexPartialSeptupletMap(1, 1, Layout.OddR);

        PartialSeptuplet<VectorXYInt> adjacency = topology[new VectorXYInt(0, 0)];

        Assert.That(adjacency.Main, Is.EqualTo(new VectorXYInt(0, 0)));
        Assert.That(adjacency.Adjacent0, Is.EqualTo(new VectorXYInt(1, 0)));
        Assert.That(adjacency.Adjacent1, Is.EqualTo(new VectorXYInt(0, 1)));
        Assert.That(adjacency.Adjacent2, Is.EqualTo(new VectorXYInt(-1, 1)));
        Assert.That(adjacency.Adjacent3, Is.EqualTo(new VectorXYInt(-1, 0)));
        Assert.That(adjacency.Adjacent4, Is.EqualTo(new VectorXYInt(-1, -1)));
        Assert.That(adjacency.Adjacent5, Is.EqualTo(new VectorXYInt(0, -1)));
        Assert.That(adjacency.Presence, Is.EqualTo(SeptupletPresenceFlags.Main));
    }

    [TestCase(Layout.OddQ, 2, 1, 1, 0, 0, 1, 0, 2, 1, 2, 2, 2)]
    [TestCase(Layout.EvenQ, 2, 0, 1, 0, 0, 0, 0, 1, 1, 2, 2, 1)]
    public void Constructor_ForFlatTopLayouts_UsesAdjacent0AsNorthEast(
        Layout layout,
        int adjacent0X,
        int adjacent0Y,
        int adjacent1X,
        int adjacent1Y,
        int adjacent2X,
        int adjacent2Y,
        int adjacent3X,
        int adjacent3Y,
        int adjacent4X,
        int adjacent4Y,
        int adjacent5X,
        int adjacent5Y)
    {
        var topology = new IndexPartialSeptupletMap(3, 3, layout);

        PartialSeptuplet<VectorXYInt> adjacency = topology[new VectorXYInt(1, 1)];

        Assert.That(adjacency.Main, Is.EqualTo(new VectorXYInt(1, 1)));
        Assert.That(adjacency.Adjacent0, Is.EqualTo(new VectorXYInt(adjacent0X, adjacent0Y)));
        Assert.That(adjacency.Adjacent1, Is.EqualTo(new VectorXYInt(adjacent1X, adjacent1Y)));
        Assert.That(adjacency.Adjacent2, Is.EqualTo(new VectorXYInt(adjacent2X, adjacent2Y)));
        Assert.That(adjacency.Adjacent3, Is.EqualTo(new VectorXYInt(adjacent3X, adjacent3Y)));
        Assert.That(adjacency.Adjacent4, Is.EqualTo(new VectorXYInt(adjacent4X, adjacent4Y)));
        Assert.That(adjacency.Adjacent5, Is.EqualTo(new VectorXYInt(adjacent5X, adjacent5Y)));
        Assert.That(adjacency.Presence, Is.EqualTo(SeptupletPresenceFlags.All));
    }

    [Test]
    public void Indexer_WhenIndexIsOutsideTopology_Throws()
    {
        var topology = new IndexPartialSeptupletMap(3, 2, Layout.OddR);

        Assert.Throws<IndexOutOfRangeException>(() => _ = topology[new VectorXYInt(3, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = topology[new VectorXYInt(0, 2)]);
    }

    [Test]
    public void Constructor_WhenDimensionIsNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexPartialSeptupletMap(-1, 1, Layout.OddR));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexPartialSeptupletMap(1, -1, Layout.OddR));
    }
}
