using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Tests.VectorsQRS;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Geometry.HexIndex;

public class VectorQRSHexLayoutExtensionsTests
{
    [TestCase(Layout.OddR, 3.5f, 2.5981f)]
    [TestCase(Layout.EvenR, 3.5f, 2.5981f)]
    [TestCase(Layout.OddQ, 1.7321f, 4f)]
    [TestCase(Layout.EvenQ, 1.7321f, 4f)]
    public void ToVectorXY_WithLayout_UsesNormalizedQrAxes(Layout layout, float expectedX, float expectedY)
    {
        VectorXY actual = new VectorQRS(2f, 3f).ToVectorXY(layout);

        VectorAssert.AreEqual(actual, expectedX, expectedY);
    }

    [TestCase(Layout.OddR, 1f, 0f, 0.5f, 0.866f)]
    [TestCase(Layout.EvenR, 1f, 0f, 0.5f, 0.866f)]
    [TestCase(Layout.OddQ, 0.866f, 0.5f, 0f, 1f)]
    [TestCase(Layout.EvenQ, 0.866f, 0.5f, 0f, 1f)]
    public void ToVectorXY_WithLayout_MapsUnitQrAxesToUnitXyVectors(
        Layout layout,
        float expectedQX,
        float expectedQY,
        float expectedRX,
        float expectedRY)
    {
        VectorXY qAxis = new VectorQRS(1f, 0f).ToVectorXY(layout);
        VectorXY rAxis = new VectorQRS(0f, 1f).ToVectorXY(layout);

        VectorAssert.AreEqual(qAxis, expectedQX, expectedQY);
        VectorAssert.AreEqual(rAxis, expectedRX, expectedRY);
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

    [TestCase(Layout.OddR, 3.5f, 2.5981f)]
    [TestCase(Layout.EvenR, 3.5f, 2.5981f)]
    [TestCase(Layout.OddQ, 1.7321f, 4f)]
    [TestCase(Layout.EvenQ, 1.7321f, 4f)]
    public void ToVectorQRS_WithLayout_UsesNormalizedQrAxes(Layout layout, float x, float y)
    {
        VectorQRS actual = new VectorXY(x, y).ToVectorQRS(layout);

        VectorAssert.AreEqual(actual, 2f, 3f);
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
}
