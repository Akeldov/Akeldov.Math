using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class IndexPartialSeptupletMapTests
{
    [Test]
    public void Constructor_ExposesDimensionsAndLayout()
    {
        var topology = new IndexPartialSeptupletMap(new HexMapTopology(3, 2, Layout.EvenQ));

        Assert.That(topology.Topology.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(topology.Count, Is.EqualTo(6));
        Assert.That(topology.Topology.Layout, Is.EqualTo(Layout.EvenQ));
        Assert.That(typeof(IndexPartialSeptupletMap).GetProperty("Adjacent"), Is.Null);
    }

    [Test]
    public void IndexPartialSeptupletMap_ImplementsReadOnlyISpatialHexMap()
    {
        var geometry = new HexMapGeometry(3, 2, new VectorXY(10f, -20f), 2f, Layout.OddR);
        var source = new IndexPartialSeptupletMap(geometry);
        ISpatialHexMap<PartialSeptuplet<VectorXYInt>> topology = source;

        PartialSeptuplet<VectorXYInt> adjacency = topology[new VectorXYInt(1, 0)];

        Assert.That(source, Is.Not.InstanceOf<HexMap<PartialSeptuplet<VectorXYInt>>>());
        Assert.That(typeof(IndexPartialSeptupletMap).GetProperty("Item", new[] { typeof(VectorXYInt) })!.SetMethod, Is.Null);
        Assert.That(typeof(IndexPartialSeptupletMap).GetProperty("Item", new[] { typeof(int) })!.SetMethod, Is.Null);
        Assert.That(topology.Topology.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(topology.Topology.Layout, Is.EqualTo(Layout.OddR));
        Assert.That(topology.Geometry, Is.EqualTo(geometry));
        Assert.That(topology[1], Is.EqualTo(adjacency));
    }

    [Test]
    public void Constructor_CreatesAdjacencyAndPresence()
    {
        var topology = new IndexPartialSeptupletMap(new HexMapTopology(3, 2, Layout.OddR));

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
        var topology = new IndexPartialSeptupletMap(new HexMapTopology(1, 1, Layout.OddR));

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

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Constructor_OrdersAdjacenciesByHexEdge(Layout layout)
    {
        var map = new IndexPartialSeptupletMap(new HexMapTopology(4, 4, layout));

        foreach (VectorXYInt index in new[] { new VectorXYInt(1, 1), new VectorXYInt(2, 2) })
        {
            PartialSeptuplet<VectorXYInt> adjacency = map[index];

            Assert.Multiple(() =>
            {
                Assert.That(adjacency.Main, Is.EqualTo(index));
                Assert.That(adjacency.Adjacent0, Is.EqualTo(index.GetAdjacent(HexEdge.Edge0, layout)));
                Assert.That(adjacency.Adjacent1, Is.EqualTo(index.GetAdjacent(HexEdge.Edge1, layout)));
                Assert.That(adjacency.Adjacent2, Is.EqualTo(index.GetAdjacent(HexEdge.Edge2, layout)));
                Assert.That(adjacency.Adjacent3, Is.EqualTo(index.GetAdjacent(HexEdge.Edge3, layout)));
                Assert.That(adjacency.Adjacent4, Is.EqualTo(index.GetAdjacent(HexEdge.Edge4, layout)));
                Assert.That(adjacency.Adjacent5, Is.EqualTo(index.GetAdjacent(HexEdge.Edge5, layout)));
                Assert.That(adjacency.Presence, Is.EqualTo(SeptupletPresenceFlags.All));
            });
        }
    }

    [Test]
    public void Indexer_WhenIndexIsOutsideTopology_Throws()
    {
        var topology = new IndexPartialSeptupletMap(new HexMapTopology(3, 2, Layout.OddR));

        Assert.Throws<IndexOutOfRangeException>(() => _ = topology[new VectorXYInt(3, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = topology[new VectorXYInt(0, 2)]);
    }

    [Test]
    public void Constructor_WhenDimensionIsNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexPartialSeptupletMap(new HexMapTopology(-1, 1, Layout.OddR)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexPartialSeptupletMap(new HexMapTopology(1, -1, Layout.OddR)));
    }
}
