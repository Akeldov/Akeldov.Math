using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class HexMapEqualityOperatorTests
{
    [Test]
    public void OrdinaryMapEquality_ReturnsIndependentCellwiseMasks()
    {
        var topology = new HexMapTopology(4, 1, Layout.OddR);
        var boolLeft = new BoolHexMap(topology, new[] { true, true, false, false });
        var boolRight = new BoolHexMap(topology, new[] { true, false, true, false });
        var floatLeft = new FloatHexMap(topology, new[] { 1f, 2f, float.NaN, 16_777_216f });
        var intRight = new IntHexMap(topology, new[] { 1, 3, 0, 16_777_217 });

        BoolHexMap boolEqual = boolLeft == boolRight;
        BoolHexMap boolDifferent = boolLeft != boolRight;
        BoolHexMap numericEqual = floatLeft == intRight;
        BoolHexMap numericDifferent = intRight != floatLeft;

        Assert.Multiple(() =>
        {
            AssertMask(boolEqual, true, false, false, true);
            AssertMask(boolDifferent, false, true, true, false);
            AssertMask(numericEqual, true, false, false, true);
            AssertMask(numericDifferent, false, true, true, false);
            Assert.That(boolEqual.Topology, Is.EqualTo(topology));
        });

        boolEqual[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(boolLeft[0], Is.True);
            Assert.That(boolRight[0], Is.True);
            Assert.That(boolDifferent[0], Is.False);
        });
    }

    [Test]
    public void ScalarEquality_ReturnsOrdinaryAndSpatialMasksForEitherOperandOrder()
    {
        var topology = new HexMapTopology(3, 1, Layout.EvenQ);
        var boolMap = new BoolHexMap(topology, new[] { true, false, true });
        var floatMap = new FloatHexMap(topology, new[] { 2f, float.NaN, -1f });
        var intMap = new IntHexMap(topology, new[] { 2, 3, -1 });
        var geometry = new HexMapGeometry(topology, new VectorXY(4f, -3f), 2f);
        var spatialBool = new SpatialBoolHexMap(geometry, new[] { true, false, true });
        var spatialFloat = new SpatialFloatHexMap(geometry, new[] { 2f, float.NaN, -1f });
        var spatialInt = new SpatialIntHexMap(geometry, new[] { 2, 3, -1 });

        Assert.Multiple(() =>
        {
            AssertMask(boolMap == true, true, false, true);
            AssertMask(false != boolMap, true, false, true);
            AssertMask(floatMap == 2f, true, false, false);
            AssertMask(float.NaN != floatMap, true, true, true);
            AssertMask(intMap == 2, true, false, false);
            AssertMask(-1 != intMap, true, true, false);

            AssertSpatialMask(spatialBool == true, geometry, true, false, true);
            AssertSpatialMask(false != spatialBool, geometry, true, false, true);
            AssertSpatialMask(spatialFloat == 2f, geometry, true, false, false);
            AssertSpatialMask(float.NaN != spatialFloat, geometry, true, true, true);
            AssertSpatialMask(spatialInt == 2, geometry, true, false, false);
            AssertSpatialMask(-1 != spatialInt, geometry, true, true, false);
        });
    }

    [Test]
    public void OrdinaryMapEquality_ValidatesTopologyAndNullOperands()
    {
        var topology = new HexMapTopology(2, 1, Layout.OddR);
        var differentTopology = new HexMapTopology(1, 2, Layout.OddR);
        var boolMap = new BoolHexMap(topology);
        var floatMap = new FloatHexMap(topology);
        var intMap = new IntHexMap(topology);
        var differentBool = new BoolHexMap(differentTopology);
        var differentFloat = new FloatHexMap(differentTopology);
        var differentInt = new IntHexMap(differentTopology);
        BoolHexMap? missingBool = null;
        FloatHexMap? missingFloat = null;
        IntHexMap? missingInt = null;

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentException>(() => _ = boolMap == differentBool)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = floatMap != differentFloat)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = intMap == differentInt)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = floatMap != differentInt)!.ParamName, Is.EqualTo("right"));

#pragma warning disable CS8604
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missingBool == boolMap)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = boolMap != missingBool)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missingFloat == floatMap)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = floatMap != missingInt)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missingInt == intMap)!.ParamName, Is.EqualTo("left"));
#pragma warning restore CS8604
        });
    }

    private static void AssertMask(IHexMap<bool> actual, params bool[] expected)
    {
        Assert.That(actual.Topology.Count, Is.EqualTo(expected.Length));
        for (int index = 0; index < expected.Length; index++)
            Assert.That(actual[index], Is.EqualTo(expected[index]), $"Unexpected value at index {index}.");
    }

    private static void AssertSpatialMask(
        SpatialBoolHexMap actual,
        HexMapGeometry expectedGeometry,
        params bool[] expected)
    {
        Assert.That(actual.Geometry, Is.EqualTo(expectedGeometry));
        AssertMask(actual, expected);
    }
}
