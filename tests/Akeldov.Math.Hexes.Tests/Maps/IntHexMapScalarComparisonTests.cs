using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class IntHexMapScalarComparisonTests
{
    private static readonly int[] SourceValues =
        { int.MinValue, -1, 2, int.MaxValue };

    [Test]
    public void OrdinaryComparisons_WithScalarOnRight_ReturnCellwiseMasks()
    {
        var topology = new HexMapTopology(SourceValues.Length, 1, Layout.OddR);
        var map = new IntHexMap(topology, (int[])SourceValues.Clone());

        BoolHexMap less = map < 2;
        BoolHexMap lessOrEqual = map <= 2;
        BoolHexMap greater = map > 2;
        BoolHexMap greaterOrEqual = map >= 2;

        Assert.Multiple(() =>
        {
            Assert.That(less, Is.TypeOf<BoolHexMap>());
            Assert.That(less.Topology, Is.EqualTo(topology));
            AssertMask(less, true, true, false, false);
            AssertMask(lessOrEqual, true, true, true, false);
            AssertMask(greater, false, false, false, true);
            AssertMask(greaterOrEqual, false, false, true, true);
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
        var map = new IntHexMap(topology, (int[])SourceValues.Clone());

        BoolHexMap less = 2 < map;
        BoolHexMap lessOrEqual = 2 <= map;
        BoolHexMap greater = 2 > map;
        BoolHexMap greaterOrEqual = 2 >= map;

        Assert.Multiple(() =>
        {
            Assert.That(less, Is.TypeOf<BoolHexMap>());
            Assert.That(less.Topology, Is.EqualTo(topology));
            AssertMask(less, false, false, false, true);
            AssertMask(lessOrEqual, false, false, true, true);
            AssertMask(greater, true, true, false, false);
            AssertMask(greaterOrEqual, true, true, true, false);
            AssertMapUnchanged(map);
        });
    }

    [Test]
    public void SpatialComparisons_WithScalarOnRight_ReturnCellwiseMasksAndPreserveGeometry()
    {
        HexMapGeometry geometry = Geometry();
        var map = new SpatialIntHexMap(geometry, (int[])SourceValues.Clone());

        SpatialBoolHexMap less = map < 2;
        SpatialBoolHexMap lessOrEqual = map <= 2;
        SpatialBoolHexMap greater = map > 2;
        SpatialBoolHexMap greaterOrEqual = map >= 2;

        Assert.Multiple(() =>
        {
            Assert.That(less, Is.TypeOf<SpatialBoolHexMap>());
            Assert.That(less.Geometry, Is.EqualTo(geometry));
            Assert.That(lessOrEqual.Geometry, Is.EqualTo(geometry));
            Assert.That(greater.Geometry, Is.EqualTo(geometry));
            Assert.That(greaterOrEqual.Geometry, Is.EqualTo(geometry));
            AssertMask(less, true, true, false, false);
            AssertMask(lessOrEqual, true, true, true, false);
            AssertMask(greater, false, false, false, true);
            AssertMask(greaterOrEqual, false, false, true, true);
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
        var map = new SpatialIntHexMap(geometry, (int[])SourceValues.Clone());

        SpatialBoolHexMap less = 2 < map;
        SpatialBoolHexMap lessOrEqual = 2 <= map;
        SpatialBoolHexMap greater = 2 > map;
        SpatialBoolHexMap greaterOrEqual = 2 >= map;

        Assert.Multiple(() =>
        {
            Assert.That(less, Is.TypeOf<SpatialBoolHexMap>());
            Assert.That(less.Geometry, Is.EqualTo(geometry));
            Assert.That(lessOrEqual.Geometry, Is.EqualTo(geometry));
            Assert.That(greater.Geometry, Is.EqualTo(geometry));
            Assert.That(greaterOrEqual.Geometry, Is.EqualTo(geometry));
            AssertMask(less, false, false, false, true);
            AssertMask(lessOrEqual, false, false, true, true);
            AssertMask(greater, true, true, false, false);
            AssertMask(greaterOrEqual, true, true, true, false);
            AssertMapUnchanged(map);
        });
    }

    [Test]
    public void Comparisons_WithEmptyMaps_ReturnEmptyMasks()
    {
        var topology = new HexMapTopology(0, 0, Layout.EvenR);
        var ordinaryMap = new IntHexMap(topology);
        var geometry = new HexMapGeometry(topology, new VectorXY(4f, -3f), 2.5f);
        var spatialMap = new SpatialIntHexMap(geometry);
        BoolHexMap[] ordinaryResults =
        {
            ordinaryMap < 0, ordinaryMap <= 0, ordinaryMap > 0, ordinaryMap >= 0,
            0 < ordinaryMap, 0 <= ordinaryMap, 0 > ordinaryMap, 0 >= ordinaryMap,
        };
        SpatialBoolHexMap[] spatialResults =
        {
            spatialMap < 0, spatialMap <= 0, spatialMap > 0, spatialMap >= 0,
            0 < spatialMap, 0 <= spatialMap, 0 > spatialMap, 0 >= spatialMap,
        };

        Assert.Multiple(() =>
        {
            foreach (BoolHexMap result in ordinaryResults)
            {
                Assert.That(result, Is.TypeOf<BoolHexMap>());
                Assert.That(result.Topology, Is.EqualTo(topology));
                Assert.That(result.Topology.Count, Is.Zero);
            }

            foreach (SpatialBoolHexMap result in spatialResults)
            {
                Assert.That(result, Is.TypeOf<SpatialBoolHexMap>());
                Assert.That(result.Geometry, Is.EqualTo(geometry));
                Assert.That(result.Topology.Count, Is.Zero);
            }
        });
    }

    [Test]
    public void OrdinaryComparisons_WithNullMap_Throw()
    {
        IntHexMap? map = null;

#pragma warning disable CS8604
        AssertNullMap(
            () => _ = map < 2,
            () => _ = map <= 2,
            () => _ = map > 2,
            () => _ = map >= 2,
            () => _ = 2 < map,
            () => _ = 2 <= map,
            () => _ = 2 > map,
            () => _ = 2 >= map);
#pragma warning restore CS8604
    }

    [Test]
    public void SpatialComparisons_WithNullMap_Throw()
    {
        SpatialIntHexMap? map = null;

#pragma warning disable CS8604
        AssertNullMap(
            () => _ = map < 2,
            () => _ = map <= 2,
            () => _ = map > 2,
            () => _ = map >= 2,
            () => _ = 2 < map,
            () => _ = 2 <= map,
            () => _ = 2 > map,
            () => _ = 2 >= map);
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

    private static void AssertMapUnchanged(IHexMap<int> map)
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
