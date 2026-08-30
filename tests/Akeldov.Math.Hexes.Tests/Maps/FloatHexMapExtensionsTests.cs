using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class FloatHexMapExtensionsTests
{
    [Test]
    public void GetMinMax_ReturnsBothExtremaForInterfaceTypedSpatialMap()
    {
        var geometry = new HexMapGeometry(4, 1, 2f, Layout.OddR);
        IHexMap<float> map = new SpatialFloatHexMap(geometry, new[] { 7f, -4f, 12f, 3f });

        (float min, float max) = map.GetMinMax();

        Assert.Multiple(() =>
        {
            Assert.That(min, Is.EqualTo(-4f));
            Assert.That(max, Is.EqualTo(12f));
        });
    }

    [Test]
    public void GetMinMax_WhenMapContainsNaN_PropagatesNaN()
    {
        IHexMap<float> map = new HexMap<float>(
            new HexMapTopology(3, 1, Layout.OddR),
            new[] { 1f, float.NaN, 3f });

        (float min, float max) = map.GetMinMax();

        Assert.Multiple(() =>
        {
            Assert.That(min, Is.NaN);
            Assert.That(max, Is.NaN);
        });
    }

    [Test]
    public void TryGetMinMax_WhenMapIsEmpty_ReturnsFalseAndZeroOutputs()
    {
        IHexMap<float> map = new HexMap<float>(new HexMapTopology(0, 0, Layout.OddR));

        bool found = map.TryGetMinMax(out float min, out float max);

        Assert.Multiple(() =>
        {
            Assert.That(found, Is.False);
            Assert.That(min, Is.Zero);
            Assert.That(max, Is.Zero);
            Assert.Throws<InvalidOperationException>(() => map.GetMinMax());
        });
    }

    [Test]
    public void GetMinMax_WhenMapIsNull_Throws()
    {
        IHexMap<float>? map = null;

#pragma warning disable CS8604
        var getException = Assert.Throws<ArgumentNullException>(() => map.GetMinMax());
        var tryException = Assert.Throws<ArgumentNullException>(() => map.TryGetMinMax(out _, out _));
#pragma warning restore CS8604

        Assert.Multiple(() =>
        {
            Assert.That(getException!.ParamName, Is.EqualTo("map"));
            Assert.That(tryException!.ParamName, Is.EqualTo("map"));
        });
    }

    [Test]
    public void ToSpatialHexMap_FromFloatFieldAndGeometry_SamplesHexCenters()
    {
        var geometry = new HexMapGeometry(3, 2, new VectorXY(-4f, 8f), 1.5f, Layout.OddR);
        var centers = new HexCenterMap(geometry);
        IFloatField field = new CoordinateFloatField();

        SpatialFloatHexMap result = field.ToSpatialHexMap(geometry);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(result.Geometry, Is.EqualTo(geometry));

            for (int index = 0; index < geometry.Topology.Count; index++)
                Assert.That(result[index], Is.EqualTo(field.Sample(centers[index])).Within(0.0001f));
        });
    }

    [Test]
    public void ToSpatialHexMap_FromFloatFieldAndHexCenters_SamplesProvidedCenters()
    {
        var geometry = new HexMapGeometry(2, 2, new VectorXY(3f, -1f), 0.75f, Layout.EvenQ);
        var centers = new HexCenterMap(geometry);
        IFloatField field = new CoordinateFloatField();

        SpatialFloatHexMap result = field.ToSpatialHexMap(centers);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));

            for (int index = 0; index < geometry.Topology.Count; index++)
                Assert.That(result[index], Is.EqualTo(field.Sample(centers[index])).Within(0.0001f));
        });
    }

    [Test]
    public void ToSpatialHexMap_FromFloatField_WhenGeometryIsEmpty_ReturnsEmptySpatialMap()
    {
        var geometry = new HexMapGeometry(0, 0, new VectorXY(3f, -2f), 1.25f, Layout.EvenR);
        IFloatField field = new CoordinateFloatField();

        SpatialFloatHexMap result = field.ToSpatialHexMap(geometry);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void ToSpatialHexMap_FromFloatField_WhenArgumentsAreInvalid_Throws()
    {
        var geometry = new HexMapGeometry(1, 1, 1f, Layout.OddR);
        IFloatField? field = null;
        IFloatField source = new CoordinateFloatField();
        HexCenterMap? hexCenters = null;

#pragma warning disable CS8604
        var fieldException = Assert.Throws<ArgumentNullException>(() => field.ToSpatialHexMap(geometry));
        var hexCentersException = Assert.Throws<ArgumentNullException>(() => source.ToSpatialHexMap(hexCenters));
#pragma warning restore CS8604
        var geometryException = Assert.Throws<ArgumentOutOfRangeException>(() => source.ToSpatialHexMap(default(HexMapGeometry)));

        Assert.Multiple(() =>
        {
            Assert.That(fieldException!.ParamName, Is.EqualTo("field"));
            Assert.That(hexCentersException!.ParamName, Is.EqualTo("hexCenters"));
            Assert.That(geometryException!.ParamName, Is.EqualTo("geometry"));
        });
    }

    [Test]
    public void ToFloatHexMap_ReturnsIndependentMutableCopy()
    {
        var topology = new HexMapTopology(3, 1, Layout.EvenQ);
        var source = new HexMap<float>(topology, new[] { 1.5f, -2f, 4f });
        IHexMap<float> readOnlySource = source;

        FloatHexMap result = readOnlySource.ToFloatHexMap();
        source[0] = 10f;
        result[1] = 20f;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(1.5f));
            Assert.That(result[1], Is.EqualTo(20f));
            Assert.That(result[2], Is.EqualTo(4f));
            Assert.That(source[1], Is.EqualTo(-2f));
        });
    }

    [Test]
    public void ToFloatHexMap_WhenSourceIsFloatHexMap_ReturnsIndependentCopy()
    {
        var source = new FloatHexMap(
            new HexMapTopology(2, 1, Layout.OddR),
            new[] { 1f, 2f });

        FloatHexMap result = source.ToFloatHexMap();
        source[0] = 10f;
        result[1] = 20f;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.SameAs(source));
            Assert.That(result[0], Is.EqualTo(1f));
            Assert.That(source[1], Is.EqualTo(2f));
        });
    }

    [Test]
    public void ToFloatHexMap_WhenMapIsEmpty_ReturnsEmptyMapWithSameTopology()
    {
        var topology = new HexMapTopology(0, 0, Layout.EvenR);
        IHexMap<float> source = new HexMap<float>(topology);

        FloatHexMap result = source.ToFloatHexMap();

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void ToFloatHexMap_WhenMapIsNull_Throws()
    {
        IHexMap<float>? map = null;

#pragma warning disable CS8604
        var exception = Assert.Throws<ArgumentNullException>(() => map.ToFloatHexMap());
#pragma warning restore CS8604

        Assert.That(exception!.ParamName, Is.EqualTo("map"));
    }

    private sealed class CoordinateFloatField : IFloatField
    {
        public float Min => -1_000_000f;

        public float Max => 1_000_000f;

        public float Sample(PointXY point) => point.X * 10f + point.Y;
    }
}
