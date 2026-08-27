using Akeldov.Math.Hexes.Geometry;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class FloatHexMapExtensionsTests
{
    [Test]
    public void GetMinMax_ReturnsBothExtremaForInterfaceTypedSpatialMap()
    {
        var geometry = new HexMapGeometry(4, 1, 2f, Layout.OddR);
        IHexMap<float> map = new SpatialFloatHexMap(geometry, new[] { 7f, -4f, 12f, 3f });

        (float min, float max) = map.GetMinMax();

        Assert.Multiple(() =>
        {
            Assert.That(min, Is.EqualTo(-4f));
            Assert.That(max, Is.EqualTo(12f));
        });
    }

    [Test]
    public void GetMinMax_WhenMapContainsNaN_PropagatesNaN()
    {
        IHexMap<float> map = new HexMap<float>(
            new HexMapTopology(3, 1, Layout.OddR),
            new[] { 1f, float.NaN, 3f });

        (float min, float max) = map.GetMinMax();

        Assert.Multiple(() =>
        {
            Assert.That(min, Is.NaN);
            Assert.That(max, Is.NaN);
        });
    }

    [Test]
    public void TryGetMinMax_WhenMapIsEmpty_ReturnsFalseAndZeroOutputs()
    {
        IHexMap<float> map = new HexMap<float>(new HexMapTopology(0, 0, Layout.OddR));

        bool found = map.TryGetMinMax(out float min, out float max);

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
        IHexMap<float>? map = null;

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
