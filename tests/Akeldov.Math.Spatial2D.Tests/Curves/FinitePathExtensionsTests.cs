using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class FinitePathExtensionsTests
{
    [TestCase(0f, 2f, 3f)]
    [TestCase(0.25f, 3f, 3f)]
    [TestCase(0.5f, 4f, 3f)]
    [TestCase(1f, 6f, 3f)]
    public void GetPointAtNormalizedCoordinate_WhenCoordinateIsValid_ReturnsPointAtScaledCurveCoordinate(
        float normalizedCurveCoordinate,
        float expectedX,
        float expectedY)
    {
        IFinitePath path = new ParameterizedSegment(
            new PointXY(2f, 3f),
            new PointXY(6f, 3f));

        PointXY point = path.GetPointAtNormalizedCoordinate(normalizedCurveCoordinate);

        AssertVector(point, expectedX, expectedY);
    }

    [Test]
    public void GetPointAtNormalizedCoordinate_WhenPathHasZeroLength_ReturnsStartPoint()
    {
        IFinitePath path = new ParameterizedSegment(
            new PointXY(2f, 3f),
            new PointXY(2f, 3f));

        PointXY point = path.GetPointAtNormalizedCoordinate(0.5f);

        AssertVector(point, 2f, 3f);
    }

    [Test]
    public void GetPointAtNormalizedCoordinate_WhenPathIsNull_Throws()
    {
        IFinitePath path = null!;

        var exception = Assert.Throws<ArgumentNullException>(() =>
            path.GetPointAtNormalizedCoordinate(0f));

        Assert.That(exception!.ParamName, Is.EqualTo("path"));
    }

    [TestCase(-0.001f)]
    [TestCase(1.001f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void GetPointAtNormalizedCoordinate_WhenCoordinateIsInvalid_Throws(float normalizedCurveCoordinate)
    {
        IFinitePath path = new ParameterizedSegment(
            new PointXY(2f, 3f),
            new PointXY(6f, 3f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            path.GetPointAtNormalizedCoordinate(normalizedCurveCoordinate));

        Assert.That(exception!.ParamName, Is.EqualTo("normalizedCurveCoordinate"));
    }

    private static void AssertVector(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }
}
