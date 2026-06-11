using Akeldov.Math.Spatial2D.Curves;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class ArcTests
{
    [Test]
    public void ProjectWithParameter_WhenParameterizedArcPointAngleIsWithinWrappedArc_ProjectsToCircle()
    {
        var arc = new ParameterizedArc(
            new PointXY(0f, 0f),
            2f,
            3f * MathF.PI / 2f,
            MathF.PI / 2f,
            AngularDirection.Counterclockwise);
        var point = new PointXY(3f, 0f);

        var projection = arc.ProjectWithParameter(point);

        AssertVector(projection.ProjectedPoint, 2f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(MathF.PI).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void Constructor_WhenCenterCoordinateIsInvalid_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Arc(new PointXY(x, y), 1f, 0f, MathF.PI));

        Assert.That(exception!.ParamName, Is.EqualTo("center"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Constructor_WhenRadiusIsInvalid_Throws(float radius)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Arc(new PointXY(0f, 0f), radius, 0f, MathF.PI));

        Assert.That(exception!.ParamName, Is.EqualTo("radius"));
    }

    [TestCase(float.NaN, "startAngle")]
    [TestCase(float.PositiveInfinity, "startAngle")]
    [TestCase(float.NegativeInfinity, "startAngle")]
    public void Constructor_WhenStartAngleIsInvalid_Throws(float startAngle, string paramName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Arc(new PointXY(0f, 0f), 1f, startAngle, MathF.PI));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [TestCase(float.NaN, "endAngle")]
    [TestCase(float.PositiveInfinity, "endAngle")]
    [TestCase(float.NegativeInfinity, "endAngle")]
    public void Constructor_WhenEndAngleIsInvalid_Throws(float endAngle, string paramName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Arc(new PointXY(0f, 0f), 1f, 0f, endAngle));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void RayIntersections_WhenRayHitsArc_ReturnsIntersectionOnArc()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI / 2f);
        var ray = new Ray(new PointXY(-1f, 0f));

        var intersections = arc.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 1f, 0f);
    }

    [Test]
    public void FiniteTwoEndpointCurveContract_WhenArcIsUsed_ExposesEndpointsAndLength()
    {
        IFiniteTwoEndpointCurve curve = new Arc(new PointXY(0f, 0f), 2f, 0f, MathF.PI / 2f);

        AssertVector(curve.EndpointA, 2f, 0f);
        AssertVector(curve.EndpointB, 0f, 2f);
        Assert.That(curve.Length, Is.EqualTo(MathF.PI).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ExplicitConversionToArc_WhenParameterizedArcIsClockwise_ReturnsSameGeometricRegion()
    {
        var parameterizedArc = new ParameterizedArc(
            new PointXY(0f, 0f),
            1f,
            0f,
            MathF.PI / 2f,
            AngularDirection.Clockwise);

        Arc arc = (Arc)parameterizedArc;

        Assert.That(arc.IsWithinAngularRegion(new PointXY(-1f, 0f)), Is.True);
        Assert.That(arc.IsWithinAngularRegion(new PointXY(1f, 1f)), Is.False);
    }

    [Test]
    public void ExplicitConversionToArc_WhenParameterizedArcIsFullCircle_PreservesFullCircle()
    {
        var parameterizedArc = new ParameterizedArc(
            new PointXY(0f, 0f),
            1f,
            0f,
            -2f * MathF.PI,
            AngularDirection.Clockwise);

        Arc arc = (Arc)parameterizedArc;

        Assert.That(arc.IsFullCircle, Is.True);
        Assert.That(arc.Length, Is.EqualTo(2f * MathF.PI).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void RayIntersections_WhenRayHitsCircleOutsideArc_ReturnsEmpty()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI / 2f);
        var ray = new Ray(new PointXY(-2f, -1f));

        var intersections = arc.GetRayIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void RayIntersections_WithCustomGeometryEpsilon_WhenRayNearlyTouchesArc_ReturnsSingleIntersection()
    {
        const float geometryEpsilon = 0.001f;
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI);
        var ray = new Ray(new PointXY(-2f, 1.00005f));

        var defaultIntersections = arc.GetRayIntersections(ray);
        var tolerantIntersections = arc.GetRayIntersections(ray, geometryEpsilon);

        Assert.That(defaultIntersections, Is.Empty);
        Assert.That(tolerantIntersections, Has.Count.EqualTo(1));
        AssertVector(tolerantIntersections[0], 0f, 1.00005f);
    }

    [Test]
    public void RayIntersections_WithCustomGeometryEpsilon_WhenParameterizedArcIntersectionIsJustBehindRayOrigin_ReturnsIntersection()
    {
        const float geometryEpsilon = 0.001f;
        var arc = new ParameterizedArc(
            new PointXY(0f, 0f),
            1f,
            0f,
            2f * MathF.PI,
            AngularDirection.Counterclockwise);
        var ray = new Ray(new PointXY(1.0005f, 0f));

        var defaultIntersections = arc.GetRayIntersections(ray);
        var tolerantIntersections = arc.GetRayIntersections(ray, geometryEpsilon);

        Assert.That(defaultIntersections, Is.Empty);
        Assert.That(tolerantIntersections, Has.Count.EqualTo(1));
        AssertVector(tolerantIntersections[0], 1f, 0f);
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void RayIntersections_WhenGeometryEpsilonIsInvalid_Throws(float geometryEpsilon)
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI);
        var ray = new Ray(new PointXY(0f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            arc.GetRayIntersections(ray, geometryEpsilon));

        Assert.That(exception!.ParamName, Is.EqualTo("geometryEpsilon"));
    }

    [Test]
    public void Distance_WhenPointIsNearArcEndpoint_UsesNearestEndpoint()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI / 2f);
        var point = new PointXY(0f, 2f);

        var distance = arc.Distance(point);

        Assert.That(distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void IsWithinAngularRegion_WhenPointAngleIsWithinArc_ReturnsTrue()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI / 2f);

        bool contains = arc.IsWithinAngularRegion(new PointXY(2f, 2f));

        Assert.That(contains, Is.True);
    }

    [Test]
    public void IsWithinAngularRegion_WhenPointAngleIsOutsideArc_ReturnsFalse()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI / 2f);

        bool contains = arc.IsWithinAngularRegion(new PointXY(-1f, 1f));

        Assert.That(contains, Is.False);
    }

    [Test]
    public void ProjectWithParameter_WhenParameterizedArcStartAndEndAnglesAreEqual_TreatsArcAsZeroLength()
    {
        var arc = new ParameterizedArc(new PointXY(0f, 0f), 1f, 0f, 0f, AngularDirection.Counterclockwise);

        Assert.That(arc.IsFullCircle, Is.False);

        var projection = arc.ProjectWithParameter(new PointXY(0f, 1f));

        AssertVector(projection.ProjectedPoint, 1f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(MathF.Sqrt(2f)).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ProjectWithParameter_WhenParameterizedArcSweepIsAlmostZero_PrefersStableEndpointProjection()
    {
        float tinySweep = GeometryConstants.GeometryEpsilon * 0.5f;
        var arc = new ParameterizedArc(
            new PointXY(0f, 0f),
            1f,
            0f,
            tinySweep,
            AngularDirection.Counterclockwise);

        var projection = arc.ProjectWithParameter(new PointXY(2f, 0f));

        Assert.That(arc.IsFullCircle, Is.False);
        Assert.That(arc.Length, Is.EqualTo(tinySweep).Within(GeometryConstants.GeometryEpsilon));
        AssertVector(projection.ProjectedPoint, 1f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void RayIntersections_WhenStartAndEndAnglesAreEqual_ReturnsOnlyZeroArcPoint()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 0f);
        var ray = new Ray(new PointXY(-2f, 0f));

        var intersections = arc.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 1f, 0f);
    }

    [Test]
    public void RayIntersections_WhenStartAndEndAnglesAreEqualAndRayMissesZeroArcPoint_ReturnsEmpty()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 0f);
        var ray = new Ray(new PointXY(0f, -2f), MathF.PI / 2f);

        var intersections = arc.GetRayIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void ProjectWithParameter_WhenParameterizedArcStopAngleIsOneFullTurnAfterStart_TreatsArcAsFullCircle()
    {
        var arc = new ParameterizedArc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI, AngularDirection.Counterclockwise);

        Assert.That(arc.IsFullCircle, Is.True);

        var projection = arc.ProjectWithParameter(new PointXY(0f, 2f));

        AssertVector(projection.ProjectedPoint, 0f, 1f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(MathF.PI / 2f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ProjectWithParameter_WhenParameterizedArcPointIsAtArcCenter_ReturnsStartPoint()
    {
        var arc = new ParameterizedArc(
            new PointXY(1f, 1f),
            2f,
            MathF.PI / 2f,
            MathF.PI,
            AngularDirection.Counterclockwise);

        var projection = arc.ProjectWithParameter(new PointXY(1f, 1f));

        AssertVector(projection.ProjectedPoint, 1f, 3f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Project_WhenRadiusIsZero_ReturnsCenter()
    {
        var arc = new Arc(new PointXY(1f, 1f), 0f, 0f, MathF.PI);

        var projection = arc.Project(new PointXY(4f, 5f));

        AssertVector(projection.ProjectedPoint, 1f, 1f);
        Assert.That(projection.Distance, Is.EqualTo(5f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void IsWithinAngularRegion_WhenPointCoordinateIsInvalid_Throws()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            arc.IsWithinAngularRegion(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void ProjectWithParameter_WhenParameterizedArcPointCoordinateIsInvalid_Throws()
    {
        var arc = new ParameterizedArc(new PointXY(0f, 0f), 1f, 0f, MathF.PI, AngularDirection.Counterclockwise);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            arc.ProjectWithParameter(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void RayIntersections_WhenRadiusIsZeroAndRayPassesThroughCenter_ReturnsCenter()
    {
        var arc = new Arc(new PointXY(1f, 1f), 0f, MathF.PI / 2f, MathF.PI);
        var ray = new Ray(new PointXY(1f, -1f), MathF.PI / 2f);

        var intersections = arc.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 1f, 1f);
    }

    [Test]
    public void RayIntersections_WhenStopAngleIsOneFullTurnAfterStart_ReturnsCircleIntersections()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);
        var ray = new Ray(new PointXY(-2f, 0f));

        var intersections = arc.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertVector(intersections[0], -1f, 0f);
        AssertVector(intersections[1], 1f, 0f);
    }

    [Test]
    public void Equals_WhenOneArcIsZeroLengthAndOtherIsFullCircle_ReturnsFalse()
    {
        var zeroArc = new Arc(new PointXY(0f, 0f), 1f, 0f, 0f);
        var fullCircle = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);

        Assert.That(zeroArc, Is.Not.EqualTo(fullCircle));
        Assert.That(zeroArc.GetHashCode(), Is.Not.EqualTo(fullCircle.GetHashCode()));
    }

    [Test]
    public void ToString_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        try
        {
            var arc = new ParameterizedArc(
                new PointXY(1.5f, 2.25f),
                3.5f,
                0.25f,
                1.5f,
                AngularDirection.Counterclockwise);

            Assert.That(
                arc.ToString(),
                Is.EqualTo("ParameterizedArc(center: (1.5, 2.25), radius: 3.5, rad: 0.25 - 1.5, direction: Counterclockwise, fullCircle: False)"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private static void AssertVector(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }
}
