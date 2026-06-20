using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class CircleTests
{
    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void Constructor_WhenCenterCoordinateIsInvalid_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Circle(new PointXY(x, y), 1f));

        Assert.That(exception!.ParamName, Is.EqualTo("center"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Constructor_WhenRadiusIsInvalid_Throws(float radius)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Circle(new PointXY(0f, 0f), radius));

        Assert.That(exception!.ParamName, Is.EqualTo("radius"));
    }

    [Test]
    public void Distance_WhenPointIsInsideCircle_ReturnsDistanceToCircumference()
    {
        var circle = new Circle(new PointXY(0f, 0f), 5f);

        var distance = circle.Distance(new PointXY(3f, 0f));

        Assert.That(distance, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void IContour_ExposesClosedBoundaryContract()
    {
        var circle = new Circle(new PointXY(0f, 0f), 5f);

        Assert.That(circle, Is.InstanceOf<IContour>());
        Assert.That(circle.Length, Is.EqualTo(2f * MathF.PI * 5f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ParameterizedCircle_ExposesParameterizedContourContract()
    {
        var circle = new ParameterizedCircle(new PointXY(0f, 0f), 5f);

        Assert.That(circle, Is.InstanceOf<IParameterizedContour>());
        Assert.That(circle, Is.InstanceOf<IContour>());
        Assert.That(circle, Is.InstanceOf<IParameterizedCurve>());
        Assert.That(circle, Is.InstanceOf<IFiniteCurve>());
        Assert.That(circle.Length, Is.EqualTo(2f * MathF.PI * 5f).Within(GeometryConstants.GeometryEpsilon));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void ParameterizedCircleConstructor_WhenStartAngleIsInvalid_Throws(float startAngle)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedCircle(new PointXY(0f, 0f), 1f, startAngle));

        Assert.That(exception!.ParamName, Is.EqualTo("startAngle"));
    }

    [Test]
    public void ParameterizedCircleConstructor_WhenContourDirectionIsUnsupported_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedCircle(new PointXY(0f, 0f), 1f, 0f, (ContourDirection)42));

        Assert.That(exception!.ParamName, Is.EqualTo("contourDirection"));
    }

    [Test]
    public void ParameterizedCircle_GetPoint_UsesLengthCoordinateAroundCircumference()
    {
        var circle = new ParameterizedCircle(
            new PointXY(1f, 1f),
            2f,
            MathF.PI / 2f,
            ContourDirection.Counterclockwise);

        AssertVector(circle.GetPoint(0f), 1f, 3f);
        AssertVector(circle.GetPoint(MathF.PI), -1f, 1f);
        AssertVector(circle.GetPoint(circle.Length), 1f, 3f);
    }

    [Test]
    public void ParameterizedCircle_GetPoint_WhenDirectionIsClockwise_UsesReversedTraversal()
    {
        var circle = new ParameterizedCircle(
            new PointXY(0f, 0f),
            2f,
            MathF.PI / 2f,
            ContourDirection.Clockwise);

        AssertVector(circle.GetPoint(MathF.PI), 2f, 0f);
    }

    [TestCase(-1e-6f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void ParameterizedCircle_GetPoint_WhenCurveCoordinateIsInvalid_Throws(float curveCoordinate)
    {
        var circle = new ParameterizedCircle(new PointXY(0f, 0f), 1f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            circle.GetPoint(curveCoordinate));

        Assert.That(exception!.ParamName, Is.EqualTo("curveCoordinate"));
    }

    [Test]
    public void ParameterizedCircle_GetPoint_WhenCurveCoordinateExceedsLength_Throws()
    {
        var circle = new ParameterizedCircle(new PointXY(0f, 0f), 1f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            circle.GetPoint(circle.Length + 0.001f));

        Assert.That(exception!.ParamName, Is.EqualTo("curveCoordinate"));
    }

    [Test]
    public void ParameterizedCircle_ProjectWithParameter_ReturnsClosestCircumferenceCoordinate()
    {
        var circle = new ParameterizedCircle(
            new PointXY(0f, 0f),
            2f,
            MathF.PI / 2f,
            ContourDirection.Counterclockwise);

        ParameterizedCurveProjection projection = circle.ProjectWithParameter(new PointXY(-4f, 0f));

        AssertVector(projection.ProjectedPoint, -2f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(MathF.PI).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ParameterizedCircle_ProjectWithParameter_WhenPointIsAtCenter_ReturnsStartPoint()
    {
        var circle = new ParameterizedCircle(
            new PointXY(1f, 1f),
            2f,
            MathF.PI / 2f,
            ContourDirection.Counterclockwise);

        ParameterizedCurveProjection projection = circle.ProjectWithParameter(new PointXY(1f, 1f));

        AssertVector(projection.ProjectedPoint, 1f, 3f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ParameterizedCircle_ProjectWithParameter_WhenRadiusIsZero_ReturnsCenter()
    {
        var circle = new ParameterizedCircle(
            new PointXY(1f, 1f),
            0f,
            MathF.PI / 2f,
            ContourDirection.Counterclockwise);

        ParameterizedCurveProjection projection = circle.ProjectWithParameter(new PointXY(4f, 5f));

        AssertVector(projection.ProjectedPoint, 1f, 1f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(5f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ParameterizedCircle_ExplicitConversionToCircle_ReturnsGeometricCircle()
    {
        var source = new Circle(new PointXY(1f, 2f), 3f);
        var parameterizedCircle = new ParameterizedCircle(source.Center, source.Radius, MathF.PI);

        Circle converted = (Circle)parameterizedCircle;

        Assert.That(converted, Is.EqualTo(source));
    }

    [Test]
    public void ParameterizedCircle_ToDegreesString_UsesInvariantCulture()
    {
        var circle = new ParameterizedCircle(
            new PointXY(1.5f, 2.25f),
            3.5f,
            MathF.PI / 2f,
            ContourDirection.Clockwise);

        Assert.That(
            circle.ToDegreesString(),
            Is.EqualTo("ParameterizedCircle(center: (1.5, 2.25), radius: 3.5, deg: 90, direction: Clockwise)"));
    }

    [TestCase(0f, 0f, true)]
    [TestCase(3f, 4f, true)]
    [TestCase(5f, 0f, true)]
    [TestCase(5.001f, 0f, false)]
    public void Encloses_ClassifiesPointsAgainstFilledCircle(float x, float y, bool expected)
    {
        var circle = new Circle(new PointXY(0f, 0f), 5f);

        bool encloses = circle.Encloses(new PointXY(x, y));

        Assert.That(encloses, Is.EqualTo(expected));
    }

    [Test]
    public void Encloses_WithCustomGeometryEpsilon_IncludesNearbyOutsidePoint()
    {
        var circle = new Circle(new PointXY(0f, 0f), 5f);
        var point = new PointXY(5.0005f, 0f);

        Assert.That(circle.Encloses(point), Is.False);
        Assert.That(circle.Encloses(point, 0.001f), Is.True);
    }

    [Test]
    public void Encloses_WhenPointCoordinateIsInvalid_Throws()
    {
        var circle = new Circle(new PointXY(0f, 0f), 1f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            circle.Encloses(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Encloses_WhenGeometryEpsilonIsInvalid_Throws(float geometryEpsilon)
    {
        var circle = new Circle(new PointXY(0f, 0f), 1f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            circle.Encloses(new PointXY(0f, 0f), geometryEpsilon));

        Assert.That(exception!.ParamName, Is.EqualTo("geometryEpsilon"));
    }

    [Test]
    public void SignedDistance_ReturnsNegativeInsideAndPositiveOutside()
    {
        var circle = new Circle(new PointXY(0f, 0f), 5f);

        Assert.That(circle.SignedDistance(new PointXY(3f, 0f)), Is.EqualTo(-2f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(circle.SignedDistance(new PointXY(7f, 0f)), Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(circle.SignedDistance(new PointXY(5f, 0f)), Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void SignedDistance_WithCustomGeometryEpsilon_WhenPointIsWithinTolerance_ReturnsNegativeDistance()
    {
        var circle = new Circle(new PointXY(0f, 0f), 5f);

        float signedDistance = circle.SignedDistance(new PointXY(5.0005f, 0f), 0.001f);

        Assert.That(signedDistance, Is.EqualTo(-0.0005f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Project_WhenPointIsOutsideCircle_ReturnsNearestPointOnCircumference()
    {
        var circle = new Circle(new PointXY(0f, 0f), 2f);

        var projection = circle.Project(new PointXY(3f, 0f));

        AssertVector(projection.ProjectedPoint, 2f, 0f);
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Project_WhenPointIsAtCenter_ReturnsPointOnPositiveXAxis()
    {
        var circle = new Circle(new PointXY(1f, 1f), 2f);

        var projection = circle.Project(new PointXY(1f, 1f));

        AssertVector(projection.ProjectedPoint, 3f, 1f);
        Assert.That(projection.Distance, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Project_WhenPointCoordinateIsInvalid_Throws()
    {
        var circle = new Circle(new PointXY(0f, 0f), 1f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            circle.Project(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void RayIntersections_WhenRayStartsInsideCircle_ReturnsForwardExitPoint()
    {
        var circle = new Circle(new PointXY(0f, 0f), 2f);
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = circle.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 2f, 0f);
    }

    [Test]
    public void RayIntersections_WhenRayIsTangent_ReturnsSingleIntersection()
    {
        var circle = new Circle(new PointXY(0f, 0f), 1f);
        var ray = new Ray(new PointXY(-2f, 1f));

        var intersections = circle.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 0f, 1f);
    }

    [Test]
    public void RayIntersections_WithCustomGeometryEpsilon_WhenRayNearlyTouchesCircle_ReturnsSingleIntersection()
    {
        const float geometryEpsilon = 0.001f;
        var circle = new Circle(new PointXY(0f, 0f), 1f);
        var ray = new Ray(new PointXY(-2f, 1.00005f));

        var defaultIntersections = circle.GetRayIntersections(ray);
        var tolerantIntersections = circle.GetRayIntersections(ray, geometryEpsilon);

        Assert.That(defaultIntersections, Is.Empty);
        Assert.That(tolerantIntersections, Has.Count.EqualTo(1));
        AssertVector(tolerantIntersections[0], 0f, 1.00005f);
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void RayIntersections_WhenGeometryEpsilonIsInvalid_Throws(float geometryEpsilon)
    {
        var circle = new Circle(new PointXY(0f, 0f), 1f);
        var ray = new Ray(new PointXY(0f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            circle.GetRayIntersections(ray, geometryEpsilon));

        Assert.That(exception!.ParamName, Is.EqualTo("geometryEpsilon"));
    }

    [Test]
    public void RayIntersections_WhenCircleIsBehindRay_ReturnsEmpty()
    {
        var circle = new Circle(new PointXY(0f, 0f), 1f);
        var ray = new Ray(new PointXY(2f, 0f));

        var intersections = circle.GetRayIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    private static void AssertVector(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }
}
