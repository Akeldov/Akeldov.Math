using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class SpatialBoolHexMapCrossOperatorTests
{
    [TestCaseSource(nameof(OperatorCases))]
    public void Operator_ReturnsIndependentSpatialMapWithCellwiseValues(CrossOperatorCase operatorCase)
    {
        HexMapGeometry geometry = Geometry();
        var spatialMap = new SpatialBoolHexMap(geometry, new[] { true, true, false, false });
        var ordinaryMap = new BoolHexMap(
            geometry.Topology,
            new[] { true, false, true, false });

        SpatialBoolHexMap result = operatorCase.Apply(spatialMap, ordinaryMap);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<SpatialBoolHexMap>());
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(Values(result), Is.EqualTo(operatorCase.ExpectedValues));
        });

        result[0] = !result[0];
        spatialMap[1] = !spatialMap[1];
        ordinaryMap[2] = !ordinaryMap[2];

        Assert.Multiple(() =>
        {
            Assert.That(spatialMap[0], Is.True);
            Assert.That(ordinaryMap[0], Is.True);
            Assert.That(result[1], Is.EqualTo(operatorCase.ExpectedValues[1]));
            Assert.That(result[2], Is.EqualTo(operatorCase.ExpectedValues[2]));
        });
    }

    [TestCaseSource(nameof(OperatorCases))]
    public void Operator_WhenTopologiesDiffer_Throws(CrossOperatorCase operatorCase)
    {
        var geometry = new HexMapGeometry(2, 2, VectorXY.Zero, 1f, Layout.OddR);
        var spatialMap = new SpatialBoolHexMap(geometry);
        var ordinaryMap = new BoolHexMap(new HexMapTopology(2, 2, Layout.EvenR));

        var exception = Assert.Throws<ArgumentException>(() => operatorCase.Apply(spatialMap, ordinaryMap));

        Assert.That(exception!.ParamName, Is.EqualTo("right"));
    }

    [TestCaseSource(nameof(OperatorCases))]
    public void Operator_WhenEitherOperandIsNull_Throws(CrossOperatorCase operatorCase)
    {
        HexMapGeometry geometry = Geometry();
        var spatialMap = new SpatialBoolHexMap(geometry);
        var ordinaryMap = new BoolHexMap(geometry.Topology);

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => operatorCase.Apply(null!, ordinaryMap))!.ParamName,
                Is.EqualTo(operatorCase.SpatialMapParameterName));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => operatorCase.Apply(spatialMap, null!))!.ParamName,
                Is.EqualTo(operatorCase.OrdinaryMapParameterName));
        });
    }

    private static IEnumerable<TestCaseData> OperatorCases()
    {
        yield return Case(
            "SpatialAndOrdinary",
            (spatialMap, ordinaryMap) => spatialMap & ordinaryMap,
            new[] { true, false, false, false },
            "left",
            "right");
        yield return Case(
            "OrdinaryAndSpatial",
            (spatialMap, ordinaryMap) => ordinaryMap & spatialMap,
            new[] { true, false, false, false },
            "right",
            "left");
        yield return Case(
            "SpatialOrOrdinary",
            (spatialMap, ordinaryMap) => spatialMap | ordinaryMap,
            new[] { true, true, true, false },
            "left",
            "right");
        yield return Case(
            "OrdinaryOrSpatial",
            (spatialMap, ordinaryMap) => ordinaryMap | spatialMap,
            new[] { true, true, true, false },
            "right",
            "left");
        yield return Case(
            "SpatialXorOrdinary",
            (spatialMap, ordinaryMap) => spatialMap ^ ordinaryMap,
            new[] { false, true, true, false },
            "left",
            "right");
        yield return Case(
            "OrdinaryXorSpatial",
            (spatialMap, ordinaryMap) => ordinaryMap ^ spatialMap,
            new[] { false, true, true, false },
            "right",
            "left");
    }

    private static TestCaseData Case(
        string name,
        Func<SpatialBoolHexMap, BoolHexMap, SpatialBoolHexMap> apply,
        bool[] expectedValues,
        string spatialMapParameterName,
        string ordinaryMapParameterName) =>
        new TestCaseData(
            new CrossOperatorCase(
                apply,
                expectedValues,
                spatialMapParameterName,
                ordinaryMapParameterName))
            .SetName($"{{m}}_{name}");

    private static HexMapGeometry Geometry() =>
        new(2, 2, new VectorXY(10f, -20f), 2f, Layout.OddR);

    private static bool[] Values(IHexMap<bool> map)
    {
        var values = new bool[map.Topology.Count];
        for (int index = 0; index < values.Length; index++)
            values[index] = map[index];

        return values;
    }

    public sealed class CrossOperatorCase
    {
        public CrossOperatorCase(
            Func<SpatialBoolHexMap, BoolHexMap, SpatialBoolHexMap> apply,
            bool[] expectedValues,
            string spatialMapParameterName,
            string ordinaryMapParameterName)
        {
            Apply = apply;
            ExpectedValues = expectedValues;
            SpatialMapParameterName = spatialMapParameterName;
            OrdinaryMapParameterName = ordinaryMapParameterName;
        }

        public Func<SpatialBoolHexMap, BoolHexMap, SpatialBoolHexMap> Apply { get; }

        public bool[] ExpectedValues { get; }

        public string SpatialMapParameterName { get; }

        public string OrdinaryMapParameterName { get; }
    }
}
