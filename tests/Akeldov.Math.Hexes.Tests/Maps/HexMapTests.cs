using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class HexMapTests
{
    [Test]
    public void Constructor_UsesTopology()
    {
        var topology = new IndexSeptupletMap(3, 2, Layout.EvenQ);

        var map = new HexMap<int>(topology);

        Assert.That(map.Topology, Is.SameAs(topology));
        Assert.That(map.Width, Is.EqualTo(3));
        Assert.That(map.Height, Is.EqualTo(2));
        Assert.That(map.Layout, Is.EqualTo(Layout.EvenQ));
    }

    [Test]
    public void Constructor_UsesHexMapTopology()
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);

        var map = new HexMap<int>(topology);

        Assert.That(map.Topology.Width, Is.EqualTo(3));
        Assert.That(map.Topology.Height, Is.EqualTo(2));
        Assert.That(map.Topology.Layout, Is.EqualTo(Layout.EvenQ));
        Assert.That(map.Width, Is.EqualTo(3));
        Assert.That(map.Height, Is.EqualTo(2));
        Assert.That(map.Layout, Is.EqualTo(Layout.EvenQ));
    }

    [Test]
    public void HexMapTopology_ExposesDimensionsLayoutAndCount()
    {
        var topology = new HexMapTopology(3, 2, Layout.OddQ);

        Assert.That(topology.Width, Is.EqualTo(3));
        Assert.That(topology.Height, Is.EqualTo(2));
        Assert.That(topology.Layout, Is.EqualTo(Layout.OddQ));
        Assert.That(topology.Count, Is.EqualTo(6));
    }

    [Test]
    public void Indexer_UsesTopologyWidthForFlatIndex()
    {
        var topology = new IndexSeptupletMap(3, 2, Layout.OddR);
        var map = new HexMap<int>(topology);

        map[new VectorXYInt(2, 1)] = 42;

        Assert.That(map[5], Is.EqualTo(42));
    }

    [Test]
    public void HexMap_ImplementsIHexMap()
    {
        var topology = new IndexSeptupletMap(3, 2, Layout.OddR);
        var source = new HexMap<int>(topology);
        IHexMap<int> map = source;

        source[new VectorXYInt(2, 1)] = 42;

        Assert.That(map.Width, Is.EqualTo(3));
        Assert.That(map.Height, Is.EqualTo(2));
        Assert.That(map[5], Is.EqualTo(42));
    }

    [Test]
    public void Indexer_WhenIndexIsOutsideTopology_Throws()
    {
        var topology = new IndexSeptupletMap(3, 2, Layout.OddR);
        var map = new HexMap<int>(topology);

        Assert.Throws<IndexOutOfRangeException>(() => _ = map[new VectorXYInt(3, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => map[new VectorXYInt(0, 2)] = 1);
    }

    [Test]
    public void Constructor_WhenTopologyIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new HexMap<int>(null!));
    }

    [Test]
    public void HexMapTopology_WhenArgumentsAreInvalid_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexMapTopology(-1, 1, Layout.OddR));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexMapTopology(1, -1, Layout.OddR));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexMapTopology(1, 1, (Layout)42));
    }
}
