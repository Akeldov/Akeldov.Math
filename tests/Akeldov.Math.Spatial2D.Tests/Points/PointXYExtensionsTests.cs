namespace Akeldov.Math.Spatial2D.Tests.Points;

public class PointXYExtensionsTests
{
    [Test]
    public void LerpTo_WhenParameterIsBetweenZeroAndOne_ReturnsInterpolatedPoint()
    {
        var source = new PointXY(1f, 2f);
        var target = new PointXY(5f, 10f);

        PointXY point = source.LerpTo(target, 0.25f);

        Assert.That(point, Is.EqualTo(new PointXY(2f, 4f)));
    }

    [TestCase(0f, 1f, 2f)]
    [TestCase(1f, 5f, 10f)]
    public void LerpTo_WhenParameterIsZeroOrOne_ReturnsEndpoint(
        float t,
        float expectedX,
        float expectedY)
    {
        var source = new PointXY(1f, 2f);
        var target = new PointXY(5f, 10f);

        PointXY point = source.LerpTo(target, t);

        Assert.That(point, Is.EqualTo(new PointXY(expectedX, expectedY)));
    }

    [TestCase(-0.25f, 0f, 0f)]
    [TestCase(1.25f, 6f, 12f)]
    public void LerpTo_WhenParameterIsOutsideZeroToOne_ReturnsExtrapolatedPoint(
        float t,
        float expectedX,
        float expectedY)
    {
        var source = new PointXY(1f, 2f);
        var target = new PointXY(5f, 10f);

        PointXY point = source.LerpTo(target, t);

        Assert.That(point, Is.EqualTo(new PointXY(expectedX, expectedY)));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void LerpTo_WhenParameterIsInvalid_Throws(float t)
    {
        var source = new PointXY(1f, 2f);
        var target = new PointXY(5f, 10f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            source.LerpTo(target, t));

        Assert.That(exception!.ParamName, Is.EqualTo("t"));
    }

    [Test]
    public void SquaredDistanceTo_ReturnsSquaredDistanceBetweenPoints()
    {
        var source = new PointXY(1f, 2f);
        var target = new PointXY(4f, 6f);

        Assert.That(source.SquaredDistanceTo(target), Is.EqualTo(25f));
    }

    [Test]
    public void AlmostEquals_WhenPointsAreWithinEuclideanDistanceTolerance_ReturnsTrue()
    {
        var source = new PointXY(1f, 2f);
        var target = new PointXY(1f + GeometryConstants.GeometryEpsilon / 2f, 2f);

        Assert.That(source.AlmostEquals(target), Is.True);
    }

    [Test]
    public void AlmostEquals_WhenEuclideanDistanceExceedsTolerance_ReturnsFalse()
    {
        var source = new PointXY(1f, 2f);
        var target = new PointXY(
            1f + GeometryConstants.GeometryEpsilon,
            2f + GeometryConstants.GeometryEpsilon);

        Assert.That(source.AlmostEquals(target), Is.False);
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void RotateAroundPivot_WhenAngleIsInvalid_Throws(float angle)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PointXY(1f, 0f).Rotate(new PointXY(0f, 0f), angle));

        Assert.That(exception!.ParamName, Is.EqualTo("angle"));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void RotateAroundIntegerPivot_WhenAngleIsInvalid_Throws(float angle)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PointXY(1f, 0f).Rotate(VectorXYInt.Zero, angle));

        Assert.That(exception!.ParamName, Is.EqualTo("angle"));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Transform_WhenAngleIsInvalid_Throws(float angle)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PointXY(1f, 0f).Transform(angle, VectorXY.Zero));

        Assert.That(exception!.ParamName, Is.EqualTo("angle"));
    }

    [Test]
    public void TransformOverloads_ReturnExpectedPoints()
    {
        var point = new PointXY(2f, 0f);
        var offset = new VectorXY(10f, 20f);
        var intOffset = new VectorXYInt(10, 20);

        Assert.Multiple(() =>
        {
            AssertPoint(point.Transform(MathF.PI / 3f, offset), 11f, 21.73205f);
            AssertPoint(point.Transform(MathF.PI / 3f, intOffset), 11f, 21.73205f);
            AssertPoint(point.Transform(2f, MathF.PI / 3f, offset), 12f, 23.464102f);
            AssertPoint(point.Transform(2f, MathF.PI / 3f, intOffset), 12f, 23.464102f);
        });
    }

    [Test]
    public void RotateAroundPivotOverloads_ReturnExpectedPoints()
    {
        var point = new PointXY(3f, 1f);
        var pivot = new PointXY(1f, 1f);
        var intPivot = new VectorXYInt(1, 1);

        Assert.Multiple(() =>
        {
            AssertPoint(point.Rotate(pivot, MathF.PI / 3f), 2f, 2.7320508f);
            AssertPoint(point.Rotate(intPivot, MathF.PI / 3f), 2f, 2.7320508f);
        });
    }

    private static void AssertPoint(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(0.0001f));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(0.0001f));
    }
}
