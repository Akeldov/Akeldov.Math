using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps.Filtering;

public class FloatHexMapBlurExtensionsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void GaussianBlur_WithRadiusOne_UsesHexCenterDistance(Layout layout)
    {
        var topology = new HexMapTopology(5, 5, layout);
        var values = new float[topology.Count];
        values[12] = 1f;
        var map = new FloatHexMap(topology, values);

        FloatHexMap result = map.GaussianBlur(1f, 1);

        double adjacentWeight = System.Math.Exp(-0.5d);
        float expectedCenter = (float)(1d / (1d + 6d * adjacentWeight));

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[12], Is.EqualTo(expectedCenter).Within(1e-7f));
            Assert.That(map[12], Is.EqualTo(1f));
        });
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void GaussianBlur_AtBoundaries_RenormalizesPresentWeights(Layout layout)
    {
        var topology = new HexMapTopology(4, 3, layout);
        var map = new FloatHexMap(topology, CreateValues(topology.Count, 7f));

        FloatHexMap result = map.GaussianBlur(1.25f, 2);

        for (int index = 0; index < topology.Count; index++)
            Assert.That(result[index], Is.EqualTo(7f).Within(1e-6f), $"Unexpected value at flat index {index}.");
    }

    [Test]
    public void GaussianBlur_WithoutRadius_UsesThreeSigmaTruncation()
    {
        var topology = new HexMapTopology(7, 7, Layout.OddR);
        var values = new float[topology.Count];
        for (int index = 0; index < values.Length; index++)
            values[index] = index % 5;

        var map = new FloatHexMap(topology, values);

        FloatHexMap automatic = map.GaussianBlur(0.6f);
        FloatHexMap explicitRadius = map.GaussianBlur(0.6f, 2);

        for (int index = 0; index < topology.Count; index++)
            Assert.That(automatic[index], Is.EqualTo(explicitRadius[index]).Within(1e-7f));
    }

    [Test]
    public void GaussianBlur_WithZeroRadius_ReturnsIndependentCopy()
    {
        var topology = new HexMapTopology(2, 1, Layout.OddR);
        var map = new FloatHexMap(topology, new[] { 2f, -3f });

        FloatHexMap result = map.GaussianBlur(1f, 0);
        result[0] = 100f;

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(100f));
            Assert.That(result[1], Is.EqualTo(-3f));
            Assert.That(map[0], Is.EqualTo(2f));
        });
    }

    [Test]
    public void GaussianBlur_WhenMapIsEmpty_ReturnsEmptyMap()
    {
        var topology = new HexMapTopology(0, 0, Layout.OddR);
        var map = new FloatHexMap(topology);

        FloatHexMap result = map.GaussianBlur(1f);

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void GaussianBlur_WithInvalidArguments_Throws()
    {
        var map = new FloatHexMap(new HexMapTopology(1, 1, Layout.OddR));
        IHexMap<float>? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => missing.GaussianBlur(1f))!.ParamName,
                Is.EqualTo("map"));
