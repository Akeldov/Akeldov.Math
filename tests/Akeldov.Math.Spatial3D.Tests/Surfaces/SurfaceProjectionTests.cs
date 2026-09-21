using Akeldov.Math.Spatial3D.Surfaces;

namespace Akeldov.Math.Spatial3D.Tests.Surfaces;

public class SurfaceProjectionTests
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

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new SurfaceProjection(point, 0f));

        Assert.That(exception!.ParamName, Is.EqualTo("projectedPoint"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Constructor_WhenDistanceIsInvalid_Throws(float distance)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SurfaceProjection(new PointXYZ(1f, -2f, 3f), distance));

        Assert.That(exception!.ParamName, Is.EqualTo("distance"));
    }

    [TestCase(0f)]
    [TestCase(float.Epsilon)]
    [TestCase(float.MaxValue)]
    public void Constructor_WhenDistanceIsFiniteAndNonNegative_AcceptsBoundaryValues(float distance)
    {
        var point = new PointXYZ(float.MaxValue, -float.MaxValue, 3f);

        var projection = new SurfaceProjection(point, distance);

        Assert.Multiple(() =>
        {
            Assert.That(projection.ProjectedPoint, Is.EqualTo(point));
            Assert.That(projection.Distance, Is.EqualTo(distance));
        });
    }
}
