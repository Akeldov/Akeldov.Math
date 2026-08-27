using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class SpatialHexMapConversionExtensionsTests
{
    [Test]
    public void ToSpatialHexMap_FromBooleanInterface_ReturnsIndependentSpecializedCopyWithGeometry()
    {
        var geometry = new HexMapGeometry(3, 1, new VectorXY(10f, -20f), 2f, Layout.EvenQ);
        var sourceMap = new HexMap<bool>(geometry.Topology, new[] { true, false, true });
        IHexMap<bool> source = sourceMap;

        SpatialBoolHexMap result = source.ToSpatialHexMap(geometry);
        sourceMap[0] = false;
        result[1] = true;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<SpatialBoolHexMap>());
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.True);
            Assert.That(result[2], Is.True);
            Assert.That(sourceMap[1], Is.False);
        });
    }

    [Test]
    public void ToSpatialHexMap_FromFloatInterface_ReturnsIndependentSpecializedCopyWithGeometry()
    {
        var geometry = new HexMapGeometry(3, 1, new VectorXY(-4f, 8f), 1.5f, Layout.OddR);
        var sourceMap = new HexMap<float>(geometry.Topology, new[] { 1.5f, -2f, 4f });
        IHexMap<float> source = sourceMap;

        SpatialFloatHexMap result = source.ToSpatialHexMap(geometry);
        sourceMap[0] = 10f;
        result[1] = 20f;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.EqualTo(1.5f));
            Assert.That(result[1], Is.EqualTo(20f));
            Assert.That(result[2], Is.EqualTo(4f));
            Assert.That(sourceMap[1], Is.EqualTo(-2f));
        });
    }

    [Test]
    public void ToSpatialHexMap_FromIntInterface_ReturnsIndependentSpecializedCopyWithGeometry()
    {
        var geometry = new HexMapGeometry(3, 1, new VectorXY(3f, 7f), 0.75f, Layout.EvenR);
        var sourceMap = new HexMap<int>(geometry.Topology, new[] { 1, -2, 4 });
        IHexMap<int> source = sourceMap;

        SpatialIntHexMap result = source.ToSpatialHexMap(geometry);
        sourceMap[0] = 10;
        result[1] = 20;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<SpatialIntHexMap>());
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.EqualTo(1));
            Assert.That(result[1], Is.EqualTo(20));
            Assert.That(result[2], Is.EqualTo(4));
            Assert.That(sourceMap[1], Is.EqualTo(-2));
        });
    }

    [Test]
    public void ToHexMap_FromBooleanSpatialInterface_ReturnsIndependentSpecializedCopy()
    {
        var geometry = new HexMapGeometry(3, 1, new VectorXY(10f, -20f), 2f, Layout.EvenQ);
        var sourceMap = new SpatialHexMap<bool>(geometry, new[] { true, false, true });
        ISpatialHexMap<bool> source = sourceMap;

        BoolHexMap result = source.ToHexMap();
        sourceMap[0] = false;
        result[1] = true;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<BoolHexMap>());
            Assert.That(result.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.True);
            Assert.That(result[2], Is.True);
            Assert.That(sourceMap[1], Is.False);
        });
    }

    [Test]
    public void ToHexMap_FromFloatSpatialInterface_ReturnsIndependentSpecializedCopy()
    {
        var geometry = new HexMapGeometry(3, 1, new VectorXY(-4f, 8f), 1.5f, Layout.OddR);
        var sourceMap = new SpatialHexMap<float>(geometry, new[] { 1.5f, -2f, 4f });
        ISpatialHexMap<float> source = sourceMap;

        FloatHexMap result = source.ToHexMap();
        sourceMap[0] = 10f;
        result[1] = 20f;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<FloatHexMap>());
            Assert.That(result.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(result[0], Is.EqualTo(1.5f));
            Assert.That(result[1], Is.EqualTo(20f));
            Assert.That(result[2], Is.EqualTo(4f));
            Assert.That(sourceMap[1], Is.EqualTo(-2f));
        });
    }

    [Test]
    public void ToHexMap_FromIntSpatialInterface_ReturnsIndependentSpecializedCopy()
    {
        var geometry = new HexMapGeometry(3, 1, new VectorXY(3f, 7f), 0.75f, Layout.EvenR);
        var sourceMap = new SpatialHexMap<int>(geometry, new[] { 1, -2, 4 });
        ISpatialHexMap<int> source = sourceMap;

        IntHexMap result = source.ToHexMap();
        sourceMap[0] = 10;
        result[1] = 20;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<IntHexMap>());
            Assert.That(result.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(result[0], Is.EqualTo(1));
            Assert.That(result[1], Is.EqualTo(20));
            Assert.That(result[2], Is.EqualTo(4));
            Assert.That(sourceMap[1], Is.EqualTo(-2));
        });
    }

    [Test]
    public void ToSpatialHexMap_WhenMapsAreEmpty_ReturnsEmptySpecializedMapsWithGeometry()
    {
        var geometry = new HexMapGeometry(0, 0, new VectorXY(3f, -2f), 1.25f, Layout.EvenR);
        IHexMap<bool> boolSource = new HexMap<bool>(geometry.Topology);
        IHexMap<float> floatSource = new HexMap<float>(geometry.Topology);
        IHexMap<int> intSource = new HexMap<int>(geometry.Topology);

        SpatialBoolHexMap boolResult = boolSource.ToSpatialHexMap(geometry);
        SpatialFloatHexMap floatResult = floatSource.ToSpatialHexMap(geometry);
        SpatialIntHexMap intResult = intSource.ToSpatialHexMap(geometry);

        Assert.Multiple(() =>
        {
            Assert.That(boolResult, Is.TypeOf<SpatialBoolHexMap>());
            Assert.That(floatResult, Is.TypeOf<SpatialFloatHexMap>());
            Assert.That(intResult, Is.TypeOf<SpatialIntHexMap>());
            Assert.That(boolResult.Geometry, Is.EqualTo(geometry));
            Assert.That(floatResult.Geometry, Is.EqualTo(geometry));
            Assert.That(intResult.Geometry, Is.EqualTo(geometry));
            Assert.That(boolResult.Topology.Count, Is.Zero);
            Assert.That(floatResult.Topology.Count, Is.Zero);
            Assert.That(intResult.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void ToHexMap_WhenMapsAreEmpty_ReturnsEmptySpecializedMapsWithSameTopology()
    {
        var geometry = new HexMapGeometry(0, 0, new VectorXY(3f, -2f), 1.25f, Layout.EvenR);
        ISpatialHexMap<bool> boolSource = new SpatialHexMap<bool>(geometry);
        ISpatialHexMap<float> floatSource = new SpatialHexMap<float>(geometry);
        ISpatialHexMap<int> intSource = new SpatialHexMap<int>(geometry);

        BoolHexMap boolResult = boolSource.ToHexMap();
        FloatHexMap floatResult = floatSource.ToHexMap();
        IntHexMap intResult = intSource.ToHexMap();

        Assert.Multiple(() =>
        {
            Assert.That(boolResult, Is.TypeOf<BoolHexMap>());
            Assert.That(floatResult, Is.TypeOf<FloatHexMap>());
            Assert.That(intResult, Is.TypeOf<IntHexMap>());
            Assert.That(boolResult.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(floatResult.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(intResult.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(boolResult.Topology.Count, Is.Zero);
            Assert.That(floatResult.Topology.Count, Is.Zero);
            Assert.That(intResult.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void ToSpatialHexMap_WhenMapIsNull_ThrowsForEveryValueType()
    {
        var geometry = new HexMapGeometry(1, 1, 1f, Layout.OddR);
        IHexMap<bool>? boolMap = null;
        IHexMap<float>? floatMap = null;
        IHexMap<int>? intMap = null;

#pragma warning disable CS8604
        var boolException = Assert.Throws<ArgumentNullException>(() => boolMap.ToSpatialHexMap(geometry));
        var floatException = Assert.Throws<ArgumentNullException>(() => floatMap.ToSpatialHexMap(geometry));
        var intException = Assert.Throws<ArgumentNullException>(() => intMap.ToSpatialHexMap(geometry));
#pragma warning restore CS8604

        Assert.Multiple(() =>
        {
            Assert.That(boolException!.ParamName, Is.EqualTo("map"));
            Assert.That(floatException!.ParamName, Is.EqualTo("map"));
            Assert.That(intException!.ParamName, Is.EqualTo("map"));
        });
    }

    [Test]
    public void ToHexMap_WhenMapIsNull_ThrowsForEveryValueType()
    {
        ISpatialHexMap<bool>? boolMap = null;
        ISpatialHexMap<float>? floatMap = null;
        ISpatialHexMap<int>? intMap = null;

#pragma warning disable CS8604
        var boolException = Assert.Throws<ArgumentNullException>(() => boolMap.ToHexMap());
        var floatException = Assert.Throws<ArgumentNullException>(() => floatMap.ToHexMap());
        var intException = Assert.Throws<ArgumentNullException>(() => intMap.ToHexMap());
#pragma warning restore CS8604

        Assert.Multiple(() =>
        {
            Assert.That(boolException!.ParamName, Is.EqualTo("map"));
            Assert.That(floatException!.ParamName, Is.EqualTo("map"));
            Assert.That(intException!.ParamName, Is.EqualTo("map"));
        });
    }

    [Test]
    public void ToSpatialHexMap_WhenGeometryIsDefault_ThrowsForEveryValueType()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        IHexMap<bool> boolMap = new HexMap<bool>(topology);
        IHexMap<float> floatMap = new HexMap<float>(topology);
        IHexMap<int> intMap = new HexMap<int>(topology);

        var boolException = Assert.Throws<ArgumentOutOfRangeException>(() => boolMap.ToSpatialHexMap(default));
        var floatException = Assert.Throws<ArgumentOutOfRangeException>(() => floatMap.ToSpatialHexMap(default));
        var intException = Assert.Throws<ArgumentOutOfRangeException>(() => intMap.ToSpatialHexMap(default));

        Assert.Multiple(() =>
        {
            Assert.That(boolException!.ParamName, Is.EqualTo("geometry"));
            Assert.That(floatException!.ParamName, Is.EqualTo("geometry"));
            Assert.That(intException!.ParamName, Is.EqualTo("geometry"));
        });
    }

    [Test]
    public void ToSpatialHexMap_WhenTopologyDiffersWithSameCount_ThrowsForEveryValueType()
    {
        var sourceTopology = new HexMapTopology(2, 2, Layout.OddR);
        var otherLayout = new HexMapGeometry(2, 2, 1f, Layout.EvenR);
        var otherResolution = new HexMapGeometry(4, 1, 1f, Layout.OddR);
        IHexMap<bool> boolMap = new HexMap<bool>(sourceTopology);
        IHexMap<float> floatMap = new HexMap<float>(sourceTopology);
        IHexMap<int> intMap = new HexMap<int>(sourceTopology);

        var boolLayoutException = Assert.Throws<ArgumentException>(() => boolMap.ToSpatialHexMap(otherLayout));
        var boolResolutionException = Assert.Throws<ArgumentException>(() => boolMap.ToSpatialHexMap(otherResolution));
        var floatLayoutException = Assert.Throws<ArgumentException>(() => floatMap.ToSpatialHexMap(otherLayout));
        var floatResolutionException = Assert.Throws<ArgumentException>(() => floatMap.ToSpatialHexMap(otherResolution));
        var intLayoutException = Assert.Throws<ArgumentException>(() => intMap.ToSpatialHexMap(otherLayout));
        var intResolutionException = Assert.Throws<ArgumentException>(() => intMap.ToSpatialHexMap(otherResolution));

        Assert.Multiple(() =>
        {
            Assert.That(boolLayoutException!.ParamName, Is.EqualTo("geometry"));
            Assert.That(boolResolutionException!.ParamName, Is.EqualTo("geometry"));
            Assert.That(floatLayoutException!.ParamName, Is.EqualTo("geometry"));
            Assert.That(floatResolutionException!.ParamName, Is.EqualTo("geometry"));
            Assert.That(intLayoutException!.ParamName, Is.EqualTo("geometry"));
            Assert.That(intResolutionException!.ParamName, Is.EqualTo("geometry"));
        });
    }
}
