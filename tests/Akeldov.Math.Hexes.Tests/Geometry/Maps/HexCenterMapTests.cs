using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Hexes.Tests.VectorsQRS;
using Akeldov.Math.Spatial2D.Regions;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Geometry.Maps;

public class HexCenterMapTests
{
    [Test]
    public void Constructor_WithTopology_UsesUnitRadiusGeometry()
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);

        var map = new HexCenterMap(topology);

        Assert.That(map.Geometry, Is.EqualTo(new HexMapGeometry(topology, 1f)));
    }

    [Test]
    public void Constructor_UsesOriginAsZeroHexCenter_ForEveryLayout()
    {
        var origin = new VectorXY(10f, 20f);
        const float radius = 2f;

        foreach (Layout layout in Enum.GetValues(typeof(Layout)))
        {
            var geometry = new HexCenterMap(new HexMapGeometry(2, 2, origin, radius, layout));

            Assert.That(geometry.Topology.Resolution, Is.EqualTo(new VectorXYInt(2, 2)));
            Assert.That(geometry.Geometry, Is.EqualTo(new HexMapGeometry(2, 2, origin, radius, layout)));
            Assert.That(geometry.Geometry.Origin, Is.EqualTo(origin));
            Assert.That(geometry.Geometry.Apothem, Is.EqualTo(radius.ConvertHexRadiusToApothem()));
            Assert.That(geometry.Topology.Layout, Is.EqualTo(layout));
            Assert.That(typeof(HexCenterMap).GetProperty("Centers"), Is.Null);
            VectorAssert.AreEqual(geometry[0], origin.X, origin.Y);
        }
    }

    [Test]
    public void Constructor_UsesHexMapGeometry()
    {
        var geometry = new HexMapGeometry(
            new HexMapTopology(2, 2, Layout.OddR),
            new VectorXY(10f, 20f),
            2f);

        var map = new HexCenterMap(geometry);

        Assert.That(map.Geometry, Is.EqualTo(geometry));
        Assert.That(map.Topology.Resolution, Is.EqualTo(new VectorXYInt(2, 2)));
        Assert.That(map.Geometry.Origin, Is.EqualTo(new VectorXY(10f, 20f)));
        Assert.That(map.Geometry.Apothem, Is.EqualTo(2f.ConvertHexRadiusToApothem()));
        Assert.That(map.Topology.Layout, Is.EqualTo(Layout.OddR));
        VectorAssert.AreEqual(map[0], 10f, 20f);
    }

    [Test]
    public void HexMapGeometry_WithRadius_UsesDefaultZeroHexCenter()
    {
        var geometry = new HexMapGeometry(1, 1, 2f.ConvertHexApothemToRadius(), Layout.EvenQ);

        Assert.That(geometry.Topology, Is.EqualTo(new HexMapTopology(1, 1, Layout.EvenQ)));
        Assert.That(geometry.Apothem, Is.EqualTo(2f).Within(0.00001f));
        VectorAssert.AreEqual(geometry.Origin, 2f.ConvertHexApothemToRadius(), 6f);
    }

    [TestCase(Layout.OddR, 8f, 17.6906f, 22f, 25.7735f)]
    [TestCase(Layout.EvenR, 6f, 17.6906f, 20f, 25.7735f)]
    [TestCase(Layout.OddQ, 7.6906f, 18f, 19.2376f, 28f)]
    [TestCase(Layout.EvenQ, 7.6906f, 16f, 19.2376f, 26f)]
    public void HexMapGeometryBoundingBox_ReturnsRectangleAroundWholeMap(
        Layout layout,
        float expectedMinX,
        float expectedMinY,
        float expectedMaxX,
        float expectedMaxY)
    {
        var geometry = new HexMapGeometry(
            width: 3,
            height: 2,
            origin: new VectorXY(10f, 20f),
            radius: 2f.ConvertHexApothemToRadius(),
            layout: layout);

        Rectangle boundingBox = geometry.GetBoundingBox();

        Assert.That(boundingBox.Min.X, Is.EqualTo(expectedMinX).Within(0.0001f));
        Assert.That(boundingBox.Min.Y, Is.EqualTo(expectedMinY).Within(0.0001f));
        Assert.That(boundingBox.Max.X, Is.EqualTo(expectedMaxX).Within(0.0001f));
        Assert.That(boundingBox.Max.Y, Is.EqualTo(expectedMaxY).Within(0.0001f));
        Assert.That(geometry.GetBoundingBoxSize().X, Is.EqualTo(boundingBox.Size.X).Within(0.0001f));
        Assert.That(geometry.GetBoundingBoxSize().Y, Is.EqualTo(boundingBox.Size.Y).Within(0.0001f));
    }

    [Test]
    public void HexMapGeometryBoundingBox_WhenMapIsEmpty_Throws()
    {
        var geometry = new HexMapGeometry(
            new HexMapTopology(0, 1, Layout.OddR),
            VectorXY.Zero,
            2f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => geometry.GetBoundingBox());

        Assert.That(exception!.ParamName, Is.EqualTo("geometry"));
    }

    [Test]
    public void HexMapTopologyBoundingBox_WithRadiusAndOrigin_ReturnsSameRectangleAsGeometry()
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);
        var origin = new VectorXY(10f, 20f);
        const float radius = 2f;

        Rectangle topologyBoundingBox = topology.GetBoundingBox(origin, radius);
        Rectangle geometryBoundingBox = new HexMapGeometry(topology, origin, radius).GetBoundingBox();

        Assert.That(topologyBoundingBox, Is.EqualTo(geometryBoundingBox));
    }

    [Test]
    public void HexMapGeometryToRasterGeometry_UsesBoundingBoxAndPixelsPerApothem()
    {
        var geometry = new HexMapGeometry(
            width: 1,
            height: 1,
            origin: VectorXY.Zero,
            radius: 2f.ConvertHexApothemToRadius(),
            layout: Layout.OddR);

        RasterGeometry grid = geometry.ToRasterGeometry(pixelsPerApothem: 3f);

        Assert.That(grid.Origin.X, Is.EqualTo(-2f).Within(0.0001f));
        Assert.That(grid.Origin.Y, Is.EqualTo(-2.3094f).Within(0.0001f));
        Assert.That(grid.Size.X, Is.EqualTo(4f).Within(0.0001f));
        Assert.That(grid.Size.Y, Is.EqualTo(4.6188f).Within(0.0001f));
        Assert.That(grid.Resolution, Is.EqualTo(new VectorXYInt(6, 7)));
    }

    [Test]
    public void HexMapTopologyToRasterGeometry_WithRadiusAndOrigin_ReturnsSameGridAsGeometry()
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);
        var origin = new VectorXY(10f, 20f);
        const float radius = 2f;
        const float pixelsPerApothem = 3f;

        RasterGeometry topologyGrid = topology.ToRasterGeometry(radius, origin, pixelsPerApothem);
        RasterGeometry geometryGrid = new HexMapGeometry(topology, origin, radius).ToRasterGeometry(pixelsPerApothem);

        Assert.That(topologyGrid, Is.EqualTo(geometryGrid));
    }

    [Test]
    public void HexMapTopologyToRasterGeometry_WithMargin_ReturnsSameGridAsGeometry()
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);
        var origin = new VectorXY(10f, 20f);
        const float radius = 2f;
        const float pixelsPerApothem = 3f;
        const float margin = 1.5f;

        RasterGeometry topologyGrid = topology.ToRasterGeometry(radius, origin, pixelsPerApothem, margin);
        RasterGeometry geometryGrid = new HexMapGeometry(topology, origin, radius).ToRasterGeometry(pixelsPerApothem, margin);

        Assert.That(topologyGrid, Is.EqualTo(geometryGrid));
    }

    [Test]
    public void HexMapGeometryToRasterGeometry_WhenPixelsPerApothemIsInvalid_Throws()
    {
        var geometry = new HexMapGeometry(1, 1, VectorXY.Zero, 2f, Layout.OddR);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            geometry.ToRasterGeometry(0f));

        Assert.That(exception!.ParamName, Is.EqualTo("pixelsPerApothem"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void HexMapGeometryToRasterGeometry_WhenMarginIsInvalid_Throws(float margin)
    {
        var geometry = new HexMapGeometry(1, 1, VectorXY.Zero, 2f, Layout.OddR);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            geometry.ToRasterGeometry(3f, margin));

        Assert.That(exception!.ParamName, Is.EqualTo("margin"));
    }

    [Test]
    public void HexMapGeometryToRasterGeometry_WhenRasterResolutionDoesNotFitInt32_Throws()
    {
        var geometry = new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR);

        Assert.Throws<OverflowException>(() =>
            geometry.ToRasterGeometry(float.MaxValue));
    }

    [Test]
    public void GetNormalizedHexVertices_ReturnsCopy()
    {
        VectorXY[] vertices = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertices(Layout.OddR);
        vertices[0] = VectorXY.Zero;

        VectorXY[] freshVertices = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertices(Layout.OddR);

        Assert.That(freshVertices[0], Is.Not.EqualTo(VectorXY.Zero));
    }

    [TestCase(Layout.OddR, 2f, 2.3094f)]
    [TestCase(Layout.EvenR, 6f, 2.3094f)]
    [TestCase(Layout.OddQ, 2.3094f, 2f)]
    [TestCase(Layout.EvenQ, 2.3094f, 6f)]
    public void Constructor_WithoutOrigin_PreservesDefaultZeroHexCenter(Layout layout, float expectedX, float expectedY)
    {
        var geometry = new HexCenterMap(new HexMapGeometry(1, 1, 2f.ConvertHexApothemToRadius(), layout));
        VectorAssert.AreEqual(geometry[0], expectedX, expectedY);
    }

    [Test]
    public void Constructor_WhenOriginIsNotFinite_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexCenterMap(new HexMapGeometry(1, 1, new VectorXY(float.PositiveInfinity, 0f), 2f, Layout.OddR)));

        Assert.That(exception!.ParamName, Is.EqualTo("origin"));
    }

    [TestCase(0f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void Constructor_WithOrigin_WhenRadiusIsInvalid_Throws(float radius)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, radius, Layout.OddR)));

        Assert.That(exception!.ParamName, Is.EqualTo("radius"));
    }

    [Test]
    public void Constructor_WhenGeometryIsDefault_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexCenterMap(default(HexMapGeometry)));

        Assert.That(exception!.ParamName, Is.EqualTo("geometry"));
    }

    [TestCase(0f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void Constructor_WithoutOrigin_WhenRadiusIsInvalid_Throws(float radius)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexCenterMap(new HexMapGeometry(1, 1, radius, Layout.OddR)));

        Assert.That(exception!.ParamName, Is.EqualTo("radius"));
    }

    [Test]
    public void HexCenterMap_ImplementsReadOnlyISpatialHexMap()
    {
        var source = new HexCenterMap(new HexMapGeometry(3, 2, VectorXY.Zero, 2f, Layout.OddR));
        ISpatialHexMap<PointXY> map = source;

        PointXY center = source[5];

        Assert.That(source, Is.Not.InstanceOf<HexMap<PointXY>>());
        Assert.That(typeof(HexCenterMap).GetProperty("Item", new[] { typeof(VectorXYInt) })!.SetMethod, Is.Null);
        Assert.That(typeof(HexCenterMap).GetProperty("Item", new[] { typeof(int) })!.SetMethod, Is.Null);
        Assert.That(map.Topology.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(map.Topology.Layout, Is.EqualTo(Layout.OddR));
        Assert.That(map.Geometry, Is.EqualTo(source.Geometry));
        Assert.That(map[new VectorXYInt(2, 1)], Is.EqualTo(center));
        Assert.That(map[5], Is.EqualTo(center));
    }

    [Test]
    public void Indexer_WhenIndexIsOutsideMap_Throws()
    {
        var geometry = new HexCenterMap(new HexMapGeometry(3, 2, VectorXY.Zero, 2f, Layout.OddR));

        Assert.Throws<IndexOutOfRangeException>(() => _ = geometry[new VectorXYInt(3, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = geometry[new VectorXYInt(0, 2)]);
    }
}
