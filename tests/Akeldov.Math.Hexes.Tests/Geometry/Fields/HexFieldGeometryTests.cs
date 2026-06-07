using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Hexes.Tests.VectorsQRS;

namespace Akeldov.Math.Hexes.Tests.Geometry.Fields;

public class HexFieldGeometryTests
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
            Assert.That(geometry.Origin, Is.EqualTo(origin));
            Assert.That(geometry.Apothem, Is.EqualTo(apothem));
            Assert.That(geometry.Layout, Is.EqualTo(layout));
            VectorAssert.AreEqual(geometry.Centers[0], origin.X, origin.Y);
        }
    }

    [TestCase(Layout.OddR, 2f, 2.3094f)]
    [TestCase(Layout.EvenR, 6f, 2.3094f)]
    [TestCase(Layout.OddQ, 2.3094f, 2f)]
    [TestCase(Layout.EvenQ, 2.3094f, 6f)]
    public void Constructor_WithoutOrigin_PreservesDefaultZeroHexCenter(Layout layout, float expectedX, float expectedY)
    {
        var geometry = new HexCenterMap(1, 1, 2f.ConvertHexApothemToRadius(), layout);
        VectorAssert.AreEqual(geometry.Centers[0], expectedX, expectedY);
    }

    [Test]
    public void HexCenterMap_ImplementsIHexMap()
    {
        var source = new HexCenterMap(3, 2, VectorXY.Zero, 2f, Layout.OddR);
        IHexMap<VectorXY> map = source;

        VectorXY center = source.Centers[5];

        Assert.That(map.Width, Is.EqualTo(3));
        Assert.That(map.Height, Is.EqualTo(2));
        Assert.That(map[new VectorXYInt(2, 1)], Is.EqualTo(center));
        Assert.That(map[5], Is.EqualTo(center));
    }

    [Test]
    public void Indexer_WhenIndexIsOutsideGeometry_Throws()
    {
        var geometry = new HexCenterMap(3, 2, VectorXY.Zero, 2f, Layout.OddR);

        Assert.Throws<IndexOutOfRangeException>(() => _ = geometry[new VectorXYInt(3, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = geometry[new VectorXYInt(0, 2)]);
    }
}
