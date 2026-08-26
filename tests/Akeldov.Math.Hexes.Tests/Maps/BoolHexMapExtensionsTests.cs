namespace Akeldov.Math.Hexes.Tests.Maps;

public class BoolHexMapExtensionsTests
{
    [Test]
    public void ToBoolHexMap_ReturnsIndependentMutableCopy()
    {
        var topology = new HexMapTopology(3, 1, Layout.EvenQ);
        var source = new HexMap<bool>(topology, new[] { true, false, true });
        IHexMap<bool> readOnlySource = source;

        BoolHexMap result = readOnlySource.ToBoolHexMap();
        source[0] = false;
        result[1] = true;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.True);
            Assert.That(result[2], Is.True);
            Assert.That(source[1], Is.False);
        });
    }

    [Test]
    public void ToBoolHexMap_WhenSourceIsBoolHexMap_ReturnsIndependentCopy()
    {
        var source = new BoolHexMap(
            new HexMapTopology(2, 1, Layout.OddR),
            new[] { true, false });

        BoolHexMap result = source.ToBoolHexMap();
        source[0] = false;
        result[1] = true;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.SameAs(source));
            Assert.That(result[0], Is.True);
            Assert.That(source[1], Is.False);
        });
    }

    [Test]
    public void ToBoolHexMap_WhenMapIsEmpty_ReturnsEmptyMapWithSameTopology()
    {
        var topology = new HexMapTopology(0, 0, Layout.EvenR);
        IHexMap<bool> source = new HexMap<bool>(topology);

        BoolHexMap result = source.ToBoolHexMap();

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void ToBoolHexMap_WhenMapIsNull_Throws()
    {
        IHexMap<bool>? map = null;

#pragma warning disable CS8604
        var exception = Assert.Throws<ArgumentNullException>(() => map.ToBoolHexMap());
#pragma warning restore CS8604

        Assert.That(exception!.ParamName, Is.EqualTo("map"));
    }
}
