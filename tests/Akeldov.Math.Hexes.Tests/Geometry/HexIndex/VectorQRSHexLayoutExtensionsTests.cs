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

    [TestCase(Layout.OddR, 6.0622f, 4.5f)]
    [TestCase(Layout.EvenR, 6.0622f, 4.5f)]
    [TestCase(Layout.OddQ, 3f, 6.9282f)]
    [TestCase(Layout.EvenQ, 3f, 6.9282f)]
    public void ToVectorQRS_WithLayout_UsesNormalizedQrAxes(Layout layout, float x, float y)
    {
        VectorQRS actual = new VectorXY(x, y).ToVectorQRS(layout);

        VectorAssert.AreEqual(actual, 2f, 3f);
    }

    [TestCase(Layout.OddR, 14f, 10.3923f)]
    [TestCase(Layout.EvenR, 14f, 10.3923f)]
    [TestCase(Layout.OddQ, 6.9282f, 16f)]
    [TestCase(Layout.EvenQ, 6.9282f, 16f)]
    public void ToVectorQRS_WithGeometry_UsesGeometryScaleAndLayout(Layout layout, float x, float y)
    {
        var geometry = new HexMapGeometry(4, 3, new VectorXY(10f, 20f), 2f, layout);

        VectorQRS actual = new VectorXY(x, y).ToVectorQRS(geometry);

        VectorAssert.AreEqual(actual, 2f, 3f);
    }

    [Test]
    public void ToVectorQRS_WithGeometry_DoesNotApplyOrigin()
    {
        var zeroOriginGeometry = new HexMapGeometry(4, 3, VectorXY.Zero, 2f, Layout.OddR);
        var shiftedOriginGeometry = new HexMapGeometry(4, 3, new VectorXY(10f, 20f), 2f, Layout.OddR);
        var vector = new VectorXY(14f, 10.3923f);

        VectorQRS zeroOriginResult = vector.ToVectorQRS(zeroOriginGeometry);
        VectorQRS shiftedOriginResult = vector.ToVectorQRS(shiftedOriginGeometry);

        Assert.That(shiftedOriginResult, Is.EqualTo(zeroOriginResult));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToVectorQRS_WithLayout_RoundTripsToVectorXY(Layout layout)
    {
        var expected = new VectorQRS(-2.5f, 3.25f);

        VectorQRS actual = expected.ToVectorXY(layout).ToVectorQRS(layout);

        VectorAssert.AreEqual(actual, expected.Q, expected.R);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToVectorQRS_WithGeometry_RoundTripsToVectorXY(Layout layout)
    {
        var geometry = new HexMapGeometry(4, 3, new VectorXY(10f, 20f), 2f, layout);
        var expected = new VectorQRS(-2.5f, 3.25f);

        VectorQRS actual = expected.ToVectorXY(geometry).ToVectorQRS(geometry);

        VectorAssert.AreEqual(actual, expected.Q, expected.R);
    }

    [Test]
    public void ToVectorQRS_WithInvalidLayout_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorXY(1f, 2f).ToVectorQRS((Layout)42));
    }

    [TestCase(float.NaN, 0f)]
    [TestCase(float.PositiveInfinity, 0f)]
    public void ToVectorQRS_WithNonFiniteVector_Throws(float x, float y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorXY(x, y).ToVectorQRS(Layout.OddR));
    }

    [Test]
    public void ToVectorQRS_WithInvalidGeometry_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorXY(1f, 2f).ToVectorQRS(default(HexMapGeometry)));
    }
}
