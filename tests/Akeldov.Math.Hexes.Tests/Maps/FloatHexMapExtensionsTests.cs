namespace Akeldov.Math.Hexes.Tests.Maps;

public class FloatHexMapExtensionsTests
{
    [Test]
    public void ToFloatHexMap_ReturnsIndependentMutableCopy()
    {
        var topology = new HexMapTopology(3, 1, Layout.EvenQ);
        var source = new HexMap<float>(topology, new[] { 1.5f, -2f, 4f });
        IHexMap<float> readOnlySource = source;

        FloatHexMap result = readOnlySource.ToFloatHexMap();
        source[0] = 10f;
        result[1] = 20f;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(1.5f));
            Assert.That(result[1], Is.EqualTo(20f));
            Assert.That(result[2], Is.EqualTo(4f));
            Assert.That(source[1], Is.EqualTo(-2f));
        });
    }

    [Test]
    public void ToFloatHexMap_WhenSourceIsFloatHexMap_ReturnsIndependentCopy()
    {
        var source = new FloatHexMap(
            new HexMapTopology(2, 1, Layout.OddR),
            new[] { 1f, 2f });

        FloatHexMap result = source.ToFloatHexMap();
        source[0] = 10f;
        result[1] = 20f;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.SameAs(source));
            Assert.That(result[0], Is.EqualTo(1f));
            Assert.That(source[1], Is.EqualTo(2f));
        });
    }

    [Test]
    public void ToFloatHexMap_WhenMapIsEmpty_ReturnsEmptyMapWithSameTopology()
    {
        var topology = new HexMapTopology(0, 0, Layout.EvenR);
        IHexMap<float> source = new HexMap<float>(topology);

        FloatHexMap result = source.ToFloatHexMap();

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void ToFloatHexMap_WhenMapIsNull_Throws()
    {
        IHexMap<float>? map = null;

#pragma warning disable CS8604
        var exception = Assert.Throws<ArgumentNullException>(() => map.ToFloatHexMap());
#pragma warning restore CS8604

        Assert.That(exception!.ParamName, Is.EqualTo("map"));
    }
}
