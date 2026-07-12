using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Geometry.HexIndex;

public class HexGeometryArgumentValidationTests
{
    [TestCase(0f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void ConvertHexApothemToRadius_WhenApothemIsInvalid_Throws(float apothem)
    {
        AssertArgumentOutOfRange(() => _ = apothem.ConvertHexApothemToRadius(), "apothem");
    }

    [TestCase(0f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void ConvertHexRadiusToApothem_WhenRadiusIsInvalid_Throws(float radius)
    {
        AssertArgumentOutOfRange(() => _ = radius.ConvertHexRadiusToApothem(), "radius");
    }

    [Test]
    public void ParameterReconstruction_WhenArgumentsAreInvalid_Throws()
    {
        Assert.Multiple(() =>
        {
            AssertArgumentOutOfRange(
                () => _ = ParametersReconstructor.GetApothem(new VectorXY(float.PositiveInfinity, 1f), VectorXYInt.One, true),
                "size");
            AssertArgumentOutOfRange(
                () => _ = ParametersReconstructor.GetApothem(VectorXY.One, new VectorXYInt(0, 1), true),
                "dim");
            AssertArgumentOutOfRange(
                () => _ = ParametersReconstructor.GetDim(VectorXY.One, float.PositiveInfinity, true),
                "hexApothem");
            AssertArgumentOutOfRange(
                () => _ = ParametersReconstructor.GetDim(new VectorXY(float.PositiveInfinity, 1f), 1f, true),
                "landscapeMetricSize");
        });
    }

    [Test]
    public void GetApothem_WithLargeDimension_UsesWideDimensionArithmetic()
    {
        int width = int.MaxValue / 2 + 1;
        float actual = ParametersReconstructor.GetApothem(
            VectorXY.One,
            new VectorXYInt(width, 2),
            xOriented: true);

        float expected = (float)(1d / ((double)width * 2d + 1d));

        Assert.That(actual, Is.EqualTo(expected));
        Assert.That(actual, Is.GreaterThan(0f));
    }

    [Test]
    public void GetDim_WhenReconstructedDimensionDoesNotFitInt32_ThrowsOverflowException()
    {
        Assert.Throws<OverflowException>(() =>
            ParametersReconstructor.GetDim(
                new VectorXY(float.MaxValue, float.MaxValue),
                hexApothem: 1f,
                xOrientation: true));
    }

    [Test]
    public void HexLayoutHelpers_WhenApothemOrRadiusIsInvalid_Throw()
    {
        Assert.Multiple(() =>
        {
            AssertArgumentOutOfRange(
                () => _ = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexCenter(0, 0, float.PositiveInfinity, 1f, Layout.OddR),
                "hexApothem");
            AssertArgumentOutOfRange(
                () => _ = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexCenter(0, 0, 1f, float.PositiveInfinity, Layout.OddR),
                "hexRadius");
            AssertArgumentOutOfRange(
                () => _ = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexVertices(0, 0, float.PositiveInfinity, 1f, Layout.OddR),
                "hexApothem");
            AssertArgumentOutOfRange(
                () => _ = VectorXY.Zero.GetHexVertices(float.PositiveInfinity, Layout.OddR),
                "hexRadius");
            AssertArgumentOutOfRange(
                () => _ = VectorXYInt.Zero.GetHexCenter(float.PositiveInfinity, 1f, Layout.OddR),
                "hexApothem");
            AssertArgumentOutOfRange(
                () => _ = VectorXYInt.Zero.GetHexCenter(1f, float.PositiveInfinity, Layout.OddR),
                "hexRadius");
            AssertArgumentOutOfRange(
                () => _ = VectorXYInt.Zero.GetHexCenter(1f, 1f, new VectorXY(float.PositiveInfinity, 0f), Layout.OddR),
                "origin");
            AssertArgumentOutOfRange(
                () => _ = new VectorQRSInt(1, 1).GetHexOffset(float.PositiveInfinity, 1f, Layout.OddR),
                "hexApothem");
        });
    }

    [Test]
    public void RadiusBasedCoordinateHelpers_WhenRadiusOrOriginIsInvalid_Throw()
    {
        Assert.Multiple(() =>
        {
            AssertArgumentOutOfRange(
                () => _ = new VectorQRS(1f, 2f).ToNormalizedAxial(float.PositiveInfinity),
                "hexRadius");
            AssertArgumentOutOfRange(
                () => _ = new PointXY(0f, 0f).ToXYIndex(float.PositiveInfinity, VectorXY.Zero, Layout.OddR),
                "hexRadius");
            AssertArgumentOutOfRange(
                () => _ = new PointXY(0f, 0f).ToXYIndex(1f, new VectorXY(float.PositiveInfinity, 0f), Layout.OddR),
                "hexFieldOrigin");
            AssertArgumentOutOfRange(
                () => _ = new PointXY(float.PositiveInfinity, 0f).ToXYIndex(1f, VectorXY.Zero, Layout.OddR),
                "point");
        });
    }

    [Test]
    public void VertexProximityHelpers_WhenGeometryIsInvalid_Throw()
    {
        Assert.Multiple(() =>
        {
            AssertArgumentOutOfRange(
                () => _ = new PointXY(float.PositiveInfinity, 0f).GetClosestVertexIndex(1f, VectorXY.Zero, Layout.OddR),
                "point");
            AssertArgumentOutOfRange(
                () => _ = new PointXY(0f, 0f).GetClosestVertexIndex(0f, VectorXY.Zero, Layout.OddR),
                "radius");
            AssertArgumentOutOfRange(
                () => _ = new PointXY(0f, 0f).GetClosestVertexIndex(float.PositiveInfinity, VectorXY.Zero, Layout.OddR),
                "radius");
            AssertArgumentOutOfRange(
                () => _ = new PointXY(0f, 0f).GetClosestVertexIndex(1f, new VectorXY(float.PositiveInfinity, 0f), Layout.OddR),
                "hexCenter");
            AssertArgumentOutOfRange(
                () => _ = new PointXY(0f, 0f).GetClosestHexVertexIndex(0f, VectorXY.Zero, Layout.OddR),
                "radius");
            AssertArgumentOutOfRange(
                () => _ = new PointXY(0f, 0f).GetClosestHexVertexIndex(1f, new VectorXY(float.PositiveInfinity, 0f), Layout.OddR),
                "hexFieldOrigin");
        });
    }

    [Test]
    public void GetClosestHexVertexIndex_DerivesApothemFromRadius()
    {
        var result = new PointXY(0f, 0f).GetClosestHexVertexIndex(2f, VectorXY.Zero, Layout.OddR);

        Assert.That(result.hexIndex, Is.EqualTo(VectorXYInt.Zero));
        Assert.That(result.hexVertex, Is.EqualTo(HexVertex.Vertex0));
    }

    private static void AssertArgumentOutOfRange(TestDelegate action, string parameterName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(action);

        Assert.That(exception!.ParamName, Is.EqualTo(parameterName));
    }
}
