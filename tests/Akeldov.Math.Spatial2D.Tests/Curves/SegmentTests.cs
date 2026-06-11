using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class SegmentTests
{
    [TestCase(float.PositiveInfinity, 0f, "startPoint")]
    [TestCase(0f, float.NegativeInfinity, "startPoint")]
    [TestCase(float.PositiveInfinity, 0f, "endPoint")]
    [TestCase(0f, float.NegativeInfinity, "endPoint")]
    public void Constructor_WhenEndpointCoordinateIsInvalid_Throws(float x, float y, string paramName)
    {
        PointXY startPoint = paramName == "startPoint" ? new PointXY(x, y) : new PointXY(0f, 0f);
        PointXY endPoint = paramName == "endPoint" ? new PointXY(x, y) : new PointXY(1f, 1f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Segment(startPoint, endPoint));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Shorten_WhenAmountIsInvalid_Throws(float amount)
    {
        var segment = new Segment(new PointXY(0f, 0f), new PointXY(10f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => segment.Shorten(amount));

        Assert.That(exception!.ParamName, Is.EqualTo("amount"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Extend_WhenAmountIsInvalid_Throws(float amount)
    {
        var segment = new Segment(new PointXY(0f, 0f), new PointXY(10f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => segment.Extend(amount));

        Assert.That(exception!.ParamName, Is.EqualTo("amount"));
    }

    [Test]
    public void Shorten_WhenSegmentHasEndpointInclusion_PreservesEndpointInclusion()
    {
        var segment = new Segment(
            new PointXY(0f, 0f),
            new PointXY(10f, 0f),
            includesEndpointA: false,
            includesEndpointB: true);

        var shortened = segment.Shorten(1f);

        AssertVector(shortened.EndpointA, 1f, 0f);
        AssertVector(shortened.EndpointB, 9f, 0f);
        Assert.That(shortened.IncludesEndpointA, Is.False);
        Assert.That(shortened.IncludesEndpointB, Is.True);
    }

    [Test]
    public void Extend_WhenSegmentHasEndpointInclusion_PreservesEndpointInclusion()
    {
        var segment = new Segment(
            new PointXY(0f, 0f),
            new PointXY(10f, 0f),
            includesEndpointA: true,
            includesEndpointB: false);

        var extended = segment.Extend(1f);

        AssertVector(extended.EndpointA, -1f, 0f);
        AssertVector(extended.EndpointB, 11f, 0f);
        Assert.That(extended.IncludesEndpointA, Is.True);
        Assert.That(extended.IncludesEndpointB, Is.False);
    }

    [Test]
    public void FiniteTwoEndpointCurveContract_WhenSegmentIsUsed_ExposesEndpointsAndLength()
    {
        IFiniteTwoEndpointCurve curve = new Segment(new PointXY(1f, 2f), new PointXY(4f, 6f));

        AssertVector(curve.EndpointA, 1f, 2f);
        AssertVector(curve.EndpointB, 4f, 6f);
        Assert.That(curve.Length, Is.EqualTo(5f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ExplicitConversionToSegment_PreservesEndpointsAndEndpointInclusion()
    {
        var parameterizedSegment = new ParameterizedSegment(
            new PointXY(1f, 2f),
            new PointXY(4f, 6f),
            includesStartPoint: false,
            includesEndPoint: true);

        Segment segment = (Segment)parameterizedSegment;

        AssertVector(segment.EndpointA, 1f, 2f);
        AssertVector(segment.EndpointB, 4f, 6f);
        Assert.That(segment.IncludesEndpointA, Is.False);
        Assert.That(segment.IncludesEndpointB, Is.True);
    }

    [Test]
    public void RayIntersections_WhenRayCrossesSegmentInterior_ReturnsIntersection()
    {
        var segment = new Segment(new PointXY(1f, -1f), new PointXY(1f, 1f));
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 1f, 0f);
    }

    [Test]
    public void RayIntersections_WhenEndpointIsExcluded_DoesNotReturnThatEndpoint()
    {
        var segment = new Segment(new PointXY(1f, 0f), new PointXY(1f, 1f), includesEndpointA: false, includesEndpointB: true);
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetRayIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void RayIntersections_WhenEndpointIsIncluded_ReturnsThatEndpoint()
    {
        var segment = new Segment(new PointXY(1f, 0f), new PointXY(1f, 1f), includesEndpointA: true, includesEndpointB: true);
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 1f, 0f);
    }

    [Test]
    public void RayIntersections_WhenRayStartsInsideCollinearSegment_ReturnsRayOrigin()
    {
        var segment = new Segment(new PointXY(0f, 0f), new PointXY(10f, 0f));
        var ray = new Ray(new PointXY(4f, 0f));

        var intersections = segment.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 4f, 0f);
    }

    [Test]
    public void RayIntersections_WhenCollinearSegmentIsAhead_ReturnsFirstIncludedEndpoint()
    {
        var segment = new Segment(new PointXY(4f, 0f), new PointXY(10f, 0f));
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 4f, 0f);
    }

    [Test]
    public void RayIntersections_WithCustomGeometryEpsilon_WhenSegmentIsNearlyCollinear_ReturnsFirstEndpoint()
    {
        const float geometryEpsilon = 0.01f;
        ICurve segment = new Segment(new PointXY(4f, 0.005f), new PointXY(10f, 0.005f));
        var ray = new Ray(new PointXY(0f, 0f));

        var defaultIntersections = segment.GetRayIntersections(ray);
        var tolerantIntersections = segment.GetRayIntersections(ray, geometryEpsilon);

        Assert.That(defaultIntersections, Is.Empty);
        Assert.That(tolerantIntersections, Has.Count.EqualTo(1));
        AssertVector(tolerantIntersections[0], 4f, 0.005f);
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void RayIntersections_WhenGeometryEpsilonIsInvalid_Throws(float geometryEpsilon)
    {
        ICurve segment = new Segment(new PointXY(1f, -1f), new PointXY(1f, 1f));
        var ray = new Ray(new PointXY(0f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            segment.GetRayIntersections(ray, geometryEpsilon));

        Assert.That(exception!.ParamName, Is.EqualTo("geometryEpsilon"));
    }

    [Test]
    public void RayIntersections_WhenCollinearSegmentStartsAtExcludedRayOrigin_ReturnsEmpty()
    {
        var segment = new Segment(new PointXY(0f, 0f), new PointXY(10f, 0f), includesEndpointA: false, includesEndpointB: true);
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetRayIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void ProjectWithParameter_WhenPointProjectsOutsideSegment_ClampsToNearestEndpoint()
    {
        var segment = new ParameterizedSegment(new PointXY(2f, 0f), new PointXY(4f, 0f));

        var projection = segment.ProjectWithParameter(new PointXY(0f, 0f));

        AssertVector(projection.ProjectedPoint, 2f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void RayIntersections_WhenSegmentIsBehindRay_ReturnsEmpty()
    {
        var segment = new Segment(new PointXY(-4f, 0f), new PointXY(-2f, 0f));
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetRayIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void RayIntersections_WhenSegmentCrossesJustAheadOfRayOrigin_ReturnsIntersection()
    {
        float tiny = GeometryConstants.GeometryEpsilon * 0.5f;
        var segment = new Segment(
            new PointXY(tiny, -1f),
            new PointXY(tiny, 1f));
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], tiny, 0f);
    }

    [Test]
    public void RayIntersections_WhenDegenerateSegmentPointIsOnRay_ReturnsPoint()
    {
        var segment = new Segment(new PointXY(2f, 0f), new PointXY(2f, 0f));
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 2f, 0f);
    }

    [Test]
    public void RayIntersections_WhenDegenerateSegmentPointIsExcluded_ReturnsEmpty()
    {
        var segment = new Segment(new PointXY(2f, 0f), new PointXY(2f, 0f), includesEndpointA: false, includesEndpointB: false);
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetRayIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void ProjectWithParameter_WhenPointProjectsInsideSegment_ReturnsInteriorProjection()
    {
        var segment = new ParameterizedSegment(new PointXY(2f, 0f), new PointXY(4f, 0f));

        var projection = segment.ProjectWithParameter(new PointXY(3f, 2f));

        AssertVector(projection.ProjectedPoint, 3f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ProjectWithParameter_WhenSegmentIsDegenerate_ReturnsEndpoint()
    {
        var segment = new ParameterizedSegment(new PointXY(2f, 3f), new PointXY(2f, 3f));

        var projection = segment.ProjectWithParameter(new PointXY(5f, 7f));

        AssertVector(projection.ProjectedPoint, 2f, 3f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(5f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ProjectWithParameter_WhenEndpointsAlmostCoincide_TreatsSegmentAsPoint()
    {
        float tiny = GeometryConstants.GeometryEpsilon * 0.5f;
        var segment = new ParameterizedSegment(
            new PointXY(0f, 0f),
            new PointXY(tiny, tiny));

        var projection = segment.ProjectWithParameter(new PointXY(1f, 1f));

        AssertVector(projection.ProjectedPoint, 0f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(MathF.Sqrt(2f)).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Distance_WhenPointCoordinateIsInvalid_Throws()
    {
        var segment = new Segment(new PointXY(0f, 0f), new PointXY(1f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            segment.Distance(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void ProjectWithParameter_WhenPointCoordinateIsInvalid_Throws()
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 1f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            segment.ProjectWithParameter(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void Equals_WhenEndpointInclusionDiffers_ReturnsFalse()
    {
        var closed = new Segment(new PointXY(1f, 0f), new PointXY(1f, 1f), includesEndpointA: true, includesEndpointB: true);
        var openAtA = new Segment(new PointXY(1f, 0f), new PointXY(1f, 1f), includesEndpointA: false, includesEndpointB: true);

        Assert.That(closed, Is.Not.EqualTo(openAtA));
        Assert.That(closed.GetHashCode(), Is.Not.EqualTo(openAtA.GetHashCode()));
    }

    private static void AssertVector(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }
}
