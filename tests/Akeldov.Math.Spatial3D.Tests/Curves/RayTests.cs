using System.Globalization;
using Akeldov.Math.Spatial3D.Curves;

namespace Akeldov.Math.Spatial3D.Tests.Curves;

public class RayTests
{
    [Test]
    public void DefaultRay_StartsAtOriginAndPointsAlongPositiveXAxis()
    {
        var ray = default(Ray);

        ParameterizedCurveProjection projection = ray.ProjectWithParameter(new PointXYZ(3f, 5f, 12f));

        Assert.Multiple(() =>
        {
            AssertPoint(ray.Origin, 0f, 0f, 0f);
            AssertPoint(ray.Endpoint, 0f, 0f, 0f);
            AssertVector(ray.Direction, 1f, 0f, 0f);
            AssertPoint(projection.ProjectedPoint, 3f, 0f, 0f);
            Assert.That(projection.CurveCoordinate, Is.EqualTo(3f));
            Assert.That(projection.Distance, Is.EqualTo(13f));
        });
    }

    [TestCase(float.PositiveInfinity, 0f, 0f)]
    [TestCase(0f, float.NegativeInfinity, 0f)]
    [TestCase(0f, 0f, float.PositiveInfinity)]
    public void Constructor_WhenOriginCoordinateIsInvalid_Throws(float x, float y, float z)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Ray(new PointXYZ(x, y, z)));

        Assert.That(exception!.ParamName, Is.EqualTo("origin"));
    }

    [Test]
    public void Constructor_WhenDirectionIsZero_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Ray(default, VectorXYZ.Zero));

        Assert.That(exception!.ParamName, Is.EqualTo("direction"));
    }

    [TestCase(float.NaN, 0f, 0f)]
    [TestCase(0f, float.PositiveInfinity, 0f)]
    [TestCase(0f, 0f, float.NegativeInfinity)]
    public void Constructor_WhenDirectionComponentIsInvalid_Throws(float x, float y, float z)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Ray(default, new VectorXYZ(x, y, z)));

        Assert.That(exception!.ParamName, Is.EqualTo("direction"));
    }

    [Test]
    public void Constructor_WithOriginAndDirection_NormalizesDirection()
    {
        var ray = new Ray(
            new PointXYZ(2f, 3f, 4f),
            new VectorXYZ(-2f, 0f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(ray.Origin, 2f, 3f, 4f);
            AssertPoint(ray.Endpoint, 2f, 3f, 4f);
            AssertVector(ray.Direction, -1f, 0f, 0f);
        });
    }

    [Test]
    public void Constructor_WithExtremeFiniteDirection_NormalizesWithoutOverflowOrUnderflow()
    {
        var largeDirectionRay = new Ray(
            default,
            new VectorXYZ(-float.MaxValue, float.MaxValue, 0f));
        var smallDirectionRay = new Ray(
            default,
            new VectorXYZ(-float.Epsilon, 0f, 0f));

        Assert.Multiple(() =>
        {
            Assert.That(
                largeDirectionRay.Direction.Length,
                Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
            AssertVector(smallDirectionRay.Direction, -1f, 0f, 0f);
        });
    }

    [Test]
    public void ProjectWithParameter_WhenPointIsAhead_ReturnsClosestPointAndCoordinate()
    {
        var ray = new Ray(default, new VectorXYZ(1f, 1f, 1f));

        ParameterizedCurveProjection projection = ray.ProjectWithParameter(new PointXYZ(3f, 0f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 1f, 1f, 1f);
            Assert.That(
                projection.CurveCoordinate,
                Is.EqualTo(MathF.Sqrt(3f)).Within(GeometryConstants.GeometryEpsilon));
            Assert.That(
                projection.Distance,
                Is.EqualTo(MathF.Sqrt(6f)).Within(GeometryConstants.GeometryEpsilon));
        });
    }

    [Test]
    public void ProjectWithParameter_WhenPointIsBehindRay_ClampsToOrigin()
    {
        var ray = new Ray(new PointXYZ(1f, 2f, 3f), VectorXYZ.BasisX);

        ParameterizedCurveProjection projection = ray.ProjectWithParameter(new PointXYZ(-4f, 5f, 7f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 1f, 2f, 3f);
            Assert.That(projection.CurveCoordinate, Is.Zero);
            Assert.That(projection.Distance, Is.EqualTo(MathF.Sqrt(50f)).Within(GeometryConstants.GeometryEpsilon));
        });
    }

    [Test]
    public void ProjectWithParameter_WithExtremeFiniteCoordinates_AvoidsIntermediateOverflow()
    {
        var ray = new Ray(
            new PointXYZ(-float.MaxValue, 0f, 0f),
            VectorXYZ.BasisX);

        ParameterizedCurveProjection projection = ray.ProjectWithParameter(
            new PointXYZ(float.MaxValue, 1f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, float.MaxValue, 0f, 0f);
            Assert.That(projection.CurveCoordinate, Is.EqualTo(float.PositiveInfinity));
            Assert.That(projection.Distance, Is.EqualTo(1f));
        });
    }

    [TestCase(float.PositiveInfinity, 0f, 0f)]
    [TestCase(0f, float.NegativeInfinity, 0f)]
    [TestCase(0f, 0f, float.PositiveInfinity)]
    public void ProjectWithParameter_WhenPointCoordinateIsInvalid_Throws(float x, float y, float z)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(Ray).ProjectWithParameter(new PointXYZ(x, y, z)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void Project_ReturnsProjectionWithoutCurveCoordinate()
    {
        var ray = new Ray(new PointXYZ(1f, 2f, 3f), VectorXYZ.BasisZ);

        CurveProjection projection = ray.Project(new PointXYZ(4f, 6f, 8f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 1f, 2f, 8f);
            Assert.That(projection.Distance, Is.EqualTo(5f));
        });
    }

    [Test]
    public void Distance_ReturnsDistanceToRay()
    {
        var ray = new Ray(new PointXYZ(1f, 2f, 3f), VectorXYZ.BasisZ);

        float distance = ray.Distance(new PointXYZ(4f, 6f, 8f));

        Assert.That(distance, Is.EqualTo(5f));
    }

    [Test]
    public void GetPoint_ReturnsPointAtNonNegativeCoordinate()
    {
        var ray = new Ray(
            new PointXYZ(2f, 3f, 4f),
            new VectorXYZ(0f, -2f, 0f));

        PointXYZ point = ray.GetPoint(5f);

        AssertPoint(point, 2f, -2f, 4f);
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void GetPoint_WhenCurveCoordinateIsInvalid_Throws(float curveCoordinate)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(Ray).GetPoint(curveCoordinate));

        Assert.That(exception!.ParamName, Is.EqualTo("curveCoordinate"));
    }

    [Test]
    public void Ray_ImplementsRayPathContract()
    {
        IRayPath ray = default(Ray);

        Assert.Multiple(() =>
        {
            Assert.That(ray, Is.InstanceOf<IOneEndpointCurve>());
            Assert.That(ray, Is.InstanceOf<IParameterizedCurve>());
            Assert.That(ray, Is.InstanceOf<IPointDistanceProvider>());
        });
    }

    [Test]
    public void Equality_UsesOriginAndDirection()
    {
        var ray = new Ray(new PointXYZ(1f, 2f, 3f), new VectorXYZ(2f, 0f, 0f));
        var sameRay = new Ray(new PointXYZ(1f, 2f, 3f), VectorXYZ.BasisX);
        var differentOrigin = new Ray(new PointXYZ(2f, 2f, 3f), VectorXYZ.BasisX);
        var differentDirection = new Ray(new PointXYZ(1f, 2f, 3f), new VectorXYZ(-1f, 0f, 0f));

        Assert.Multiple(() =>
        {
            Assert.That(ray.Equals(sameRay), Is.True);
            Assert.That(ray.GetHashCode(), Is.EqualTo(sameRay.GetHashCode()));
            Assert.That(ray == sameRay, Is.True);
            Assert.That(ray != sameRay, Is.False);
            Assert.That(ray.Equals(differentOrigin), Is.False);
            Assert.That(ray.Equals(differentDirection), Is.False);
        });
    }

    [Test]
    public void ToString_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        try
        {
            var ray = new Ray(
                new PointXYZ(1.5f, 2.25f, 3.75f),
                VectorXYZ.BasisX);

            Assert.That(ray.ToString(), Is.EqualTo("((1.5, 2.25, 3.75) + t*(1, 0, 0), t >= 0)"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private static void AssertVector(VectorXYZ actual, float expectedX, float expectedY, float expectedZ)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
            Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
            Assert.That(actual.Z, Is.EqualTo(expectedZ).Within(GeometryConstants.GeometryEpsilon));
        });
    }

    private static void AssertPoint(PointXYZ actual, float expectedX, float expectedY, float expectedZ)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
            Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
            Assert.That(actual.Z, Is.EqualTo(expectedZ).Within(GeometryConstants.GeometryEpsilon));
        });
    }
}
