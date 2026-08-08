namespace Akeldov.Math.Hexes.Tests.Maps;

public class FloatHexMapTests
{
    [Test]
    public void MinAndMax_ReturnCurrentValueRange()
    {
        var values = new[] { 3.5f, -2f, 7f };
        var map = new FloatHexMap(new HexMapTopology(3, 1, Layout.OddR), values);
        IFloatHexMap readOnlyMap = map;

        map[0] = -4f;
        values[2] = 9f;

        Assert.Multiple(() =>
        {
            Assert.That(readOnlyMap.Min, Is.EqualTo(-4f));
            Assert.That(readOnlyMap.Max, Is.EqualTo(9f));
        });
    }

    [Test]
    public void MinAndMax_WhenMapIsEmpty_Throw()
    {
        var map = new FloatHexMap(new HexMapTopology(0, 0, Layout.OddR));

        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidOperationException>(() => _ = map.Min);
            Assert.Throws<InvalidOperationException>(() => _ = map.Max);
        });
    }

    [Test]
    public void Addition_ReturnsElementWiseSumWithoutChangingOperands()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var left = new FloatHexMap(topology, new[] { 1.5f, -2f, 4f });
        var right = new FloatHexMap(topology, new[] { 2.5f, 3f, -1f });

        FloatHexMap result = left + right;
        result[0] = 100f;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(100f));
            Assert.That(result[1], Is.EqualTo(1f));
            Assert.That(result[2], Is.EqualTo(3f));
            Assert.That(left[0], Is.EqualTo(1.5f));
            Assert.That(right[0], Is.EqualTo(2.5f));
        });
    }

    [Test]
    public void Addition_WithEquivalentTopologies_ReturnsElementWiseSum()
    {
        var left = new FloatHexMap(
            new HexMapTopology(2, 1, Layout.EvenQ),
            new[] { 1f, 2f });
        var right = new FloatHexMap(
            new HexMapTopology(2, 1, Layout.EvenQ),
            new[] { 3f, 4f });

        FloatHexMap result = left + right;

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(4f));
            Assert.That(result[1], Is.EqualTo(6f));
        });
    }

    [Test]
    public void Addition_WithDifferentTopologies_Throws()
    {
        var left = new FloatHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var differentResolution = new FloatHexMap(new HexMapTopology(1, 2, Layout.OddR));
        var differentLayout = new FloatHexMap(new HexMapTopology(2, 1, Layout.EvenR));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left + differentResolution)!.ParamName,
                Is.EqualTo("right"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left + differentLayout)!.ParamName,
                Is.EqualTo("right"));
        });
    }

    [Test]
    public void Addition_WithNullOperand_Throws()
    {
        var map = new FloatHexMap(new HexMapTopology(1, 1, Layout.OddR));
        FloatHexMap? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = missing + map)!.ParamName,
                Is.EqualTo("left"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = map + missing)!.ParamName,
                Is.EqualTo("right"));
#pragma warning restore CS8604
        });
    }
}
