using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class NumericHexMapRangeExtensionsTests
{
    [Test]
    public void Clamp_OrdinaryMaps_ClampsValuesAndReturnsIndependentSpecializations()
    {
        var topology = new HexMapTopology(3, 1, Layout.EvenQ);
        var floatSourceMap = new HexMap<float>(topology, new[] { -3f, 0.5f, 4f });
        var intSourceMap = new HexMap<int>(topology, new[] { -3, 0, 4 });
        IHexMap<float> floatSource = floatSourceMap;
        IHexMap<int> intSource = intSourceMap;

        FloatHexMap floatResult = floatSource.Clamp(-1f, 2f);
        IntHexMap intResult = intSource.Clamp(-1, 2);

        Assert.Multiple(() =>
        {
            Assert.That(floatResult, Is.TypeOf<FloatHexMap>());
            Assert.That(intResult, Is.TypeOf<IntHexMap>());
            Assert.That(floatResult.Topology, Is.EqualTo(topology));
            Assert.That(intResult.Topology, Is.EqualTo(topology));
            Assert.That(ReadValues(floatResult), Is.EqualTo(new[] { -1f, 0.5f, 2f }));
            Assert.That(ReadValues(intResult), Is.EqualTo(new[] { -1, 0, 2 }));
            Assert.That(ReadValues(floatSource), Is.EqualTo(new[] { -3f, 0.5f, 4f }));
            Assert.That(ReadValues(intSource), Is.EqualTo(new[] { -3, 0, 4 }));
        });

        floatSourceMap[0] = 100f;
        intSourceMap[0] = 100;
        floatResult[1] = 100f;
        intResult[1] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(floatResult[0], Is.EqualTo(-1f));
            Assert.That(intResult[0], Is.EqualTo(-1));
            Assert.That(floatSourceMap[1], Is.EqualTo(0.5f));
            Assert.That(intSourceMap[1], Is.Zero);
        });
    }

    [Test]
    public void Clamp_SpatialMaps_ClampsValuesAndPreservesGeometry()
    {
        var geometry = new HexMapGeometry(
            3,
            1,
            new VectorXY(4f, -7f),
            2.5f,
            Layout.OddR);
        var floatSourceMap = new SpatialHexMap<float>(geometry, new[] { -3f, 0.5f, 4f });
        var intSourceMap = new SpatialHexMap<int>(geometry, new[] { -3, 0, 4 });
        ISpatialHexMap<float> floatSource = floatSourceMap;
        ISpatialHexMap<int> intSource = intSourceMap;

        SpatialFloatHexMap floatResult = floatSource.Clamp(-1f, 2f);
        SpatialIntHexMap intResult = intSource.Clamp(-1, 2);

        Assert.Multiple(() =>
        {
            Assert.That(floatResult, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(intResult, Is.TypeOf<SpatialIntHexMap>());
            Assert.That(floatResult.Geometry, Is.EqualTo(geometry));
            Assert.That(intResult.Geometry, Is.EqualTo(geometry));
            Assert.That(ReadValues(floatResult), Is.EqualTo(new[] { -1f, 0.5f, 2f }));
            Assert.That(ReadValues(intResult), Is.EqualTo(new[] { -1, 0, 2 }));
        });

        floatSourceMap[0] = 100f;
        intSourceMap[0] = 100;
        floatResult[1] = 100f;
        intResult[1] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(floatResult[0], Is.EqualTo(-1f));
            Assert.That(intResult[0], Is.EqualTo(-1));
            Assert.That(floatSourceMap[1], Is.EqualTo(0.5f));
            Assert.That(intSourceMap[1], Is.Zero);
        });
    }

    [Test]
    public void Rescale_OrdinaryMaps_MapsCurrentRangeAndReturnsIndependentSpecializations()
    {
        var topology = new HexMapTopology(5, 1, Layout.EvenR);
        var floatSourceMap = new HexMap<float>(topology, new[] { -10f, -5f, 0f, 5f, 10f });
        var intSourceMap = new HexMap<int>(topology, new[] { 0, 1, 2, 3, 4 });
        IHexMap<float> floatSource = floatSourceMap;
        IHexMap<int> intSource = intSourceMap;

        FloatHexMap floatResult = floatSource.Rescale(2f, 6f);
        IntHexMap intResult = intSource.Rescale(10, 14);

        Assert.Multiple(() =>
        {
            Assert.That(floatResult, Is.TypeOf<FloatHexMap>());
            Assert.That(intResult, Is.TypeOf<IntHexMap>());
            Assert.That(floatResult.Topology, Is.EqualTo(topology));
            Assert.That(intResult.Topology, Is.EqualTo(topology));
            Assert.That(ReadValues(floatResult), Is.EqualTo(new[] { 2f, 3f, 4f, 5f, 6f }));
            Assert.That(ReadValues(intResult), Is.EqualTo(new[] { 10, 11, 12, 13, 14 }));
            Assert.That(ReadValues(floatSource), Is.EqualTo(new[] { -10f, -5f, 0f, 5f, 10f }));
            Assert.That(ReadValues(intSource), Is.EqualTo(new[] { 0, 1, 2, 3, 4 }));
        });

        floatSourceMap[0] = 100f;
        intSourceMap[0] = 100;
        floatResult[1] = 100f;
        intResult[1] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(floatResult[0], Is.EqualTo(2f));
            Assert.That(intResult[0], Is.EqualTo(10));
            Assert.That(floatSourceMap[1], Is.EqualTo(-5f));
            Assert.That(intSourceMap[1], Is.EqualTo(1));
        });
    }

    [Test]
    public void Rescale_SpatialMaps_MapsCurrentRangeAndPreservesGeometry()
    {
        var geometry = new HexMapGeometry(
            3,
            1,
            new VectorXY(-2f, 9f),
            1.25f,
            Layout.EvenQ);
        var floatSourceMap = new SpatialHexMap<float>(geometry, new[] { -2f, 0f, 2f });
        var intSourceMap = new SpatialHexMap<int>(geometry, new[] { -2, 0, 2 });
        ISpatialHexMap<float> floatSource = floatSourceMap;
        ISpatialHexMap<int> intSource = intSourceMap;

        SpatialFloatHexMap floatResult = floatSource.Rescale(10f, 20f);
        SpatialIntHexMap intResult = intSource.Rescale(-10, 10);

        Assert.Multiple(() =>
        {
            Assert.That(floatResult, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(intResult, Is.TypeOf<SpatialIntHexMap>());
            Assert.That(floatResult.Geometry, Is.EqualTo(geometry));
            Assert.That(intResult.Geometry, Is.EqualTo(geometry));
            Assert.That(ReadValues(floatResult), Is.EqualTo(new[] { 10f, 15f, 20f }));
            Assert.That(ReadValues(intResult), Is.EqualTo(new[] { -10, 0, 10 }));
        });

        floatSourceMap[0] = 100f;
        intSourceMap[0] = 100;
        floatResult[1] = 100f;
        intResult[1] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(floatResult[0], Is.EqualTo(10f));
            Assert.That(intResult[0], Is.EqualTo(-10));
            Assert.That(floatSourceMap[1], Is.Zero);
            Assert.That(intSourceMap[1], Is.Zero);
        });
    }

    [Test]
    public void RangeOperations_WithEqualBounds_MapEveryCellToTheBound()
    {
        IHexMap<float> floatMap = new HexMap<float>(
            new HexMapTopology(3, 1, Layout.OddQ),
            new[] { -2f, 0f, 3f });
        IHexMap<int> intMap = new HexMap<int>(
            new HexMapTopology(3, 1, Layout.OddQ),
            new[] { -2, 0, 3 });

        Assert.Multiple(() =>
        {
            Assert.That(ReadValues(floatMap.Clamp(4f, 4f)), Is.EqualTo(new[] { 4f, 4f, 4f }));
            Assert.That(ReadValues(floatMap.Rescale(4f, 4f)), Is.EqualTo(new[] { 4f, 4f, 4f }));
            Assert.That(ReadValues(intMap.Clamp(4, 4)), Is.EqualTo(new[] { 4, 4, 4 }));
            Assert.That(ReadValues(intMap.Rescale(4, 4)), Is.EqualTo(new[] { 4, 4, 4 }));
        });
    }

    [Test]
    public void RangeOperations_WithReversedBounds_ThrowWithMaximumParameterName()
    {
        IHexMap<float> floatMap = new HexMap<float>(new HexMapTopology(1, 1, Layout.OddR));
        IHexMap<int> intMap = new HexMap<int>(new HexMapTopology(1, 1, Layout.OddR));
        var geometry = new HexMapGeometry(1, 1, 1f, Layout.OddR);
        ISpatialHexMap<float> spatialFloatMap = new SpatialHexMap<float>(geometry);
        ISpatialHexMap<int> spatialIntMap = new SpatialHexMap<int>(geometry);

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => floatMap.Clamp(2f, 1f))!.ParamName,
                Is.EqualTo("max"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => intMap.Clamp(2, 1))!.ParamName,
                Is.EqualTo("max"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => spatialFloatMap.Clamp(2f, 1f))!.ParamName,
                Is.EqualTo("max"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => spatialIntMap.Clamp(2, 1))!.ParamName,
                Is.EqualTo("max"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => floatMap.Rescale(2f, 1f))!.ParamName,
                Is.EqualTo("newMax"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => intMap.Rescale(2, 1))!.ParamName,
                Is.EqualTo("newMax"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => spatialFloatMap.Rescale(2f, 1f))!.ParamName,
                Is.EqualTo("newMax"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => spatialIntMap.Rescale(2, 1))!.ParamName,
                Is.EqualTo("newMax"));
        });
    }

    [Test]
    public void RangeOperations_WhenMapIsNull_Throw()
    {
        IHexMap<float> floatMap = null!;
        IHexMap<int> intMap = null!;
        ISpatialHexMap<float> spatialFloatMap = null!;
        ISpatialHexMap<int> spatialIntMap = null!;

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => floatMap.Clamp(0f, 1f))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => intMap.Clamp(0, 1))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatialFloatMap.Clamp(0f, 1f))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatialIntMap.Clamp(0, 1))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => floatMap.Rescale(0f, 1f))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => intMap.Rescale(0, 1))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatialFloatMap.Rescale(0f, 1f))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatialIntMap.Rescale(0, 1))!.ParamName, Is.EqualTo("map"));
        });
    }

    [Test]
    public void RangeOperations_WhenMapsAreEmpty_ReturnEmptySpecializedMaps()
    {
        var topology = new HexMapTopology(0, 0, Layout.EvenR);
        IHexMap<float> floatMap = new HexMap<float>(topology);
        IHexMap<int> intMap = new HexMap<int>(topology);
        FloatHexMap clampedFloat = floatMap.Clamp(0f, 1f);
        FloatHexMap rescaledFloat = floatMap.Rescale(0f, 1f);
        IntHexMap clampedInt = intMap.Clamp(0, 1);
        IntHexMap rescaledInt = intMap.Rescale(0, 1);

        var geometry = new HexMapGeometry(0, 0, new VectorXY(3f, -2f), 1.5f, Layout.EvenR);
        ISpatialHexMap<float> spatialFloatMap = new SpatialHexMap<float>(geometry);
        ISpatialHexMap<int> spatialIntMap = new SpatialHexMap<int>(geometry);
        SpatialFloatHexMap clampedSpatialFloat = spatialFloatMap.Clamp(0f, 1f);
        SpatialFloatHexMap rescaledSpatialFloat = spatialFloatMap.Rescale(0f, 1f);
        SpatialIntHexMap clampedSpatialInt = spatialIntMap.Clamp(0, 1);
        SpatialIntHexMap rescaledSpatialInt = spatialIntMap.Rescale(0, 1);

        Assert.Multiple(() =>
        {
            Assert.That(clampedFloat, Is.TypeOf<FloatHexMap>());
            Assert.That(rescaledFloat, Is.TypeOf<FloatHexMap>());
            Assert.That(clampedInt, Is.TypeOf<IntHexMap>());
            Assert.That(rescaledInt, Is.TypeOf<IntHexMap>());
            Assert.That(clampedFloat.Topology, Is.EqualTo(topology));
            Assert.That(rescaledFloat.Topology, Is.EqualTo(topology));
            Assert.That(clampedInt.Topology, Is.EqualTo(topology));
            Assert.That(rescaledInt.Topology, Is.EqualTo(topology));
            Assert.That(clampedFloat.Topology.Count, Is.Zero);
            Assert.That(rescaledFloat.Topology.Count, Is.Zero);
            Assert.That(clampedInt.Topology.Count, Is.Zero);
            Assert.That(rescaledInt.Topology.Count, Is.Zero);
            Assert.That(clampedSpatialFloat, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(rescaledSpatialFloat, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(clampedSpatialInt, Is.TypeOf<SpatialIntHexMap>());
            Assert.That(rescaledSpatialInt, Is.TypeOf<SpatialIntHexMap>());
            Assert.That(clampedSpatialFloat.Geometry, Is.EqualTo(geometry));
            Assert.That(rescaledSpatialFloat.Geometry, Is.EqualTo(geometry));
            Assert.That(clampedSpatialInt.Geometry, Is.EqualTo(geometry));
            Assert.That(rescaledSpatialInt.Geometry, Is.EqualTo(geometry));
            Assert.That(clampedSpatialFloat.Topology.Count, Is.Zero);
            Assert.That(rescaledSpatialFloat.Topology.Count, Is.Zero);
            Assert.That(clampedSpatialInt.Topology.Count, Is.Zero);
            Assert.That(rescaledSpatialInt.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void Rescale_WhenSourceIsConstant_MapsEveryCellToNewMinimum()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        IHexMap<float> floatMap = new HexMap<float>(topology, new[] { 7f, 7f, 7f });
        IHexMap<int> intMap = new HexMap<int>(topology, new[] { 7, 7, 7 });
        var geometry = new HexMapGeometry(3, 1, new VectorXY(2f, 3f), 1.25f, Layout.OddR);
        ISpatialHexMap<float> spatialFloatMap = new SpatialHexMap<float>(geometry, new[] { 7f, 7f, 7f });
        ISpatialHexMap<int> spatialIntMap = new SpatialHexMap<int>(geometry, new[] { 7, 7, 7 });

        SpatialFloatHexMap spatialFloatResult = spatialFloatMap.Rescale(-2f, 5f);
        SpatialIntHexMap spatialIntResult = spatialIntMap.Rescale(-2, 5);

        Assert.Multiple(() =>
        {
            Assert.That(ReadValues(floatMap.Rescale(-2f, 5f)), Is.EqualTo(new[] { -2f, -2f, -2f }));
            Assert.That(ReadValues(intMap.Rescale(-2, 5)), Is.EqualTo(new[] { -2, -2, -2 }));
            Assert.That(ReadValues(spatialFloatResult), Is.EqualTo(new[] { -2f, -2f, -2f }));
            Assert.That(ReadValues(spatialIntResult), Is.EqualTo(new[] { -2, -2, -2 }));
            Assert.That(spatialFloatResult.Geometry, Is.EqualTo(geometry));
            Assert.That(spatialIntResult.Geometry, Is.EqualTo(geometry));
        });
    }

    [Test]
    public void Rescale_WithWideFiniteFloatRange_DoesNotOverflowIntermediateValues()
    {
        IHexMap<float> map = new HexMap<float>(
            new HexMapTopology(3, 1, Layout.OddR),
            new[] { -float.MaxValue, 0f, float.MaxValue });

        FloatHexMap result = map.Rescale(-2f, 6f);

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(-2f).Within(1e-6f));
            Assert.That(result[1], Is.EqualTo(2f).Within(1e-6f));
            Assert.That(result[2], Is.EqualTo(6f).Within(1e-6f));
            Assert.That(float.IsNaN(result[0]) || float.IsInfinity(result[0]), Is.False);
            Assert.That(float.IsNaN(result[1]) || float.IsInfinity(result[1]), Is.False);
            Assert.That(float.IsNaN(result[2]) || float.IsInfinity(result[2]), Is.False);
        });
    }

    [Test]
    public void FloatRangeOperations_WhenSourceContainsNaN_PropagateNaNConsistently()
    {
        IHexMap<float> clampMap = new HexMap<float>(
            new HexMapTopology(4, 1, Layout.OddR),
            new[] { float.NaN, float.NegativeInfinity, 0f, float.PositiveInfinity });
        IHexMap<float> rescaleMap = new HexMap<float>(
            new HexMapTopology(3, 1, Layout.OddR),
            new[] { 1f, float.NaN, 3f });

        FloatHexMap clamped = clampMap.Clamp(-1f, 1f);
        FloatHexMap rescaled = rescaleMap.Rescale(0f, 1f);

        Assert.Multiple(() =>
        {
            Assert.That(clamped[0], Is.NaN);
            Assert.That(clamped[1], Is.EqualTo(-1f));
            Assert.That(clamped[2], Is.Zero);
            Assert.That(clamped[3], Is.EqualTo(1f));
            Assert.That(rescaled[0], Is.NaN);
            Assert.That(rescaled[1], Is.NaN);
            Assert.That(rescaled[2], Is.NaN);
        });
    }

    [Test]
    public void IntRescale_HandlesFullDomainAndUsesToEvenMidpointRounding()
    {
        IHexMap<int> fullSourceRange = new HexMap<int>(
            new HexMapTopology(3, 1, Layout.OddR),
            new[] { int.MinValue, 0, int.MaxValue });
        IHexMap<int> fullTargetRange = new HexMap<int>(
            new HexMapTopology(3, 1, Layout.OddR),
            new[] { -1, 0, 1 });
        IHexMap<int> midpointMap = new HexMap<int>(
            new HexMapTopology(3, 1, Layout.OddR),
            new[] { 0, 1, 2 });

        Assert.Multiple(() =>
        {
            Assert.That(ReadValues(fullSourceRange.Rescale(-1, 1)), Is.EqualTo(new[] { -1, 0, 1 }));
            Assert.That(
                ReadValues(fullTargetRange.Rescale(int.MinValue, int.MaxValue)),
                Is.EqualTo(new[] { int.MinValue, 0, int.MaxValue }));
            Assert.That(ReadValues(midpointMap.Rescale(0, 3)), Is.EqualTo(new[] { 0, 2, 3 }));
            Assert.That(ReadValues(midpointMap.Rescale(-3, 0)), Is.EqualTo(new[] { -3, -2, 0 }));
        });
    }

    [Test]
    public void SpatialRangeOperations_WhenTopologyAndGeometryDiffer_Throw()
    {
        ISpatialHexMap<float> floatMap = new InconsistentSpatialHexMap<float>();
        ISpatialHexMap<int> intMap = new InconsistentSpatialHexMap<int>();

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentException>(() => floatMap.Clamp(0f, 1f))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentException>(() => intMap.Clamp(0, 1))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentException>(() => floatMap.Rescale(0f, 1f))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentException>(() => intMap.Rescale(0, 1))!.ParamName, Is.EqualTo("map"));
        });
    }

    private static T[] ReadValues<T>(IHexMap<T> map) =>
        Enumerable.Range(0, map.Topology.Count).Select(index => map[index]).ToArray();

    private sealed class InconsistentSpatialHexMap<T> : ISpatialHexMap<T>
    {
        public HexMapTopology Topology { get; } = new(1, 1, Layout.OddR);

        public HexMapGeometry Geometry { get; } = new(2, 1, VectorXY.Zero, 1f, Layout.OddR);

        public T this[VectorXYInt index] => default!;

        public T this[int index] => default!;
    }
}
