using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class HexMapTests
{
    [Test]
    public void Constructor_UsesHexMapTopology()
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);

        var map = new HexMap<int>(topology);

        Assert.That(map.Topology.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(map.Topology.Layout, Is.EqualTo(Layout.EvenQ));
        Assert.That(map.Topology.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(map.Topology.Layout, Is.EqualTo(Layout.EvenQ));
    }

    [Test]
    public void Constructor_WithValues_UsesProvidedArrayAsBackingStorage()
    {
        var values = new[] { 10, 20 };
        var map = new HexMap<int>(new HexMapTopology(2, 1, Layout.OddR), values);

        values[0] = 30;
        map[1] = 40;

        Assert.Multiple(() =>
        {
            Assert.That(map[0], Is.EqualTo(30));
            Assert.That(values[1], Is.EqualTo(40));
        });
    }

    [Test]
    public void Constructor_WithValues_WhenArrayIsInvalid_Throws()
    {
        var topology = new HexMapTopology(2, 1, Layout.OddR);

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => new HexMap<int>(topology, null!))!.ParamName,
                Is.EqualTo("values"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => new HexMap<int>(topology, new int[1]))!.ParamName,
                Is.EqualTo("values"));
        });
    }

    [Test]
    public void HexMapTopology_ExposesDimensionsLayoutAndCount()
    {
        var topology = new HexMapTopology(3, 2, Layout.OddQ);

        Assert.That(topology.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(topology.Layout, Is.EqualTo(Layout.OddQ));
        Assert.That(topology.Count, Is.EqualTo(6));
    }

    [Test]
    public void HexMapTopology_WithResolution_ExposesResolutionLayoutAndCount()
    {
        var topology = new HexMapTopology(new VectorXYInt(3, 2), Layout.OddQ);

        Assert.That(topology.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(topology.Layout, Is.EqualTo(Layout.OddQ));
        Assert.That(topology.Count, Is.EqualTo(6));
    }

    [Test]
    public void Indexer_UsesTopologyWidthForFlatIndex()
    {
        var map = new HexMap<int>(new HexMapTopology(3, 2, Layout.OddR));

        map[new VectorXYInt(2, 1)] = 42;

        Assert.That(map[5], Is.EqualTo(42));
    }

    [Test]
    public void HexMap_ImplementsIHexMap()
    {
        var source = new HexMap<int>(new HexMapTopology(3, 2, Layout.OddR));
        IHexMap<int> map = source;

        source[new VectorXYInt(2, 1)] = 42;

        Assert.That(map.Topology.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(map.Topology.Layout, Is.EqualTo(Layout.OddR));
        Assert.That(map[5], Is.EqualTo(42));
    }

    [Test]
    public void Indexer_WhenIndexIsOutsideTopology_Throws()
    {
        var map = new HexMap<int>(new HexMapTopology(3, 2, Layout.OddR));

        Assert.Throws<IndexOutOfRangeException>(() => _ = map[new VectorXYInt(3, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => map[new VectorXYInt(0, 2)] = 1);
    }

    [Test]
    public void HexMapTopology_WhenArgumentsAreInvalid_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexMapTopology(-1, 1, Layout.OddR));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexMapTopology(1, -1, Layout.OddR));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexMapTopology(1, 1, (Layout)42));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexMapTopology(new VectorXYInt(-1, 1), Layout.OddR));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexMapTopology(new VectorXYInt(1, -1), Layout.OddR));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexMapTopology(VectorXYInt.One, (Layout)42));
    }
}
