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
    public void ToVectorXY_WithLayout_UsesUnitRadiusQMinusSAndRMinusSNeighborBasis(
        Layout layout,
        float expectedX,
        float expectedY)
    {
        VectorXY actual = new VectorQRS(2f, 3f).ToVectorXY(layout);

        VectorAssert.AreEqual(actual, expectedX, expectedY);
    }

    [TestCase(Layout.OddR, 1.7321f, 0f, 0.866f, 1.5f)]
    [TestCase(Layout.EvenR, 1.7321f, 0f, 0.866f, 1.5f)]
    [TestCase(Layout.OddQ, 1.5f, 0.866f, 0f, 1.7321f)]
    [TestCase(Layout.EvenQ, 1.5f, 0.866f, 0f, 1.7321f)]
    public void ToVectorXY_WithLayout_MapsQMinusSAndRMinusSNeighborStepsToUnitRadiusCenterOffsets(
        Layout layout,
        float expectedQMinusSX,
        float expectedQMinusSY,
        float expectedRMinusSX,
        float expectedRMinusSY)
    {
        VectorXY qMinusSNeighborOffset = new VectorQRS(1f, 0f).ToVectorXY(layout);
        VectorXY rMinusSNeighborOffset = new VectorQRS(0f, 1f).ToVectorXY(layout);

        VectorAssert.AreEqual(qMinusSNeighborOffset, expectedQMinusSX, expectedQMinusSY);
        VectorAssert.AreEqual(rMinusSNeighborOffset, expectedRMinusSX, expectedRMinusSY);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToVectorXY_WithLayout_MapsEveryNeighborStepToCenterDistance(Layout layout)
    {
        var neighborSteps = new[]
        {
            new VectorQRS(1f, 0f),
            new VectorQRS(0f, 1f),
            new VectorQRS(-1f, 1f),
            new VectorQRS(-1f, 0f),
            new VectorQRS(0f, -1f),
            new VectorQRS(1f, -1f)
        };

        foreach (VectorQRS neighborStep in neighborSteps)
        {
            Assert.That(
                neighborStep.ToVectorXY(layout).Length,
                Is.EqualTo(MathF.Sqrt(3f)).Within(VectorAssert.Epsilon));
        }
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

    [TestCase(Layout.OddR, 6.0622f, 4.5f)]
    [TestCase(Layout.EvenR, 6.0622f, 4.5f)]
    [TestCase(Layout.OddQ, 3f, 6.9282f)]
    [TestCase(Layout.EvenQ, 3f, 6.9282f)]
    public void ToVectorQRS_WithLayout_UsesUnitRadiusQMinusSAndRMinusSNeighborBasis(
        Layout layout,
        float x,
        float y)
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
