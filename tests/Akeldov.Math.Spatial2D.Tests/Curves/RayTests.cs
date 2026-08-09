using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class RayTests
{
    [Test]
    public void DefaultRay_StartsAtOriginAndPointsAlongPositiveXAxis()
    {
        var ray = default(Ray);

        AssertVector(ray.Origin, 0f, 0f);
        Assert.That(ray.Angle, Is.EqualTo(0f));
        AssertVector(ray.Direction, 1f, 0f);

        var projection = ray.ProjectWithParameter(new PointXY(3f, 4f));
        AssertVector(projection.ProjectedPoint, 3f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(3f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(4f).Within(GeometryConstants.GeometryEpsilon));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Constructor_WhenAngleIsInvalid_Throws(float angle)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Ray(new PointXY(0f, 0f), angle));

        Assert.That(exception!.ParamName, Is.EqualTo("angle"));
    }

    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void Constructor_WhenOriginCoordinateIsInvalid_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Ray(new PointXY(x, y)));

        Assert.That(exception!.ParamName, Is.EqualTo("origin"));
    }

    [Test]
    public void PerpendicularAt_WhenPointIsArbitrary_CreatesLineThroughPointPerpendicularToRay()
    {
        var ray = new Ray(new PointXY(10f, -2f), MathF.Atan2(4f, 3f));
        var point = new PointXY(-1f, 5f);

        Line perpendicular = ray.PerpendicularAt(point);

        Assert.That(perpendicular.Distance(point), Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(
            MathF.Abs(VectorXY.Dot(perpendicular.Direction, ray.Direction)),
            Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void PerpendicularAt_WhenPointCoordinateIsInvalid_Throws()
    {
        var ray = default(Ray);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ray.PerpendicularAt(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void RayIntersections_WhenThisRayOriginBelongsToOtherCollinearRay_ReturnsThisOrigin()
    {
        var ray = new Ray(new PointXY(2f, 0f));
        var other = new Ray(new PointXY(0f, 0f));

        var intersections = ray.GetRayIntersections(other);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 2f, 0f);
    }

    [Test]
    public void PointIntersections_WhenCollinearRaysOverlapContinuously_ReturnsEmpty()
    {
        var ray = new Ray(new PointXY(2f, 0f));
        var other = new Ray(new PointXY(0f, 0f));

        var intersections = ray.GetPointIntersections(other);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void PointIntersections_WhenOpposingRaysShareOrigin_ReturnsOrigin()
    {
        var ray = new Ray(new PointXY(0f, 0f));
        var other = new Ray(new PointXY(0f, 0f), MathF.PI);

        var intersections = ray.GetPointIntersections(other);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 0f, 0f);
    }

    [Test]
    public void RayIntersections_WhenOtherCollinearRayStartsAhead_ReturnsOtherOrigin()
    {
        var ray = new Ray(new PointXY(0f, 0f));
        var other = new Ray(new PointXY(2f, 0f));

        var intersections = ray.GetRayIntersections(other);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 2f, 0f);
    }

    [Test]
    public void RayIntersections_WhenCollinearRaysFaceEachOther_ReturnsThisOrigin()
    {
        var ray = new Ray(new PointXY(0f, 0f));
        var other = new Ray(new PointXY(2f, 0f), MathF.PI);

        var intersections = ray.GetRayIntersections(other);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 0f, 0f);
    }

    [Test]
    public void RayIntersections_WhenRaysAreParallelButNotCollinear_ReturnsEmpty()
    {
        var ray = new Ray(new PointXY(0f, 0f));
        var other = new Ray(new PointXY(0f, 1f));

        var intersections = ray.GetRayIntersections(other);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void RayIntersections_WithCustomGeometryEpsilon_WhenRaysAreNearlyCollinear_ReturnsOtherOrigin()
    {
        const float geometryEpsilon = 0.01f;
        var ray = new Ray(new PointXY(0f, 0f));
        var other = new Ray(new PointXY(2f, 0.005f));

        var defaultIntersections = ray.GetRayIntersections(other);
        var tolerantIntersections = ray.GetRayIntersections(other, geometryEpsilon);

        Assert.That(defaultIntersections, Is.Empty);
        Assert.That(tolerantIntersections, Has.Count.EqualTo(1));
        AssertVector(tolerantIntersections[0], 2f, 0.005f);
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void RayIntersections_WhenGeometryEpsilonIsInvalid_Throws(float geometryEpsilon)
    {
        var ray = new Ray(new PointXY(0f, 0f));
        var other = new Ray(new PointXY(2f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ray.GetRayIntersections(other, geometryEpsilon));

        Assert.That(exception!.ParamName, Is.EqualTo("geometryEpsilon"));
    }

    [Test]
    public void RayIntersections_WhenNonParallelRaysCrossAhead_ReturnsIntersection()
    {
        var ray = new Ray(new PointXY(0f, 0f));
        var other = new Ray(new PointXY(2f, -1f), MathF.PI / 2f);

        var intersections = ray.GetRayIntersections(other);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 2f, 0f);
    }

    [Test]
    public void RayIntersections_WhenIntersectionIsBehindOneRay_ReturnsEmpty()
    {
        var ray = new Ray(new PointXY(0f, 0f));
        var other = new Ray(new PointXY(-2f, -1f), MathF.PI / 2f);

        var intersections = ray.GetRayIntersections(other);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void ProjectWithParameter_WhenPointIsBehindRay_ClampsToOrigin()
    {
        var ray = new Ray(new PointXY(1f, 0f));

        var projection = ray.ProjectWithParameter(new PointXY(0f, 0f));

        AssertVector(projection.ProjectedPoint, 1f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ProjectWithParameter_WhenPointCoordinateIsInvalid_Throws()
    {
        var ray = default(Ray);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ray.ProjectWithParameter(new PointXY(float.PositiveInfinity, 0f)));

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
