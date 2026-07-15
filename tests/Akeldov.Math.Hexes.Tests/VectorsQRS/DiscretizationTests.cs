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

    [TestCase(Layout.OddR, float.NaN, 0f)]
    [TestCase(Layout.OddQ, float.NaN, 0f)]
    [TestCase(Layout.OddR, float.PositiveInfinity, 0f)]
    [TestCase(Layout.OddQ, 0f, float.PositiveInfinity)]
    public void FractionalQRS_ToQRSIndex_WhenComponentIsNotFinite_Throws(Layout layout, float q, float r)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _ = new VectorQRS(q, r).ToQRSIndex(layout));

        Assert.That(exception!.ParamName, Is.EqualTo("axialPoint"));
    }

    [TestCase(Layout.OddR, 3_000_000_000f, 0f)]
    [TestCase(Layout.OddQ, 3_000_000_000f, 0f)]
    [TestCase(Layout.OddR, 2_000_000_000f, 2_000_000_000f)]
    [TestCase(Layout.OddQ, 2_000_000_000f, 2_000_000_000f)]
    public void FractionalQRS_ToQRSIndex_WhenRoundedComponentDoesNotFitInt32_Throws(Layout layout, float q, float r)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _ = new VectorQRS(q, r).ToQRSIndex(layout));

        Assert.That(exception!.ParamName, Is.EqualTo("axialPoint"));
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

    [TestCase(Layout.OddR, 12.25f, -17.75f)]
    [TestCase(Layout.EvenR, 6.5f, -25.25f)]
    [TestCase(Layout.OddQ, 15.75f, -13.5f)]
    [TestCase(Layout.EvenQ, 4.25f, -22.5f)]
    public void LayoutSpecificXYIndexConversion_MatchesGeneralConversion(Layout layout, float x, float y)
    {
        var point = new PointXY(x, y);
        var origin = new VectorXY(10f, -20f);
        const float radius = 3f;

        VectorXYInt actualIndex = layout switch
        {
            Layout.OddR => point.ToOddRXYIndex(radius, origin),
            Layout.EvenR => point.ToEvenRXYIndex(radius, origin),
            Layout.OddQ => point.ToOddQXYIndex(radius, origin),
            Layout.EvenQ => point.ToEvenQXYIndex(radius, origin),
            _ => throw new ArgumentOutOfRangeException(nameof(layout))
        };

        Assert.That(actualIndex, Is.EqualTo(point.ToXYIndex(radius, origin, layout)));
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
