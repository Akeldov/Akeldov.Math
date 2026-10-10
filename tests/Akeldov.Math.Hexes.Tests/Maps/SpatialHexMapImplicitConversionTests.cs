using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class SpatialHexMapImplicitConversionTests
{
    [TestCase(2, 2, Layout.OddR)]
    [TestCase(2, 2, Layout.EvenR)]
    [TestCase(2, 2, Layout.OddQ)]
    [TestCase(2, 2, Layout.EvenQ)]
    [TestCase(0, 0, Layout.OddR)]
    public void BoolConversions_ShareValuesAndPreserveGeometryInBothDirections(int width, int height, Layout layout)
    {
        var geometry = new HexMapGeometry(width, height, new VectorXY(10f, -20f), 2f, layout);
        bool[] values = geometry.Topology.Count == 0 ? Array.Empty<bool>() : new[] { true, false, false, true };
        var specializedSource = new SpatialBoolHexMap(geometry, values);
        SpatialHexMap<bool> genericFacade = specializedSource;
        SpatialBoolHexMap specializedRoundTrip = genericFacade;
        AssertSharedValues(geometry, values, true, false, specializedSource, genericFacade, specializedRoundTrip);

        var genericSource = new SpatialHexMap<bool>(geometry, values);
        SpatialBoolHexMap specializedFacade = genericSource;
        SpatialHexMap<bool> genericRoundTrip = specializedFacade;
        AssertSharedValues(geometry, values, true, false, genericSource, specializedFacade, genericRoundTrip);
    }

    [TestCase(2, 2, Layout.OddR)]
    [TestCase(2, 2, Layout.EvenR)]
    [TestCase(2, 2, Layout.OddQ)]
    [TestCase(2, 2, Layout.EvenQ)]
    [TestCase(0, 0, Layout.OddR)]
    public void FloatConversions_ShareValuesAndPreserveGeometryInBothDirections(int width, int height, Layout layout)
    {
        var geometry = new HexMapGeometry(width, height, new VectorXY(10f, -20f), 2f, layout);
        float[] values = geometry.Topology.Count == 0 ? Array.Empty<float>() : new[] { float.NaN, -2.5f, float.PositiveInfinity, -0f };
        var specializedSource = new SpatialFloatHexMap(geometry, values);
        SpatialHexMap<float> genericFacade = specializedSource;
        SpatialFloatHexMap specializedRoundTrip = genericFacade;
        AssertSharedValues(geometry, values, 12.5f, -4.25f, specializedSource, genericFacade, specializedRoundTrip);

        var genericSource = new SpatialHexMap<float>(geometry, values);
        SpatialFloatHexMap specializedFacade = genericSource;
        SpatialHexMap<float> genericRoundTrip = specializedFacade;
        AssertSharedValues(geometry, values, 12.5f, -4.25f, genericSource, specializedFacade, genericRoundTrip);
    }

    [TestCase(2, 2, Layout.OddR)]
    [TestCase(2, 2, Layout.EvenR)]
    [TestCase(2, 2, Layout.OddQ)]
    [TestCase(2, 2, Layout.EvenQ)]
    [TestCase(0, 0, Layout.OddR)]
    public void IntConversions_ShareValuesAndPreserveGeometryInBothDirections(int width, int height, Layout layout)
    {
        var geometry = new HexMapGeometry(width, height, new VectorXY(10f, -20f), 2f, layout);
        int[] values = geometry.Topology.Count == 0 ? Array.Empty<int>() : new[] { int.MinValue, 2, int.MaxValue, -4 };
        var specializedSource = new SpatialIntHexMap(geometry, values);
        SpatialHexMap<int> genericFacade = specializedSource;
        SpatialIntHexMap specializedRoundTrip = genericFacade;
        AssertSharedValues(geometry, values, 12, -4, specializedSource, genericFacade, specializedRoundTrip);

        var genericSource = new SpatialHexMap<int>(geometry, values);
        SpatialIntHexMap specializedFacade = genericSource;
        SpatialHexMap<int> genericRoundTrip = specializedFacade;
        AssertSharedValues(geometry, values, 12, -4, genericSource, specializedFacade, genericRoundTrip);
    }

    [Test]
    public void NullConversions_ReturnNullInBothDirections()
    {
        SpatialBoolHexMap? specializedBool = null;
        SpatialFloatHexMap? specializedFloat = null;
        SpatialIntHexMap? specializedInt = null;
        SpatialHexMap<bool>? genericBool = specializedBool;
        SpatialHexMap<float>? genericFloat = specializedFloat;
        SpatialHexMap<int>? genericInt = specializedInt;
        SpatialBoolHexMap? boolRoundTrip = genericBool;
        SpatialFloatHexMap? floatRoundTrip = genericFloat;
        SpatialIntHexMap? intRoundTrip = genericInt;

        Assert.Multiple(() =>
        {
            Assert.That(genericBool, Is.Null);
            Assert.That(genericFloat, Is.Null);
            Assert.That(genericInt, Is.Null);
            Assert.That(boolRoundTrip, Is.Null);
            Assert.That(floatRoundTrip, Is.Null);
            Assert.That(intRoundTrip, Is.Null);
        });
    }

    [Test]
    public void MixedSpatialOperators_PreserveGeometryAndOperandOrder()
    {
        var geometry = new HexMapGeometry(1, 1, new VectorXY(3f, -2f), 2f, Layout.OddR);
        var specializedBool = new SpatialBoolHexMap(geometry, new[] { true });
        var genericBool = new SpatialHexMap<bool>(geometry, new[] { false });
        var specializedFloat = new SpatialFloatHexMap(geometry, new[] { 8f });
        var genericFloat = new SpatialHexMap<float>(geometry, new[] { 2f });
        var specializedInt = new SpatialIntHexMap(geometry, new[] { 7 });
        var genericInt = new SpatialHexMap<int>(geometry, new[] { 3 });

        SpatialBoolHexMap boolResult = specializedBool ^ genericBool;
        SpatialBoolHexMap reverseBoolResult = genericBool ^ specializedBool;
        SpatialFloatHexMap floatResult = specializedFloat / genericFloat;
        SpatialFloatHexMap reverseFloatResult = genericFloat / specializedFloat;
        SpatialIntHexMap intResult = specializedInt - genericInt;
        SpatialIntHexMap reverseIntResult = genericInt - specializedInt;
        SpatialFloatHexMap mixedResult = genericInt / specializedFloat;
        SpatialFloatHexMap reverseMixedResult = specializedFloat / genericInt;

        Assert.Multiple(() =>
        {
            Assert.That(boolResult[0], Is.True);
            Assert.That(reverseBoolResult[0], Is.True);
            Assert.That(floatResult[0], Is.EqualTo(4f));
            Assert.That(reverseFloatResult[0], Is.EqualTo(0.25f));
            Assert.That(intResult[0], Is.EqualTo(4));
            Assert.That(reverseIntResult[0], Is.EqualTo(-4));
            Assert.That(mixedResult[0], Is.EqualTo(3f / 8f));
            Assert.That(reverseMixedResult[0], Is.EqualTo(8f / 3f));
            Assert.That(boolResult.Geometry, Is.EqualTo(geometry));
            Assert.That(reverseBoolResult.Geometry, Is.EqualTo(geometry));
            Assert.That(floatResult.Geometry, Is.EqualTo(geometry));
            Assert.That(reverseFloatResult.Geometry, Is.EqualTo(geometry));
            Assert.That(intResult.Geometry, Is.EqualTo(geometry));
            Assert.That(reverseIntResult.Geometry, Is.EqualTo(geometry));
            Assert.That(mixedResult.Geometry, Is.EqualTo(geometry));
            Assert.That(reverseMixedResult.Geometry, Is.EqualTo(geometry));
        });
    }

    private static void AssertSharedValues<T>(HexMapGeometry geometry, T[] values, T first, T second, params HexMap<T>[] maps)
    {
        for (int i = 0; i < maps.Length; i++)
        {
            Assert.That(maps[i].Topology, Is.EqualTo(geometry.Topology));
            Assert.That(((ISpatialHexMap<T>)maps[i]).Geometry, Is.EqualTo(geometry));
            Assert.That(Enumerable.Range(0, values.Length).Select(index => maps[i][index]), Is.EqualTo(values));
            for (int j = i + 1; j < maps.Length; j++)
                Assert.That(maps[i], Is.Not.SameAs(maps[j]));
        }

        if (values.Length == 0)
            return;

        values[0] = first;
        foreach (HexMap<T> map in maps)
            Assert.That(map[0], Is.EqualTo(first));

        for (int i = 0; i < maps.Length; i++)
        {
            T value = i % 2 == 0 ? second : first;
            maps[i][new VectorXYInt(0, 0)] = value;
            Assert.That(values[0], Is.EqualTo(value));
            foreach (HexMap<T> map in maps)
                Assert.That(map[0], Is.EqualTo(value));
        }
    }
}
