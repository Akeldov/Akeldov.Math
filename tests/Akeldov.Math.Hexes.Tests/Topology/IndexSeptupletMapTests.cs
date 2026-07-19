using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class IndexSeptupletMapTests
{
    [Test]
    public void Constructor_ExposesDimensionsAndLayout()
    {
        var topology = new IndexSeptupletMap(new HexMapTopology(3, 2, Layout.EvenQ));

        Assert.That(topology.Topology.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(topology.Topology.Count, Is.EqualTo(6));
        Assert.That(topology.Topology.Layout, Is.EqualTo(Layout.EvenQ));
        Assert.That(typeof(IndexSeptupletMap).GetProperty("Adjacent"), Is.Null);
    }

    [Test]
    public void Constructor_UsesHexMapTopology()
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);

        var map = new IndexSeptupletMap(topology);

        Assert.That(map.Topology.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(map.Topology.Count, Is.EqualTo(6));
        Assert.That(map.Topology.Layout, Is.EqualTo(Layout.EvenQ));
    }

    [Test]
    public void IndexSeptupletMap_ImplementsReadOnlyISpatialHexMap()
    {
        var geometry = new HexMapGeometry(3, 2, new VectorXY(10f, -20f), 2f, Layout.OddR);
        var source = new IndexSeptupletMap(geometry);
        ISpatialHexMap<Septuplet<VectorXYInt>> topology = source;

        Septuplet<VectorXYInt> adjacency = topology[new VectorXYInt(1, 0)];

        Assert.That(source, Is.Not.InstanceOf<HexMap<Septuplet<VectorXYInt>>>());
        Assert.That(typeof(IndexSeptupletMap).GetProperty("Item", new[] { typeof(VectorXYInt) })!.SetMethod, Is.Null);
        Assert.That(typeof(IndexSeptupletMap).GetProperty("Item", new[] { typeof(int) })!.SetMethod, Is.Null);
        Assert.That(topology.Topology.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(topology.Topology.Layout, Is.EqualTo(Layout.OddR));
        Assert.That(topology.Geometry, Is.EqualTo(geometry));
        Assert.That(topology[1], Is.EqualTo(adjacency));
    }

    [Test]
    public void Constructor_CreatesAdjacency()
    {
        var topology = new IndexSeptupletMap(new HexMapTopology(3, 2, Layout.OddR));
        Septuplet<VectorXYInt> adjacency = topology[new VectorXYInt(1, 0)];

        Assert.That(adjacency.Main, Is.EqualTo(new VectorXYInt(1, 0)));
        Assert.That(adjacency.Adjacent0, Is.EqualTo(new VectorXYInt(2, 0)));
        Assert.That(adjacency.Adjacent1, Is.EqualTo(new VectorXYInt(1, 1)));
        Assert.That(adjacency.Adjacent2, Is.EqualTo(new VectorXYInt(0, 1)));
        Assert.That(adjacency.Adjacent3, Is.EqualTo(new VectorXYInt(0, 0)));
        Assert.That(adjacency.Adjacent4, Is.EqualTo(new VectorXYInt(0, -1)));
        Assert.That(adjacency.Adjacent5, Is.EqualTo(new VectorXYInt(1, -1)));
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
        var topology = new IndexSeptupletMap(new HexMapTopology(3, 3, layout));

        Septuplet<VectorXYInt> adjacency = topology[new VectorXYInt(1, 1)];

        Assert.That(adjacency.Main, Is.EqualTo(new VectorXYInt(1, 1)));
        Assert.That(adjacency.Adjacent0, Is.EqualTo(new VectorXYInt(adjacent0X, adjacent0Y)));
        Assert.That(adjacency.Adjacent1, Is.EqualTo(new VectorXYInt(adjacent1X, adjacent1Y)));
        Assert.That(adjacency.Adjacent2, Is.EqualTo(new VectorXYInt(adjacent2X, adjacent2Y)));
        Assert.That(adjacency.Adjacent3, Is.EqualTo(new VectorXYInt(adjacent3X, adjacent3Y)));
        Assert.That(adjacency.Adjacent4, Is.EqualTo(new VectorXYInt(adjacent4X, adjacent4Y)));
        Assert.That(adjacency.Adjacent5, Is.EqualTo(new VectorXYInt(adjacent5X, adjacent5Y)));
    }

    [Test]
    public void Indexer_WhenIndexIsOutsideTopology_Throws()
    {
        var topology = new IndexSeptupletMap(new HexMapTopology(3, 2, Layout.OddR));

        Assert.Throws<IndexOutOfRangeException>(() => _ = topology[new VectorXYInt(3, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = topology[new VectorXYInt(0, 2)]);
    }

    [Test]
    public void Constructor_WhenDimensionIsNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexSeptupletMap(new HexMapTopology(-1, 1, Layout.OddR)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexSeptupletMap(new HexMapTopology(1, -1, Layout.OddR)));
    }
}
