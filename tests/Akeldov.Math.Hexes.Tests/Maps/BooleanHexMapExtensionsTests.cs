using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class BooleanHexMapExtensionsTests
{
    [Test]
    public void And_WithHexMaps_ReturnsNewMapWithCellwiseConjunction()
    {
        var topology = new HexMapTopology(2, 2, Layout.EvenQ);
        IHexMap<bool> left = new HexMap<bool>(topology, new[] { true, true, false, false });
        IHexMap<bool> right = new HexMap<bool>(topology, new[] { true, false, true, false });

        HexMap<bool> result = left.And(right);
        result[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.False);
            Assert.That(result[1], Is.False);
            Assert.That(result[2], Is.False);
            Assert.That(result[3], Is.False);
            Assert.That(left[0], Is.True);
            Assert.That(right[0], Is.True);
        });
    }

    [Test]
    public void And_WithSpatialMaps_ReturnsNewMapWithCellwiseConjunction()
    {
        var geometry = new HexMapGeometry(2, 2, new VectorXY(10f, -20f), 2f, Layout.EvenQ);
        ISpatialHexMap<bool> left = new SpatialHexMap<bool>(
            geometry,
            new[] { true, true, false, false });
        ISpatialHexMap<bool> right = new SpatialHexMap<bool>(
            geometry,
            new[] { true, false, true, false });

        SpatialHexMap<bool> result = left.And(right);
        result[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.False);
            Assert.That(result[1], Is.False);
            Assert.That(result[2], Is.False);
            Assert.That(result[3], Is.False);
            Assert.That(left[0], Is.True);
            Assert.That(right[0], Is.True);
        });
    }

    [Test]
    public void And_WithSpatialLeftAndHexRight_RetainsLeftGeometry()
    {
        var geometry = new HexMapGeometry(2, 1, new VectorXY(10f, -20f), 2f, Layout.OddR);
        ISpatialHexMap<bool> left = new SpatialHexMap<bool>(geometry, new[] { true, false });
        IHexMap<bool> right = new HexMap<bool>(geometry.Topology, new[] { true, true });

        SpatialHexMap<bool> result = left.And(right);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.False);
        });
    }

    [Test]
    public void And_WithHexLeftAndSpatialRight_RetainsRightGeometry()
    {
        var geometry = new HexMapGeometry(2, 1, new VectorXY(10f, -20f), 2f, Layout.OddR);
        IHexMap<bool> left = new HexMap<bool>(geometry.Topology, new[] { true, false });
        ISpatialHexMap<bool> right = new SpatialHexMap<bool>(geometry, new[] { true, true });

        SpatialHexMap<bool> result = left.And(right);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.False);
        });
    }

    [Test]
    public void And_WhenMapIsNull_Throws()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        IHexMap<bool> map = new HexMap<bool>(topology);
        IHexMap<bool> nullMap = null!;

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullMap.And(map))!.ParamName,
                Is.EqualTo("left"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => map.And(nullMap))!.ParamName,
                Is.EqualTo("right"));
        });
    }

    [Test]
    public void And_WhenTopologiesDiffer_Throws()
    {
        IHexMap<bool> left = new HexMap<bool>(new HexMapTopology(1, 1, Layout.OddR));
        IHexMap<bool> right = new HexMap<bool>(new HexMapTopology(2, 1, Layout.OddR));

        var exception = Assert.Throws<ArgumentException>(() => left.And(right));

        Assert.That(exception!.ParamName, Is.EqualTo("right"));
    }

    [Test]
    public void And_WhenSpatialGeometriesDiffer_Throws()
    {
        ISpatialHexMap<bool> left = new SpatialHexMap<bool>(
            new HexMapGeometry(1, 1, new VectorXY(0f, 0f), 1f, Layout.OddR));
        ISpatialHexMap<bool> right = new SpatialHexMap<bool>(
            new HexMapGeometry(1, 1, new VectorXY(1f, 0f), 1f, Layout.OddR));

        var exception = Assert.Throws<ArgumentException>(() => left.And(right));

        Assert.That(exception!.ParamName, Is.EqualTo("right"));
    }

    [Test]
    public void And_WithSpatialLeftAndRuntimeSpatialRight_WhenGeometriesDiffer_Throws()
    {
        ISpatialHexMap<bool> left = new SpatialHexMap<bool>(
            new HexMapGeometry(1, 1, new VectorXY(0f, 0f), 1f, Layout.OddR));
        IHexMap<bool> right = new SpatialHexMap<bool>(
            new HexMapGeometry(1, 1, new VectorXY(1f, 0f), 1f, Layout.OddR));

        var exception = Assert.Throws<ArgumentException>(() => left.And(right));

        Assert.That(exception!.ParamName, Is.EqualTo("right"));
    }

    [Test]
    public void And_WithRuntimeSpatialLeftAndSpatialRight_WhenGeometriesDiffer_Throws()
    {
        IHexMap<bool> left = new SpatialHexMap<bool>(
            new HexMapGeometry(1, 1, new VectorXY(0f, 0f), 1f, Layout.OddR));
        ISpatialHexMap<bool> right = new SpatialHexMap<bool>(
            new HexMapGeometry(1, 1, new VectorXY(1f, 0f), 1f, Layout.OddR));

        var exception = Assert.Throws<ArgumentException>(() => left.And(right));

        Assert.That(exception!.ParamName, Is.EqualTo("right"));
    }

    [Test]
    public void Or_WithHexMaps_ReturnsNewMapWithCellwiseDisjunction()
    {
        var topology = new HexMapTopology(2, 2, Layout.EvenQ);
        IHexMap<bool> left = new HexMap<bool>(topology, new[] { true, true, false, false });
        IHexMap<bool> right = new HexMap<bool>(topology, new[] { true, false, true, false });

        HexMap<bool> result = left.Or(right);
        result[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.False);
            Assert.That(result[1], Is.True);
            Assert.That(result[2], Is.True);
            Assert.That(result[3], Is.False);
            Assert.That(left[0], Is.True);
            Assert.That(right[0], Is.True);
        });
    }

    [Test]
    public void Or_WithSpatialMaps_ReturnsNewMapWithCellwiseDisjunction()
    {
        var geometry = new HexMapGeometry(2, 2, new VectorXY(10f, -20f), 2f, Layout.EvenQ);
        ISpatialHexMap<bool> left = new SpatialHexMap<bool>(
            geometry,
            new[] { true, true, false, false });
        ISpatialHexMap<bool> right = new SpatialHexMap<bool>(
            geometry,
            new[] { true, false, true, false });

        SpatialHexMap<bool> result = left.Or(right);
        result[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.False);
            Assert.That(result[1], Is.True);
            Assert.That(result[2], Is.True);
            Assert.That(result[3], Is.False);
            Assert.That(left[0], Is.True);
            Assert.That(right[0], Is.True);
        });
    }

    [Test]
    public void Or_WithSpatialLeftAndHexRight_RetainsLeftGeometry()
    {
        var geometry = new HexMapGeometry(2, 1, new VectorXY(10f, -20f), 2f, Layout.OddR);
        ISpatialHexMap<bool> left = new SpatialHexMap<bool>(geometry, new[] { true, false });
        IHexMap<bool> right = new HexMap<bool>(geometry.Topology, new[] { false, true });

        SpatialHexMap<bool> result = left.Or(right);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.True);
        });
    }

    [Test]
    public void Or_WithHexLeftAndSpatialRight_RetainsRightGeometry()
    {
        var geometry = new HexMapGeometry(2, 1, new VectorXY(10f, -20f), 2f, Layout.OddR);
        IHexMap<bool> left = new HexMap<bool>(geometry.Topology, new[] { true, false });
        ISpatialHexMap<bool> right = new SpatialHexMap<bool>(geometry, new[] { false, true });

        SpatialHexMap<bool> result = left.Or(right);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.True);
        });
    }

    [Test]
    public void Or_WhenMapIsNull_Throws()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        IHexMap<bool> map = new HexMap<bool>(topology);
        IHexMap<bool> nullMap = null!;

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullMap.Or(map))!.ParamName,
                Is.EqualTo("left"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => map.Or(nullMap))!.ParamName,
                Is.EqualTo("right"));
        });
    }

    [Test]
    public void Or_WhenTopologiesDiffer_Throws()
    {
        IHexMap<bool> left = new HexMap<bool>(new HexMapTopology(1, 1, Layout.OddR));
        IHexMap<bool> right = new HexMap<bool>(new HexMapTopology(2, 1, Layout.OddR));

        var exception = Assert.Throws<ArgumentException>(() => left.Or(right));

        Assert.That(exception!.ParamName, Is.EqualTo("right"));
    }

    [Test]
    public void Or_WhenSpatialGeometriesDiffer_Throws()
    {
        ISpatialHexMap<bool> left = new SpatialHexMap<bool>(
            new HexMapGeometry(1, 1, new VectorXY(0f, 0f), 1f, Layout.OddR));
        ISpatialHexMap<bool> right = new SpatialHexMap<bool>(
            new HexMapGeometry(1, 1, new VectorXY(1f, 0f), 1f, Layout.OddR));

        var exception = Assert.Throws<ArgumentException>(() => left.Or(right));

        Assert.That(exception!.ParamName, Is.EqualTo("right"));
    }

    [Test]
    public void Or_WithSpatialLeftAndRuntimeSpatialRight_WhenGeometriesDiffer_Throws()
    {
        ISpatialHexMap<bool> left = new SpatialHexMap<bool>(
            new HexMapGeometry(1, 1, new VectorXY(0f, 0f), 1f, Layout.OddR));
        IHexMap<bool> right = new SpatialHexMap<bool>(
            new HexMapGeometry(1, 1, new VectorXY(1f, 0f), 1f, Layout.OddR));

        var exception = Assert.Throws<ArgumentException>(() => left.Or(right));

        Assert.That(exception!.ParamName, Is.EqualTo("right"));
    }

    [Test]
    public void Or_WithRuntimeSpatialLeftAndSpatialRight_WhenGeometriesDiffer_Throws()
    {
        IHexMap<bool> left = new SpatialHexMap<bool>(
            new HexMapGeometry(1, 1, new VectorXY(0f, 0f), 1f, Layout.OddR));
        ISpatialHexMap<bool> right = new SpatialHexMap<bool>(
            new HexMapGeometry(1, 1, new VectorXY(1f, 0f), 1f, Layout.OddR));

        var exception = Assert.Throws<ArgumentException>(() => left.Or(right));

        Assert.That(exception!.ParamName, Is.EqualTo("right"));
    }
}
