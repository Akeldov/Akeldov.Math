using Akeldov.Math.Spatial3D.Surfaces;

namespace Akeldov.Math.Spatial3D.Tests.Surfaces;

public class PlaneTests
{
    [Test]
    public void Constructor_WhenPointsAreCollinear_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Plane(
            new PointXYZ(0f, 0f, 0f),
            new PointXYZ(1f, 1f, 1f),
            new PointXYZ(2f, 2f, 2f)));
    }

    [Test]
    public void Constructor_WhenPointsAreAlmostCollinear_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new Plane(
            new PointXYZ(0f, 0f, 0f),
            new PointXYZ(1f, 0f, 0f),
            new PointXYZ(1f, GeometryConstants.GeometryEpsilon * 0.5f, 0f)));
    }

    [TestCase(float.PositiveInfinity, 0f, 0f, "a")]
    [TestCase(0f, float.NegativeInfinity, 0f, "a")]
    [TestCase(0f, 0f, float.PositiveInfinity, "b")]
    [TestCase(float.NegativeInfinity, 0f, 0f, "c")]
    public void Constructor_WhenPointCoordinateIsInvalid_Throws(float x, float y, float z, string paramName)
    {
        PointXYZ a = paramName == "a" ? new PointXYZ(x, y, z) : new PointXYZ(0f, 0f, 0f);
        PointXYZ b = paramName == "b" ? new PointXYZ(x, y, z) : new PointXYZ(1f, 0f, 0f);
        PointXYZ c = paramName == "c" ? new PointXYZ(x, y, z) : new PointXYZ(0f, 1f, 0f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Plane(a, b, c));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Constructor_WithPointAndNormal_CreatesPerpendicularPlaneThroughPoint()
    {
        var point = new PointXYZ(2f, 3f, 4f);
        var normal = new VectorXYZ(1f, -2f, 2f);

        var plane = new Plane(point, normal);

        Assert.Multiple(() =>
        {
            Assert.That(plane.Distance(point), Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
            Assert.That(
                MathF.Abs(VectorXYZ.Dot(plane.Normal, normal.Normalize())),
                Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
        });
    }

    [Test]
    public void Constructor_WhenNormalIsZero_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Plane(default, VectorXYZ.Zero));
    }

    [TestCase(float.PositiveInfinity, 0f, 0f, "point")]
    [TestCase(0f, float.NegativeInfinity, 0f, "point")]
    [TestCase(0f, 0f, float.PositiveInfinity, "normal")]
    public void Constructor_WhenPointOrNormalComponentIsInvalid_Throws(
        float x,
        float y,
        float z,
        string paramName)
    {
        PointXYZ point = paramName == "point" ? new PointXYZ(x, y, z) : default;
        VectorXYZ normal = paramName == "normal" ? new VectorXYZ(x, y, z) : VectorXYZ.BasisZ;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Plane(point, normal));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Constructor_WhenLinearEquationCoefficientsAreZero_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Plane(0f, 0f, 0f, 1f));
    }

    [TestCase(float.NaN, 1f, 0f, 0f, "a")]
    [TestCase(float.PositiveInfinity, 1f, 0f, 0f, "a")]
    [TestCase(1f, float.NaN, 0f, 0f, "b")]
    [TestCase(1f, float.NegativeInfinity, 0f, 0f, "b")]
    [TestCase(1f, 0f, float.NaN, 0f, "c")]
    [TestCase(1f, 0f, float.PositiveInfinity, 0f, "c")]
    [TestCase(1f, 0f, 0f, float.NaN, "d")]
    [TestCase(1f, 0f, 0f, float.NegativeInfinity, "d")]
    public void Constructor_WhenEquationCoefficientIsInvalid_Throws(
        float a,
        float b,
        float c,
        float d,
        string paramName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Plane(a, b, c, d));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Constructor_FromEquation_NormalizesCoefficientsAndFixesSign()
    {
        var plane = new Plane(0f, 0f, -2f, 6f);

        Assert.Multiple(() =>
        {
            Assert.That(plane.EquationA, Is.EqualTo(0f));
            Assert.That(plane.EquationB, Is.EqualTo(0f));
            Assert.That(plane.EquationC, Is.EqualTo(1f));
            Assert.That(plane.EquationD, Is.EqualTo(-3f));
            AssertVector(plane.Normal, 0f, 0f, 1f);
            AssertPoint(plane.ClosestPointToOrigin, 0f, 0f, 3f);
        });
    }

    [Test]
    public void DefaultPlane_RepresentsHorizontalXYPlane()
    {
        var plane = default(Plane);
        var samePlane = new Plane(
            new PointXYZ(0f, 0f, 0f),
            new PointXYZ(1f, 0f, 0f),
            new PointXYZ(0f, 1f, 0f));

        var projection = plane.Project(new PointXYZ(2f, 3f, 4f));

        Assert.Multiple(() =>
        {
            Assert.That(plane, Is.EqualTo(samePlane));
            Assert.That(plane.EquationA, Is.EqualTo(0f));
            Assert.That(plane.EquationB, Is.EqualTo(0f));
            Assert.That(plane.EquationC, Is.EqualTo(1f));
            Assert.That(plane.EquationD, Is.EqualTo(0f));
            AssertVector(plane.Normal, 0f, 0f, 1f);
            AssertPoint(plane.ClosestPointToOrigin, 0f, 0f, 0f);
            Assert.That(plane.Distance(new PointXYZ(2f, 3f, 4f)), Is.EqualTo(4f));
            AssertPoint(projection.ProjectedPoint, 2f, 3f, 0f);
            Assert.That(projection.Distance, Is.EqualTo(4f));
        });
    }

    [Test]
    public void ISurface_ImplementsPointDistanceProviderAndProjectionContracts()
    {
        ISurface surface = default(Plane);

        Assert.Multiple(() =>
        {
            Assert.That(surface, Is.InstanceOf<IPointDistanceProvider>());
            AssertPoint(surface.Project(new PointXYZ(1f, 2f, 3f)).ProjectedPoint, 1f, 2f, 0f);
        });
    }

    [Test]
    public void Equals_WhenSamePlaneIsBuiltFromDifferentPointTriples_ReturnsTrue()
    {
        var plane = new Plane(
            new PointXYZ(0f, 0f, 3f),
            new PointXYZ(1f, 0f, 3f),
            new PointXYZ(0f, 1f, 3f));
        var samePlane = new Plane(
            new PointXYZ(-2f, 4f, 3f),
            new PointXYZ(-2f, 2f, 3f),
            new PointXYZ(5f, 4f, 3f));

        Assert.Multiple(() =>
        {
            Assert.That(plane.Equals(samePlane), Is.True);
            Assert.That(plane.GetHashCode(), Is.EqualTo(samePlane.GetHashCode()));
            Assert.That(plane == samePlane, Is.True);
            Assert.That(plane != samePlane, Is.False);
        });
    }

    [Test]
    public void Project_WhenPlaneDoesNotPassThroughGlobalOrigin_ReturnsClosestPoint()
    {
        var plane = new Plane(new PointXYZ(0f, 0f, 3f), VectorXYZ.BasisZ);

        var projection = plane.Project(new PointXYZ(2f, 5f, 7f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 2f, 5f, 3f);
            Assert.That(projection.Distance, Is.EqualTo(4f).Within(GeometryConstants.GeometryEpsilon));
        });
    }

    [Test]
    public void Distance_WhenPointCoordinateIsInvalid_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(Plane).Distance(new PointXYZ(float.PositiveInfinity, 0f, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void Project_WhenPointCoordinateIsInvalid_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(Plane).Project(new PointXYZ(0f, float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
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
