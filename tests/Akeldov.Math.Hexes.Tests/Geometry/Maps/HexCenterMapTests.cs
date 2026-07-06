using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Hexes.Tests.VectorsQRS;

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

        Assert.That(map.Width, Is.EqualTo(3));
        Assert.That(map.Height, Is.EqualTo(2));
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
