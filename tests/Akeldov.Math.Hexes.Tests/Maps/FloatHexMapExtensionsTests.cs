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
    public void ToSpatialHexMap_FromFloatFieldRangeAndGeometry_InterpolatesSampledBounds()
    {
        var geometry = new HexMapGeometry(3, 1, new VectorXY(2f, -1f), 1f, Layout.OddR);
        var centers = new HexCenterMap(geometry);
        var range = new FloatFieldRange(
            new DelegateFloatField(point => point.X - 3f),
            new DelegateFloatField(point => point.X + 5f));
        var random = new SequenceRandom(0d, 0.25d, 0.75d);

        SpatialFloatHexMap result = range.ToSpatialHexMap(geometry, random);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.EqualTo(centers[0].X - 3f).Within(0.0001f));
            Assert.That(result[1], Is.EqualTo(centers[1].X - 1f).Within(0.0001f));
            Assert.That(result[2], Is.EqualTo(centers[2].X + 3f).Within(0.0001f));
            Assert.That(random.NextDoubleCallCount, Is.EqualTo(geometry.Topology.Count));
        });
    }

    [Test]
    public void ToSpatialHexMap_FromFloatFieldRangeAndHexCenters_UsesProvidedCenters()
    {
        var geometry = new HexMapGeometry(2, 1, new VectorXY(-4f, 3f), 0.75f, Layout.EvenQ);
        var centers = new HexCenterMap(geometry);
        var range = new FloatFieldRange(
            new DelegateFloatField(_ => -10f),
            new DelegateFloatField(_ => 10f));
        var random = new SequenceRandom(0.1d, 0.9d);

        SpatialFloatHexMap result = range.ToSpatialHexMap(centers, random);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.EqualTo(-8f).Within(0.0001f));
            Assert.That(result[1], Is.EqualTo(8f).Within(0.0001f));
        });
    }

    [Test]
    public void ToSpatialHexMap_FromFloatFieldRange_WhenArgumentsAreInvalid_Throws()
    {
        var geometry = new HexMapGeometry(1, 1, 1f, Layout.OddR);
        var range = new FloatFieldRange(
            new DelegateFloatField(_ => 0f),
            new DelegateFloatField(_ => 1f));
        var random = new Random(1);
        HexCenterMap? hexCenters = null;
        Random? nullRandom = null;

        var rangeException = Assert.Throws<ArgumentException>(() => default(FloatFieldRange).ToSpatialHexMap(geometry, random));
#pragma warning disable CS8604
        var hexCentersException = Assert.Throws<ArgumentNullException>(() => range.ToSpatialHexMap(hexCenters, random));
        var randomException = Assert.Throws<ArgumentNullException>(() => range.ToSpatialHexMap(geometry, nullRandom));
#pragma warning restore CS8604
        var geometryException = Assert.Throws<ArgumentOutOfRangeException>(() => range.ToSpatialHexMap(default(HexMapGeometry), random));

        Assert.Multiple(() =>
        {
            Assert.That(rangeException!.ParamName, Is.EqualTo("range"));
            Assert.That(hexCentersException!.ParamName, Is.EqualTo("hexCenters"));
            Assert.That(randomException!.ParamName, Is.EqualTo("random"));
            Assert.That(geometryException!.ParamName, Is.EqualTo("geometry"));
        });
    }

    [TestCase(float.NaN, 1f)]
    [TestCase(0f, float.NaN)]
    [TestCase(float.NegativeInfinity, 1f)]
    [TestCase(0f, float.PositiveInfinity)]
    [TestCase(2f, 1f)]
    public void ToSpatialHexMap_FromFloatFieldRange_WhenSampledBoundsAreInvalid_Throws(float min, float max)
    {
        var geometry = new HexMapGeometry(1, 1, 1f, Layout.OddR);
        var range = new FloatFieldRange(
            new DelegateFloatField(_ => min),
            new DelegateFloatField(_ => max));

        var exception = Assert.Throws<InvalidOperationException>(() => range.ToSpatialHexMap(geometry, new Random(1)));

        Assert.That(exception!.Message, Does.Contain("hex index 0"));
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

    private sealed class DelegateFloatField : IFloatField
    {
        private readonly Func<PointXY, float> _sample;

        public DelegateFloatField(Func<PointXY, float> sample)
        {
            _sample = sample;
        }

        public float Min => float.MinValue;

        public float Max => float.MaxValue;

        public float Sample(PointXY point) => _sample(point);
    }

    private sealed class SequenceRandom : Random
    {
        private readonly double[] _values;
        private int _index;

        public SequenceRandom(params double[] values)
        {
            _values = values;
        }

        public int NextDoubleCallCount => _index;

        public override double NextDouble() => _values[_index++];
    }
}
