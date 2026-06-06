using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class HexMapTests
{
    [Test]
    public void Constructor_UsesTopology()
    {
        var topology = new HexAdjacencyMap(3, 2, Layout.EvenQ);

        var map = new HexMap<int>(topology);

        Assert.That(map.Topology, Is.SameAs(topology));
        Assert.That(map.Width, Is.EqualTo(3));
        Assert.That(map.Height, Is.EqualTo(2));
        Assert.That(map.Layout, Is.EqualTo(Layout.EvenQ));
    }

    [Test]
    public void Indexer_UsesTopologyWidthForFlatIndex()
    {
        var topology = new HexAdjacencyMap(3, 2, Layout.OddR);
        var map = new HexMap<int>(topology);

        map[new VectorXYInt(2, 1)] = 42;

        Assert.That(map[5], Is.EqualTo(42));
    }

    [Test]
    public void HexMap_ImplementsIHexMap()
    {
        var topology = new HexAdjacencyMap(3, 2, Layout.OddR);
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
        var topology = new HexAdjacencyMap(3, 2, Layout.OddR);
        var map = new HexMap<int>(topology);

        Assert.Throws<IndexOutOfRangeException>(() => _ = map[new VectorXYInt(3, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => map[new VectorXYInt(0, 2)] = 1);
    }

    [Test]
    public void Constructor_WhenTopologyIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new HexMap<int>(null!));
    }
}
