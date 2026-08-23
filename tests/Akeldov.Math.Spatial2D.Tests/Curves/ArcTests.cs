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
    public void PointIntersections_WhenRayHitsArc_ReturnsIntersectionOnArc()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI / 2f);
        var ray = new Ray(new PointXY(-1f, 0f));

        var intersections = arc.GetPointIntersections(ray);

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
    public void PointIntersections_WhenRayHitsCircleOutsideArc_ReturnsEmpty()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI / 2f);
        var ray = new Ray(new PointXY(-2f, -1f));

        var intersections = arc.GetPointIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void PointIntersections_WhenLineCrossesArc_ReturnsPointsInLineDirection()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI);
        var line = new Line(new PointXY(-2f, 0f), new PointXY(2f, 0f));

        List<PointXY> intersections = arc.GetPointIntersections(line);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertVector(intersections[0], -1f, 0f);
        AssertVector(intersections[1], 1f, 0f);
    }

    [Test]
    public void PointIntersections_WhenParameterizedLineDirectionIsReversed_ReturnsPointsInParameterizedDirection()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI);
        var geometricLine = new Line(new PointXY(-2f, 0f), new PointXY(2f, 0f));
        var line = new ParameterizedLine(geometricLine, new PointXY(0f, 0f), new VectorXY(-1f, 0f));

        List<PointXY> intersections = arc.GetPointIntersections(line);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertVector(intersections[0], 1f, 0f);
        AssertVector(intersections[1], -1f, 0f);
    }

    [Test]
    public void PointIntersections_WhenSegmentDirectionIsReversed_ReturnsPointsFromEndpointAToEndpointB()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI);
        var segment = new Segment(new PointXY(2f, 0f), new PointXY(-2f, 0f));

        List<PointXY> intersections = arc.GetPointIntersections(segment);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertVector(intersections[0], 1f, 0f);
        AssertVector(intersections[1], -1f, 0f);
    }

    [Test]
    public void PointIntersections_WithParameterizedSegment_ReturnsPointsFromStartToEnd()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI);
        var segment = new ParameterizedSegment(new PointXY(2f, 0f), new PointXY(-2f, 0f));

        List<PointXY> intersections = arc.GetPointIntersections(segment);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertVector(intersections[0], 1f, 0f);
        AssertVector(intersections[1], -1f, 0f);
    }

    [Test]
    public void PointIntersections_WhenLineCrossesCircleOutsideArc_ReturnsEmpty()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI / 2f);
        var line = new Line(new PointXY(-0.5f, -2f), new PointXY(-0.5f, 2f));

        List<PointXY> intersections = arc.GetPointIntersections(line);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void PointIntersections_WhenLineIsTangentToFullCircle_ReturnsOnePoint()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);
        var line = new Line(new PointXY(-2f, 1f), new PointXY(2f, 1f));

        List<PointXY> intersections = arc.GetPointIntersections(line);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 0f, 1f);
    }

    [Test]
    public void PointIntersections_WhenLineIsOutsideFullCircleWithinGeometryEpsilon_ReturnsEmpty()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);
        float lineY = 1f + GeometryConstants.GeometryEpsilon * 0.5f;
        var line = new Line(new PointXY(-2f, lineY), new PointXY(2f, lineY));

        List<PointXY> intersections = arc.GetPointIntersections(line);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void PointIntersections_WhenZeroRadiusArcLiesOnLine_ReturnsCenter()
    {
        var arc = new Arc(new PointXY(1f, 2f), 0f, 0f, MathF.PI);
        var line = new Line(new PointXY(1f, -1f), new PointXY(1f, 3f));

        List<PointXY> intersections = arc.GetPointIntersections(line);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 1f, 2f);
    }

    [Test]
    public void ParameterizedArc_PointIntersections_WhenLineCrossesArc_ReturnsIntersection()
    {
        var arc = new ParameterizedArc(
            new PointXY(0f, 0f),
            1f,
            MathF.PI / 2f,
            0f,
            AngularDirection.Clockwise);
        var line = new Line(new PointXY(0f, -2f), new PointXY(0f, 2f));

        List<PointXY> intersections = arc.GetPointIntersections(line);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 0f, 1f);
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
    public void IsWithinAngularRegion_WhenPointIsAtArcCenter_ReturnsTrue()
    {
        var arc = new Arc(new PointXY(1f, 2f), 3f, MathF.PI / 2f, MathF.PI);

        bool contains = arc.IsWithinAngularRegion(new PointXY(1f, 2f));

        Assert.That(contains, Is.True);
    }

    [Test]
    public void IsWithinAngularRegion_WhenParameterizedArcPointIsAtArcCenter_ReturnsTrue()
    {
        var arc = new ParameterizedArc(
            new PointXY(1f, 2f),
            3f,
            MathF.PI / 2f,
            MathF.PI,
            AngularDirection.Counterclockwise);

        bool contains = arc.IsWithinAngularRegion(new PointXY(1f, 2f));

        Assert.That(contains, Is.True);
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
    public void PointIntersections_WhenStartAndEndAnglesAreEqual_ReturnsOnlyZeroArcPoint()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 0f);
        var ray = new Ray(new PointXY(-2f, 0f));

        var intersections = arc.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 1f, 0f);
    }

    [Test]
    public void PointIntersections_WhenStartAndEndAnglesAreEqualAndRayMissesZeroArcPoint_ReturnsEmpty()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 0f);
        var ray = new Ray(new PointXY(0f, -2f), MathF.PI / 2f);

        var intersections = arc.GetPointIntersections(ray);

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
    public void PointIntersections_WhenRadiusIsZeroAndRayPassesThroughCenter_ReturnsCenter()
    {
        var arc = new Arc(new PointXY(1f, 1f), 0f, MathF.PI / 2f, MathF.PI);
        var ray = new Ray(new PointXY(1f, -1f), MathF.PI / 2f);

        var intersections = arc.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 1f, 1f);
    }

    [Test]
    public void PointIntersections_WhenStopAngleIsOneFullTurnAfterStart_ReturnsCircleIntersections()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);
        var ray = new Ray(new PointXY(-2f, 0f));

        var intersections = arc.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertVector(intersections[0], -1f, 0f);
        AssertVector(intersections[1], 1f, 0f);
    }

    [Test]
    public void PointIntersections_WithArc_ReturnsPointsCounterclockwiseFromTargetStartAngle()
    {
        var line = new Line(new PointXY(0f, -2f), new PointXY(0f, 2f));
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);

        List<PointXY> intersections = line.GetPointIntersections(arc);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertVector(intersections[0], 0f, 1f);
        AssertVector(intersections[1], 0f, -1f);
    }

    [Test]
    public void PointIntersections_WithClockwiseParameterizedArc_ReturnsPointsInAngularDirection()
    {
        var line = new Line(new PointXY(0f, -2f), new PointXY(0f, 2f));
        var arc = new ParameterizedArc(
            new PointXY(0f, 0f),
            1f,
            0f,
            2f * MathF.PI,
            AngularDirection.Clockwise);

        List<PointXY> intersections = line.GetPointIntersections(arc);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertVector(intersections[0], 0f, -1f);
        AssertVector(intersections[1], 0f, 1f);
    }

    [Test]
    public void PointIntersections_WhenTwoCirclesIntersect_ReturnsPointsInTargetArcOrder()
    {
        var source = new Arc(new PointXY(1f, 0f), 1f, 0f, 2f * MathF.PI);
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);

        List<PointXY> intersections = source.GetPointIntersections(arc);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertVector(intersections[0], 0.5f, MathF.Sqrt(3f) / 2f);
        AssertVector(intersections[1], 0.5f, -MathF.Sqrt(3f) / 2f);
    }

    [Test]
    public void PointIntersections_WhenConcentricArcsTouchAtEndpoint_ReturnsIsolatedPoint()
    {
        var source = new Arc(new PointXY(0f, 0f), 1f, MathF.PI / 2f, MathF.PI);
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI / 2f);

        List<PointXY> intersections = source.GetPointIntersections(arc);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 0f, 1f);
    }

    [Test]
    public void PointIntersections_WhenConcentricArcsContinuouslyOverlap_ReturnsEmpty()
    {
        var source = new Arc(new PointXY(0f, 0f), 1f, 0f, MathF.PI);
        var arc = new Arc(new PointXY(0f, 0f), 1f, MathF.PI / 2f, 3f * MathF.PI / 2f);

        List<PointXY> intersections = source.GetPointIntersections(arc);

        Assert.That(intersections, Is.Empty);
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
    public void DegreeMembers_WhenAnglesAreNormalized_ReturnDegrees()
    {
        var arc = new Arc(new PointXY(1.5f, 2.25f), 3.5f, -MathF.PI / 2f, MathF.PI);

        Assert.That(arc.StartAngleDeg, Is.EqualTo(270f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(arc.EndAngleDeg, Is.EqualTo(180f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ToDegreesString_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        try
        {
            var arc = new Arc(
                new PointXY(1.5f, 2.25f),
                3.5f,
                0f,
                MathF.PI);

            Assert.That(
                arc.ToDegreesString(),
                Is.EqualTo("Arc(center: (1.5, 2.25), radius: 3.5, deg: 0 - 180, fullCircle: False)"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
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
