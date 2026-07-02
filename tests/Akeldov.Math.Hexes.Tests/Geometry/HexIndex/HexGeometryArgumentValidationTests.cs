using Akeldov.Math.Hexes.Geometry;
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
    public void BoundingBoxHelpers_WhenApothemOrRadiusIsInvalid_Throw()
    {
        Assert.Multiple(() =>
        {
            AssertArgumentOutOfRange(
                () => _ = new VectorXYInt(1, 1).BoundingBox(float.PositiveInfinity, 1f, Layout.OddR),
                "hexApothem");
            AssertArgumentOutOfRange(
                () => _ = new VectorXYInt(1, 1).BoundingBox(1f, float.PositiveInfinity, Layout.OddR),
                "hexRadius");
            AssertArgumentOutOfRange(
                () => _ = new VectorQRSInt(1, 1).BoundingBoxSize(float.PositiveInfinity, 1f, Layout.OddR),
                "apothem");
            AssertArgumentOutOfRange(
                () => _ = new VectorQRSInt(1, 1).BoundingBoxSize(1f, float.PositiveInfinity, Layout.OddR),
                "radius");
        });
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
                () => _ = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexVertexes(0, 0, float.PositiveInfinity, 1f, Layout.OddR),
                "hexApothem");
            AssertArgumentOutOfRange(
                () => _ = VectorXY.Zero.GetHexVertexes(float.PositiveInfinity, Layout.OddR),
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

    private static void AssertArgumentOutOfRange(TestDelegate action, string parameterName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(action);

        Assert.That(exception!.ParamName, Is.EqualTo(parameterName));
    }
}
