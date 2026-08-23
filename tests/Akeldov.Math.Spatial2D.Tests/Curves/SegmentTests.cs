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

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void ParameterizedSegmentShorten_WhenAmountIsInvalid_Throws(float amount)
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(10f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => segment.Shorten(amount));

        Assert.That(exception!.ParamName, Is.EqualTo("amount"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void ParameterizedSegmentExtend_WhenAmountIsInvalid_Throws(float amount)
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(10f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => segment.Extend(amount));

        Assert.That(exception!.ParamName, Is.EqualTo("amount"));
    }

    [TestCase("ShortenStart")]
    [TestCase("ShortenEnd")]
    [TestCase("ExtendStart")]
    [TestCase("ExtendEnd")]
    public void ParameterizedSegmentOneSidedResize_WhenAmountIsInvalid_Throws(string methodName)
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(10f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ResizeParameterizedSegment(segment, methodName, float.NaN));

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
    public void ParameterizedSegmentShorten_WhenSegmentHasEndpointInclusion_PreservesDirectionAndEndpointInclusion()
    {
        var segment = new ParameterizedSegment(
            new PointXY(10f, 0f),
            new PointXY(0f, 0f),
            includesStartPoint: false,
            includesEndPoint: true);

        var shortened = segment.Shorten(1f);

        AssertVector(shortened.StartPoint, 9f, 0f);
        AssertVector(shortened.EndPoint, 1f, 0f);
        Assert.That(shortened.IncludesStartPoint, Is.False);
        Assert.That(shortened.IncludesEndPoint, Is.True);
    }

    [Test]
    public void ParameterizedSegmentExtend_WhenSegmentHasEndpointInclusion_PreservesDirectionAndEndpointInclusion()
    {
        var segment = new ParameterizedSegment(
            new PointXY(10f, 0f),
            new PointXY(0f, 0f),
            includesStartPoint: true,
            includesEndPoint: false);

        var extended = segment.Extend(1f);

        AssertVector(extended.StartPoint, 11f, 0f);
        AssertVector(extended.EndPoint, -1f, 0f);
        Assert.That(extended.IncludesStartPoint, Is.True);
        Assert.That(extended.IncludesEndPoint, Is.False);
    }

    [Test]
    public void ParameterizedSegmentOneSidedShorten_WhenSegmentDirectionIsReversed_PreservesDirectionAndEndpointInclusion()
    {
        var segment = new ParameterizedSegment(
            new PointXY(10f, 0f),
            new PointXY(0f, 0f),
            includesStartPoint: false,
            includesEndPoint: true);

        var shortenedStart = segment.ShortenStart(2f);
        var shortenedEnd = segment.ShortenEnd(2f);

        AssertVector(shortenedStart.StartPoint, 8f, 0f);
        AssertVector(shortenedStart.EndPoint, 0f, 0f);
        Assert.That(shortenedStart.IncludesStartPoint, Is.False);
        Assert.That(shortenedStart.IncludesEndPoint, Is.True);

        AssertVector(shortenedEnd.StartPoint, 10f, 0f);
        AssertVector(shortenedEnd.EndPoint, 2f, 0f);
        Assert.That(shortenedEnd.IncludesStartPoint, Is.False);
        Assert.That(shortenedEnd.IncludesEndPoint, Is.True);
    }

    [Test]
    public void ParameterizedSegmentOneSidedExtend_WhenSegmentDirectionIsReversed_PreservesDirectionAndEndpointInclusion()
    {
        var segment = new ParameterizedSegment(
            new PointXY(10f, 0f),
            new PointXY(0f, 0f),
            includesStartPoint: true,
            includesEndPoint: false);

        var extendedStart = segment.ExtendStart(2f);
        var extendedEnd = segment.ExtendEnd(2f);

        AssertVector(extendedStart.StartPoint, 12f, 0f);
        AssertVector(extendedStart.EndPoint, 0f, 0f);
        Assert.That(extendedStart.IncludesStartPoint, Is.True);
        Assert.That(extendedStart.IncludesEndPoint, Is.False);

        AssertVector(extendedEnd.StartPoint, 10f, 0f);
        AssertVector(extendedEnd.EndPoint, -2f, 0f);
        Assert.That(extendedEnd.IncludesStartPoint, Is.True);
        Assert.That(extendedEnd.IncludesEndPoint, Is.False);
    }

    [Test]
    public void ParameterizedSegmentShorten_WhenAmountIsHalfLength_ReturnsZeroLengthPathAtMidpoint()
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(10f, 0f));

        var shortened = segment.Shorten(5f);

        AssertVector(shortened.StartPoint, 5f, 0f);
        AssertVector(shortened.EndPoint, 5f, 0f);
    }

    [Test]
    public void ParameterizedSegmentOneSidedShorten_WhenAmountIsLength_ReturnsZeroLengthPathAtOppositeEndpoint()
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(10f, 0f));

        var shortenedStart = segment.ShortenStart(10f);
        var shortenedEnd = segment.ShortenEnd(10f);

        AssertVector(shortenedStart.StartPoint, 10f, 0f);
        AssertVector(shortenedStart.EndPoint, 10f, 0f);

        AssertVector(shortenedEnd.StartPoint, 0f, 0f);
        AssertVector(shortenedEnd.EndPoint, 0f, 0f);
    }

    [Test]
    public void ParameterizedSegmentShorten_WhenAmountIsTooLarge_Throws()
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(10f, 0f));

        Assert.Throws<InvalidOperationException>(() => segment.Shorten(6f));
    }

    [Test]
    public void ParameterizedSegmentOneSidedShorten_WhenAmountIsTooLarge_Throws()
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(10f, 0f));

        Assert.Throws<InvalidOperationException>(() => segment.ShortenStart(11f));
        Assert.Throws<InvalidOperationException>(() => segment.ShortenEnd(11f));
    }

    [Test]
    public void ParameterizedSegmentShorten_WhenSegmentHasEqualEndpoints_Throws()
    {
        var segment = new ParameterizedSegment(new PointXY(1f, 2f), new PointXY(1f, 2f));

        Assert.Throws<InvalidOperationException>(() => segment.Shorten(0f));
    }

    [Test]
    public void ParameterizedSegmentExtend_WhenSegmentHasEqualEndpoints_Throws()
    {
        var segment = new ParameterizedSegment(new PointXY(1f, 2f), new PointXY(1f, 2f));

        Assert.Throws<InvalidOperationException>(() => segment.Extend(0f));
    }

    [TestCase("ShortenStart")]
    [TestCase("ShortenEnd")]
    [TestCase("ExtendStart")]
    [TestCase("ExtendEnd")]
    public void ParameterizedSegmentOneSidedResize_WhenSegmentHasEqualEndpoints_Throws(string methodName)
    {
        var segment = new ParameterizedSegment(new PointXY(1f, 2f), new PointXY(1f, 2f));

        Assert.Throws<InvalidOperationException>(() =>
            ResizeParameterizedSegment(segment, methodName, 0f));
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
    public void PointIntersections_WhenRayCrossesSegmentInterior_ReturnsIntersection()
    {
        var segment = new Segment(new PointXY(1f, -1f), new PointXY(1f, 1f));
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 1f, 0f);
    }

    [Test]
    public void PointIntersections_WhenEndpointIsExcluded_DoesNotReturnThatEndpoint()
    {
        var segment = new Segment(new PointXY(1f, 0f), new PointXY(1f, 1f), includesEndpointA: false, includesEndpointB: true);
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetPointIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void PointIntersections_WhenEndpointIsIncluded_ReturnsThatEndpoint()
    {
        var segment = new Segment(new PointXY(1f, 0f), new PointXY(1f, 1f), includesEndpointA: true, includesEndpointB: true);
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 1f, 0f);
    }

    [Test]
    public void PointIntersections_WhenRayOverlapsSegmentContinuously_ReturnsEmpty()
    {
        var segment = new Segment(new PointXY(0f, 0f), new PointXY(10f, 0f));
        var ray = new Ray(new PointXY(4f, 0f));

        var intersections = segment.GetPointIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void PointIntersections_WhenRayTouchesOnlySegmentEndpoint_ReturnsEndpoint()
    {
        var segment = new Segment(new PointXY(-10f, 0f), new PointXY(0f, 0f));
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 0f, 0f);
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
    public void PointIntersections_WhenSegmentIsBehindRay_ReturnsEmpty()
    {
        var segment = new Segment(new PointXY(-4f, 0f), new PointXY(-2f, 0f));
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetPointIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void PointIntersections_WhenSegmentCrossesJustAheadOfRayOrigin_ReturnsIntersection()
    {
        float tiny = GeometryConstants.GeometryEpsilon * 0.5f;
        var segment = new Segment(
            new PointXY(tiny, -1f),
            new PointXY(tiny, 1f));
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], tiny, 0f);
    }

    [Test]
    public void PointIntersections_WhenDegenerateSegmentPointIsOnRay_ReturnsPoint()
    {
        var segment = new Segment(new PointXY(2f, 0f), new PointXY(2f, 0f));
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 2f, 0f);
    }

    [Test]
    public void PointIntersections_WhenDegenerateSegmentPointIsExcluded_ReturnsEmpty()
    {
        var segment = new Segment(new PointXY(2f, 0f), new PointXY(2f, 0f), includesEndpointA: false, includesEndpointB: false);
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = segment.GetPointIntersections(ray);

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
    public void GetPoint_WhenSegmentIsShorterThanGeometryEpsilon_InterpolatesAlongSegment()
    {
        float tiny = GeometryConstants.GeometryEpsilon * 0.5f;
        var segment = new ParameterizedSegment(
            new PointXY(0f, 0f),
            new PointXY(tiny, tiny));

        PointXY point = segment.GetPoint(segment.Length);

        Assert.That(point.X, Is.EqualTo(tiny).Within(tiny * 0.01f));
        Assert.That(point.Y, Is.EqualTo(tiny).Within(tiny * 0.01f));
    }

    [Test]
    public void ProjectWithParameter_WhenEndpointsAlmostCoincide_ProjectsOntoShortSegment()
    {
        float tiny = GeometryConstants.GeometryEpsilon * 0.5f;
        var segment = new ParameterizedSegment(
            new PointXY(0f, 0f),
            new PointXY(tiny, tiny));

        var projection = segment.ProjectWithParameter(new PointXY(1f, 1f));

        Assert.That(projection.ProjectedPoint.X, Is.EqualTo(tiny).Within(tiny * 0.01f));
        Assert.That(projection.ProjectedPoint.Y, Is.EqualTo(tiny).Within(tiny * 0.01f));
        Assert.That(projection.CurveCoordinate, Is.EqualTo(segment.Length).Within(tiny * 0.01f));
        Assert.That(
            projection.Distance,
            Is.EqualTo(new PointXY(1f, 1f).Distance(segment.EndPoint)).Within(GeometryConstants.GeometryEpsilon));
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
    public void GetHalfPlaneSide_WhenPointIsLeftOfStartToEndDirection_ReturnsLeft()
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(2f, 0f));

        HalfPlaneSide side = segment.GetHalfPlaneSide(new PointXY(1f, 3f));

        Assert.That(side, Is.EqualTo(HalfPlaneSide.Left));
    }

    [Test]
    public void GetHalfPlaneSide_WhenPointIsRightOfStartToEndDirection_ReturnsRight()
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(2f, 0f));

        HalfPlaneSide side = segment.GetHalfPlaneSide(new PointXY(1f, -3f));

        Assert.That(side, Is.EqualTo(HalfPlaneSide.Right));
    }

    [Test]
    public void GetHalfPlaneSide_WhenSegmentDirectionIsReversed_UsesStartToEndDirection()
    {
        var segment = new ParameterizedSegment(new PointXY(2f, 0f), new PointXY(0f, 0f));

        HalfPlaneSide side = segment.GetHalfPlaneSide(new PointXY(1f, 3f));

        Assert.That(side, Is.EqualTo(HalfPlaneSide.Right));
    }

    [Test]
    public void GetHalfPlaneSide_WhenPointIsOnSupportingLineOutsideSegment_ReturnsOnTheLine()
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(2f, 0f));

        HalfPlaneSide side = segment.GetHalfPlaneSide(new PointXY(3f, 0f));

        Assert.That(side, Is.EqualTo(HalfPlaneSide.OnTheLine));
    }

    [Test]
    public void GetHalfPlaneSide_WhenPointIsOnSupportingLineWithinTolerance_ReturnsOnTheLine()
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(2f, 0f));

        HalfPlaneSide side = segment.GetHalfPlaneSide(new PointXY(3f, 0.005f), geometryEpsilon: 0.01f);

        Assert.That(side, Is.EqualTo(HalfPlaneSide.OnTheLine));
    }

    [Test]
    public void GetHalfPlaneSide_WhenSegmentHasEqualEndpoints_Throws()
    {
        var segment = new ParameterizedSegment(new PointXY(1f, 2f), new PointXY(1f, 2f));

        Assert.Throws<InvalidOperationException>(() =>
            segment.GetHalfPlaneSide(new PointXY(1f, 3f)));
    }

    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void GetHalfPlaneSide_WhenPointCoordinateIsInvalid_Throws(float x, float y)
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            segment.GetHalfPlaneSide(new PointXY(x, y)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void GetHalfPlaneSide_WhenGeometryEpsilonIsInvalid_Throws(float geometryEpsilon)
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            segment.GetHalfPlaneSide(new PointXY(0f, 0f), geometryEpsilon));

        Assert.That(exception!.ParamName, Is.EqualTo("geometryEpsilon"));
    }

    [Test]
    public void Equals_WhenEndpointInclusionDiffers_ReturnsFalse()
    {
        var closed = new Segment(new PointXY(1f, 0f), new PointXY(1f, 1f), includesEndpointA: true, includesEndpointB: true);
        var openAtA = new Segment(new PointXY(1f, 0f), new PointXY(1f, 1f), includesEndpointA: false, includesEndpointB: true);

        Assert.That(closed, Is.Not.EqualTo(openAtA));
        Assert.That(closed.GetHashCode(), Is.Not.EqualTo(openAtA.GetHashCode()));
    }

    [Test]
    public void PointIntersections_WhenLineCrossesSegment_ReturnsIntersection()
    {
        var segment = new Segment(new PointXY(0f, 0f), new PointXY(4f, 0f));
        var line = new Line(new PointXY(2f, -1f), new PointXY(2f, 1f));

        List<PointXY> intersections = segment.GetPointIntersections(line);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 2f, 0f);
    }

    [Test]
    public void PointIntersections_WhenLineCrossesExcludedEndpoint_ReturnsEmpty()
    {
        var segment = new Segment(
            new PointXY(0f, 0f),
            new PointXY(4f, 0f),
            includesEndpointA: false,
            includesEndpointB: true);
        var line = new Line(new PointXY(0f, -1f), new PointXY(0f, 1f));

        List<PointXY> intersections = segment.GetPointIntersections(line);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void PointIntersections_WhenLineContainsSegment_ReturnsEmpty()
    {
        var segment = new Segment(new PointXY(0f, 0f), new PointXY(4f, 0f));
        var line = new Line(new PointXY(-1f, 0f), new PointXY(1f, 0f));

        List<PointXY> intersections = segment.GetPointIntersections(line);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void ParameterizedSegment_PointIntersections_WhenLineCrossesSegment_ReturnsIntersection()
    {
        var segment = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(4f, 0f));
        var line = new Line(new PointXY(2f, -1f), new PointXY(2f, 1f));

        List<PointXY> intersections = segment.GetPointIntersections(line);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertVector(intersections[0], 2f, 0f);
    }

    private static void AssertVector(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }

    private static ParameterizedSegment ResizeParameterizedSegment(
        ParameterizedSegment segment,
        string methodName,
        float amount)
    {
        switch (methodName)
        {
            case "ShortenStart":
                return segment.ShortenStart(amount);
            case "ShortenEnd":
                return segment.ShortenEnd(amount);
            case "ExtendStart":
                return segment.ExtendStart(amount);
            case "ExtendEnd":
                return segment.ExtendEnd(amount);
            default:
                throw new ArgumentOutOfRangeException(nameof(methodName), methodName, "Unknown resize method.");
        }
    }
}
