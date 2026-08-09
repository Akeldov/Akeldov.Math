namespace Akeldov.Math.Hexes.Tests.Maps;

public class BoolHexMapTests
{
    [Test]
    public void Constructor_InitializesCellsToFalse()
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);

        var map = new BoolHexMap(topology);
        IHexMap<bool> mapView = map;

        Assert.Multiple(() =>
        {
            Assert.That(map.Topology, Is.EqualTo(topology));
            Assert.That(mapView, Is.SameAs(map));
            for (int index = 0; index < topology.Count; index++)
                Assert.That(map[index], Is.False);
        });
    }

    [Test]
    public void ValuesConstructor_RetainsBackingArray()
    {
        var topology = new HexMapTopology(2, 2, Layout.OddR);
        var values = new[] { true, false, false, true };
        var map = new BoolHexMap(topology, values);

        values[1] = true;
        map[2] = true;

        Assert.Multiple(() =>
        {
            Assert.That(map[1], Is.True);
            Assert.That(values[2], Is.True);
        });
    }

    [Test]
    public void BitwiseAnd_ReturnsElementWiseConjunctionWithoutChangingOperands()
    {
        var topology = new HexMapTopology(2, 2, Layout.EvenQ);
        var left = new BoolHexMap(topology, new[] { true, true, false, false });
        var right = new BoolHexMap(topology, new[] { true, false, true, false });

        BoolHexMap result = left & right;
        Assert.That(result[3], Is.False);
        result[3] = true;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.False);
            Assert.That(result[2], Is.False);
            Assert.That(result[3], Is.True);
            Assert.That(left[3], Is.False);
            Assert.That(right[3], Is.False);
        });
    }

    [Test]
    public void BitwiseAnd_WithEquivalentTopologies_ReturnsElementWiseConjunction()
    {
        var left = new BoolHexMap(
            new HexMapTopology(2, 1, Layout.OddR),
            new[] { true, false });
        var right = new BoolHexMap(
            new HexMapTopology(2, 1, Layout.OddR),
            new[] { true, true });

        BoolHexMap result = left & right;

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.False);
        });
    }

    [Test]
    public void BitwiseAnd_WithDifferentTopologies_Throws()
    {
        var left = new BoolHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var differentResolution = new BoolHexMap(new HexMapTopology(1, 2, Layout.OddR));
        var differentLayout = new BoolHexMap(new HexMapTopology(2, 1, Layout.EvenR));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left & differentResolution)!.ParamName,
                Is.EqualTo("right"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left & differentLayout)!.ParamName,
                Is.EqualTo("right"));
        });
    }

    [Test]
    public void BitwiseAnd_WithNullOperand_Throws()
    {
        var map = new BoolHexMap(new HexMapTopology(1, 1, Layout.OddR));
        BoolHexMap? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = missing & map)!.ParamName,
                Is.EqualTo("left"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = map & missing)!.ParamName,
                Is.EqualTo("right"));
#pragma warning restore CS8604
        });
    }

    [Test]
    public void BitwiseOr_ReturnsElementWiseDisjunctionWithoutChangingOperands()
    {
        var topology = new HexMapTopology(2, 2, Layout.EvenQ);
        var left = new BoolHexMap(topology, new[] { true, true, false, false });
        var right = new BoolHexMap(topology, new[] { true, false, true, false });

        BoolHexMap result = left | right;
        Assert.That(result[3], Is.False);
        result[3] = true;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.True);
            Assert.That(result[2], Is.True);
            Assert.That(result[3], Is.True);
            Assert.That(left[3], Is.False);
            Assert.That(right[3], Is.False);
        });
    }

    [Test]
    public void BitwiseOr_WithEquivalentTopologies_ReturnsElementWiseDisjunction()
    {
        var left = new BoolHexMap(
            new HexMapTopology(2, 1, Layout.OddR),
            new[] { false, false });
        var right = new BoolHexMap(
            new HexMapTopology(2, 1, Layout.OddR),
            new[] { true, false });

        BoolHexMap result = left | right;

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.False);
        });
    }

    [Test]
    public void BitwiseOr_WithDifferentTopologies_Throws()
    {
        var left = new BoolHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var differentResolution = new BoolHexMap(new HexMapTopology(1, 2, Layout.OddR));
        var differentLayout = new BoolHexMap(new HexMapTopology(2, 1, Layout.EvenR));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left | differentResolution)!.ParamName,
                Is.EqualTo("right"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left | differentLayout)!.ParamName,
                Is.EqualTo("right"));
        });
    }

    [Test]
    public void BitwiseOr_WithNullOperand_Throws()
    {
        var map = new BoolHexMap(new HexMapTopology(1, 1, Layout.OddR));
        BoolHexMap? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = missing | map)!.ParamName,
                Is.EqualTo("left"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = map | missing)!.ParamName,
                Is.EqualTo("right"));
#pragma warning restore CS8604
        });
    }

    [Test]
    public void BitwiseXor_ReturnsElementWiseExclusiveDisjunctionWithoutChangingOperands()
    {
        var topology = new HexMapTopology(2, 2, Layout.EvenQ);
        var left = new BoolHexMap(topology, new[] { true, true, false, false });
        var right = new BoolHexMap(topology, new[] { true, false, true, false });

        BoolHexMap result = left ^ right;
        Assert.That(result[3], Is.False);
        result[3] = true;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.False);
            Assert.That(result[1], Is.True);
            Assert.That(result[2], Is.True);
            Assert.That(result[3], Is.True);
            Assert.That(left[3], Is.False);
            Assert.That(right[3], Is.False);
        });
    }

    [Test]
    public void BitwiseXor_WithEquivalentTopologies_ReturnsElementWiseExclusiveDisjunction()
    {
        var left = new BoolHexMap(
            new HexMapTopology(2, 1, Layout.OddR),
            new[] { true, false });
        var right = new BoolHexMap(
            new HexMapTopology(2, 1, Layout.OddR),
            new[] { false, false });

        BoolHexMap result = left ^ right;

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.False);
        });
    }

    [Test]
    public void BitwiseXor_WithDifferentTopologies_Throws()
    {
        var left = new BoolHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var differentResolution = new BoolHexMap(new HexMapTopology(1, 2, Layout.OddR));
        var differentLayout = new BoolHexMap(new HexMapTopology(2, 1, Layout.EvenR));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left ^ differentResolution)!.ParamName,
                Is.EqualTo("right"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left ^ differentLayout)!.ParamName,
                Is.EqualTo("right"));
        });
    }

    [Test]
    public void BitwiseXor_WithNullOperand_Throws()
    {
        var map = new BoolHexMap(new HexMapTopology(1, 1, Layout.OddR));
        BoolHexMap? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = missing ^ map)!.ParamName,
                Is.EqualTo("left"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = map ^ missing)!.ParamName,
                Is.EqualTo("right"));
#pragma warning restore CS8604
        });
    }
}
