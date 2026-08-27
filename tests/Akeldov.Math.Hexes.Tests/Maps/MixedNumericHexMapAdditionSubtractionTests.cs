using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class MixedNumericHexMapAdditionSubtractionTests
{
    [TestCaseSource(nameof(OrdinaryOperatorCases))]
    public void OrdinaryOperator_ReturnsIndependentFloatMapWithCellwiseValues(OrdinaryOperatorCase operatorCase)
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var floatMap = new FloatHexMap(topology, new[] { 1.5f, -2f, 5.25f });
        var intMap = new IntHexMap(topology, new[] { 2, -3, 4 });

        FloatHexMap result = operatorCase.Apply(floatMap, intMap);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<FloatHexMap>());
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(Values(result), Is.EqualTo(operatorCase.ExpectedValues));
        });

        result[0] = 100f;
        floatMap[1] = 200f;
        intMap[2] = 300;

        Assert.Multiple(() =>
        {
            Assert.That(floatMap[0], Is.EqualTo(1.5f));
            Assert.That(intMap[0], Is.EqualTo(2));
            Assert.That(result[1], Is.EqualTo(operatorCase.ExpectedValues[1]));
            Assert.That(result[2], Is.EqualTo(operatorCase.ExpectedValues[2]));
        });
    }

    [TestCaseSource(nameof(OrdinaryOperatorCases))]
    public void OrdinaryOperator_WhenTopologiesDiffer_Throws(OrdinaryOperatorCase operatorCase)
    {
        var floatMap = new FloatHexMap(new HexMapTopology(2, 2, Layout.OddR));
        var intMap = new IntHexMap(new HexMapTopology(2, 2, Layout.EvenR));

        var exception = Assert.Throws<ArgumentException>(() => operatorCase.Apply(floatMap, intMap));

        Assert.That(exception!.ParamName, Is.EqualTo("right"));
    }

    [TestCaseSource(nameof(OrdinaryOperatorCases))]
    public void OrdinaryOperator_WhenEitherOperandIsNull_Throws(OrdinaryOperatorCase operatorCase)
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var floatMap = new FloatHexMap(topology);
        var intMap = new IntHexMap(topology);

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => operatorCase.Apply(null!, intMap))!.ParamName,
                Is.EqualTo(operatorCase.FloatMapParameterName));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => operatorCase.Apply(floatMap, null!))!.ParamName,
                Is.EqualTo(operatorCase.IntMapParameterName));
        });
    }

    [TestCaseSource(nameof(SpatialOperatorCases))]
    public void SpatialOperator_ReturnsIndependentSpatialFloatMapWithCellwiseValues(SpatialOperatorCase operatorCase)
    {
        HexMapGeometry geometry = Geometry();
        var equivalentGeometry = new HexMapGeometry(
            new HexMapTopology(3, 1, Layout.OddR),
            geometry.Origin,
            geometry.Radius);
        var floatMap = new SpatialFloatHexMap(geometry, new[] { 1.5f, -2f, 5.25f });
        var intMap = new SpatialIntHexMap(equivalentGeometry, new[] { 2, -3, 4 });

        SpatialFloatHexMap result = operatorCase.Apply(floatMap, intMap);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(Values(result), Is.EqualTo(operatorCase.ExpectedValues));
        });

        result[0] = 100f;
        floatMap[1] = 200f;
        intMap[2] = 300;

        Assert.Multiple(() =>
        {
            Assert.That(floatMap[0], Is.EqualTo(1.5f));
            Assert.That(intMap[0], Is.EqualTo(2));
            Assert.That(result[1], Is.EqualTo(operatorCase.ExpectedValues[1]));
            Assert.That(result[2], Is.EqualTo(operatorCase.ExpectedValues[2]));
        });
    }

    [TestCaseSource(nameof(SpatialOperatorCases))]
    public void SpatialOperator_WhenGeometriesDifferButTopologiesMatch_Throws(SpatialOperatorCase operatorCase)
    {
        HexMapGeometry geometry = Geometry();
        var otherGeometry = new HexMapGeometry(
            geometry.Topology,
            new VectorXY(geometry.Origin.X + 1f, geometry.Origin.Y),
            geometry.Radius);
        var floatMap = new SpatialFloatHexMap(geometry);
        var intMap = new SpatialIntHexMap(otherGeometry);

        var exception = Assert.Throws<ArgumentException>(() => operatorCase.Apply(floatMap, intMap));

        Assert.That(exception!.ParamName, Is.EqualTo("right"));
    }

    [TestCaseSource(nameof(SpatialOperatorCases))]
    public void SpatialOperator_WhenEitherOperandIsNull_Throws(SpatialOperatorCase operatorCase)
    {
        HexMapGeometry geometry = Geometry();
        var floatMap = new SpatialFloatHexMap(geometry);
        var intMap = new SpatialIntHexMap(geometry);

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => operatorCase.Apply(null!, intMap))!.ParamName,
                Is.EqualTo(operatorCase.FloatMapParameterName));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => operatorCase.Apply(floatMap, null!))!.ParamName,
                Is.EqualTo(operatorCase.IntMapParameterName));
        });
    }

    private static IEnumerable<TestCaseData> OrdinaryOperatorCases()
    {
        yield return OrdinaryCase(
            "FloatPlusInt",
            (floatMap, intMap) => floatMap + intMap,
            new[] { 3.5f, -5f, 9.25f },
            "left",
            "right");
        yield return OrdinaryCase(
            "IntPlusFloat",
            (floatMap, intMap) => intMap + floatMap,
            new[] { 3.5f, -5f, 9.25f },
            "right",
            "left");
        yield return OrdinaryCase(
            "FloatMinusInt",
            (floatMap, intMap) => floatMap - intMap,
            new[] { -0.5f, 1f, 1.25f },
            "left",
            "right");
        yield return OrdinaryCase(
            "IntMinusFloat",
            (floatMap, intMap) => intMap - floatMap,
            new[] { 0.5f, -1f, -1.25f },
            "right",
            "left");
    }

    private static IEnumerable<TestCaseData> SpatialOperatorCases()
    {
        yield return SpatialCase(
            "SpatialFloatPlusSpatialInt",
            (floatMap, intMap) => floatMap + intMap,
            new[] { 3.5f, -5f, 9.25f },
            "left",
            "right");
        yield return SpatialCase(
            "SpatialIntPlusSpatialFloat",
            (floatMap, intMap) => intMap + floatMap,
            new[] { 3.5f, -5f, 9.25f },
            "right",
            "left");
        yield return SpatialCase(
            "SpatialFloatMinusSpatialInt",
            (floatMap, intMap) => floatMap - intMap,
            new[] { -0.5f, 1f, 1.25f },
            "left",
            "right");
        yield return SpatialCase(
            "SpatialIntMinusSpatialFloat",
            (floatMap, intMap) => intMap - floatMap,
            new[] { 0.5f, -1f, -1.25f },
            "right",
            "left");
    }

    private static TestCaseData OrdinaryCase(
        string name,
        Func<FloatHexMap, IntHexMap, FloatHexMap> apply,
        float[] expectedValues,
        string floatMapParameterName,
        string intMapParameterName) =>
        new TestCaseData(
            new OrdinaryOperatorCase(
                apply,
                expectedValues,
                floatMapParameterName,
                intMapParameterName))
            .SetName($"{{m}}_{name}");

    private static TestCaseData SpatialCase(
        string name,
        Func<SpatialFloatHexMap, SpatialIntHexMap, SpatialFloatHexMap> apply,
        float[] expectedValues,
        string floatMapParameterName,
        string intMapParameterName) =>
        new TestCaseData(
            new SpatialOperatorCase(
                apply,
                expectedValues,
                floatMapParameterName,
                intMapParameterName))
            .SetName($"{{m}}_{name}");

    private static HexMapGeometry Geometry() =>
        new(3, 1, new VectorXY(10f, -20f), 2f, Layout.OddR);

    private static T[] Values<T>(IHexMap<T> map)
    {
        var values = new T[map.Topology.Count];
        for (int index = 0; index < values.Length; index++)
            values[index] = map[index];

        return values;
    }

    public sealed class OrdinaryOperatorCase
    {
        public OrdinaryOperatorCase(
            Func<FloatHexMap, IntHexMap, FloatHexMap> apply,
            float[] expectedValues,
            string floatMapParameterName,
            string intMapParameterName)
        {
            Apply = apply;
            ExpectedValues = expectedValues;
            FloatMapParameterName = floatMapParameterName;
            IntMapParameterName = intMapParameterName;
        }

        public Func<FloatHexMap, IntHexMap, FloatHexMap> Apply { get; }

        public float[] ExpectedValues { get; }

        public string FloatMapParameterName { get; }

        public string IntMapParameterName { get; }
    }

    public sealed class SpatialOperatorCase
    {
        public SpatialOperatorCase(
            Func<SpatialFloatHexMap, SpatialIntHexMap, SpatialFloatHexMap> apply,
            float[] expectedValues,
            string floatMapParameterName,
            string intMapParameterName)
        {
            Apply = apply;
            ExpectedValues = expectedValues;
            FloatMapParameterName = floatMapParameterName;
            IntMapParameterName = intMapParameterName;
        }

        public Func<SpatialFloatHexMap, SpatialIntHexMap, SpatialFloatHexMap> Apply { get; }

        public float[] ExpectedValues { get; }

        public string FloatMapParameterName { get; }

        public string IntMapParameterName { get; }
    }
}
