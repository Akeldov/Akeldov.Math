using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.VectorsQRS;

public class DiscretizationTests
{
    private const float Sqrt3 = 1.7320508f;

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void FractionalQRS_ToQRSIndex_RoundsToNearestHex(Layout layout)
    {
        var index = new VectorQRS(1.2f, -2.3f).ToQRSIndex(layout);

        Assert.That(index, Is.EqualTo(new VectorQRSInt(1, -2)));
    }

    [Test]
    public void FractionalQRS_ToQRSIndex_ThrowsForInvalidLayout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorQRS(1f, 2f).ToQRSIndex((Layout)42));
    }

    [TestCase(Layout.OddR, 2, 3)]
    [TestCase(Layout.EvenR, 2, 3)]
    [TestCase(Layout.OddQ, 2, 3)]
    [TestCase(Layout.EvenQ, 2, 3)]
    [TestCase(Layout.OddR, -2, -3)]
    [TestCase(Layout.EvenR, -2, -3)]
    [TestCase(Layout.OddQ, -2, -3)]
    [TestCase(Layout.EvenQ, -2, -3)]
    public void XYPoint_ToXYIndex_ReturnsHexIndexAtHexCenter(Layout layout, int x, int y)
    {
        var origin = new VectorXY(10f, -20f);
        const float radius = 3f;
        var expectedIndex = new VectorXYInt(x, y);
        var qrsIndex = expectedIndex.ToQRSIndex(layout);
        PointXY point = GetHexCenter(qrsIndex, layout, origin, radius);

        var actualIndex = point.ToXYIndex(radius, origin, layout);

        Assert.That(actualIndex, Is.EqualTo(expectedIndex));
    }

    [Test]
    public void XYPoint_ToXYIndex_ThrowsWhenHexRadiusIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new PointXY(0f, 0f).ToXYIndex(0f, VectorXY.Zero, Layout.OddR));
    }

    [Test]
    public void XYPoint_ToXYIndex_ThrowsForInvalidLayout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new PointXY(0f, 0f).ToXYIndex(1f, VectorXY.Zero, (Layout)42));
    }

    private static PointXY GetHexCenter(VectorQRSInt qrs, Layout layout, VectorXY origin, float radius)
    {
        VectorXY center = layout.IsPointyTop()
            ? origin + new VectorXY(Sqrt3 * radius * (qrs.Q + qrs.R / 2f), 1.5f * radius * qrs.R)
            : origin + new VectorXY(1.5f * radius * qrs.Q, Sqrt3 * radius * (qrs.R + qrs.Q / 2f));

        return new PointXY(center.X, center.Y);
    }
}
