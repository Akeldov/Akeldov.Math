using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class ParameterizedSegmentChainTests
{
    [TestCase(2f, 3f, -3f)]
    [TestCase(2f, -3f, 3f)]
    [TestCase(2f, 0f, 0f)]
    [TestCase(-2f, 0f, 2f)]
    [TestCase(6f, 0f, 2f)]
    public void SignedDistance_UsesDirectedSideAndPositiveCollinearExtensions(float x, float y, float expected)
    {
        var chain = new ParameterizedSegmentChain(default(PointXY), new PointXY(4, 0));
        var point = new PointXY(x, y);
        Assert.That(chain.SignedDistance(point), Is.EqualTo(expected));
        Assert.That(MathF.Abs(chain.SignedDistance(point)), Is.EqualTo(chain.Distance(point)));
        if (y != 0)
        {
            var reversed = new ParameterizedSegmentChain(new PointXY(4, 0), default(PointXY));
            Assert.That(reversed.SignedDistance(point), Is.EqualTo(-expected));
        }
    }

    [Test]
    public void SignedDistance_CrossingAndOverlappingSegmentsSelectFirstProjection()
    {
        var crossing = new ParameterizedSegmentChain(new PointXY(-4, 0), new PointXY(4, 0),
            new PointXY(0, -4), new PointXY(0, 4));
        Assert.That(crossing.SignedDistance(default), Is.Zero);
        var tie = new PointXY(1, 1);
        Assert.That(crossing.SignedDistance(tie), Is.EqualTo(-1));
        Assert.That(crossing.ProjectWithParameter(tie).CurveCoordinate, Is.EqualTo(5));
        Assert.That(crossing.SignedDistance(new PointXY(0.5f, 1)), Is.EqualTo(0.5f));
        var overlap = new ParameterizedSegmentChain(default(PointXY), new PointXY(4, 0), default(PointXY));
        Assert.That(overlap.SignedDistance(new PointXY(2, 1)), Is.EqualTo(-1));
        Assert.That(overlap.ProjectWithParameter(new PointXY(2, 1)).CurveCoordinate, Is.EqualTo(2));
    }

    [Test]
    public void SignedDistance_SharedVertexUsesUnitNormalBisector()
    {
        var chain = new ParameterizedSegmentChain(default(PointXY), new PointXY(2, 0), new PointXY(0, 2));
        Assert.That(chain.SignedDistance(new PointXY(3, 0.5f)), Is.EqualTo(MathF.Sqrt(1.25f)).Within(1e-6));
        Assert.That(chain.ProjectWithParameter(new PointXY(3, 0.5f)).CurveCoordinate, Is.EqualTo(2));
        var reversed = new ParameterizedSegmentChain(new PointXY(0, 2), new PointXY(2, 0), default(PointXY));
        Assert.That(reversed.SignedDistance(new PointXY(3, 0.5f)), Is.EqualTo(-MathF.Sqrt(1.25f)).Within(1e-6));
        var unequal = new ParameterizedSegmentChain(new PointXY(-200, 0), new PointXY(2, 0), new PointXY(0, 2));
        Assert.That(unequal.SignedDistance(new PointXY(3, 0.5f)), Is.EqualTo(chain.SignedDistance(new PointXY(3, 0.5f))));
    }

    [Test]
    public void SignedDistance_ReversalAtVertexFallsBackToSelectedSegment()
    {
        var chain = new ParameterizedSegmentChain(default(PointXY), new PointXY(2, 0), new PointXY(-4, 0));
        Assert.That(chain.SignedDistance(new PointXY(3, 1)), Is.EqualTo(-MathF.Sqrt(2)).Within(1e-6));
        Assert.That(chain.SignedDistance(new PointXY(3, -1)), Is.EqualTo(MathF.Sqrt(2)).Within(1e-6));
        Assert.That(chain.SignedDistance(new PointXY(3, 0)), Is.EqualTo(1));
    }

    [TestCase(-0.01f)]
    [TestCase(0f)]
    [TestCase(0.01f)]
    public void SignedDistance_OutsideCornerKeepsSideAcrossIncomingExtension(float y)
    {
        var chain = new ParameterizedSegmentChain(default(PointXY), new PointXY(2, 0), new PointXY(0, 2));
        var point = new PointXY(3, y);
        Assert.That(chain.SignedDistance(point), Is.GreaterThan(0));
        Assert.That(MathF.Abs(chain.SignedDistance(point)), Is.EqualTo(chain.Distance(point)));
    }

    [TestCase(float.NaN, 0f)]
    [TestCase(0f, float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity, 0f)]
    public void SignedDistance_RejectsNonFinitePoints(float x, float y)
    {
        var chain = new ParameterizedSegmentChain(default(PointXY), new PointXY(1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => chain.SignedDistance(new PointXY(x, y)));
    }

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
