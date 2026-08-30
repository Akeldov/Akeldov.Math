using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class IntHexMapExtensionsTests
{
    [Test]
    public void GetMinMax_ReturnsBothExtremaForInterfaceTypedSpatialMap()
    {
        var geometry = new HexMapGeometry(4, 1, 2f, Layout.OddR);
        IHexMap<int> map = new SpatialIntHexMap(geometry, new[] { 7, -4, 12, 3 });

        (int min, int max) = map.GetMinMax();

        Assert.Multiple(() =>
        {
            Assert.That(min, Is.EqualTo(-4));
            Assert.That(max, Is.EqualTo(12));
        });
    }

    [Test]
    public void TryGetMinMax_WhenMapIsEmpty_ReturnsFalseAndZeroOutputs()
    {
        IHexMap<int> map = new HexMap<int>(new HexMapTopology(0, 0, Layout.OddR));

        bool found = map.TryGetMinMax(out int min, out int max);

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
        IHexMap<int>? map = null;

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
    public void ToSpatialHexMap_FromIntFieldAndGeometry_SamplesHexCenters()
    {
        var geometry = new HexMapGeometry(3, 2, new VectorXY(-4f, 8f), 1.5f, Layout.OddR);
        var centers = new HexCenterMap(geometry);
        IIntField field = new CoordinateIntField();

        SpatialIntHexMap result = field.ToSpatialHexMap(geometry);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<SpatialIntHexMap>());
            Assert.That(result.Geometry, Is.EqualTo(geometry));

            for (int index = 0; index < geometry.Topology.Count; index++)
                Assert.That(result[index], Is.EqualTo(field.Sample(centers[index])));
        });
    }

    [Test]
    public void ToSpatialHexMap_FromIntFieldAndHexCenters_SamplesProvidedCenters()
    {
        var geometry = new HexMapGeometry(2, 2, new VectorXY(3f, -1f), 0.75f, Layout.EvenQ);
        var centers = new HexCenterMap(geometry);
        IIntField field = new CoordinateIntField();

        SpatialIntHexMap result = field.ToSpatialHexMap(centers);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));

            for (int index = 0; index < geometry.Topology.Count; index++)
                Assert.That(result[index], Is.EqualTo(field.Sample(centers[index])));
        });
    }

    [Test]
    public void ToSpatialHexMap_FromIntField_WhenGeometryIsEmpty_ReturnsEmptySpatialMap()
    {
        var geometry = new HexMapGeometry(0, 0, new VectorXY(3f, -2f), 1.25f, Layout.EvenR);
        IIntField field = new CoordinateIntField();

        SpatialIntHexMap result = field.ToSpatialHexMap(geometry);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void ToSpatialHexMap_FromIntField_WhenArgumentsAreInvalid_Throws()
    {
        var geometry = new HexMapGeometry(1, 1, 1f, Layout.OddR);
        IIntField? field = null;
        IIntField source = new CoordinateIntField();
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
    public void ToValueMask_ReturnsBooleanMaskForListedValues()
    {
        var topology = new HexMapTopology(6, 1, Layout.OddR);
        IIntHexMap map = new IntHexMap(topology, new[] { 1, 2, 3, 2, -1, 5 });

        BoolHexMap result = map.ToValueMask(new[] { 2, -1, 2 });
        result[0] = true;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<BoolHexMap>());
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.True);
            Assert.That(result[2], Is.False);
            Assert.That(result[3], Is.True);
            Assert.That(result[4], Is.True);
            Assert.That(result[5], Is.False);
            Assert.That(map[0], Is.EqualTo(1));
        });
    }

    [Test]
    public void ToValueMask_WhenValueListIsEmpty_ReturnsAllFalseMask()
    {
        var topology = new HexMapTopology(3, 1, Layout.EvenQ);
        IIntHexMap map = new IntHexMap(topology, new[] { 1, 2, 3 });

        BoolHexMap result = map.ToValueMask(Array.Empty<int>());

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.False);
            Assert.That(result[1], Is.False);
            Assert.That(result[2], Is.False);
        });
    }

    [Test]
    public void ToValueMask_WhenMapIsEmpty_ReturnsEmptyMaskWithSameTopology()
    {
        var topology = new HexMapTopology(0, 0, Layout.EvenR);
        IIntHexMap map = new IntHexMap(topology);

        BoolHexMap result = map.ToValueMask(new[] { 1, 2 });

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void ToValueMask_FromSpatialMap_ReturnsSpatialBooleanMaskWithGeometry()
    {
        var geometry = new HexMapGeometry(4, 1, 1f, Layout.OddQ);
        ISpatialIntHexMap map = new SpatialIntHexMap(geometry, new[] { 4, 1, 4, 7 });

        SpatialBoolHexMap result = map.ToValueMask(new[] { 4 });
        result[1] = true;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<SpatialBoolHexMap>());
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.True);
            Assert.That(result[2], Is.True);
            Assert.That(result[3], Is.False);
            Assert.That(map[1], Is.EqualTo(1));
        });
    }

    [Test]
    public void ToValueMask_WhenArgumentsAreNull_Throws()
    {
        IIntHexMap? map = null;
        var source = new IntHexMap(new HexMapTopology(1, 1, Layout.OddR), new[] { 1 });
        IReadOnlyList<int>? values = null;

#pragma warning disable CS8604
        var mapException = Assert.Throws<ArgumentNullException>(() => map.ToValueMask(new[] { 1 }));
        var valuesException = Assert.Throws<ArgumentNullException>(() => source.ToValueMask(values));
#pragma warning restore CS8604

        Assert.Multiple(() =>
        {
            Assert.That(mapException!.ParamName, Is.EqualTo("map"));
            Assert.That(valuesException!.ParamName, Is.EqualTo("values"));
        });
    }

    [Test]
    public void ToValueMask_FromSpatialMap_WhenArgumentsAreInvalid_Throws()
    {
        ISpatialIntHexMap? map = null;
        var geometry = new HexMapGeometry(1, 1, 1f, Layout.OddR);
        var source = new SpatialIntHexMap(geometry, new[] { 1 });
        IReadOnlyList<int>? values = null;
        var inconsistent = new InconsistentSpatialIntMap(
            topology: new HexMapTopology(1, 1, Layout.OddR),
            geometry: new HexMapGeometry(2, 1, 1f, Layout.OddR));

#pragma warning disable CS8604
        var mapException = Assert.Throws<ArgumentNullException>(() => map.ToValueMask(new[] { 1 }));
        var valuesException = Assert.Throws<ArgumentNullException>(() => source.ToValueMask(values));
#pragma warning restore CS8604
        var inconsistentException = Assert.Throws<ArgumentException>(() => inconsistent.ToValueMask(new[] { 1 }));

        Assert.Multiple(() =>
        {
            Assert.That(mapException!.ParamName, Is.EqualTo("map"));
            Assert.That(valuesException!.ParamName, Is.EqualTo("values"));
            Assert.That(inconsistentException!.ParamName, Is.EqualTo("map"));
        });
    }

    [Test]
    public void ToIntHexMap_ReturnsIndependentMutableCopy()
    {
        var topology = new HexMapTopology(3, 1, Layout.EvenQ);
        var source = new HexMap<int>(topology, new[] { 1, -2, 4 });
        IHexMap<int> readOnlySource = source;

        IntHexMap result = readOnlySource.ToIntHexMap();
        source[0] = 10;
        result[1] = 20;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(1));
            Assert.That(result[1], Is.EqualTo(20));
            Assert.That(result[2], Is.EqualTo(4));
            Assert.That(source[1], Is.EqualTo(-2));
        });
    }

    [Test]
    public void ToIntHexMap_WhenSourceIsIntHexMap_ReturnsIndependentCopy()
    {
        var source = new IntHexMap(
            new HexMapTopology(2, 1, Layout.OddR),
            new[] { 1, 2 });

        IntHexMap result = source.ToIntHexMap();
        source[0] = 10;
        result[1] = 20;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.SameAs(source));
            Assert.That(result[0], Is.EqualTo(1));
            Assert.That(source[1], Is.EqualTo(2));
        });
    }

    [Test]
    public void ToIntHexMap_WhenMapIsEmpty_ReturnsEmptyMapWithSameTopology()
    {
        var topology = new HexMapTopology(0, 0, Layout.EvenR);
        IHexMap<int> source = new HexMap<int>(topology);

        IntHexMap result = source.ToIntHexMap();

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void ToIntHexMap_WhenMapIsNull_Throws()
    {
        IHexMap<int>? map = null;

#pragma warning disable CS8604
        var exception = Assert.Throws<ArgumentNullException>(() => map.ToIntHexMap());
#pragma warning restore CS8604

        Assert.That(exception!.ParamName, Is.EqualTo("map"));
    }

    private sealed class CoordinateIntField : IIntField
    {
        public int Min => -1_000_000;

        public int Max => 1_000_000;

        public int Sample(PointXY point) => (int)System.MathF.Round(point.X * 10f + point.Y);
    }

    private sealed class InconsistentSpatialIntMap : ISpatialIntHexMap
    {
        public InconsistentSpatialIntMap(HexMapTopology topology, HexMapGeometry geometry)
        {
            Topology = topology;
            Geometry = geometry;
        }

        public HexMapTopology Topology { get; }

        public HexMapGeometry Geometry { get; }

        public int this[VectorXYInt index] => 0;

        public int this[int index] => 0;

        public int Min => 0;

        public int Max => 0;
    }
}
