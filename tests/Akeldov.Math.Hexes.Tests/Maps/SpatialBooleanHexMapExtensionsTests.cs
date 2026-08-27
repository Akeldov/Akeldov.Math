using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class SpatialBooleanHexMapExtensionsTests
{
    [Test]
    public void BooleanConversions_AreDeclaredOnConsolidatedExtensionType()
    {
        Type extensionsType = typeof(BooleanHexMapExtensions);

        Assert.Multiple(() =>
        {
            Assert.That(
                extensionsType.GetMethod(
                    nameof(BooleanHexMapExtensions.ToBoolHexMap),
                    new[] { typeof(IHexMap<bool>) }),
                Is.Not.Null);
            Assert.That(
                extensionsType.GetMethod(
                    nameof(BooleanHexMapExtensions.ToSpatialHexMap),
                    new[] { typeof(IHexMap<bool>), typeof(HexMapGeometry) }),
                Is.Not.Null);
            Assert.That(
                extensionsType.GetMethod(
                    nameof(BooleanHexMapExtensions.ToHexMap),
                    new[] { typeof(ISpatialHexMap<bool>) }),
                Is.Not.Null);
            Assert.That(
                extensionsType.Assembly.GetType("Akeldov.Math.Hexes.BoolHexMapExtensions"),
                Is.Null);
        });
    }

    [Test]
    public void AndAndOr_WithConcreteSpatialMaps_ReturnSpecializedIndependentMaps()
    {
        HexMapGeometry geometry = Geometry();
        var equivalentGeometry = new HexMapGeometry(
            geometry.Topology,
            geometry.Origin,
            geometry.Radius);
        var left = new SpatialBoolHexMap(geometry, new[] { true, true, false, false });
        var right = new SpatialBoolHexMap(equivalentGeometry, new[] { true, false, true, false });

        SpatialBoolHexMap conjunction = left.And(right);
        SpatialBoolHexMap disjunction = left.Or(right);

        Assert.Multiple(() =>
        {
            Assert.That(conjunction, Is.TypeOf<SpatialBoolHexMap>());
            Assert.That(disjunction, Is.TypeOf<SpatialBoolHexMap>());
            Assert.That(conjunction.Geometry, Is.EqualTo(geometry));
            Assert.That(disjunction.Geometry, Is.EqualTo(geometry));
            Assert.That(Values(conjunction), Is.EqualTo(new[] { true, false, false, false }));
            Assert.That(Values(disjunction), Is.EqualTo(new[] { true, true, true, false }));
        });

        conjunction[0] = false;
        disjunction[1] = false;

        Assert.Multiple(() =>
        {
            Assert.That(Values(left), Is.EqualTo(new[] { true, true, false, false }));
            Assert.That(Values(right), Is.EqualTo(new[] { true, false, true, false }));
        });
    }

    [Test]
    public void AndAndOr_ValidateNullOperandsAndGeometry()
    {
        HexMapGeometry geometry = Geometry();
        var map = new SpatialBoolHexMap(geometry);
        var otherGeometry = new HexMapGeometry(
            geometry.Topology,
            new VectorXY(geometry.Origin.X + 1f, geometry.Origin.Y),
            geometry.Radius);
        var other = new SpatialBoolHexMap(otherGeometry);
        SpatialBoolHexMap nullMap = null!;

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullMap.And(map))!.ParamName,
                Is.EqualTo("left"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => map.And(nullMap))!.ParamName,
                Is.EqualTo("right"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => map.And(other))!.ParamName,
                Is.EqualTo("right"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullMap.Or(map))!.ParamName,
                Is.EqualTo("left"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => map.Or(nullMap))!.ParamName,
                Is.EqualTo("right"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => map.Or(other))!.ParamName,
                Is.EqualTo("right"));
        });
    }

    [Test]
    public void Select_WithConcreteSpatialMaps_ReturnsSpecializedIndependentMaps()
    {
        HexMapGeometry geometry = Geometry();
        var condition = new SpatialBoolHexMap(geometry, new[] { true, false, true, false });
        var floatWhenTrue = new SpatialFloatHexMap(geometry, new[] { 1f, 2f, 3f, 4f });
        var floatWhenFalse = new SpatialFloatHexMap(geometry, new[] { 10f, 20f, 30f, 40f });
        var intWhenTrue = new SpatialIntHexMap(geometry, new[] { 1, 2, 3, 4 });
        var intWhenFalse = new SpatialIntHexMap(geometry, new[] { 10, 20, 30, 40 });
        var boolWhenTrue = new SpatialBoolHexMap(geometry, new[] { true, true, false, false });
        var boolWhenFalse = new SpatialBoolHexMap(geometry, new[] { false, true, false, true });

        SpatialFloatHexMap floatResult = condition.Select(floatWhenTrue, floatWhenFalse);
        SpatialIntHexMap intResult = condition.Select(intWhenTrue, intWhenFalse);
        SpatialBoolHexMap boolResult = condition.Select(boolWhenTrue, boolWhenFalse);

        Assert.Multiple(() =>
        {
            Assert.That(floatResult, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(intResult, Is.TypeOf<SpatialIntHexMap>());
            Assert.That(boolResult, Is.TypeOf<SpatialBoolHexMap>());
            Assert.That(floatResult.Geometry, Is.EqualTo(condition.Geometry));
            Assert.That(intResult.Geometry, Is.EqualTo(condition.Geometry));
            Assert.That(boolResult.Geometry, Is.EqualTo(condition.Geometry));
            Assert.That(Values(floatResult), Is.EqualTo(new[] { 1f, 20f, 3f, 40f }));
            Assert.That(Values(intResult), Is.EqualTo(new[] { 1, 20, 3, 40 }));
            Assert.That(Values(boolResult), Is.EqualTo(new[] { true, true, false, true }));
        });

        floatResult[0] = -1f;
        intResult[0] = -1;
        boolResult[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(floatWhenTrue[0], Is.EqualTo(1f));
            Assert.That(intWhenTrue[0], Is.EqualTo(1));
            Assert.That(boolWhenTrue[0], Is.True);
            Assert.That(condition[0], Is.True);
        });
    }

    [Test]
    public void Select_WithConcreteSpatialMaps_ValidatesNullOperands()
    {
        HexMapGeometry geometry = Geometry();
        var condition = new SpatialBoolHexMap(geometry);
        var floatMap = new SpatialFloatHexMap(geometry);
        var intMap = new SpatialIntHexMap(geometry);
        var boolMap = new SpatialBoolHexMap(geometry);
        SpatialBoolHexMap nullCondition = null!;
        SpatialFloatHexMap nullFloatMap = null!;
        SpatialIntHexMap nullIntMap = null!;
        SpatialBoolHexMap nullBoolMap = null!;

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullCondition.Select(floatMap, floatMap))!.ParamName,
                Is.EqualTo("condition"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => condition.Select(nullFloatMap, floatMap))!.ParamName,
                Is.EqualTo("whenTrue"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => condition.Select(floatMap, nullFloatMap))!.ParamName,
                Is.EqualTo("whenFalse"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullCondition.Select(intMap, intMap))!.ParamName,
                Is.EqualTo("condition"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => condition.Select(nullIntMap, intMap))!.ParamName,
                Is.EqualTo("whenTrue"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => condition.Select(intMap, nullIntMap))!.ParamName,
                Is.EqualTo("whenFalse"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullCondition.Select(boolMap, boolMap))!.ParamName,
                Is.EqualTo("condition"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => condition.Select(nullBoolMap, boolMap))!.ParamName,
                Is.EqualTo("whenTrue"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => condition.Select(boolMap, nullBoolMap))!.ParamName,
                Is.EqualTo("whenFalse"));
        });
    }

    [Test]
    public void Select_WithConcreteSpatialMaps_RequiresEveryGeometryToMatchCondition()
    {
        HexMapGeometry geometry = Geometry();
        var condition = new SpatialBoolHexMap(geometry);
        var otherGeometry = new HexMapGeometry(
            geometry.Topology,
            new VectorXY(geometry.Origin.X + 1f, geometry.Origin.Y),
            geometry.Radius);
        var matchingFloat = new SpatialFloatHexMap(geometry);
        var otherFloat = new SpatialFloatHexMap(otherGeometry);
        var matchingInt = new SpatialIntHexMap(geometry);
        var otherInt = new SpatialIntHexMap(otherGeometry);
        var matchingBool = new SpatialBoolHexMap(geometry);
        var otherBool = new SpatialBoolHexMap(otherGeometry);

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => condition.Select(otherFloat, matchingFloat))!.ParamName,
                Is.EqualTo("whenTrue"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => condition.Select(matchingFloat, otherFloat))!.ParamName,
                Is.EqualTo("whenFalse"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => condition.Select(otherInt, matchingInt))!.ParamName,
                Is.EqualTo("whenTrue"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => condition.Select(matchingInt, otherInt))!.ParamName,
                Is.EqualTo("whenFalse"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => condition.Select(otherBool, matchingBool))!.ParamName,
                Is.EqualTo("whenTrue"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => condition.Select(matchingBool, otherBool))!.ParamName,
                Is.EqualTo("whenFalse"));
        });
    }

    private static HexMapGeometry Geometry() =>
        new(2, 2, new VectorXY(10f, -20f), 2f, Layout.OddR);

    private static T[] Values<T>(IHexMap<T> map)
    {
        var values = new T[map.Topology.Count];
        for (int index = 0; index < values.Length; index++)
            values[index] = map[index];

        return values;
    }
}
