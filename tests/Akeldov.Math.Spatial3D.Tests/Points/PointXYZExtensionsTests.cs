namespace Akeldov.Math.Spatial3D.Tests.Points;

public class PointXYZExtensionsTests
{
    [TestCase(1f, 2f, 3f, 0f)]
    [TestCase(4f, 2f, 3f, 9f)]
    [TestCase(1f, 6f, 3f, 16f)]
    [TestCase(1f, 2f, 15f, 144f)]
    [TestCase(4f, 6f, 15f, 169f)]
    public void SquaredDistanceTo_ReturnsSquaredEuclideanDistance(float x, float y, float z, float expected)
    {
        var source = new PointXYZ(1f, 2f, 3f);
        var target = new PointXYZ(x, y, z);

        Assert.Multiple(() =>
        {
            Assert.That(source.SquaredDistanceTo(target), Is.EqualTo(expected));
            Assert.That(target.SquaredDistanceTo(source), Is.EqualTo(expected));
        });
    }

    [Test]
    public void LerpTo_WhenParameterIsBetweenZeroAndOne_ReturnsInterpolatedPoint()
    {
        var source = new PointXYZ(1f, 2f, 3f);
        var target = new PointXYZ(5f, 10f, -9f);

        PointXYZ point = source.LerpTo(target, 0.25f);

        Assert.That(point, Is.EqualTo(new PointXYZ(2f, 4f, 0f)));
    }

    [TestCase(0f, 1f, 2f, 3f)]
    [TestCase(1f, 5f, 10f, -9f)]
    public void LerpTo_WhenParameterIsZeroOrOne_ReturnsEndpoint(
        float t,
        float expectedX,
        float expectedY,
        float expectedZ)
    {
        var source = new PointXYZ(1f, 2f, 3f);
        var target = new PointXYZ(5f, 10f, -9f);

        PointXYZ point = source.LerpTo(target, t);

        Assert.That(point, Is.EqualTo(new PointXYZ(expectedX, expectedY, expectedZ)));
    }

    [TestCase(-0.25f, 0f, 0f, 6f)]
    [TestCase(1.25f, 6f, 12f, -12f)]
    public void LerpTo_WhenParameterIsOutsideZeroToOne_ReturnsExtrapolatedPoint(
        float t,
        float expectedX,
        float expectedY,
        float expectedZ)
    {
        var source = new PointXYZ(1f, 2f, 3f);
        var target = new PointXYZ(5f, 10f, -9f);

        PointXYZ point = source.LerpTo(target, t);

        Assert.That(point, Is.EqualTo(new PointXYZ(expectedX, expectedY, expectedZ)));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void LerpTo_WhenParameterIsInvalid_Throws(float t)
    {
        var source = new PointXYZ(1f, 2f, 3f);
        var target = new PointXYZ(5f, 10f, -9f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => source.LerpTo(target, t));

        Assert.That(exception!.ParamName, Is.EqualTo("t"));
    }

    [TestCase(0f)]
    [TestCase(1f)]
    public void LerpTo_WhenEndpointCoordinatesHaveDifferentMagnitudes_ReturnsExactEndpoint(float t)
    {
        var source = new PointXYZ(float.MaxValue, float.Epsilon, -float.MaxValue);
        var target = new PointXYZ(float.Epsilon, -float.MaxValue, 1f);

        PointXYZ point = source.LerpTo(target, t);

        Assert.That(point, Is.EqualTo(t == 0f ? source : target));
    }

    [TestCase(0.25f, -0.5f)]
    [TestCase(0.5f, 0f)]
    [TestCase(0.75f, 0.5f)]
    public void LerpTo_WhenFiniteCoordinatesAreLarge_DoesNotOverflow(float t, float expectedFactor)
    {
        var source = new PointXYZ(-float.MaxValue, float.MaxValue, -float.MaxValue);
        var target = new PointXYZ(float.MaxValue, -float.MaxValue, float.MaxValue);
        float expected = float.MaxValue * expectedFactor;

        PointXYZ point = source.LerpTo(target, t);

        Assert.That(point, Is.EqualTo(new PointXYZ(expected, -expected, expected)));
    }

    [TestCase(-1f)]
    [TestCase(0.5f)]
    [TestCase(2f)]
    public void LerpTo_WhenPointsAreEqual_ReturnsSamePoint(float t)
    {
        var source = new PointXYZ(1f, -2f, 3f);

        Assert.That(source.LerpTo(source, t), Is.EqualTo(source));
    }
}
