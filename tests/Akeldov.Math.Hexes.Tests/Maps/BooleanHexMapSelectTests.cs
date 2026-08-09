namespace Akeldov.Math.Hexes.Tests.Maps;

public class BooleanHexMapSelectTests
{
    [Test]
    public void Select_ReturnsIndependentCellwiseSelection()
    {
        var topology = new HexMapTopology(2, 2, Layout.EvenQ);
        var condition = new BoolHexMap(topology, new[] { true, false, true, false });
        var whenTrue = new FloatHexMap(topology, new[] { 1f, 2f, 3f, 4f });
        var whenFalse = new FloatHexMap(topology, new[] { 10f, 20f, 30f, 40f });

        FloatHexMap result = condition.Select(whenTrue, whenFalse);
        condition[0] = false;
        whenTrue[2] = 300f;
        whenFalse[1] = 200f;
        result[3] = -40f;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(1f));
            Assert.That(result[1], Is.EqualTo(20f));
            Assert.That(result[2], Is.EqualTo(3f));
            Assert.That(result[3], Is.EqualTo(-40f));
            Assert.That(whenFalse[3], Is.EqualTo(40f));
        });
    }

    [Test]
    public void Select_WhenMapIsNull_Throws()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var condition = new BoolHexMap(topology);
        var whenTrue = new FloatHexMap(topology);
        var whenFalse = new FloatHexMap(topology);
        BoolHexMap nullCondition = null!;
        FloatHexMap nullFloatMap = null!;

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullCondition.Select(whenTrue, whenFalse))!.ParamName,
                Is.EqualTo("condition"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => condition.Select(nullFloatMap, whenFalse))!.ParamName,
                Is.EqualTo("whenTrue"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => condition.Select(whenTrue, nullFloatMap))!.ParamName,
                Is.EqualTo("whenFalse"));
        });
    }

    [Test]
    public void Select_WhenTopologiesDiffer_Throws()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var otherTopology = new HexMapTopology(2, 1, Layout.OddR);
        var condition = new BoolHexMap(topology);
        var matchingMap = new FloatHexMap(topology);
        var otherMap = new FloatHexMap(otherTopology);

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => condition.Select(otherMap, matchingMap))!.ParamName,
                Is.EqualTo("whenTrue"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => condition.Select(matchingMap, otherMap))!.ParamName,
                Is.EqualTo("whenFalse"));
        });
    }

    [Test]
    public void Select_WhenMapsAreEmpty_ReturnsEmptyMapWithSameTopology()
    {
        var topology = new HexMapTopology(0, 0, Layout.EvenR);
        var condition = new BoolHexMap(topology);

        FloatHexMap result = condition.Select(
            new FloatHexMap(topology),
            new FloatHexMap(topology));

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void Select_WithIntMaps_ReturnsIndependentCellwiseSelection()
    {
        var topology = new HexMapTopology(2, 2, Layout.EvenQ);
        var condition = new BoolHexMap(topology, new[] { true, false, true, false });
        var whenTrue = new IntHexMap(topology, new[] { 1, 2, 3, 4 });
        var whenFalse = new IntHexMap(topology, new[] { 10, 20, 30, 40 });

        IntHexMap result = condition.Select(whenTrue, whenFalse);
        condition[0] = false;
        whenTrue[2] = 300;
        whenFalse[1] = 200;
        result[3] = -40;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(1));
            Assert.That(result[1], Is.EqualTo(20));
            Assert.That(result[2], Is.EqualTo(3));
            Assert.That(result[3], Is.EqualTo(-40));
            Assert.That(whenFalse[3], Is.EqualTo(40));
        });
    }

    [Test]
    public void Select_WithIntMaps_WhenMapIsNull_Throws()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var condition = new BoolHexMap(topology);
        var whenTrue = new IntHexMap(topology);
        var whenFalse = new IntHexMap(topology);
        BoolHexMap nullCondition = null!;
        IntHexMap nullIntMap = null!;

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullCondition.Select(whenTrue, whenFalse))!.ParamName,
                Is.EqualTo("condition"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => condition.Select(nullIntMap, whenFalse))!.ParamName,
                Is.EqualTo("whenTrue"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => condition.Select(whenTrue, nullIntMap))!.ParamName,
                Is.EqualTo("whenFalse"));
        });
    }

    [Test]
    public void Select_WithIntMaps_WhenTopologiesDiffer_Throws()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var otherTopology = new HexMapTopology(2, 1, Layout.OddR);
        var condition = new BoolHexMap(topology);
        var matchingMap = new IntHexMap(topology);
        var otherMap = new IntHexMap(otherTopology);

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => condition.Select(otherMap, matchingMap))!.ParamName,
                Is.EqualTo("whenTrue"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => condition.Select(matchingMap, otherMap))!.ParamName,
                Is.EqualTo("whenFalse"));
        });
    }

    [Test]
    public void Select_WithIntMaps_WhenMapsAreEmpty_ReturnsEmptyMapWithSameTopology()
    {
        var topology = new HexMapTopology(0, 0, Layout.EvenR);
        var condition = new BoolHexMap(topology);

        IntHexMap result = condition.Select(
            new IntHexMap(topology),
            new IntHexMap(topology));

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result.Topology.Count, Is.Zero);
        });
    }
}
