using Akeldov.Math.Spatial3D.Curves;

namespace Akeldov.Math.Spatial3D.Tests.Curves;

public class ParameterizedCurveProjectionTests
{
    [TestCase(float.PositiveInfinity, 0f, 0f)]
    [TestCase(float.NegativeInfinity, 0f, 0f)]
    [TestCase(0f, float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity, 0f)]
    [TestCase(0f, 0f, float.PositiveInfinity)]
    [TestCase(0f, 0f, float.NegativeInfinity)]
    public void Constructor_WhenProjectedPointIsInfinite_Throws(float x, float y, float z)
    {
        var point = new PointXYZ(x, y, z);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedCurveProjection(point, 0f, 0f));

        Assert.That(exception!.ParamName, Is.EqualTo("projectedPoint"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Constructor_WhenDistanceIsInvalid_Throws(float distance)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedCurveProjection(new PointXYZ(1f, -2f, 3f), 0f, distance));

        Assert.That(exception!.ParamName, Is.EqualTo("distance"));
    }

    [Test]
    public void Constructor_WhenCurveCoordinateIsNaN_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedCurveProjection(new PointXYZ(1f, -2f, 3f), float.NaN, 0f));

        Assert.That(exception!.ParamName, Is.EqualTo("curveCoordinate"));
    }

    [TestCase(-float.MaxValue)]
    [TestCase(-1f)]
    [TestCase(0f)]
    [TestCase(1f)]
    [TestCase(float.MaxValue)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Constructor_WhenCurveCoordinateIsNotNaN_PreservesCoordinate(float curveCoordinate)
    {
        var point = new PointXYZ(1f, -2f, 3f);

        var projection = new ParameterizedCurveProjection(point, curveCoordinate, 4f);

        Assert.Multiple(() =>
        {
            Assert.That(projection.ProjectedPoint, Is.EqualTo(point));
            Assert.That(projection.CurveCoordinate, Is.EqualTo(curveCoordinate));
            Assert.That(projection.Distance, Is.EqualTo(4f));
        });
    }

    [TestCase(0f)]
    [TestCase(float.Epsilon)]
    [TestCase(float.MaxValue)]
    public void Constructor_WhenDistanceIsFiniteAndNonNegative_AcceptsBoundaryValues(float distance)
    {
        var point = new PointXYZ(float.MaxValue, -float.MaxValue, 3f);

        var projection = new ParameterizedCurveProjection(point, -2f, distance);

        Assert.Multiple(() =>
        {
            Assert.That(projection.ProjectedPoint, Is.EqualTo(point));
            Assert.That(projection.CurveCoordinate, Is.EqualTo(-2f));
            Assert.That(projection.Distance, Is.EqualTo(distance));
        });
    }
}
