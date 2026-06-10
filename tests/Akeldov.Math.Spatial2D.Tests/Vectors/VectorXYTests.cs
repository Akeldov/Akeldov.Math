using System.Globalization;

namespace Akeldov.Math.Spatial2D.Tests.Vectors;

public class VectorXYTests
{
    [Test]
    public void Normalize_WhenVectorHasLength_ReturnsUnitVector()
    {
        var vector = new VectorXY(3f, 4f);

        var normalized = vector.Normalize();

        Assert.That(normalized.Length, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(normalized.X, Is.EqualTo(0.6f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(normalized.Y, Is.EqualTo(0.8f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Normalize_WhenVectorIsZero_ReturnsZero()
    {
        var normalized = VectorXY.Zero.Normalize();

        Assert.That(normalized, Is.EqualTo(VectorXY.Zero));
    }

    [Test]
    public void Angle_ReturnsSignedAngleFromFirstVectorToSecondVector()
    {
        var angle = VectorXY.Angle(new VectorXY(1f, 0f), new VectorXY(0f, 1f));

        Assert.That(angle, Is.EqualTo(MathF.PI / 2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Angle_WhenRotationIsClockwise_ReturnsNegativeAngle()
    {
        var angle = VectorXY.Angle(new VectorXY(0f, 1f), new VectorXY(1f, 0f));

        Assert.That(angle, Is.EqualTo(-MathF.PI / 2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Rotate_WhenAngleIsInvalid_Throws(float angle)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new VectorXY(1f, 0f).Rotate(angle));

        Assert.That(exception!.ParamName, Is.EqualTo("angle"));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void RotateAroundPivot_WhenAngleIsInvalid_Throws(float angle)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VectorXY(1f, 0f).Rotate(VectorXY.Zero, angle));

        Assert.That(exception!.ParamName, Is.EqualTo("angle"));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Transform_WhenAngleIsInvalid_Throws(float angle)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VectorXY(1f, 0f).Transform(angle, VectorXY.Zero));

        Assert.That(exception!.ParamName, Is.EqualTo("angle"));
    }

    [Test]
    public void Equals_WhenComponentsDifferWithinGeometryEpsilon_ReturnsFalse()
    {
        var left = new VectorXY(1f, 2f);
        var right = new VectorXY(1f + GeometryConstants.GeometryEpsilon / 2f, 2f);

        Assert.That(left.Equals(right), Is.False);
        Assert.That(left == right, Is.False);
        Assert.That(left.AlmostEquals(right), Is.True);
    }

    [Test]
    public void AlmostEquals_WhenEuclideanDistanceExceedsEpsilon_ReturnsFalse()
    {
        var left = VectorXY.Zero;
        var right = new VectorXY(GeometryConstants.GeometryEpsilon, GeometryConstants.GeometryEpsilon);

        Assert.That(left.AlmostEquals(right), Is.False);
    }

    [Test]
    public void IsFinite_WhenComponentsAreFinite_ReturnsTrue()
    {
        Assert.That(new VectorXY(1f, -2f).IsFinite, Is.True);
    }

    [TestCase(float.NaN, 0f)]
    [TestCase(0f, float.NaN)]
    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void IsFinite_WhenComponentIsNaNOrInfinity_ReturnsFalse(float x, float y)
    {
        Assert.That(new VectorXY(x, y).IsFinite, Is.False);
    }

    [Test]
    public void RoundToInt_UsesMathFRoundMidpointSemantics()
    {
        var vector = new VectorXY(2.5f, 3.5f);

        var rounded = vector.RoundToInt();

        Assert.That(rounded, Is.EqualTo(new VectorXYInt(2, 4)));
    }

    [Test]
    public void ToString_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        try
        {
            Assert.That(new VectorXY(1.5f, 2.25f).ToString(), Is.EqualTo("(1.5, 2.25)"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
