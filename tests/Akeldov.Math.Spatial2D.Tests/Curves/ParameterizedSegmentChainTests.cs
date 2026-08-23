using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class ParameterizedSegmentChainTests
{
    [Test]
    public void Constructor_WhenPointsAreProvided_CopiesPointsAndCreatesSegments()
    {
        var points = new[]
        {
            new PointXY(0f, 0f),
            new PointXY(3f, 0f),
            new PointXY(3f, 4f)
        };

        var chain = new ParameterizedSegmentChain(points);
        points[1] = new PointXY(10f, 10f);

        Assert.That(chain.Points, Has.Count.EqualTo(3));
        AssertPoint(chain.Points[0], 0f, 0f);
        AssertPoint(chain.Points[1], 3f, 0f);
        AssertPoint(chain.Points[2], 3f, 4f);

        Assert.That(chain.Segments, Has.Count.EqualTo(2));
        AssertPoint(chain.Segments[0].StartPoint, 0f, 0f);
        AssertPoint(chain.Segments[0].EndPoint, 3f, 0f);
        AssertPoint(chain.Segments[1].StartPoint, 3f, 0f);
        AssertPoint(chain.Segments[1].EndPoint, 3f, 4f);
    }

    [Test]
    public void Properties_WhenChainIsCreated_ReportPathEndpointsAndLength()
    {
        var chain = new ParameterizedSegmentChain(
            new PointXY(0f, 0f),
            new PointXY(3f, 0f),
            new PointXY(3f, 4f));

        AssertPoint(chain.StartPoint, 0f, 0f);
        AssertPoint(chain.EndPoint, 3f, 4f);
        AssertPoint(chain.EndpointA, 0f, 0f);
        AssertPoint(chain.EndpointB, 3f, 4f);
        Assert.That(chain.Length, Is.EqualTo(7f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Constructor_WhenPointListIsInvalid_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ParameterizedSegmentChain(null!));
        Assert.Throws<ArgumentException>(() => new ParameterizedSegmentChain(new PointXY(0f, 0f)));
        Assert.Throws<ArgumentException>(() => new ParameterizedSegmentChain(
            new PointXY(0f, 0f),
            new PointXY(0f, 0f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterizedSegmentChain(
            new PointXY(0f, 0f),
            new PointXY(float.PositiveInfinity, 0f)));
    }

    [Test]
    public void GetPoint_WhenCoordinateCrossesSegmentBoundary_ReturnsPointOnCorrectSegment()
    {
        var chain = new ParameterizedSegmentChain(
            new PointXY(0f, 0f),
            new PointXY(3f, 0f),
            new PointXY(3f, 4f));

        AssertPoint(chain.GetPoint(0f), 0f, 0f);
        AssertPoint(chain.GetPoint(2f), 2f, 0f);
        AssertPoint(chain.GetPoint(5f), 3f, 2f);
        AssertPoint(chain.GetPoint(chain.Length), 3f, 4f);
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void GetPoint_WhenCoordinateIsInvalid_Throws(float curveCoordinate)
    {
        var chain = new ParameterizedSegmentChain(
            new PointXY(0f, 0f),
            new PointXY(3f, 0f));

        Assert.Throws<ArgumentOutOfRangeException>(() => chain.GetPoint(curveCoordinate));
    }

    [Test]
    public void ProjectWithParameter_WhenClosestPointIsOnLaterSegment_ReturnsChainCoordinate()
    {
        var chain = new ParameterizedSegmentChain(
            new PointXY(0f, 0f),
            new PointXY(3f, 0f),
            new PointXY(3f, 4f));

        ParameterizedCurveProjection projection = chain.ProjectWithParameter(new PointXY(5f, 2f));

        AssertPoint(projection.ProjectedPoint, 3f, 2f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(5f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(chain.Distance(new PointXY(5f, 2f)), Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void GetPointIntersections_WhenRayCrossesChain_ReturnsDistinctPointsInRayOrder()
    {
        var chain = new ParameterizedSegmentChain(
            new PointXY(0f, 1f),
            new PointXY(2f, 1f),
            new PointXY(2f, 3f),
            new PointXY(0f, 3f));
        var ray = new Ray(new PointXY(1f, 0f), MathF.PI / 2f);

        List<PointXY> intersections = chain.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 1f, 1f);
        AssertPoint(intersections[1], 1f, 3f);
    }

    [Test]
    public void GetPointIntersections_WhenRayHitsSharedVertex_ReturnsDistinctPoint()
    {
        var chain = new ParameterizedSegmentChain(
            new PointXY(0f, 0f),
            new PointXY(1f, 1f),
            new PointXY(2f, 0f));
        var ray = new Ray(new PointXY(1f, -1f), MathF.PI / 2f);

        List<PointXY> intersections = chain.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertPoint(intersections[0], 1f, 1f);
    }

    [Test]
    public void GetPointIntersections_WhenLineCrossesChain_ReturnsDistinctPointsInLineDirection()
    {
        var chain = new ParameterizedSegmentChain(
            new PointXY(2f, 0f),
            new PointXY(2f, 2f),
            new PointXY(0f, 2f),
            new PointXY(0f, 0f));
        var line = new Line(new PointXY(-1f, 1f), new PointXY(3f, 1f));

        List<PointXY> intersections = chain.GetPointIntersections(line);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 0f, 1f);
        AssertPoint(intersections[1], 2f, 1f);
    }

    [Test]
    public void GetPointIntersections_WhenParameterizedLineDirectionIsReversed_ReturnsPointsInParameterizedDirection()
    {
        var chain = new ParameterizedSegmentChain(
            new PointXY(2f, 0f),
            new PointXY(2f, 2f),
            new PointXY(0f, 2f),
            new PointXY(0f, 0f));
        var geometricLine = new Line(new PointXY(-1f, 1f), new PointXY(3f, 1f));
        var line = new ParameterizedLine(geometricLine, new PointXY(0f, 1f), new VectorXY(-1f, 0f));

        List<PointXY> intersections = chain.GetPointIntersections(line);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 2f, 1f);
        AssertPoint(intersections[1], 0f, 1f);
    }

    [Test]
    public void GetPointIntersections_WhenSegmentCrossesChain_ReturnsPointsFromEndpointAToEndpointB()
    {
        var chain = new ParameterizedSegmentChain(
            new PointXY(2f, 0f),
            new PointXY(2f, 2f),
            new PointXY(0f, 2f),
            new PointXY(0f, 0f));
        var segment = new Segment(new PointXY(3f, 1f), new PointXY(-1f, 1f));

        List<PointXY> intersections = chain.GetPointIntersections(segment);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 2f, 1f);
        AssertPoint(intersections[1], 0f, 1f);
    }

    [Test]
    public void GetPointIntersections_WhenVertexBelongsToContinuousSegmentOverlap_OmitsVertex()
    {
        var chain = new ParameterizedSegmentChain(
            new PointXY(0f, 0f),
            new PointXY(2f, 0f),
            new PointXY(2f, 2f));
        var segment = new Segment(new PointXY(-1f, 0f), new PointXY(3f, 0f));

        List<PointXY> intersections = chain.GetPointIntersections(segment);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void GetPointIntersections_WhenVertexBelongsToContinuousLineOverlap_OmitsVertex()
    {
        var chain = new ParameterizedSegmentChain(
            new PointXY(0f, 0f),
            new PointXY(2f, 0f),
            new PointXY(2f, 2f));
        var line = new Line(new PointXY(-1f, 0f), new PointXY(3f, 0f));

        List<PointXY> intersections = chain.GetPointIntersections(line);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void PointIntersections_WithParameterizedSegmentChain_ReturnsDistinctPointsInTargetTraversalOrder()
    {
        var line = new Line(new PointXY(-1f, 1f), new PointXY(3f, 1f));
        var segmentChain = new ParameterizedSegmentChain(
            new PointXY(2f, 0f),
            new PointXY(2f, 2f),
            new PointXY(0f, 2f),
            new PointXY(0f, 0f));

        List<PointXY> intersections = line.GetPointIntersections(segmentChain);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 2f, 1f);
        AssertPoint(intersections[1], 0f, 1f);
    }

    [Test]
    public void PointIntersections_WithParameterizedSegmentChainHitAtSharedVertex_ReturnsDistinctPoint()
    {
        var line = new Line(new PointXY(-1f, 1f), new PointXY(3f, 1f));
        var segmentChain = new ParameterizedSegmentChain(
            new PointXY(0f, 0f),
            new PointXY(1f, 1f),
            new PointXY(2f, 0f));

        List<PointXY> intersections = line.GetPointIntersections(segmentChain);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertPoint(intersections[0], 1f, 1f);
    }

    [Test]
    public void PointIntersections_WhenTargetVertexBelongsToContinuousLineOverlap_OmitsVertex()
    {
        var line = new Line(new PointXY(-1f, 0f), new PointXY(3f, 0f));
        var segmentChain = new ParameterizedSegmentChain(
            new PointXY(0f, 0f),
            new PointXY(2f, 0f),
            new PointXY(2f, 2f));

        List<PointXY> intersections = line.GetPointIntersections(segmentChain);

        Assert.That(intersections, Is.Empty);
    }

    private static void AssertPoint(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }
}
