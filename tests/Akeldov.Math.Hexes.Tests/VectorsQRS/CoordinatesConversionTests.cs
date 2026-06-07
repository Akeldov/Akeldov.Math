using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.VectorsQRS;

public class CoordinatesConversionTests
{
    [TestCase(Layout.OddR, 0.46410155f, 6f)]
    [TestCase(Layout.EvenR, 0.46410155f, 6f)]
    [TestCase(Layout.OddQ, 4f, 3.1961522f)]
    [TestCase(Layout.EvenQ, 4f, 3.1961522f)]
    public void ToQRS_UsesOriginAndLayoutOrientation(Layout layout, float expectedQ, float expectedR)
    {
        var point = new PointXY(7f, 11f);
        var origin = new VectorXY(1f, 2f);

        var qrs = point.ToQRS(origin, layout);

        VectorAssert.AreEqual(qrs, expectedQ, expectedR);
    }

    [Test]
    public void ToQRS_ThrowsForInvalidLayout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new PointXY(1f, 2f).ToQRS(VectorXY.Zero, (Layout)42));
    }

    [Test]
    public void ToNormalizedAxial_DividesByHexRadius()
    {
        var normalized = new VectorQRS(6f, -9f).ToNormalizedAxial(3f);

        Assert.That(normalized, Is.EqualTo(new VectorQRS(2f, -3f)));
    }

    [Test]
    public void ToNormalizedAxial_ThrowsWhenHexRadiusIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorQRS(1f, 2f).ToNormalizedAxial(0f));
    }

    [Test]
    public void ToQRSIndex_ThrowsForInvalidLayout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new VectorXYInt(1, 2).ToQRSIndex((Layout)42));
    }

    [Test]
    public void ToXYIndex_ThrowsForInvalidLayout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new VectorQRSInt(1, 2).ToXYIndex((Layout)42));
    }
}
