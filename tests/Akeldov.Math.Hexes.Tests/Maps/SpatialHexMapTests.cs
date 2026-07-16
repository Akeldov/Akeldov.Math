using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class SpatialHexMapTests
{
    [Test]
    public void Constructor_UsesGeometryAndImplementsISpatialHexMap()
    {
        var geometry = new HexMapGeometry(3, 2, new VectorXY(10f, -20f), 2f, Layout.EvenQ);

        var map = new SpatialHexMap<int>(geometry);
        ISpatialHexMap<int> spatialMap = map;

        Assert.Multiple(() =>
        {
            Assert.That(spatialMap.Geometry, Is.EqualTo(geometry));
            Assert.That(spatialMap.Topology, Is.EqualTo(geometry.Topology));
        });
    }

    [Test]
    public void Constructor_WithValues_UsesRowMajorArrayAsBackingStorage()
    {
        var geometry = new HexMapGeometry(3, 2, 1f, Layout.OddR);
        var values = new[] { 0, 1, 2, 3, 4, 5 };
        var map = new SpatialHexMap<int>(geometry, values);

        values[5] = 42;
        map[new VectorXYInt(1, 1)] = 10;

        Assert.Multiple(() =>
        {
            Assert.That(map[new VectorXYInt(2, 1)], Is.EqualTo(42));
            Assert.That(values[4], Is.EqualTo(10));
        });
    }

    [Test]
    public void Constructor_WithValues_WhenArrayIsInvalid_Throws()
    {
        var geometry = new HexMapGeometry(2, 1, 1f, Layout.OddR);

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => new SpatialHexMap<int>(geometry, null!))!.ParamName,
                Is.EqualTo("values"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => new SpatialHexMap<int>(geometry, new int[1]))!.ParamName,
                Is.EqualTo("values"));
        });
    }

    [Test]
    public void Constructor_WhenGeometryIsDefault_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpatialHexMap<int>(default));
    }
}
