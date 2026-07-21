using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Tests.VectorsQRS;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Geometry.HexIndex;

public class VectorXYIntHexLayoutExtensionsTests
{
    [Test]
    public void GetHexCenter_WithOrigin_UsesOriginAsZeroHexCenter_ForEveryLayout()
    {
        var origin = new VectorXY(10f, 20f);
        float radius = 2f.ConvertHexApothemToRadius();

        foreach (Layout layout in Enum.GetValues(typeof(Layout)))
        {
            VectorXY center = VectorXYInt.Zero.GetHexCenter(radius, origin, layout);

            VectorAssert.AreEqual(center, origin.X, origin.Y);
        }
    }

    [TestCase(Layout.OddR, 0, 1, 12f, 23.4641f)]
    [TestCase(Layout.EvenR, 0, 1, 8f, 23.4641f)]
    [TestCase(Layout.OddQ, 1, 0, 13.4641f, 22f)]
    [TestCase(Layout.EvenQ, 1, 0, 13.4641f, 18f)]
    public void GetHexCenter_WithOrigin_OffsetsShiftedAxesRelativeToZeroHexCenter(
        Layout layout,
        int x,
        int y,
        float expectedX,
        float expectedY)
    {
        var origin = new VectorXY(10f, 20f);
        float radius = 2f.ConvertHexApothemToRadius();

        VectorXY center = new VectorXYInt(x, y).GetHexCenter(radius, origin, layout);

        VectorAssert.AreEqual(center, expectedX, expectedY);
    }

    [TestCase(Layout.OddR, 2f, 2.3094f)]
    [TestCase(Layout.EvenR, 2f, 2.3094f)]
    [TestCase(Layout.OddQ, 2.3094f, 2f)]
    [TestCase(Layout.EvenQ, 2.3094f, 2f)]
    public void GetHexCenter_WithoutOrigin_PreservesDefaultZeroHexCenter(
        Layout layout,
        float expectedX,
        float expectedY)
    {
        float radius = 2f.ConvertHexApothemToRadius();

        VectorXY center = VectorXYInt.Zero.GetHexCenter(radius, layout);

        VectorAssert.AreEqual(center, expectedX, expectedY);
    }

    [TestCase(Layout.OddR, 0, 0)]
    [TestCase(Layout.OddR, 1, 0)]
    [TestCase(Layout.OddR, 0, 1)]
    [TestCase(Layout.OddR, -2, -3)]
    [TestCase(Layout.EvenR, 0, 0)]
    [TestCase(Layout.EvenR, 1, 0)]
    [TestCase(Layout.EvenR, 0, 1)]
    [TestCase(Layout.EvenR, -2, -3)]
    [TestCase(Layout.OddQ, 0, 0)]
    [TestCase(Layout.OddQ, 1, 0)]
    [TestCase(Layout.OddQ, 0, 1)]
    [TestCase(Layout.OddQ, -2, -3)]
    [TestCase(Layout.EvenQ, 0, 0)]
    [TestCase(Layout.EvenQ, 1, 0)]
    [TestCase(Layout.EvenQ, 0, 1)]
    [TestCase(Layout.EvenQ, -2, -3)]
    public void GetHexCenter_WithoutOrigin_MatchesQrsPathForEveryLayout(Layout layout, int q, int r)
    {
        float radius = 2f.ConvertHexApothemToRadius();
        VectorXY expected = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexCenter(q, r, radius, layout);

        VectorXY actual = new VectorQRSInt(q, r)
            .ToXYIndex(layout)
            .GetHexCenter(radius, layout);

        VectorAssert.AreEqual(actual, expected.X, expected.Y);
    }
}
