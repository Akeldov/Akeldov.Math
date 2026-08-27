using Akeldov.Math.Hexes.Geometry;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class IntHexMapExtensionsTests
{
    [Test]
    public void GetMinMax_ReturnsBothExtremaForInterfaceTypedSpatialMap()
    {
        var geometry = new HexMapGeometry(4, 1, 2f, Layout.OddR);
        IHexMap<int> map = new SpatialIntHexMap(geometry, new[] { 7, -4, 12, 3 });

        (int min, int max) = map.GetMinMax();

        Assert.Multiple(() =>
        {
            Assert.That(min, Is.EqualTo(-4));
            Assert.That(max, Is.EqualTo(12));
        });
    }

    [Test]
    public void TryGetMinMax_WhenMapIsEmpty_ReturnsFalseAndZeroOutputs()
    {
        IHexMap<int> map = new HexMap<int>(new HexMapTopology(0, 0, Layout.OddR));

        bool found = map.TryGetMinMax(out int min, out int max);

        Assert.Multiple(() =>
        {
            Assert.That(found, Is.False);
            Assert.That(min, Is.Zero);
            Assert.That(max, Is.Zero);
            Assert.Throws<InvalidOperationException>(() => map.GetMinMax());
        });
    }

    [Test]
    public void GetMinMax_WhenMapIsNull_Throws()
    {
        IHexMap<int>? map = null;

#pragma warning disable CS8604
        var getException = Assert.Throws<ArgumentNullException>(() => map.GetMinMax());
        var tryException = Assert.Throws<ArgumentNullException>(() => map.TryGetMinMax(out _, out _));
#pragma warning restore CS8604

        Assert.Multiple(() =>
        {
            Assert.That(getException!.ParamName, Is.EqualTo("map"));
            Assert.That(tryException!.ParamName, Is.EqualTo("map"));
        });
    }

    [Test]
    public void ToIntHexMap_ReturnsIndependentMutableCopy()
    {
        var topology = new HexMapTopology(3, 1, Layout.EvenQ);
        var source = new HexMap<int>(topology, new[] { 1, -2, 4 });
        IHexMap<int> readOnlySource = source;

        IntHexMap result = readOnlySource.ToIntHexMap();
        source[0] = 10;
        result[1] = 20;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(1));
            Assert.That(result[1], Is.EqualTo(20));
            Assert.That(result[2], Is.EqualTo(4));
            Assert.That(source[1], Is.EqualTo(-2));
        });
    }

    [Test]
    public void ToIntHexMap_WhenSourceIsIntHexMap_ReturnsIndependentCopy()
    {
        var source = new IntHexMap(
            new HexMapTopology(2, 1, Layout.OddR),
            new[] { 1, 2 });

        IntHexMap result = source.ToIntHexMap();
        source[0] = 10;
        result[1] = 20;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.SameAs(source));
            Assert.That(result[0], Is.EqualTo(1));
            Assert.That(source[1], Is.EqualTo(2));
        });
    }

    [Test]
    public void ToIntHexMap_WhenMapIsEmpty_ReturnsEmptyMapWithSameTopology()
    {
        var topology = new HexMapTopology(0, 0, Layout.EvenR);
        IHexMap<int> source = new HexMap<int>(topology);

        IntHexMap result = source.ToIntHexMap();

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void ToIntHexMap_WhenMapIsNull_Throws()
    {
        IHexMap<int>? map = null;

#pragma warning disable CS8604
        var exception = Assert.Throws<ArgumentNullException>(() => map.ToIntHexMap());
#pragma warning restore CS8604

        Assert.That(exception!.ParamName, Is.EqualTo("map"));
    }
}
