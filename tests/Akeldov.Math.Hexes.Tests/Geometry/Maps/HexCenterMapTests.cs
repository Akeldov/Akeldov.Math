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
    public void Constructor_UsesOriginAsZeroHexCenter_ForEveryLayout()
    {
        var origin = new VectorXY(10f, 20f);
        const float apothem = 2f;

        foreach (Layout layout in Enum.GetValues(typeof(Layout)))
        {
            var geometry = new HexCenterMap(2, 2, origin, apothem, layout);

            Assert.That(geometry.Width, Is.EqualTo(2));
            Assert.That(geometry.Height, Is.EqualTo(2));
            Assert.That(geometry.Geometry, Is.EqualTo(new HexMapGeometry(2, 2, origin, apothem, layout)));
            Assert.That(geometry.Origin, Is.EqualTo(origin));
            Assert.That(geometry.Apothem, Is.EqualTo(apothem));
            Assert.That(geometry.Layout, Is.EqualTo(layout));
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
        Assert.That(map.Width, Is.EqualTo(2));
        Assert.That(map.Height, Is.EqualTo(2));
        Assert.That(map.Origin, Is.EqualTo(new VectorXY(10f, 20f)));
        Assert.That(map.Apothem, Is.EqualTo(2f));
        Assert.That(map.Layout, Is.EqualTo(Layout.OddR));
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
            apothem: 2f,
            layout: layout);

        Rectangle boundingBox = geometry.BoundingBox();

        Assert.That(boundingBox.Min.X, Is.EqualTo(expectedMinX).Within(0.0001f));
        Assert.That(boundingBox.Min.Y, Is.EqualTo(expectedMinY).Within(0.0001f));
        Assert.That(boundingBox.Max.X, Is.EqualTo(expectedMaxX).Within(0.0001f));
        Assert.That(boundingBox.Max.Y, Is.EqualTo(expectedMaxY).Within(0.0001f));
    }

    [Test]
    public void HexMapGeometryBoundingBox_WhenMapIsEmpty_Throws()
    {
        var geometry = new HexMapGeometry(
            new HexMapTopology(0, 1, Layout.OddR),
            VectorXY.Zero,
            2f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => geometry.BoundingBox());

        Assert.That(exception!.ParamName, Is.EqualTo("geometry"));
    }

    [Test]
    public void HexMapTopologyBoundingBox_WithApothemAndOrigin_ReturnsSameRectangleAsGeometry()
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);
        var origin = new VectorXY(10f, 20f);
        const float apothem = 2f;

        Rectangle topologyBoundingBox = topology.BoundingBox(apothem, origin);
        Rectangle geometryBoundingBox = new HexMapGeometry(topology, origin, apothem).BoundingBox();

        Assert.That(topologyBoundingBox, Is.EqualTo(geometryBoundingBox));
    }

    [Test]
    public void HexMapGeometryToSpatialRasterGrid_UsesBoundingBoxAndPixelsPerApothem()
    {
        var geometry = new HexMapGeometry(
            width: 1,
            height: 1,
            origin: VectorXY.Zero,
            apothem: 2f,
            layout: Layout.OddR);

        SpatialRasterGrid grid = geometry.ToSpatialRasterGrid(pixelsPerApothem: 3f);

        Assert.That(grid.Origin.X, Is.EqualTo(-2f).Within(0.0001f));
        Assert.That(grid.Origin.Y, Is.EqualTo(-2.3094f).Within(0.0001f));
        Assert.That(grid.Size.X, Is.EqualTo(4f).Within(0.0001f));
        Assert.That(grid.Size.Y, Is.EqualTo(4.6188f).Within(0.0001f));
        Assert.That(grid.Resolution, Is.EqualTo(new VectorXYInt(6, 7)));
    }

    [Test]
    public void HexMapTopologyToSpatialRasterGrid_WithApothemAndOrigin_ReturnsSameGridAsGeometry()
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);
        var origin = new VectorXY(10f, 20f);
        const float apothem = 2f;
        const float pixelsPerApothem = 3f;

        SpatialRasterGrid topologyGrid = topology.ToSpatialRasterGrid(apothem, origin, pixelsPerApothem);
        SpatialRasterGrid geometryGrid = new HexMapGeometry(topology, origin, apothem).ToSpatialRasterGrid(pixelsPerApothem);

        Assert.That(topologyGrid, Is.EqualTo(geometryGrid));
    }

    [Test]
    public void HexMapTopologyToSpatialRasterGrid_WithMargin_ReturnsSameGridAsGeometry()
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);
        var origin = new VectorXY(10f, 20f);
        const float apothem = 2f;
        const float pixelsPerApothem = 3f;
        const float margin = 1.5f;

        SpatialRasterGrid topologyGrid = topology.ToSpatialRasterGrid(apothem, origin, pixelsPerApothem, margin);
        SpatialRasterGrid geometryGrid = new HexMapGeometry(topology, origin, apothem).ToSpatialRasterGrid(pixelsPerApothem, margin);

        Assert.That(topologyGrid, Is.EqualTo(geometryGrid));
    }

    [Test]
    public void HexMapGeometryToSpatialRasterGrid_WhenPixelsPerApothemIsInvalid_Throws()
    {
        var geometry = new HexMapGeometry(1, 1, VectorXY.Zero, 2f, Layout.OddR);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            geometry.ToSpatialRasterGrid(0f));

        Assert.That(exception!.ParamName, Is.EqualTo("pixelsPerApothem"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void HexMapGeometryToSpatialRasterGrid_WhenMarginIsInvalid_Throws(float margin)
    {
        var geometry = new HexMapGeometry(1, 1, VectorXY.Zero, 2f, Layout.OddR);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            geometry.ToSpatialRasterGrid(3f, margin));

        Assert.That(exception!.ParamName, Is.EqualTo("margin"));
    }

    [Test]
    public void HexMapGeometryToSpatialRasterGrid_WhenRasterResolutionDoesNotFitInt32_Throws()
    {
        var geometry = new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR);

        Assert.Throws<OverflowException>(() =>
            geometry.ToSpatialRasterGrid(float.MaxValue));
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
        var geometry = new HexCenterMap(1, 1, 2f.ConvertHexApothemToRadius(), layout);
        VectorAssert.AreEqual(geometry[0], expectedX, expectedY);
    }

    [Test]
    public void Constructor_WhenOriginIsNotFinite_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexCenterMap(1, 1, new VectorXY(float.PositiveInfinity, 0f), 2f, Layout.OddR));

        Assert.That(exception!.ParamName, Is.EqualTo("origin"));
    }

    [TestCase(0f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void Constructor_WhenApothemIsInvalid_Throws(float apothem)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexCenterMap(1, 1, VectorXY.Zero, apothem, Layout.OddR));

        Assert.That(exception!.ParamName, Is.EqualTo("apothem"));
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
            new HexCenterMap(1, 1, radius, Layout.OddR));

        Assert.That(exception!.ParamName, Is.EqualTo("radius"));
    }

    [Test]
    public void HexCenterMap_ImplementsIHexMap()
    {
        var source = new HexCenterMap(3, 2, VectorXY.Zero, 2f, Layout.OddR);
        IHexMap<PointXY> map = source;

        PointXY center = source[5];

        Assert.That(map.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(map.Layout, Is.EqualTo(Layout.OddR));
        Assert.That(map[new VectorXYInt(2, 1)], Is.EqualTo(center));
        Assert.That(map[5], Is.EqualTo(center));
    }

    [Test]
    public void Indexer_WhenIndexIsOutsideMap_Throws()
    {
        var geometry = new HexCenterMap(3, 2, VectorXY.Zero, 2f, Layout.OddR);

        Assert.Throws<IndexOutOfRangeException>(() => _ = geometry[new VectorXYInt(3, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = geometry[new VectorXYInt(0, 2)]);
    }
}
