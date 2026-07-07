using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Tests.VectorsQRS;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Geometry.HexIndex;

public class VectorQRSHexLayoutExtensionsTests
{
    [TestCase(Layout.OddR, 6.0622f, 4.5f)]
    [TestCase(Layout.EvenR, 6.0622f, 4.5f)]
    [TestCase(Layout.OddQ, 3f, 6.9282f)]
    [TestCase(Layout.EvenQ, 3f, 6.9282f)]
    public void ToVectorXY_WithLayout_UsesNormalizedQrAxes(Layout layout, float expectedX, float expectedY)
    {
        VectorXY actual = new VectorQRS(2f, 3f).ToVectorXY(layout);

        VectorAssert.AreEqual(actual, expectedX, expectedY);
    }

    [TestCase(Layout.OddR, 14f, 10.3923f)]
    [TestCase(Layout.EvenR, 14f, 10.3923f)]
    [TestCase(Layout.OddQ, 6.9282f, 16f)]
    [TestCase(Layout.EvenQ, 6.9282f, 16f)]
    public void ToVectorXY_WithGeometry_UsesGeometryScaleAndLayout(Layout layout, float expectedX, float expectedY)
    {
        var geometry = new HexMapGeometry(4, 3, new VectorXY(10f, 20f), 2f, layout);

        VectorXY actual = new VectorQRS(2f, 3f).ToVectorXY(geometry);

        VectorAssert.AreEqual(actual, expectedX, expectedY);
    }

    [Test]
    public void ToVectorXY_WithGeometry_DoesNotApplyOrigin()
    {
        var zeroOriginGeometry = new HexMapGeometry(4, 3, VectorXY.Zero, 2f, Layout.OddR);
        var shiftedOriginGeometry = new HexMapGeometry(4, 3, new VectorXY(10f, 20f), 2f, Layout.OddR);
        var vector = new VectorQRS(2f, 3f);

        VectorXY zeroOriginResult = vector.ToVectorXY(zeroOriginGeometry);
        VectorXY shiftedOriginResult = vector.ToVectorXY(shiftedOriginGeometry);

        Assert.That(shiftedOriginResult, Is.EqualTo(zeroOriginResult));
    }

    [Test]
    public void ToVectorXY_WithInvalidLayout_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorQRS(1f, 2f).ToVectorXY((Layout)42));
    }

    [TestCase(float.NaN, 0f)]
    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(float.MaxValue, float.MaxValue)]
    public void ToVectorXY_WithNonFiniteVector_Throws(float q, float r)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorQRS(q, r).ToVectorXY(Layout.OddR));
    }

    [Test]
    public void ToVectorXY_WithInvalidGeometry_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorQRS(1f, 2f).ToVectorXY(default(HexMapGeometry)));
    }
}
