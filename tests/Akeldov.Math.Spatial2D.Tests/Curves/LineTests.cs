using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class LineTests
{
    [Test]
    public void Constructor_WhenPointsAreEqual_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Line(new PointXY(2f, 3f), new PointXY(2f, 3f)));
    }

    [Test]
    public void Constructor_WhenPointsAreAlmostEqual_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new Line(new PointXY(0f, 0f), new PointXY(GeometryConstants.GeometryEpsilon * 0.5f, 0f)));
    }

    [TestCase(float.PositiveInfinity, 0f, "a")]
    [TestCase(0f, float.NegativeInfinity, "a")]
    [TestCase(float.PositiveInfinity, 0f, "b")]
    [TestCase(0f, float.NegativeInfinity, "b")]
    public void Constructor_WhenPointCoordinateIsInvalid_Throws(float x, float y, string paramName)
    {
        PointXY a = paramName == "a" ? new PointXY(x, y) : new PointXY(0f, 0f);
        PointXY b = paramName == "b" ? new PointXY(x, y) : new PointXY(1f, 1f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Line(a, b));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Constructor_WithPointAndAngle_CreatesLineThroughPointWithDirection()
    {
        var point = new PointXY(2f, 3f);
        var direction = new VectorXY(MathF.Sqrt(0.5f), MathF.Sqrt(0.5f));

        var line = new Line(point, MathF.PI / 4f);

        Assert.That(line.Distance(point), Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(line.Distance(point + 5f * direction), Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(
            MathF.Abs(VectorXY.Cross(line.Direction, direction)),
            Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
    }

    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void Constructor_WhenPointCoordinateForAngleConstructorIsInvalid_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Line(new PointXY(x, y), 0f));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Constructor_WhenAngleIsInvalid_Throws(float angle)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Line(new PointXY(0f, 0f), angle));

        Assert.That(exception!.ParamName, Is.EqualTo("angle"));
    }

    [Test]
    public void Constructor_WhenLinearEquationCoefficientsAreZero_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Line(0f, 0f, 1f));
    }

    [TestCase(float.NaN, 1f, 0f, "a")]
    [TestCase(float.PositiveInfinity, 1f, 0f, "a")]
    [TestCase(1f, float.NaN, 0f, "b")]
    [TestCase(1f, float.NegativeInfinity, 0f, "b")]
    [TestCase(1f, 0f, float.NaN, "c")]
    [TestCase(1f, 0f, float.PositiveInfinity, "c")]
    public void Constructor_WhenEquationCoefficientIsInvalid_Throws(
        float a,
        float b,
        float c,
        string paramName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Line(a, b, c));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Constructor_FromEquation_NormalizesCoefficientsAndFixesSign()
    {
        var line = new Line(0f, -2f, 6f);

        Assert.That(line.EquationA, Is.EqualTo(0f));
        Assert.That(line.EquationB, Is.EqualTo(1f));
        Assert.That(line.EquationC, Is.EqualTo(-3f));
        AssertVector(line.Normal, 0f, 1f);
        AssertVector(line.Direction, 1f, 0f);
        AssertVector(line.ClosestPointToOrigin, 0f, 3f);
    }

    [Test]
    public void DefaultLine_RepresentsHorizontalXAxis()
    {
        var line = default(Line);
        var sameLine = new Line(new PointXY(0f, 0f), new PointXY(1f, 0f));

        Assert.That(line, Is.EqualTo(sameLine));
        Assert.That(line.EquationA, Is.EqualTo(0f));
        Assert.That(line.EquationB, Is.EqualTo(1f));
        Assert.That(line.EquationC, Is.EqualTo(0f));
        AssertVector(line.Normal, 0f, 1f);
        AssertVector(line.Direction, 1f, 0f);
        AssertVector(line.ClosestPointToOrigin, 0f, 0f);
        Assert.That(line.Distance(new PointXY(3f, 4f)), Is.EqualTo(4f).Within(GeometryConstants.GeometryEpsilon));

        var projection = line.Project(new PointXY(3f, 4f));
        AssertVector(projection.ProjectedPoint, 3f, 0f);
        Assert.That(projection.Distance, Is.EqualTo(4f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ICurve_ImplementsPointDistanceProviderContract()
    {
        ICurve curve = default(Line);

        Assert.That(curve, Is.InstanceOf<IPointDistanceProvider>());
    }

    [Test]
    public void Equals_WhenSameLineIsBuiltFromDifferentPointPairs_ReturnsTrue()
    {
        var line = new Line(new PointXY(0f, 3f), new PointXY(2f, 3f));
        var sameLine = new Line(new PointXY(-5f, 3f), new PointXY(5f, 3f));

        Assert.That(line.Equals(sameLine), Is.True);
        Assert.That(line.GetHashCode(), Is.EqualTo(sameLine.GetHashCode()));
    }

    [Test]
    public void Project_WhenSameLineIsBuiltFromDifferentPointPairs_ReturnsSameProjection()
    {
        var line = new Line(new PointXY(2f, 0f), new PointXY(4f, 0f));
        var sameLine = new Line(new PointXY(8f, 0f), new PointXY(12f, 0f));

        var projection = line.Project(new PointXY(3f, 2f));
        var sameLineProjection = sameLine.Project(new PointXY(3f, 2f));

        Assert.That(line, Is.EqualTo(sameLine));
        AssertVector(projection.ProjectedPoint, 3f, 0f);
        AssertVector(sameLineProjection.ProjectedPoint, 3f, 0f);
        Assert.That(projection.Distance, Is.EqualTo(sameLineProjection.Distance).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void PointIntersections_WhenRayLiesOnLine_ReturnsEmpty()
    {
        var line = new Line(new PointXY(-5f, 0f), new PointXY(5f, 0f));
        var ray = new Ray(new PointXY(2f, 0f));

        var intersections = line.GetPointIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void PointIntersections_WhenRayIsParallelButNotOnLine_ReturnsEmpty()
    {
        var line = new Line(new PointXY(-5f, 1f), new PointXY(5f, 1f));
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = line.GetPointIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void PointIntersections_WhenIntersectionIsBehindRay_ReturnsEmpty()
    {
        var line = new Line(new PointXY(-1f, -1f), new PointXY(-1f, 1f));
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = line.GetPointIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void Project_WhenLineDoesNotPassThroughGlobalOrigin_ReturnsClosestPoint()
    {
        var line = new Line(new PointXY(2f, 3f), new PointXY(4f, 3f));

        var projection = line.Project(new PointXY(2f, 5f));

        AssertVector(projection.ProjectedPoint, 2f, 3f);
        Assert.That(projection.Distance, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Distance_WhenPointCoordinateIsInvalid_Throws()
    {
        var line = default(Line);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            line.Distance(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void Project_WhenPointCoordinateIsInvalid_Throws()
    {
        var line = default(Line);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            line.Project(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    private static void AssertVector(VectorXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }

    private static void AssertVector(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }
}
