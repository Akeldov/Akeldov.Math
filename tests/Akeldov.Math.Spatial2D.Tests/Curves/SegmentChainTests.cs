using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class SegmentChainTests
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

        var chain = new SegmentChain(points);
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
        var chain = new SegmentChain(
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
        Assert.Throws<ArgumentNullException>(() => new SegmentChain(null!));
        Assert.Throws<ArgumentException>(() => new SegmentChain(new PointXY(0f, 0f)));
        Assert.Throws<ArgumentException>(() => new SegmentChain(
            new PointXY(0f, 0f),
            new PointXY(0f, 0f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SegmentChain(
            new PointXY(0f, 0f),
            new PointXY(float.PositiveInfinity, 0f)));
    }

    [Test]
    public void GetPoint_WhenCoordinateCrossesSegmentBoundary_ReturnsPointOnCorrectSegment()
    {
        var chain = new SegmentChain(
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
        var chain = new SegmentChain(
            new PointXY(0f, 0f),
            new PointXY(3f, 0f));

        Assert.Throws<ArgumentOutOfRangeException>(() => chain.GetPoint(curveCoordinate));
    }

    [Test]
    public void ProjectWithParameter_WhenClosestPointIsOnLaterSegment_ReturnsChainCoordinate()
    {
        var chain = new SegmentChain(
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
    public void GetRayIntersections_WhenRayCrossesChain_ReturnsDistinctPointsInRayOrder()
    {
        var chain = new SegmentChain(
            new PointXY(0f, 1f),
            new PointXY(2f, 1f),
            new PointXY(2f, 3f),
            new PointXY(0f, 3f));
        var ray = new Ray(new PointXY(1f, 0f), MathF.PI / 2f);

        List<PointXY> intersections = chain.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 1f, 1f);
        AssertPoint(intersections[1], 1f, 3f);
    }

    [Test]
    public void GetRayIntersections_WhenRayHitsSharedVertex_ReturnsDistinctPoint()
    {
        var chain = new SegmentChain(
            new PointXY(0f, 0f),
            new PointXY(1f, 1f),
            new PointXY(2f, 0f));
        var ray = new Ray(new PointXY(1f, -1f), MathF.PI / 2f);

        List<PointXY> intersections = chain.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertPoint(intersections[0], 1f, 1f);
    }

    [Test]
    public void GetRayIntersections_WhenGeometryEpsilonIsInvalid_Throws()
    {
        var chain = new SegmentChain(
            new PointXY(0f, 0f),
            new PointXY(1f, 0f));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            chain.GetRayIntersections(default(Ray), float.NaN));
    }

    private static void AssertPoint(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }
}