#pragma warning restore CS8604
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(0f))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(-1f))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(float.NaN))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(float.PositiveInfinity))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(1f, -1))!.ParamName,
                Is.EqualTo("radius"));
        });
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void GaussianBlur_SpatialMapWithExplicitRadius_PreservesGeometryAndValues(Layout layout)
    {
        var geometry = new HexMapGeometry(4, 3, new VectorXY(10f, -5f), 2.5f, layout);
        float[] sourceValues = CreateSequentialValues(geometry.Topology.Count);
        var map = new SpatialFloatHexMap(geometry, sourceValues);
        var ordinaryMap = new FloatHexMap(geometry.Topology, CreateSequentialValues(geometry.Topology.Count));

        SpatialFloatHexMap result = map.GaussianBlur(0.8f, 2);
        FloatHexMap expected = ordinaryMap.GaussianBlur(0.8f, 2);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            for (int index = 0; index < geometry.Topology.Count; index++)
                Assert.That(result[index], Is.EqualTo(expected[index]).Within(1e-7f), $"Unexpected value at flat index {index}.");
        });

        float firstResultValue = result[0];
        map[0] = 100f;
        result[1] = 200f;

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(firstResultValue));
            Assert.That(map[1], Is.EqualTo(1f));
            Assert.That(sourceValues[0], Is.EqualTo(100f));
        });
    }

    [Test]
    public void GaussianBlur_SpatialMapWithoutRadius_UsesThreeSigmaTruncationAndPreservesGeometry()
    {
        var geometry = new HexMapGeometry(7, 7, new VectorXY(-3f, 4f), 1.25f, Layout.EvenQ);
        var values = new float[geometry.Topology.Count];
        for (int index = 0; index < values.Length; index++)
            values[index] = index % 5;

        var map = new SpatialFloatHexMap(geometry, values);

        SpatialFloatHexMap automatic = map.GaussianBlur(0.6f);
        SpatialFloatHexMap explicitRadius = map.GaussianBlur(0.6f, 2);

        Assert.Multiple(() =>
        {
            Assert.That(automatic, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(automatic.Geometry, Is.EqualTo(geometry));
            Assert.That(explicitRadius.Geometry, Is.EqualTo(geometry));
            for (int index = 0; index < geometry.Topology.Count; index++)
                Assert.That(automatic[index], Is.EqualTo(explicitRadius[index]).Within(1e-7f));
        });
    }

    [Test]
    public void GaussianBlur_SpatialExactOverloadDependsOnStaticReceiverType()
    {
        var geometry = new HexMapGeometry(2, 1, new VectorXY(5f, 6f), 2f, Layout.OddR);
        var map = new SpatialFloatHexMap(geometry, new[] { 2f, -3f });
        IHexMap<float> interfaceView = map;

        SpatialFloatHexMap spatialResult = map.GaussianBlur(1f, 0);
        FloatHexMap interfaceResult = interfaceView.GaussianBlur(1f, 0);

        Assert.Multiple(() =>
        {
            Assert.That(spatialResult, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(spatialResult.Geometry, Is.EqualTo(geometry));
            Assert.That(interfaceResult, Is.TypeOf<FloatHexMap>());
            Assert.That(interfaceResult, Is.Not.InstanceOf<SpatialFloatHexMap>());
            Assert.That(interfaceResult.Topology, Is.EqualTo(geometry.Topology));
        });
    }

    [Test]
    public void GaussianBlur_WhenSpatialMapIsEmpty_PreservesGeometryForBothOverloads()
    {
        var geometry = new HexMapGeometry(0, 0, new VectorXY(5f, -2f), 3f, Layout.EvenR);
        var map = new SpatialFloatHexMap(geometry);

        SpatialFloatHexMap automatic = map.GaussianBlur(1f);
        SpatialFloatHexMap explicitRadius = map.GaussianBlur(1f, 4);

        Assert.Multiple(() =>
        {
            Assert.That(automatic, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(explicitRadius, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(automatic, Is.Not.SameAs(map));
            Assert.That(explicitRadius, Is.Not.SameAs(map));
            Assert.That(automatic.Geometry, Is.EqualTo(geometry));
            Assert.That(explicitRadius.Geometry, Is.EqualTo(geometry));
            Assert.That(automatic.Topology.Count, Is.Zero);
            Assert.That(explicitRadius.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void GaussianBlur_WhenSpatialMapIsNull_ThrowsForBothOverloads()
    {
        SpatialFloatHexMap? map = null;

#pragma warning disable CS8604
        var automaticException = Assert.Throws<ArgumentNullException>(() => map.GaussianBlur(1f));
        var explicitRadiusException = Assert.Throws<ArgumentNullException>(() => map.GaussianBlur(1f, 1));
#pragma warning restore CS8604

        Assert.Multiple(() =>
        {
            Assert.That(automaticException!.ParamName, Is.EqualTo("map"));
            Assert.That(explicitRadiusException!.ParamName, Is.EqualTo("map"));
        });
    }

    [Test]
    public void GaussianBlur_SpatialMapWithInvalidArguments_Throws()
    {
        var geometry = new HexMapGeometry(1, 1, 1f, Layout.OddR);
        var map = new SpatialFloatHexMap(geometry);

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(0f))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(-1f))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(float.NaN))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(float.PositiveInfinity))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(0f, 1))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(1f, -1))!.ParamName,
                Is.EqualTo("radius"));
        });
    }

    private static float[] CreateValues(int count, float value)
    {
        var values = new float[count];
        Array.Fill(values, value);
        return values;
    }

    private static float[] CreateSequentialValues(int count)
    {
        var values = new float[count];
        for (int index = 0; index < values.Length; index++)
            values[index] = index;

        return values;
    }
}
