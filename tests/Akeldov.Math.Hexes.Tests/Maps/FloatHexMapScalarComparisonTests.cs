using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class FloatHexMapScalarComparisonTests
{
    private static readonly float[] SourceValues =
        { float.NegativeInfinity, -1f, 2f, float.PositiveInfinity, float.NaN };

    [Test]
    public void OrdinaryComparisons_WithScalarOnRight_ReturnCellwiseMasks()
    {
        var topology = new HexMapTopology(SourceValues.Length, 1, Layout.OddR);
        var map = new FloatHexMap(topology, (float[])SourceValues.Clone());

        BoolHexMap less = map < 2f;
        BoolHexMap lessOrEqual = map <= 2f;
        BoolHexMap greater = map > 2f;
        BoolHexMap greaterOrEqual = map >= 2f;

        Assert.Multiple(() =>
        {
            Assert.That(less, Is.TypeOf<BoolHexMap>());
            Assert.That(less.Topology, Is.EqualTo(topology));
            AssertMask(less, true, true, false, false, false);
            AssertMask(lessOrEqual, true, true, true, false, false);
            AssertMask(greater, false, false, false, true, false);
            AssertMask(greaterOrEqual, false, false, true, true, false);
            AssertMapUnchanged(map);
        });

        less[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(lessOrEqual[0], Is.True);
            AssertMapUnchanged(map);
        });
    }

    [Test]
    public void OrdinaryComparisons_WithScalarOnLeft_ReturnCellwiseMasks()
    {
        var topology = new HexMapTopology(SourceValues.Length, 1, Layout.OddR);
        var map = new FloatHexMap(topology, (float[])SourceValues.Clone());

        BoolHexMap less = 2f < map;
        BoolHexMap lessOrEqual = 2f <= map;
        BoolHexMap greater = 2f > map;
        BoolHexMap greaterOrEqual = 2f >= map;

        Assert.Multiple(() =>
        {
            Assert.That(less, Is.TypeOf<BoolHexMap>());
            Assert.That(less.Topology, Is.EqualTo(topology));
            AssertMask(less, false, false, false, true, false);
            AssertMask(lessOrEqual, false, false, true, true, false);
            AssertMask(greater, true, true, false, false, false);
            AssertMask(greaterOrEqual, true, true, true, false, false);
            AssertMapUnchanged(map);
        });
    }

    [Test]
    public void SpatialComparisons_WithScalarOnRight_ReturnCellwiseMasksAndPreserveGeometry()
    {
        HexMapGeometry geometry = Geometry();
        var map = new SpatialFloatHexMap(geometry, (float[])SourceValues.Clone());

        SpatialBoolHexMap less = map < 2f;
        SpatialBoolHexMap lessOrEqual = map <= 2f;
        SpatialBoolHexMap greater = map > 2f;
        SpatialBoolHexMap greaterOrEqual = map >= 2f;

        Assert.Multiple(() =>
        {
            Assert.That(less, Is.TypeOf<SpatialBoolHexMap>());
            Assert.That(less.Geometry, Is.EqualTo(geometry));
            Assert.That(lessOrEqual.Geometry, Is.EqualTo(geometry));
            Assert.That(greater.Geometry, Is.EqualTo(geometry));
            Assert.That(greaterOrEqual.Geometry, Is.EqualTo(geometry));
            AssertMask(less, true, true, false, false, false);
            AssertMask(lessOrEqual, true, true, true, false, false);
            AssertMask(greater, false, false, false, true, false);
            AssertMask(greaterOrEqual, false, false, true, true, false);
            AssertMapUnchanged(map);
        });

        less[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(lessOrEqual[0], Is.True);
            AssertMapUnchanged(map);
        });
    }

    [Test]
    public void SpatialComparisons_WithScalarOnLeft_ReturnCellwiseMasksAndPreserveGeometry()
    {
        HexMapGeometry geometry = Geometry();
        var map = new SpatialFloatHexMap(geometry, (float[])SourceValues.Clone());

        SpatialBoolHexMap less = 2f < map;
        SpatialBoolHexMap lessOrEqual = 2f <= map;
        SpatialBoolHexMap greater = 2f > map;
        SpatialBoolHexMap greaterOrEqual = 2f >= map;

        Assert.Multiple(() =>
        {
            Assert.That(less, Is.TypeOf<SpatialBoolHexMap>());
            Assert.That(less.Geometry, Is.EqualTo(geometry));
            Assert.That(lessOrEqual.Geometry, Is.EqualTo(geometry));
            Assert.That(greater.Geometry, Is.EqualTo(geometry));
            Assert.That(greaterOrEqual.Geometry, Is.EqualTo(geometry));
            AssertMask(less, false, false, false, true, false);
            AssertMask(lessOrEqual, false, false, true, true, false);
            AssertMask(greater, true, true, false, false, false);
            AssertMask(greaterOrEqual, true, true, true, false, false);
            AssertMapUnchanged(map);
        });
    }

    [Test]
    public void OrdinaryComparisons_WithNullMap_Throw()
    {
        FloatHexMap? map = null;

#pragma warning disable CS8604
        AssertNullMap(
            () => _ = map < 2f,
            () => _ = map <= 2f,
            () => _ = map > 2f,
            () => _ = map >= 2f,
            () => _ = 2f < map,
            () => _ = 2f <= map,
            () => _ = 2f > map,
            () => _ = 2f >= map);
#pragma warning restore CS8604
    }

    [Test]
    public void SpatialComparisons_WithNullMap_Throw()
    {
        SpatialFloatHexMap? map = null;

#pragma warning disable CS8604
        AssertNullMap(
            () => _ = map < 2f,
            () => _ = map <= 2f,
            () => _ = map > 2f,
            () => _ = map >= 2f,
            () => _ = 2f < map,
            () => _ = 2f <= map,
            () => _ = 2f > map,
            () => _ = 2f >= map);
#pragma warning restore CS8604
    }

    private static HexMapGeometry Geometry() =>
        new(SourceValues.Length, 1, new VectorXY(4f, -3f), 2.5f, Layout.EvenQ);

    private static void AssertMask(IHexMap<bool> actual, params bool[] expected)
    {
        Assert.That(actual.Topology.Count, Is.EqualTo(expected.Length));

        for (int index = 0; index < expected.Length; index++)
            Assert.That(actual[index], Is.EqualTo(expected[index]), $"Unexpected value at index {index}.");
    }

    private static void AssertMapUnchanged(IHexMap<float> map)
    {
        for (int index = 0; index < SourceValues.Length; index++)
            Assert.That(map[index], Is.EqualTo(SourceValues[index]), $"Unexpected source value at index {index}.");
    }

    private static void AssertNullMap(params TestDelegate[] actions)
    {
        Assert.Multiple(() =>
        {
            foreach (TestDelegate action in actions)
                Assert.That(Assert.Throws<ArgumentNullException>(action)!.ParamName, Is.EqualTo("map"));
        });
    }
}
